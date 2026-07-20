# Anexos (Supabase Storage) — Tasks

**Design**: `.specs/features/anexos-storage/design.md`
**Status**: Done (2026-07-20) — T1 a T11 completas. Upload/download real contra o Supabase Storage segue pendente da Service Role Key (usuário foi atrás em paralelo)

---

## Execution Plan

### Phase 1: Domain + Migration (Sequential)
```
T1 → T2
```

### Phase 2: Infraestrutura (Sequential, depende de T2)
```
T2 → T3
```

### Phase 3: Application (Sequential — todos tocam o mesmo IChamadoRepository)
```
T3 → T4 → T5 → T6
```

### Phase 4: Web API (Sequential, depende de T4-T6)
```
T6 → T7
```

### Phase 5: Frontend (Sequential — components dependem do hook, hook depende do endpoint)
```
T7 → T8 → T9 → T10
```

### Phase 6: Verificação (Sequential)
```
T10 → T11
```

---

## Task Breakdown

### T1: `Anexo` ganha `EnviadoPorId`/`EnviadoPorNome` + migration

**What**: 2 propriedades novas na entidade + migration aditiva (nullable, sem quebrar anexos futuros — hoje não existe nenhum anexo real no banco, então sem backfill necessário)
**Where**: `Domain/Entities/Anexo.cs`, `Infrastructure/Data/Configurations/AnexoConfiguration.cs`, nova migration
**Depends on**: None
**Reuses**: Padrão de migration aditiva já usado em `AddNumeroChamado`

**Done when**:
- [ ] `EnviadoPorId` (Guid?) e `EnviadoPorNome` (string, `HasMaxLength(150)`) adicionados
- [ ] Migration criada via `dotnet ef migrations add AddEnviadoPorAnexo`, aplicada contra o Supabase real
- [ ] 3 artefatos da migration em sincronia (.cs, .Designer.cs, ModelSnapshot)

**Tests**: none (propriedades simples, sem lógica de domínio nova)
**Gate**: `dotnet build`

---

### T2: `IStorageService` revisado

**What**: Assinatura final (`UploadAsync`, `ObterUrlAssinadaAsync`), remove `DownloadAsync`/`RemoverAsync`
**Where**: `Domain/Interfaces/IStorageService.cs`
**Depends on**: None
**Reuses**: N/A — é o próprio contrato sendo revisado

**Done when**:
- [ ] Interface com as 2 assinaturas do design
- [ ] Build passa (nenhum consumidor quebrado, já que não há implementação ainda)

**Tests**: none
**Gate**: `dotnet build`

---

### T3: `SupabaseStorageService` + `SupabaseSettings`

**What**: Implementação real via pacote NuGet `Supabase`, registrada em `Program.cs`
**Where**: `Infrastructure/Services/SupabaseStorageService.cs`, `Application/Common/SupabaseSettings.cs`, `Program.cs`
**Depends on**: T2
**Reuses**: Padrão de `AuthSettings` (registro via `IOptions`, validação de config obrigatória no startup)

**Done when**:
- [ ] Pacote `Supabase` adicionado ao `ChamadosCamarj.Infrastructure.csproj`
- [ ] `SupabaseSettings` (Url, ServiceRoleKey, Bucket) bound de `builder.Configuration.GetSection("Supabase")`
- [ ] `SupabaseStorageService` implementa `UploadAsync`/`ObterUrlAssinadaAsync` usando `Client.Storage.From(bucket)`
- [ ] `Supabase.Client` inicializado como singleton no startup (`InitializeAsync()`), registrado via DI
- [ ] Build passa

**Tests**: none (wrapper fino sobre SDK externo — sem lógica própria pra unitário cobrir; verificação é via curl real, ver T11)
**Gate**: `dotnet build`

---

### T4: `IChamadoRepository` ganha métodos de Anexo

**What**: `AdicionarAnexoAsync`, `ObterAnexosPorChamadoAsync`, `ObterAnexoPorIdAsync`
**Where**: `Domain/Interfaces/IChamadoRepository.cs`, `Infrastructure/Repositories/ChamadoRepository.cs`
**Depends on**: T1
**Reuses**: Espelha exatamente `AdicionarComentarioAsync`/`ObterComentariosPorChamadoAsync`

**Done when**:
- [ ] 3 métodos novos na interface e na implementação
- [ ] Build passa

**Tests**: none (repositório não tem cobertura dedicada no projeto — mesmo padrão já estabelecido)
**Gate**: `dotnet build`

---

### T5: `AdicionarAnexoCommand` + Handler + Validator

**What**: Valida (tamanho ≤10MB, extensão na allow-list), faz upload, persiste
**Where**: `Application/Features/Chamados/Commands/AdicionarAnexoCommand.cs` (+Handler), `Application/Features/Chamados/Validators/AdicionarAnexoCommandValidator.cs`, `Application/Features/Chamados/DTOs/AnexoResponse.cs`
**Depends on**: T3, T4
**Reuses**: Estrutura de `ComentarChamadoCommandHandler`

