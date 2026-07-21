import { useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAuth } from '@/auth/AuthContext'
import { useAtualizarUsuario, useUsuarios } from './hooks/useUsuarios'
import { UsuarioFormDialog } from './components/UsuarioFormDialog'
import type { UsuarioPerfilResponse } from '@/types/api'

export function UsuariosPage() {
  const { perfil } = useAuth()
  const isAdmin = perfil?.tipo === 'Admin'
  const { data: usuarios, isPending, isError } = useUsuarios()
  const atualizarUsuario = useAtualizarUsuario()
  const [dialogAberto, setDialogAberto] = useState(false)
  const [usuarioSelecionado, setUsuarioSelecionado] = useState<UsuarioPerfilResponse | null>(null)
  const [alternandoId, setAlternandoId] = useState<string | null>(null)
  const [erroAlternar, setErroAlternar] = useState<string | null>(null)

  const abrirNovo = () => {
    setUsuarioSelecionado(null)
    setDialogAberto(true)
  }

  const abrirEdicao = (usuario: UsuarioPerfilResponse) => {
    setUsuarioSelecionado(usuario)
    setDialogAberto(true)
  }

  const alternarAtivo = (usuario: UsuarioPerfilResponse) => {
    setAlternandoId(usuario.id)
    setErroAlternar(null)
    atualizarUsuario.mutate(
      { id: usuario.id, dados: { nome: usuario.nome, perfil: usuario.perfil, ativo: !usuario.ativo } },
      {
        onError: () =>
          setErroAlternar(`Não foi possível ${usuario.ativo ? 'desativar' : 'reativar'} ${usuario.nome}. Tente novamente.`),
        onSettled: () => setAlternandoId(null),
      },
    )
  }

  if (!isAdmin) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Esta área não está disponível para o seu perfil.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-xl font-heading">Usuários</h1>
        <Button onClick={abrirNovo}>Novo usuário</Button>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {erroAlternar && (
        <Alert variant="destructive">
          <AlertDescription>{erroAlternar}</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando usuários...</p>}

      {!isPending && usuarios && usuarios.length === 0 && (
        <p className="py-8 text-center text-sm text-muted-foreground">Nenhum usuário cadastrado.</p>
      )}

      {!isPending && usuarios && usuarios.length > 0 && (
        <div className="rounded-lg border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>E-mail</TableHead>
                <TableHead>Perfil</TableHead>
                <TableHead>Ativo</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {usuarios.map((usuario) => (
                <TableRow key={usuario.id}>
                  <TableCell>{usuario.nome}</TableCell>
                  <TableCell>{usuario.email}</TableCell>
                  <TableCell>{usuario.perfil}</TableCell>
                  <TableCell>
                    <Badge variant={usuario.ativo ? 'default' : 'secondary'}>
                      {usuario.ativo ? 'Ativo' : 'Inativo'}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button variant="outline" size="sm" onClick={() => abrirEdicao(usuario)}>
                      Editar
                    </Button>
                    <Button
                      variant={usuario.ativo ? 'destructive' : 'outline'}
                      size="sm"
                      disabled={alternandoId === usuario.id}
                      onClick={() => alternarAtivo(usuario)}
                    >
                      {alternandoId === usuario.id ? '...' : usuario.ativo ? 'Desativar' : 'Reativar'}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <UsuarioFormDialog open={dialogAberto} onOpenChange={setDialogAberto} usuario={usuarioSelecionado} />
    </div>
  )
}
