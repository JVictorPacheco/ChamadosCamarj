import { useQuery } from '@tanstack/react-query'
import { useAuth } from './AuthContext'
import { listarUsuarios } from './api'

// Substitui o antigo array estático ATENDENTES: busca usuários reais via API e
// filtra localmente por perfil Atendente/Admin.
export function useAtendentes() {
  const { perfil } = useAuth()

  return useQuery({
    queryKey: ['usuarios', 'atendentes'],
    queryFn: () => listarUsuarios(),
    select: (usuarios) => usuarios.filter((usuario) => usuario.perfil === 'Atendente' || usuario.perfil === 'Admin'),
    enabled: !!perfil,
  })
}
