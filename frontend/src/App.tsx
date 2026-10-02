import { useEffect } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Outlet, Route, Routes } from 'react-router'
import { temModulo } from '@/lib/modulos'
import type { ModuloSistema } from '@/types/api'
import { TooltipProvider } from '@/components/ui/tooltip'
import { ApiError } from '@/lib/api'
import { ThemeProvider } from '@/hooks/useTheme'
import { AuthProvider, useAuth } from './auth/AuthContext'
import { LoginPage } from './auth/LoginPage'
import { ResetarSenhaPage } from './auth/ResetarSenhaPage'
import { AppLayout } from './layouts/AppLayout'
import { SignalRProvider } from './hooks/useSignalR'
import { sessaoVencidaPorInatividade } from './hooks/useInactivityLogout'
import { MINUTOS_INATIVIDADE, marcarLogoutPorInatividade } from './auth/logoutInatividade'
import { AbrirChamadoPage } from './features/chamados/AbrirChamadoPage'
import { ChamadosListPage } from './features/chamados/ChamadosListPage'
import { ArquivoChamadosPage } from './features/chamados/ArquivoChamadosPage'
import { ChamadoDetailPage } from './features/chamados/ChamadoDetailPage'
import { KanbanPage } from './features/chamados/KanbanPage'
import { FilaAtendimentoPage } from './features/chamados/FilaAtendimentoPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { RelatorioMensalPage } from './features/relatorio-mensal/RelatorioMensalPage'
import { UsuariosPage } from './features/admin/UsuariosPage'
import { TiposPage } from './features/admin/TiposPage'
import { GruposPage } from './features/admin/GruposPage'
import { ControleAcessoPage } from './features/admin/ControleAcessoPage'
import { ChatPage } from './features/chat/ChatPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        // Erros 4xx (ex: 404) nao se resolvem tentando de novo - so erros de rede/5xx valem retry
        if (error instanceof ApiError && error.status !== undefined && error.status < 500) {
          return false
        }
        return failureCount < 3
      },
      staleTime: 30_000,
    },
  },
})

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <BrowserRouter>
          <TooltipProvider>
            <AuthProvider>
              <AppRoutes />
            </AuthProvider>
          </TooltipProvider>
        </BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  )
}

function LoginRoute() {
  const { perfil } = useAuth()
  if (perfil) {
    return <Navigate to="/chamados" replace />
  }
  return <LoginPage />
}

// Sessão reaberta depois do limite de inatividade (navegador fechado, aba descartada): encerra sem
// montar a área logada — nenhuma tela, chamada à API ou heartbeat do chat roda (spec
// logout-inatividade, review R2-02).
function EncerrarSessaoPorInatividade() {
  const { logout } = useAuth()
  useEffect(() => {
    marcarLogoutPorInatividade()
    logout()
  }, [logout])
  return null
}

function ProtectedRoute() {
  const { perfil } = useAuth()
  if (!perfil) {
    return <Navigate to="/login" replace />
  }
  if (sessaoVencidaPorInatividade(MINUTOS_INATIVIDADE)) {
    return <EncerrarSessaoPorInatividade />
  }
  // SignalRProvider só monta com um usuário autenticado — antes disso não há token pra conexão em
  // tempo real. A conexão em si tem retry com backoff pra falha na tentativa inicial (ver
  // useSignalR.tsx), então login sem rede/API instável no momento não deixa a pessoa sem eventos
  // pelo resto da sessão.
  return (
    <SignalRProvider>
      <AppLayout />
    </SignalRProvider>
  )
}

const AVISO_MODULO_RETIRADO = 'Seu acesso a este módulo foi retirado.'

/**
 * Tela de módulo (spec controle-de-acesso AC-11/AC-12): sem o módulo, volta para "Meus chamados" com o
 * aviso. Como o perfil se atualiza em tempo real (AcessosAtualizados), quem está na tela e perde o
 * módulo sai na hora. O servidor confere de novo onde importa (Dashboard, Relatório).
 */
function RequerModulo({ modulo }: { modulo: ModuloSistema }) {
  const { perfil } = useAuth()
  if (!temModulo(perfil, modulo)) {
    return <Navigate to="/chamados" replace state={{ avisoModulo: AVISO_MODULO_RETIRADO }} />
  }
  return <Outlet />
}

/** Telas de Administração: só Admin, também pelo endereço (antes só o menu escondia). */
function RequerAdmin() {
  const { perfil } = useAuth()
  if (perfil?.tipo !== 'Admin') {
    return <Navigate to="/chamados" replace />
  }
  return <Outlet />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />
      <Route path="/resetar-senha" element={<ResetarSenhaPage />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/chamados" element={<ChamadosListPage />} />
        <Route path="/chamados/novo" element={<AbrirChamadoPage />} />
        <Route path="/chamados/:id" element={<ChamadoDetailPage />} />
        {/* spec controle-de-acesso: cada tela de módulo exige o módulo; perdeu com a tela aberta → sai (AC-11). */}
        <Route element={<RequerModulo modulo="Arquivo" />}>
          <Route path="/chamados/arquivo" element={<ArquivoChamadosPage />} />
        </Route>
        <Route element={<RequerModulo modulo="Kanban" />}>
          <Route path="/atendimento/kanban" element={<KanbanPage />} />
        </Route>
        <Route element={<RequerModulo modulo="Dashboard" />}>
          <Route path="/atendimento/dashboard" element={<DashboardPage />} />
        </Route>
        <Route element={<RequerModulo modulo="Fila" />}>
          <Route path="/atendimento/fila" element={<FilaAtendimentoPage />} />
        </Route>
        <Route element={<RequerModulo modulo="RelatorioMensal" />}>
          <Route path="/atendimento/relatorio-mensal" element={<RelatorioMensalPage />} />
        </Route>
        <Route element={<RequerAdmin />}>
          <Route path="/admin/usuarios" element={<UsuariosPage />} />
          <Route path="/admin/tipos" element={<TiposPage />} />
          <Route path="/admin/grupos" element={<GruposPage />} />
          <Route path="/admin/acessos" element={<ControleAcessoPage />} />
        </Route>
        <Route path="/chat" element={<ChatPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/chamados" replace />} />
    </Routes>
  )
}

export default App
