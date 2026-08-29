import { apiFetch } from '@/lib/api'
import type {
  ChatConversaResponse,
  ChatMensagemResponse,
  ChatPresencaResponse,
  ChatHistoricoResponse,
  ChatPerfil,
  PagedResult,
} from '@/types/api'

// --- Conversas ---

export function listarConversas(): Promise<ChatConversaResponse[]> {
  return apiFetch<ChatConversaResponse[]>('/chat/conversas')
}

export function criarConversa(destinatarioId: string): Promise<ChatConversaResponse> {
  return apiFetch<ChatConversaResponse>('/chat/conversas', {
    method: 'POST',
    body: JSON.stringify({ destinatarioId }),
  })
}

export function criarGrupo(nome: string, participanteIds: string[]): Promise<ChatConversaResponse> {
  return apiFetch<ChatConversaResponse>('/chat/grupos', {
    method: 'POST',
    body: JSON.stringify({ nome, participanteIds }),
  })
}

// --- Mensagens ---

export function listarMensagens(conversaId: string, pagina: number): Promise<PagedResult<ChatMensagemResponse>> {
  return apiFetch<PagedResult<ChatMensagemResponse>>(
    `/chat/conversas/${conversaId}/mensagens?pagina=${pagina}&tamanhoPagina=30`
  )
}

export function enviarMensagem(
  conversaId: string,
  conteudo: string,
  respostaParaMensagemId?: string
): Promise<ChatMensagemResponse> {
  return apiFetch<ChatMensagemResponse>(`/chat/conversas/${conversaId}/mensagens`, {
    method: 'POST',
    body: JSON.stringify({ conteudo, respostaParaMensagemId }),
  })
}

export function enviarArquivo(conversaId: string, arquivo: File): Promise<ChatMensagemResponse> {
  const formData = new FormData()
  formData.append('arquivo', arquivo)
  return apiFetch<ChatMensagemResponse>(`/chat/conversas/${conversaId}/arquivos`, {
    method: 'POST',
    body: formData,
  })
}

export function editarMensagem(mensagemId: string, conteudo: string): Promise<void> {
  return apiFetch<void>(`/chat/mensagens/${mensagemId}`, {
    method: 'PATCH',
    body: JSON.stringify({ conteudo }),
  })
}

export function deletarMensagem(mensagemId: string): Promise<void> {
  return apiFetch<void>(`/chat/mensagens/${mensagemId}`, { method: 'DELETE' })
}

// --- Reações e Leitura ---

export function adicionarReacao(mensagemId: string, emoji: string): Promise<void> {
  return apiFetch<void>(`/chat/mensagens/${mensagemId}/reacoes`, {
    method: 'POST',
    body: JSON.stringify({ emoji }),
  })
}

export function marcarComoLido(conversaId: string): Promise<void> {
  return apiFetch<void>(`/chat/conversas/${conversaId}/leitura`, { method: 'POST' })
}

// --- Presença ---

export function listarPresencas(): Promise<ChatPresencaResponse[]> {
  return apiFetch<ChatPresencaResponse[]>('/chat/presencas')
}

export function heartbeat(): Promise<void> {
  return apiFetch<void>('/chat/presenca/heartbeat', { method: 'POST' })
}

// --- Admin ---

export function definirChatPerfil(usuarioId: string, chatPerfil: ChatPerfil): Promise<void> {
  return apiFetch<void>(`/usuarios/${usuarioId}/chat-perfil`, {
    method: 'PATCH',
    body: JSON.stringify({ chatPerfil }),
  })
}

// --- Histórico (Admin) ---

export function listarHistoricoChat(conversaId?: string): Promise<ChatHistoricoResponse[]> {
  const query = conversaId ? `?conversaId=${conversaId}` : ''
  return apiFetch<ChatHistoricoResponse[]>(`/chat/historico${query}`)
}
