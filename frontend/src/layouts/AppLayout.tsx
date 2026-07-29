import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
} from '@/components/ui/sidebar'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { useAuth } from '@/auth/AuthContext'
import { useTheme } from '@/hooks/useTheme'
import { Kanban, LayoutDashboard, Inbox, FileBarChart, Users, Archive, Sun, Moon, Tags, FolderKanban } from 'lucide-react'
import logoCamarj from '../assets/logo-camarj.png'

export function AppLayout() {
  const { perfil, logout } = useAuth()
  const { theme, toggleTheme } = useTheme()
  const location = useLocation()
  const navigate = useNavigate()
  const [confirmarLogout, setConfirmarLogout] = useState(false)

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
