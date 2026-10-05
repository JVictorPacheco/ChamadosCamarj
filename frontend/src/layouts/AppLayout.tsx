import { useState, useEffect, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Link, Outlet, useLocation, useNavigate } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarProvider,
  SidebarInset,
} from '@/components/ui/sidebar'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { Separator } from '@/components/ui/separator'
import { useAuth } from '@/auth/AuthContext'
import { useTheme } from '@/hooks/useTheme'
import { useSignalR } from '@/hooks/useSignalR'
import { useConversas } from '@/features/chat/hooks/useConversas'
import { useChatHeartbeat } from '@/features/chat/hooks/useChatHeartbeat'
import { useInactivityLogout } from '@/hooks/useInactivityLogout'
import { MINUTOS_INATIVIDADE, limparLogoutPorInatividade, marcarLogoutPorInatividade } from '@/auth/logoutInatividade'
import { limparLogoutPorPerfilAlterado, marcarLogoutPorPerfilAlterado } from '@/auth/logoutPerfilAlterado'
import { PreferenciasDialog } from '@/features/chat/components/PreferenciasDialog'
import { Kanban, LayoutDashboard, Inbox, FileBarChart, Users, Archive, Sun, Moon, Settings, Tags, FolderKanban, MessageSquare, ShieldCheck } from 'lucide-react'
import { temModulo } from '@/lib/modulos'
import logoCamarj from '../assets/logo-camarj.png'