**Done when**:
- [ ] Command/Handler/Validator seguindo o design
- [ ] Testes unitários (mock de `IChamadoRepository`+`IStorageService`): sucesso, arquivo grande demais rejeitado, extensão inválida rejeitada, chamado inexistente → `NotFoundException`
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: unit (Application Handlers + Validators)
**Gate**: `dotnet test --no-build`

---

### T6: `ListarAnexosQuery` + `ObterUrlDownloadAnexoQuery`

**What**: Listagem e geração de URL de download
**Where**: `Application/Features/Chamados/Queries/`
**Depends on**: T4
**Reuses**: Estrutura de `ListarComentariosQueryHandler`

**Done when**:
- [ ] 2 queries + handlers implementados
- [ ] Testes unitários: listagem retorna os anexos do chamado certo; URL de download chama `ObterUrlAssinadaAsync` com o caminho certo; anexo inexistente → `NotFoundException`
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: unit
**Gate**: `dotnet test --no-build`

**Commit**: `feat(chamados): backend de anexos (upload, listagem, download via Supabase Storage)`

---

### T7: Endpoints no `ChamadosController`

**What**: 3 endpoints novos (upload multipart, listar, gerar URL de download)
**Where**: `WebApi/Controllers/ChamadosController.cs`
**Depends on**: T5, T6
**Reuses**: Padrão dos endpoints de Comentários no mesmo controller

**Done when**:
- [ ] `POST /chamados/{id}/anexos`, `GET /chamados/{id}/anexos`, `GET /chamados/{id}/anexos/{anexoId}/download-url`
- [ ] Identidade (`UsuarioId`/`Nome`) vem de `_currentUser`, não do body
- [ ] Suite completa continua passando (sem regressão)
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: none (nenhum outro endpoint do projeto tem teste de Controller dedicado)
**Gate**: `dotnet test --no-build`

---

### T8: `api.ts` + `types/api.ts` (frontend)

**What**: Funções de acesso (`uploadAnexo`, `listarAnexos`, `obterUrlDownloadAnexo`) + tipo `AnexoResponse`
**Where**: `frontend/src/features/chamados/api.ts`, `frontend/src/types/api.ts`
**Depends on**: T7
**Reuses**: `apiFetch` — mas `uploadAnexo` precisa mandar `FormData`, não `JSON.stringify` (checar se `apiFetch` já suporta isso ou precisa de ajuste mínimo)

**Done when**:
- [ ] 3 funções + tipo `AnexoResponse`
- [ ] `npm run build` limpo

**Tests**: none
**Gate**: `npm run build`

---

### T9: `useAnexos.ts` (hooks)

**What**: `useAnexos(chamadoId)` (query) + `useUploadAnexo(chamadoId)` (mutation)
**Where**: `frontend/src/features/chamados/hooks/useAnexos.ts`
**Depends on**: T8
**Reuses**: Padrão de `useAcoesChamado.ts`/`useHistorico.ts`

**Done when**:
- [ ] 2 hooks, invalidação de `['anexos', chamadoId]` no `onSuccess` do upload
- [ ] `npm run build` limpo

**Tests**: none
**Gate**: `npm run build`

---

### T10: `AnexosList.tsx` + `UploadAnexoForm.tsx`, plugados no Detalhe

**What**: UI de listagem + upload
**Where**: `frontend/src/features/chamados/components/AnexosList.tsx`, `UploadAnexoForm.tsx`, `ChamadoDetailPage.tsx` (modificado)
**Depends on**: T9
**Reuses**: Erro inline (padrão do projeto, sem toast)

**Done when**:
- [ ] Lista mostra nome/tamanho, botão baixar busca a URL sob demanda
- [ ] Form valida tamanho no cliente antes de enviar (feedback imediato)
- [ ] `npm run build` limpo

**Tests**: none
**Gate**: `npm run build`

**Commit**: `feat(chamados): UI de anexos no detalhe do chamado`

---

### T11: Verificação

**What**: Suite completa + verificação manual possível sem a Service Role Key real
**Where**: N/A
**Depends on**: T10

**Done when**:
- [x] `dotnet test --no-build` — todos passam, contagem crescente sem exclusão silenciosa
- [x] `npm run build` — limpo
- [x] **Sem a Service Role Key real**: upload/download de ponta a ponta **não foi verificado contra o Supabase de verdade** — registrado como pendência
- [x] Validar que os endpoints existem e respondem corretamente ao guard de validação (tamanho/extensão) mesmo sem Storage real configurado

**Tests**: none (verificação)
**Gate**: full (`dotnet test --no-build` + `npm run build`)

