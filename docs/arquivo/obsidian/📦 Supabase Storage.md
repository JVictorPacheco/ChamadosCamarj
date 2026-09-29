# 📦 Supabase Storage — Anexos

> ✅ Implementado e verificado de ponta a ponta contra o Supabase real (2026-07-21). Spec/design/tasks completos em `.specs/features/anexos-storage/`.

## Decisão

Usar **Supabase Storage** (bucket S3-compatible) para armazenar anexos.

## Vantagens

- ✅ Já vamos usar Supabase p/ PostgreSQL
- ✅ Bucket S3 nativo
- ✅ URLs assinadas (temporárias) para acesso seguro
- ✅ Controle de acesso por bucket
- ✅ Sem infra extra — tudo no mesmo lugar

## Estrutura de Buckets

```
chamados-anexos/
├── {chamado-id}/
│   ├── {arquivo-uuid}.pdf
│   └── {arquivo-uuid}.jpg
```

## Regras

- Máx 10MB por arquivo
- Tipos permitidos: PDF, imagens, .doc, .xls, .zip
- URLs assinadas expiram em 1 hora
- Apenas autenticados via [[🔐 Google Workspace]] podem acessar

## Tecnologia

- **SDK:** `Supabase` NuGet package v1.3.0 (para .NET)
- **Upload:** Direto do backend, via `IStorageService`/`SupabaseStorageService` — multipart form-data (`IFormFile`)
- **Download:** URL assinada gerada sob demanda, nunca streaming pelo backend

## Implementação real (2026-07-21)

- `Anexo` ganhou `EnviadoPorId`/`EnviadoPorNome`, preenchido via `ICurrentUserService` (identidade real, não vinda do cliente)
- Endpoints: `POST /chamados/{id}/anexos` (upload), `GET /chamados/{id}/anexos` (listar), `GET /chamados/{id}/anexos/{anexoId}/download-url` (gerar URL assinada)
- `IStorageService` sempre registrado no DI, mesmo sem a Service Role Key configurada — usa `NullStorageService` como fallback (erro claro só se a feature for chamada), evitando que a API inteira deixe de subir
- Anexo nunca é removido — mesma filosofia append-only do resto do sistema (`RemoverAsync` tirado da interface por não ter uso)

## 2 bugs reais encontrados durante a verificação

1. **API não subia sem a Service Role Key** — `ValidateOnBuild` do ASP.NET Core derruba a aplicação inteira se um Handler exigir uma dependência não registrada no DI. Corrigido com `NullStorageService`.
2. **SDK devolve `CreateSignedUrl` com um `?` sobrando no final** — quebra o parse do JWT no servidor do Storage (`InvalidJWT`). Corrigido com `url.TrimEnd('?')` em `SupabaseStorageService`.

## Nota sobre chaves do Supabase

O dashboard do Supabase migrou pra um novo formato de API key (`sb_secret_...`/`sb_publishable_...`), mas o SDK `Supabase` (C#) usado aqui é da geração anterior — precisa da chave **legada** (`service_role`, formato JWT, aba "Legacy anon, service_role API keys" nas Settings > API do projeto).
