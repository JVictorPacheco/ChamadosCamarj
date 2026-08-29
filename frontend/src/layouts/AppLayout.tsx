import { useState, useEffect } from 'react'
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
import { Kanban, LayoutDashboard, Inbox, FileBarChart, Users, Archive, Sun, Moon, Tags, FolderKanban, MessageSquare } from 'lucide-react'
import logoCamarj from '../assets/logo-camarj.png'

export function AppLayout() {
  const { perfil, logout } = useAuth()
  const { theme, toggleTheme } = useTheme()
  const location = useLocation()
  const navigate = useNavigate()
  const [confirmarLogout, setConfirmarLogout] = useState(false)
  const [slaAlerta, setSlaAlerta] = useState<string | null>(null)
  const { subscribe } = useSignalR()

  const temAcessoChat = perfil?.chatPerfil && perfil.chatPerfil !== 'SemAcesso'
  const { data: conversas } = useConversas()
  const totalNaoLidas = temAcessoChat
    ? (conversas ?? []).reduce((acc, c) => acc + (c.naoLidas ?? 0), 0)
    : 0

  useEffect(() => {
    const unsub = subscribe((event) => {
      if (event.type === 'SlaAtencao' || event.type === 'SlaAtrasado') {
        setSlaAlerta(event.payload.mensagem)
        setTimeout(() => setSlaAlerta(null), 8000)
      }
    })
    return unsub
  }, [subscribe])

  const sair = () => {
    logout()
    navigate('/login')
  }

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
            <SidebarMenuItem>
              <SidebarMenuButton asChild isActive={location.pathname === '/chamados/arquivo'}>
                <Link to="/chamados/arquivo">
                  <Archive className="h-4 w-4" />
                  Arquivo
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
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

          {perfil && perfil.tipo !== 'Solicitante' && (
            <>
              <Separator className="my-2" />
              <div className="px-3 py-1 text-xs font-medium text-muted-foreground">Atendimento</div>
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/kanban'}>
                    <Link to="/atendimento/kanban">
                      <Kanban className="h-4 w-4" />
                      Kanban
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/dashboard'}>
                    <Link to="/atendimento/dashboard">
                      <LayoutDashboard className="h-4 w-4" />
                      Dashboard
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/atendimento/fila'}>
                    <Link to="/atendimento/fila">
                      <Inbox className="h-4 w-4" />
                      Fila
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
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
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/categorias'}>
                    <Link to="/admin/categorias">
                      <Tags className="h-4 w-4" />
                      Categorias
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
                <SidebarMenuItem>
                  <SidebarMenuButton asChild isActive={location.pathname === '/admin/grupos'}>
                    <Link to="/admin/grupos">
                      <FolderKanban className="h-4 w-4" />
                      Grupos
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
              <button
                type="button"
                onClick={toggleTheme}
                className="rounded-md p-1 text-muted-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
                aria-label={theme === 'dark' ? 'Alternar para tema claro' : 'Alternar para tema escuro'}
              >
                {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
              </button>
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
    </SidebarProvider>
  )
}