export function AppLayout() {
  const { perfil, logout, atualizarChatPerfil, atualizarAcessos } = useAuth()
  const { theme, toggleTheme } = useTheme()
  const location = useLocation()
  const navigate = useNavigate()
  const [confirmarLogout, setConfirmarLogout] = useState(false)
  const [preferenciasAbertas, setPreferenciasAbertas] = useState(false)
  const [slaAlerta, setSlaAlerta] = useState<string | null>(null)
  const [avisoChatPerfil, setAvisoChatPerfil] = useState<string | null>(null)
  const { subscribe } = useSignalR()
  const queryClient = useQueryClient()

  const temAcessoChat = perfil?.chatPerfil && perfil.chatPerfil !== 'SemAcesso'
  // spec controle-de-acesso: o menu segue os módulos da pessoa, não mais só o perfil.
  const temAlgumModuloDeAtendimento = (['Kanban', 'Dashboard', 'Fila', 'RelatorioMensal'] as const).some((m) => temModulo(perfil, m))
  // Aviso deixado pela proteção de rota quando a pessoa perde o módulo da tela em que está (AC-11).
  const avisoModulo = (location.state as { avisoModulo?: string } | null)?.avisoModulo
  useChatHeartbeat(Boolean(temAcessoChat))
  // review-fase9-independente.md #2: sem isso, todo usuário logado disparava GET /chat/conversas
  // mesmo sem nunca ter tido acesso ao chat — defesa em profundidade, complementando o filtro por
  // ChatPerfil no fan-out de ChatConversaAtualizada (ChatNovaMensagemNotificationHandler).
  const { data: conversas } = useConversas(Boolean(temAcessoChat))
  const totalNaoLidas = temAcessoChat
    ? (conversas ?? []).reduce((acc, c) => acc + (c.naoLidas ?? 0), 0)
    : 0

  // review-fase9-independente.md #9: ChatPage já mostra o próprio alerta de acesso revogado (e
  // navega pra fora do chat) — sem isso, quem está em /chat via dois avisos empilhados (um por
  // hub) quando o acesso é revogado. Ref porque o efeito de subscribe abaixo tem deps estáveis
  // (não reconecta a cada navegação), então precisa de um valor sempre atualizado por fora dele.
  const pathnameRef = useRef(location.pathname)
  useEffect(() => {
    pathnameRef.current = location.pathname
  }, [location.pathname])

  // review-2 R-03: mesmo motivo do pathnameRef — o efeito de subscribe não reconecta a cada render,
  // então lê o perfil da sessão e o "sair" por refs.
  const perfilTipoRef = useRef(perfil?.tipo)
  const sairRef = useRef<() => void>(() => {})
  useEffect(() => {
    perfilTipoRef.current = perfil?.tipo
  }, [perfil?.tipo])

  useEffect(() => {
    const unsub = subscribe((event) => {
      if (event.type === 'SlaAtencao' || event.type === 'SlaAtrasado') {
        setSlaAlerta(event.payload.mensagem)
        setTimeout(() => setSlaAlerta(null), 8000)
      }

      if (event.type === 'ChatConversaAtualizada') {
        queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
      }

      if (event.type === 'AcessosAtualizados') {
        // spec controle-de-acesso AC-15 (decisão de 2026-10-05): o Admin mudou o perfil desta pessoa.
        // O token ainda tem o perfil antigo, então ela sai e entra de novo em vez de seguir com
        // telas e permissões do perfil anterior.
        if (event.payload.perfil && perfilTipoRef.current && event.payload.perfil !== perfilTipoRef.current) {
          marcarLogoutPorPerfilAlterado()
          sairRef.current()
          return
        }
        atualizarAcessos(event.payload.modulos, event.payload.chatPerfil)
      }

      if (event.type === 'ChatPerfilAtualizado') {
        const novo = event.payload.chatPerfil
        atualizarChatPerfil(novo)
        const jaMostraNaTelaDeChat = novo === 'SemAcesso' && pathnameRef.current === '/chat'
        if (!jaMostraNaTelaDeChat) {
          setAvisoChatPerfil(
            novo === 'SemAcesso'
              ? 'Seu acesso ao chat foi revogado.'
              : 'Seu acesso ao chat foi restaurado.'
          )
          setTimeout(() => setAvisoChatPerfil(null), 6000)
        }
      }
    })
    return unsub
  }, [subscribe, atualizarChatPerfil, atualizarAcessos, queryClient])

  const sair = () => {
    logout()
    navigate('/login')
  }
  useEffect(() => {
    sairRef.current = sair
  })

  // Sessão ativa nesta aba: um aviso de inatividade antigo não pode aparecer num "Sair" futuro
  // (review R2-03).
  useEffect(limparLogoutPorInatividade, [])
  useEffect(limparLogoutPorPerfilAlterado, [])

  // Decisão de 2026-07-18: 20 min sem interação desconecta (spec logout-inatividade).
  useInactivityLogout(MINUTOS_INATIVIDADE, () => {
    marcarLogoutPorInatividade()
    sair()
  })

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <div className="flex flex-col items-center gap-3 px-2 pt-2">
            <img
              src={logoCamarj}
              alt="CAMARJ"
              className="h-16 w-16 rounded-xl shadow-md"
            />
            <span className="text-xs font-medium text-sidebar-foreground/60">Portal de Chamados</span>
          </div>
          <Button asChild className="mt-3 w-full">
            <Link to="/chamados/novo">Abrir Chamado</Link>
          </Button>
        </SidebarHeader>
        <SidebarContent>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild isActive={location.pathname === '/chamados'}>
                <Link to="/chamados">
                  <Inbox className="h-4 w-4" />
                  Meus Chamados
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
            {temModulo(perfil, 'Arquivo') && (
              <SidebarMenuItem>
                <SidebarMenuButton asChild isActive={location.pathname === '/chamados/arquivo'}>
                  <Link to="/chamados/arquivo">
                    <Archive className="h-4 w-4" />
                    Arquivo
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            )}
          </SidebarMenu>

          {temAcessoChat && (
            <>
              <Separator className="my-2" />
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/chat'}>
                    <Link to="/chat" className="flex items-center gap-2">
                      <MessageSquare className="h-4 w-4" />
                      Chat
                      {totalNaoLidas > 0 && (
                        <Badge
                          variant="destructive"
                          className="ml-auto min-w-[1.25rem] justify-center px-1"
                        >
                          {totalNaoLidas > 99 ? '99+' : totalNaoLidas}
                        </Badge>
                      )}
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              </SidebarMenu>
            </>
          )}

          {temAlgumModuloDeAtendimento && (
            <>
              <Separator className="my-2" />
              <div className="px-3 py-1 text-xs font-medium text-muted-foreground">Atendimento</div>
              <SidebarMenu>
                {temModulo(perfil, 'Kanban') && (
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/kanban'}>
                    <Link to="/atendimento/kanban">
                      <Kanban className="h-4 w-4" />
                      Kanban
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                )}
                {temModulo(perfil, 'Dashboard') && (
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/dashboard'}>
                    <Link to="/atendimento/dashboard">
                      <LayoutDashboard className="h-4 w-4" />
                      Dashboard
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                )}
                {temModulo(perfil, 'Fila') && (
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/fila'}>
                    <Link to="/atendimento/fila">
                      <Inbox className="h-4 w-4" />
                      Fila
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                )}
                {temModulo(perfil, 'RelatorioMensal') && (
                <SidebarMenuItem>
                  <SidebarMenuButton
                    asChild
                    isActive={location.pathname === '/atendimento/relatorio-mensal'}
                  >
                    <Link to="/atendimento/relatorio-mensal">
                      <FileBarChart className="h-4 w-4" />
                      Relatório Mensal
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                )}
              </SidebarMenu>
            </>
          )}

          {perfil && perfil.tipo === 'Admin' && (
            <>
              <Separator className="my-2" />
              <div className="px-3 py-1 text-xs font-medium text-muted-foreground">Administração</div>
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/usuarios'}>
                    <Link to="/admin/usuarios">
                      <Users className="h-4 w-4" />
                      Usuários
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/tipos'}>
                    <Link to="/admin/tipos">
                      <Tags className="h-4 w-4" />
                      Tipos de chamado
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/grupos'}>
                    <Link to="/admin/grupos">
                      <FolderKanban className="h-4 w-4" />
                      Áreas e Grupos
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/acessos'}>
                    <Link to="/admin/acessos">
                      <ShieldCheck className="h-4 w-4" />
                      Controle de acesso
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              </SidebarMenu>
            </>
          )}
        </SidebarContent>
        <SidebarFooter>
          <div className="flex flex-col gap-2 px-2 py-1 text-sm">
            <div className="flex items-center justify-between gap-2">
              <span className="font-medium text-sidebar-foreground">{perfil?.nome}</span>
              <div className="flex items-center gap-1">
                <button
                  type="button"
                  onClick={() => setPreferenciasAbertas(true)}
                  className="rounded-md p-1 text-muted-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
                  aria-label="Preferências"
                >
                  <Settings className="h-4 w-4" />
                </button>
                <button
                  type="button"
                  onClick={toggleTheme}
                  className="rounded-md p-1 text-muted-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
                  aria-label={theme === 'dark' ? 'Alternar para tema claro' : 'Alternar para tema escuro'}
                >
                  {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
                </button>
              </div>
            </div>
            <Button variant="outline" size="sm" onClick={() => setConfirmarLogout(true)}>
              Sair
            </Button>
          </div>
        </SidebarFooter>
      </Sidebar>
      <SidebarInset>
        {slaAlerta && (
          <Alert variant="destructive" className="m-2">
            <AlertDescription className="flex items-center justify-between">
              {slaAlerta}
              <button onClick={() => setSlaAlerta(null)} className="text-lg leading-none">&times;</button>
            </AlertDescription>
          </Alert>
        )}
        {avisoModulo && (
          <Alert variant="destructive" className="m-2">
            <AlertDescription>{avisoModulo}</AlertDescription>
          </Alert>
        )}
        {avisoChatPerfil && (
          <Alert variant={avisoChatPerfil.includes('revogado') ? 'destructive' : 'default'} className="m-2">
            <AlertDescription className="flex items-center justify-between">
              {avisoChatPerfil}
              <button onClick={() => setAvisoChatPerfil(null)} className="text-lg leading-none">&times;</button>
            </AlertDescription>
          </Alert>
        )}
        <Outlet />
      </SidebarInset>
      <Dialog open={confirmarLogout} onOpenChange={setConfirmarLogout}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Sair</DialogTitle>
            <DialogDescription>Tem certeza que deseja sair do sistema?</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmarLogout(false)}>
              Cancelar
            </Button>
            <Button variant="outline" onClick={() => { setConfirmarLogout(false); sair() }}>
              Sair
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <PreferenciasDialog open={preferenciasAbertas} onOpenChange={setPreferenciasAbertas} />
    </SidebarProvider>
  )
}
