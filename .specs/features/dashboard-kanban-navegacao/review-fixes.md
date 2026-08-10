# Code Review: Fixes dos 5 Pontos de Atenção

**Data:** 2026-08-07  
**Branch:** `feature/fix-review-dashboard-kanban`  
**Build:** `npm run build` — passou (sem erros TS/Vite)  
**Veredito:** Aprovado

---

## Resumo

Correção dos 5 pontos de atenção levantados na review anterior (`review.md`), todos não bloqueantes.

---

## Fixes

### Fix 1 — `categoriaId` sem validação GUID

**Arquivo:** `frontend/src/features/chamados/ChamadosListPage.tsx:14,26`

**Mudança:** Regex `GUID_REGEX` adicionada. `categoriaId` da URL só é incluído no filtro se passar na validação de formato UUID.

```diff
+const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
 ...
-...(categoriaId ? { categoriaId } : {}),
+...(categoriaId && GUID_REGEX.test(categoriaId) ? { categoriaId } : {}),
```

**Análise:** ✅ Correto. Regex cobre formato UUID v4. Parâmetros inválidos são silenciosamente ignorados.

---

### Fix 2 — Kanban limitado a 100 chamados

**Arquivos:** `useKanbanChamados.ts`, `KanbanPage.tsx`

**Mudança:** Hook agora detecta `temProxima` da API, expõe `temMais` e `carregarMais()` com merge deduplicado por ID. `KanbanPage` renderiza botão "Carregar mais".

```ts
// useKanbanChamados.ts — novo state e função
const [temMais, setTemMais] = useState(false)
const [carregandoMais, setCarregandoMais] = useState(false)
const [paginaAtual, setPaginaAtual] = useState(1)

const carregarMais = useCallback(async () => {
  // busca próxima página, faz merge via Set de IDs, atualiza cache
}, [carregandoMais, temMais, paginaAtual, queryClient])
```

```tsx
// KanbanPage.tsx — botão de carregar mais
{temMais && (
  <div className="flex justify-center px-4 pb-4">
    <Button variant="outline" size="sm" onClick={carregarMais} disabled={carregandoMais}>
      {carregandoMais ? 'Carregando...' : 'Carregar mais'}
    </Button>
  </div>
)}
```

**Análise:** ✅ Correto. Merge usa `Set` para evitar duplicatas. `carregandoMais` previne double-click. O uso de `queryClient.setQueryData` direto foge ligeiramente da convenção TanStack Query ideal (`useInfiniteQuery`), mas é funcional e não bloqueante.

---

### Fix 3 — SignalR sem filtro de evento no Kanban

**Arquivo:** `useKanbanChamados.ts:46-49`

**Mudança:** Callback `subscribe` agora filtra por `event.type` relevante.

```diff
-useEffect(() => {
-  return subscribe(() => {
-    queryClient.invalidateQueries({ queryKey: ['chamados', 'kanban'] })
-  })
-}, [subscribe, queryClient])
+useEffect(() => {
+  return subscribe((event) => {
+    if (event.type === 'ChamadoCriado' || event.type === 'StatusAlterado') {
+      queryClient.invalidateQueries({ queryKey: ['chamados', 'kanban'] })
+    }
+  })
+}, [subscribe, queryClient])
```

**Análise:** ✅ Correto. Evita refetch desnecessário em eventos irrelevantes (ex: `MetricasAtualizadas`). Os dois tipos filtrados cobrem criação de chamado e mudança de coluna (drag-and-drop).

---

### Fix 4 — Nomenclatura confusa em `handlePrioridadeClick`

**Arquivo:** `frontend/src/features/dashboard/DashboardPage.tsx:19-22,41-42,144`

**Mudança:** Interface `PrioridadeClickData` com campo `prioridadeNome`. Adapter inline no chart mapeia `categoriaNome` → `prioridadeNome`.

```ts
interface PrioridadeClickData {
  prioridadeNome: string
  quantidade: number
}

const handlePrioridadeClick = useCallback((item: PrioridadeClickData) => {
  navigate(`/chamados?prioridade=${item.prioridadeNome}`)
}, [navigate])

// no chart:
onBarClick={(item) => handlePrioridadeClick({ prioridadeNome: item.categoriaNome, quantidade: item.quantidade })}
```

**Análise:** ✅ Correto. O nome do campo agora expressa o conteúdo. Adapter inline é leve e apropriado.

---

### Fix 5 — `buildQueryString` não filtra `null`

**Arquivo:** `frontend/src/features/chamados/api.ts:35`

**Mudança:** Condição de ` !== undefined` para ` != null`, capturando ambos `null` e `undefined`.

```diff
-if (valor !== undefined) {
+if (valor != null) {
   params.set(chave, String(valor))
 }
```

**Análise:** ✅ Correto. `!= null` (comparação abstrata) captura ambos. Impede `?categoriaId=null` na URL. Solução idiomática em JavaScript.

---

## Gate Checks

| Check | Resultado |
|---|---|
| `npm run build` (frontend) | ✅ Passou — `tsc -b && vite build` sem erros |

⚠️ Warnings no build são de `@microsoft/signalr` (lib de terceiros) e chunk size (pré-existente), não introduzidos por estas correções.

---

## Veredito Final

**Aprovado.** Todos os 5 pontos de atenção foram tratados de forma correta, concisa e sem introduzir novos problemas. O build passa limpo. Nenhuma alteração bloqueante necessária.
