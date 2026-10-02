import { useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { useAuth } from '@/auth/AuthContext'
import { useTiposAdmin, useExcluirTipo } from './hooks/useTipos'
import { TipoFormDialog } from './components/TipoFormDialog'
import { ApiError } from '@/lib/api'
import type { TipoChamadoResponse } from '@/types/api'

export function TiposPage() {
  const { perfil } = useAuth()
  const isAdmin = perfil?.tipo === 'Admin'
  const { data: tipos, isPending, isError } = useTiposAdmin()
  const [dialogAberto, setDialogAberto] = useState(false)
  const [tipoSelecionada, setTipoSelecionada] = useState<TipoChamadoResponse | null>(null)
  const [excluindoTipo, setExcluindoTipo] = useState<TipoChamadoResponse | null>(null)
  const excluirMutation = useExcluirTipo()

  const abrirNovo = () => {
    setTipoSelecionada(null)
    setDialogAberto(true)
  }

  const abrirEdicao = (tipo: TipoChamadoResponse) => {
    setTipoSelecionada(tipo)
    setDialogAberto(true)
  }

  const confirmarExclusao = () => {
    if (!excluindoTipo) return
    excluirMutation.mutate(excluindoTipo.id, {
      onSettled: () => setExcluindoTipo(null),
    })
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
        <h1 className="text-xl font-heading">Tipos de chamado</h1>
        <Button onClick={abrirNovo}>Novo tipo</Button>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando tipos...</p>}

      {!isPending && tipos && tipos.length === 0 && (
        <p className="py-8 text-center text-sm text-muted-foreground">Nenhum tipo cadastrado.</p>
      )}

      {!isPending && tipos && tipos.length > 0 && (
        <div className="rounded-lg border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Descrição</TableHead>
                <TableHead>Ativo</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {tipos.map((tipo) => (
                <TableRow key={tipo.id}>
                  <TableCell>{tipo.nome}</TableCell>
                  <TableCell>{tipo.descricao}</TableCell>
                  <TableCell>
                    <Badge variant={tipo.ativo ? 'default' : 'secondary'}>
                      {tipo.ativo ? 'Ativo' : 'Inativo'}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button variant="outline" size="sm" onClick={() => abrirEdicao(tipo)}>
                      Editar
                    </Button>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => setExcluindoTipo(tipo)}
                    >
                      Excluir
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <TipoFormDialog open={dialogAberto} onOpenChange={setDialogAberto} tipo={tipoSelecionada} />

      <Dialog open={!!excluindoTipo} onOpenChange={(open) => { if (!open) setExcluindoTipo(null) }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Excluir tipo</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir o tipo "{excluindoTipo?.nome}"? Esta ação não pode ser desfeita. Tipos com chamados vinculados não poderão ser excluídos.
            </DialogDescription>
          </DialogHeader>
          {excluirMutation.isError && (
            <Alert variant="destructive">
              <AlertDescription>
                {excluirMutation.error instanceof ApiError ? excluirMutation.error.message : 'Erro ao excluir tipo.'}
              </AlertDescription>
            </Alert>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setExcluindoTipo(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              disabled={excluirMutation.isPending}
              onClick={confirmarExclusao}
            >
              {excluirMutation.isPending ? 'Excluindo...' : 'Excluir'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
