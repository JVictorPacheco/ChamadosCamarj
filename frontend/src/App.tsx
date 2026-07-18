import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { TooltipProvider } from '@/components/ui/tooltip'
import { ApiError } from '@/lib/api'
import { AuthProvider, useAuth } from './auth/AuthContext'
import { LoginPage } from './auth/LoginPage'
import { AppLayout } from './layouts/AppLayout'
import { SignalRProvider } from './hooks/useSignalR'
import { AbrirChamadoPage } from './features/chamados/AbrirChamadoPage'
import { ChamadosListPage } from './features/chamados/ChamadosListPage'
import { ArquivoChamadosPage } from './features/chamados/ArquivoChamadosPage'
import { ChamadoDetailPage } from './features/chamados/ChamadoDetailPage'
import { KanbanPage } from './features/chamados/KanbanPage'
import { FilaAtendimentoPage } from './features/chamados/FilaAtendimentoPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { RelatorioMensalPage } from './features/relatorio-mensal/RelatorioMensalPage'
import { UsuariosPage } from './features/admin/UsuariosPage'

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
    },
  },
})

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <TooltipProvider>
          <AuthProvider>
            <SignalRProvider>
              <AppRoutes />
            </SignalRProvider>
          </AuthProvider>
        </TooltipProvider>
      </BrowserRouter>
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

function ProtectedRoute() {
  const { perfil } = useAuth()
  if (!perfil) {
    return <Navigate to="/login" replace />
  }
  return <AppLayout />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/chamados" element={<ChamadosListPage />} />
        <Route path="/chamados/novo" element={<AbrirChamadoPage />} />
        <Route path="/chamados/arquivo" element={<ArquivoChamadosPage />} />
        <Route path="/chamados/:id" element={<ChamadoDetailPage />} />
        <Route path="/atendimento/kanban" element={<KanbanPage />} />
        <Route path="/atendimento/dashboard" element={<DashboardPage />} />
        <Route path="/atendimento/fila" element={<FilaAtendimentoPage />} />
        <Route path="/atendimento/relatorio-mensal" element={<RelatorioMensalPage />} />
        <Route path="/admin/usuarios" element={<UsuariosPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/chamados" replace />} />
    </Routes>
  )
}

export default App