**Resultado real (2026-07-20):**
- `dotnet test`: 216/216 (eram 197 antes desta feature — 19 testes novos: 9 validator, 7 handler de Anexo, 3 já contavam do T5/T6 combinados)
- `npm run build` (frontend): limpo
- **Bug real encontrado e corrigido durante esta verificação (fora do escopo original):** a API **não subia** sem a Service Role Key do Supabase configurada — o validador de DI do ASP.NET Core (`ValidateOnBuild`, ativo em Development) derruba a aplicação inteira no `Build()` porque `AdicionarAnexoCommandHandler` exige `IStorageService` no construtor, e nada registrava essa interface quando a chave estava ausente. Isso derrubaria **a API inteira**, não só a feature de Anexos — muito mais grave que o padrão de tolerância já usado pro `GoogleClientId` (que não bloqueia o boot). Corrigido criando `NullStorageService : IStorageService` (Infrastructure), registrado como fallback quando `Supabase:Url`/`Supabase:ServiceRoleKey` estão vazios — lança `InvalidOperationException` com mensagem clara só se alguém de fato chamar um endpoint de Anexo, sem impedir o resto da aplicação de funcionar.
- **Verificado via curl contra a API + Supabase real** (chamado real criado e depois removido, `CAM-39`): extensão inválida (`.exe`) → 400 com mensagem de validação; arquivo válido (`.pdf`) sem a Service Role Key → 400 com a mensagem clara do `NullStorageService` (não um crash); listagem de anexos (vazia) e resto da API (outros endpoints) continuam funcionando normalmente.
- **Não verificado**: upload real contra o bucket do Supabase Storage, geração de signed URL real, download real — tudo isso depende da Service Role Key, que o usuário ainda não tem (confirmado como pendência, sem previsão).

---

## Task Granularity Check

| Task | Scope | Status |
|---|---|---|
| T1 | 1 entidade + 1 migration | ✅ Granular |
| T2 | 1 interface | ✅ Granular |
| T3 | 1 serviço + 1 settings + registro DI | ✅ OK (coeso) |
| T4 | 3 métodos, 1 interface + 1 implementação | ✅ OK (coeso) |
| T5 | Command + Handler + Validator + DTO | ✅ OK (coeso, mesmo padrão de tasks anteriores) |
| T6 | 2 queries + handlers | ✅ OK (coeso) |
| T7 | 3 endpoints, 1 controller | ✅ OK (coeso) |
| T8 | 3 funções + 1 tipo | ✅ OK (coeso) |
| T9 | 2 hooks | ✅ Granular |
| T10 | 2 componentes + wiring | ✅ OK (coeso) |
| T11 | Verificação | ✅ Granular |

## Diagram-Definition Cross-Check

| Task | Depends On (corpo) | Diagrama | Status |
|---|---|---|---|
| T1 | None | Sem seta | ✅ Match |
| T2 | None | Sem seta | ✅ Match |
| T3 | T2 | T2 → T3 | ✅ Match |
| T4 | T1 | T1 → T4 (via fases) | ✅ Match |
| T5 | T3, T4 | T3, T4 → T5 (via fases sequenciais) | ✅ Match |
| T6 | T4 | T4 → T6 | ✅ Match |
| T7 | T5, T6 | T5, T6 → T7 | ✅ Match |
| T8 | T7 | T7 → T8 | ✅ Match |
| T9 | T8 | T8 → T9 | ✅ Match |
| T10 | T9 | T9 → T10 | ✅ Match |
| T11 | T10 | T10 → T11 | ✅ Match |

## Test Co-location Validation

| Task | Camada | Matriz exige | Task diz | Status |
|---|---|---|---|---|
| T1 | Domain (propriedades) | Nenhum requisito p/ propriedades simples | none | ✅ OK |
| T2 | Interface | Nenhum requisito | none | ✅ OK |
| T3 | Infrastructure (wrapper SDK externo) | Nenhum requisito — infra externa não é unitário no projeto | none | ✅ OK |
| T4 | Infrastructure (repositório) | Nenhum requisito — repositório não tem testes dedicados | none | ✅ OK |
| T5 | Application (Handler+Validator) | unit | unit | ✅ OK |
| T6 | Application (Handler) | unit | unit | ✅ OK |
| T7 | WebApi (Controller) | Nenhum requisito — nenhum controller do projeto tem teste próprio | none | ✅ OK |
| T8-T10 | Frontend | Nenhum — decisão já registrada, sem testes unitários de frontend | none | ✅ OK |
| T11 | Verificação | N/A | none, gate full | ✅ OK |

---

## Ferramentas

Context7 já usado no Design pra confirmar a API do SDK Supabase — não precisa de novo na Execute, a menos que surja dúvida de uso real. Nenhuma outra skill/MCP necessária.
