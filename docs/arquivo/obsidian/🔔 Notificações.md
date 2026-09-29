# 🔔 Notificações — Push + Desktop

## Conceito

### Push Notification (Navegador)
Notificação que aparece no **canto do navegador/desktop** mesmo se a aba não estiver aberta:

- ✅ Novo chamado atribuído a você
- ✅ Comentário no seu chamado
- ✅ Chamado próximo do SLA estourar
- ✅ Alguém mencionou você
- Usa a **API Notification do navegador** + **Service Workers** + **SignalR**

> Ex: Você está no Chrome, a aba do ChamadosCamarj tá em segundo plano, e aparece um popup no canto: *"🔔 Novo chamado #42 atribuído a você"*

### Desktop App (Futuro)
App nativo usando **Electron** ou **Tauri** (leve, em Rust):

- Notificações do sistema operacional
- Ícone na bandeja do sistema
- Abrir direto pelo atalho do desktop
- **Previsto: Fase 8+** (pós-lançamento)

## Como vai funcionar

```
SignalR (tempo real)
       ↓
Notificação no navegador (Push API)
       ↓
Se navegador fechado → fallback pra email
```

## Tecnologias

| Tecnologia | Pra quê |
|------------|---------|
| **SignalR** | Conexão WebSocket em tempo real |
| **Push API** | Notification do navegador |
| **Service Worker** | Receber push mesmo com aba inativa |
| **Tauri** (futuro) | App desktop nativo leve |
