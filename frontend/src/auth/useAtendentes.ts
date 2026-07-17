import { useQuery } from '@tanstack/react-query'
import { useAuth } from './AuthContext'
import { listarUsuarios } from './api'

// Substitui o antigo array estático ATENDENTES: busca usuários reais via API e
// filtra localmente por perfil Atendente/Admin (quem chama já precisa ser Admin
// para acessar a tela que consome isso, então usamos o próprio perfil logado
// como perfilRequisitante).
export function useAtendentes() {
  const { perfil } = useAuth()
  const perfilRequisitante = perfil?.tipo ?? 'Admin'

  return useQuery({
    queryKey: ['usuarios', 'atendentes', perfilRequisitante],
    queryFn: () => listarUsuarios(perfilRequisitante),
    select: (usuarios) => usuarios.filter((usuario) => usuario.perfil === 'Atendente' || usuario.perfil === 'Admin'),
    enabled: !!perfil,
  })
}
