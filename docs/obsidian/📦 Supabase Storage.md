# 📦 Supabase Storage — Anexos

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
- Apenas autenticados via [[🔐 Azure AD]] podem acessar

## Tecnologia

- **SDK:** `Supabase` NuGet package (para .NET)
- **Upload:** Direto do backend ou URL pré-assinada pro frontend
