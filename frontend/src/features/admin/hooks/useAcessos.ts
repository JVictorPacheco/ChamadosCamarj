import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { definirChatDeAcesso, listarAcessos, obterAcessoUsuario, salvarAcessos, voltarAoPadrao } from '../api'
import type { ChatPerfil, ModuloSistema, TipoPerfil } from '@/types/api'

// Controle de acesso (spec controle-de-acesso). Salvar mexe também no Chat da pessoa, que aparece na
// lista de Usuários — por isso invalida as duas.

export function useAcessos() {
  return useQuery({ queryKey: ['acessos'], queryFn: listarAcessos })
}

export function useAcessoUsuario(usuarioId: string | null) {
  return useQuery({
    queryKey: ['acessos', usuarioId],
    queryFn: () => obterAcessoUsuario(usuarioId!),
    enabled: !!usuarioId,
    // Sempre o cadastro atual ao abrir o painel: o formulário nasce destes dados (review-2 R-05).
    refetchOnMount: 'always',
  })
}

function useInvalidarAcessos() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['acessos'] })
    queryClient.invalidateQueries({ queryKey: ['usuarios'] })
  }
}

export function useSalvarAcessos(usuarioId: string) {
  const invalidar = useInvalidarAcessos()
  return useMutation({
    mutationFn: (dados: { modulos: ModuloSistema[]; chatPerfil: ChatPerfil; perfilEsperado: TipoPerfil }) => salvarAcessos(usuarioId, dados),
    onSuccess: invalidar,
  })
}

export function useVoltarAoPadrao(usuarioId: string) {
  const invalidar = useInvalidarAcessos()
  return useMutation({
    mutationFn: () => voltarAoPadrao(usuarioId),
    onSuccess: invalidar,
  })
}

export function useDefinirChatDeAcesso(usuarioId: string) {
  const invalidar = useInvalidarAcessos()
  return useMutation({
    mutationFn: (chatPerfil: ChatPerfil) => definirChatDeAcesso(usuarioId, chatPerfil),
    onSuccess: invalidar,
  })
}
