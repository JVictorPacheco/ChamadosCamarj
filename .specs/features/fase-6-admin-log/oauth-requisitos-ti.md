# Pré-requisitos de infraestrutura — Login real via Google Workspace (CAMARJ)

> Documento pra passar pro time de TI. Objetivo: viabilizar o "Entrar com Google" no sistema de Chamados, usando as contas corporativas `@camarj.com.br` já existentes no Google Workspace da empresa.
> Criado em 2026-07-18, referente ao item F5b de `.specs/features/fase-6-admin-log/spec.md`.

---

## Contexto rápido

Hoje o sistema de Chamados tem um login provisório (a pessoa digita o e-mail dela e entra, sem senha — usado só internamente, durante o desenvolvimento). O próximo passo é substituir isso pelo login de verdade: a pessoa clica "Entrar com Google", usa a conta `@camarj.com.br` dela (a mesma do Gmail/Workspace corporativo), e o sistema confirma automaticamente que é ela.

Pra isso funcionar, a TI precisa fazer uma configuração no **Google Cloud Console** (é gratuito, não tem custo de licença adicional — usa a mesma organização do Google Workspace já existente).

---

## O que a TI precisa fazer

### 1. Criar (ou reaproveitar) um projeto no Google Cloud Console

- Acessar [console.cloud.google.com](https://console.cloud.google.com) com uma conta admin do Workspace da CAMARJ.
- Criar um projeto novo (ex: "Chamados CAMARJ") ou usar um já existente da organização, se já tiver algum de outro sistema interno.

### 2. Configurar a "tela de consentimento OAuth" (OAuth consent screen)

- Dentro do projeto, ir em **APIs & Services > OAuth consent screen**.
- **Tipo de usuário: "Internal" (Interno).** Isso é importante — restringe o login só a contas `@camarj.com.br` do Workspace, sem precisar de aprovação pública do Google nem de verificação de domínio adicional (o "Internal" só aparece disponível se o Cloud Console estiver associado à organização do Workspace).
- Preencher nome do app ("Chamados CAMARJ"), e-mail de suporte, e-mail de contato do desenvolvedor.

### 3. Criar as credenciais (OAuth Client ID)

- Ir em **APIs & Services > Credentials > Create Credentials > OAuth Client ID**.
- Tipo de aplicação: **"Web application"**.
- **Authorized JavaScript origins** (de onde o login pode ser iniciado):
  - `http://localhost:5173` (ambiente de desenvolvimento)
  - A URL de produção do sistema, quando definida (ver pendência abaixo)
- **Authorized redirect URIs** (pra onde o Google devolve o resultado do login):
  - Mesma coisa: uma pra desenvolvimento, uma pra produção.
- Ao salvar, o Google gera um **Client ID** (uma string tipo `123456789-abc.apps.googleusercontent.com`).

### 4. Enviar de volta pro time de desenvolvimento

- O **Client ID** gerado no passo 3.
- Confirmação de que o domínio usado é `camarj.com.br` (pra garantir que só essas contas conseguem logar).

---

## ⚠️ Pendência que bloqueia o passo 3 (redirect URI de produção)

Ainda não está decidido **onde a aplicação vai rodar em produção** (VM própria, Docker, Azure App Service, etc. — ver `.specs/project/STATE.md`, seção Pendências). Sem isso, não dá pra saber a URL final do sistema, e portanto não dá pra configurar o "Authorized redirect URI" de produção ainda.

**Isso não impede começar os passos 1-3 com só o `localhost` de desenvolvimento** — a TI pode adiantar a criação do projeto/credenciais agora, e só adicionar a URL de produção depois, quando a hospedagem for decidida (é um campo que dá pra editar depois, sem precisar recriar as credenciais).

---

## O que NÃO precisa (fora de escopo desta configuração)

- **Não precisa de Client Secret** para este fluxo — o sistema usa o modelo "Sign in with Google" do lado do navegador (o front-end recebe um token e o back-end só valida esse token contra o Google), que não exige guardar um segredo no servidor.
- **Não precisa aprovar o app publicamente** — por ser "Internal", só contas da própria organização Workspace conseguem usar, sem passar pela revisão pública do Google (que existe só pra apps "External").
- **Não precisa mexer em usuários/contas do Workspace** — o sistema já tem uma tabela própria (`UsuarioPerfil`) que mapeia cada e-mail pro perfil de acesso (Admin/Atendente/Solicitante) dentro do sistema de Chamados; a TI não precisa cadastrar nada lá, isso é feito pelo Admin do sistema (tela `Admin > Usuários`).

---

## Resumo — o que a TI precisa devolver pro dev

| Item | Onde conseguir |
|---|---|
| Client ID do OAuth | Google Cloud Console → Credentials, depois de criar o "OAuth Client ID" |
| Confirmação do domínio (`camarj.com.br`) | Já sabido, só confirmar |
| URL de produção (quando decidida) | Depende da decisão de hospedagem — pendência separada, não bloqueia o resto |
