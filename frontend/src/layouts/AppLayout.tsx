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
import { useAuth } from '@/auth/AuthContext'
import { Kanban, LayoutDashboard, Inbox } from 'lucide-react'

export function AppLayout() {
  const { perfil, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  const sair = () => {
    logout()
    navigate('/login')
  }

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <Button asChild className="w-full">
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
              </SidebarMenu>
            </>
          )}
        </SidebarContent>
        <SidebarFooter>
          <div className="flex flex-col gap-2 px-2 py-1 text-sm">
            <span className="font-medium text-sidebar-foreground">{perfil?.nome}</span>
            <Button variant="outline" size="sm" onClick={sair}>
              Sair
            </Button>
          </div>
        </SidebarFooter>
      </Sidebar>
      <SidebarInset>
        <Outlet />
      </SidebarInset>
    </SidebarProvider>
  )
}
