# Grupos/Equipes — Especificação

**Status:** Em andamento  
**Data:** 2026-07-28  
**Feature:** G4

## Por quê?

Hoje um Atendente só vê os chamados que **ele mesmo** é responsável. Se o Fábio sai de férias, ninguém do setor dele consegue ver/acompanhar os chamados que estavam com ele. A ideia é agrupar usuários por equipe (Reembolso, Credenciado, Comercial, etc.) e permitir que atendentes do mesmo grupo vejam e interajam nos chamados uns dos outros.

## O que muda?

1. Nova entidade `Grupo` (Id, Nome, Ativo)
2. `UsuarioPerfil` ganha `GrupoId` (opcional — Admin não precisa de grupo)
3. CRUD de Grupos (só Admin)
4. RBAC: Atendente vê chamados do seu grupo (não só os próprios)
5. Admin continua vendo tudo. Solicitante só vê os que abriu (sem mudança).

## Regras de Negócio

- **G4-01:** Todo Atendente pode pertencer a um Grupo (opcional).
- **G4-02:** Admin e Solicitante não precisam de grupo.
- **G4-03:** Atendente com grupo vê chamados do seu grupo (além dos próprios).
- **G4-04:** Atendente sem grupo vê só os próprios (comportamento atual).
- **G4-05:** Só Admin gerencia Grupos (CRUD).
- **G4-06:** Ao atribuir um chamado, só pode escolher atendentes do mesmo grupo.
- **G4-07:** Grupos são independentes de categorias (um grupo pode atender múltiplas categorias).

## Fluxo

### Admin cria um Grupo
1. Admin acessa `/admin/grupos`
2. Clica "Novo grupo"
3. Preenche nome (ex: "Reembolso")
4. Salva

### Admin associa usuário a Grupo
1. Admin edita usuário (tela existente)
2. Seleciona o Grupo no dropdown (novo campo)
3. Salva

### Atendente vê chamados do grupo
1. Fábio (grupo "Reembolso") acessa "Meus Chamados"
2. Vê chamados onde `responsavelId = Fábio` OU `responsavelId` pertence a alguém do grupo "Reembolso"
3. Pode assumir, comentar, resolver — como se fossem dele

## Casos de Teste

- ✅ **T1:** Criar grupo "Reembolso" (Admin) → 201
- ✅ **T2:** Listar grupos → retorna lista
- ✅ **T3:** Editar nome do grupo → 200
- ✅ **T4:** Atendente sem grupo → vê só os próprios chamados
- ✅ **T5:** Atendente com grupo "Reembolso" → vê chamados do grupo + próprios
- ✅ **T6:** Solicitante → sem mudança (vê só os que abriu)
- ✅ **T7:** Admin → continua vendo tudo
- ✅ **T8:** Associar usuário a grupo → dropdown aparece na edição
- ✅ **T9:** Reatribuir → só lista atendentes do mesmo grupo

## Não escopo (v2)

- Subgrupos / hierarquia
- Permissões customizadas por grupo
- Dashboard / Relatório Mensal filtrado por grupo
