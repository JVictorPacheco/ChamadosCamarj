import { useState } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { useAuth } from '@/auth/AuthContext'
import { ApiError } from '@/lib/api'
import { formatarNumeroChamado } from '@/lib/utils'
import { StatusBadge } from './components/StatusBadge'
import { PrioridadeBadge } from './components/PrioridadeBadge'
import { SlaBadge } from './components/SlaBadge'
import { ComentarioList } from './components/ComentarioList'
import { ComentarioForm } from './components/ComentarioForm'
import { ReatribuirModal } from './components/ReatribuirModal'
import { AlterarPrioridadeModal } from './components/AlterarPrioridadeModal'
import { ForcarEncerramentoModal } from './components/ForcarEncerramentoModal'
import { TimelineHistorico } from './components/TimelineHistorico'
import { AnexosList } from './components/AnexosList'
import { useChamado } from './hooks/useChamado'
import {
  useAtribuirChamado,
  useResolverChamado,
  useFecharChamado,
  useCancelarChamado,
  useReabrirChamado,
} from './hooks/useAcoesChamado'
import type { ChamadoResponse, MotivoEncerramento } from '@/types/api'

const MOTIVO_LABELS: Record<MotivoEncerramento, string> = {
  Resolvido: 'Resolvido',
  CanceladoSolicitante: 'Cancelado pelo solicitante',
  AbertoIndevidamente: 'Aberto indevidamente',
  Duplicata: 'Duplicata',
  SemResposta: 'Sem resposta do solicitante',
  Outro: 'Outro',
}

function BotoesAcao({ chamado }: { chamado: ChamadoResponse }) {
  const { perfil } = useAuth()
  const atribuir = useAtribuirChamado(chamado.id)
  const resolver = useResolverChamado(chamado.id)
  const fechar = useFecharChamado(chamado.id)
  const cancelar = useCancelarChamado(chamado.id)
  const reabrir = useReabrirChamado(chamado.id)
  const [reatribuirAberto, setReatribuirAberto] = useState(false)
  const [prioridadeAberto, setPrioridadeAberto] = useState(false)
  const [forcarEncerramentoAberto, setForcarEncerramentoAberto] = useState(false)
  const [confirmarAcao, setConfirmarAcao] = useState<'resolver' | 'encerrar' | 'cancelar' | 'reabrir' | null>(null)
  const [motivoSelecionado, setMotivoSelecionado] = useState<MotivoEncerramento>('Resolvido')
  const [motivoOutroTexto, setMotivoOutroTexto] = useState('')
  const [observacaoTexto, setObservacaoTexto] = useState('')

  const isAdmin = perfil?.tipo === 'Admin'
  const isAtendente = perfil?.tipo === 'Admin' || perfil?.tipo === 'Atendente'
  const isSolicitante = perfil?.tipo === 'Solicitante'
  const status = chamado.status
  const statusFinal = status === 'Fechado' || status === 'Cancelado'

  const isPending =
    atribuir.isPending || resolver.isPending || fechar.isPending || cancelar.isPending || reabrir.isPending

  const precisaMotivo = confirmarAcao === 'encerrar' || confirmarAcao === 'cancelar'

  const executarAcao = () => {
    switch (confirmarAcao) {
      case 'resolver': resolver.mutate(); break
      case 'encerrar': fechar.mutate({ motivo: motivoSelecionado, motivoOutro: motivoOutroTexto || undefined, observacao: observacaoTexto.trim() || undefined }); break
      case 'cancelar': cancelar.mutate({ motivo: motivoSelecionado, motivoOutro: motivoOutroTexto || undefined, observacao: observacaoTexto.trim() || undefined }); break
      case 'reabrir': reabrir.mutate(); break
    }
    setConfirmarAcao(null)
    setMotivoSelecionado('Resolvido')
    setMotivoOutroTexto('')
    setObservacaoTexto('')
  }

  const abrirConfirmacao = (acao: 'resolver' | 'encerrar' | 'cancelar' | 'reabrir') => {
    setConfirmarAcao(acao)
    if (acao === 'encerrar') setMotivoSelecionado('Resolvido')
    if (acao === 'cancelar') setMotivoSelecionado('CanceladoSolicitante')
  }

  const tituloConfirmacao =
    confirmarAcao === 'resolver' ? 'Resolver chamado' :
    confirmarAcao === 'encerrar' ? 'Encerrar chamado' :
    confirmarAcao === 'cancelar' ? 'Cancelar chamado' :
    confirmarAcao === 'reabrir' ? 'Reabrir chamado' : ''

  const descricaoConfirmacao =
    confirmarAcao === 'resolver' ? 'Confirma que este chamado foi solucionado?' :
    confirmarAcao === 'encerrar' ? 'Tem certeza que deseja encerrar este chamado? Esta ação não pode ser desfeita.' :
    confirmarAcao === 'cancelar' ? 'Tem certeza que deseja cancelar este chamado? Esta ação não pode ser desfeita.' :
    confirmarAcao === 'reabrir' ? 'O chamado voltará para o status Em Andamento e o responsável será removido.' : ''

  return (
    <div className="flex flex-wrap gap-3">
      {isAtendente && status === 'Aberto' && (
        <Button disabled={isPending} onClick={() => atribuir.mutate()}>
          {atribuir.isPending ? 'Assumindo...' : 'Assumir'}
        </Button>
      )}

      {isAtendente && status === 'EmAndamento' && (
        <Button
          disabled={isPending}
          onClick={() => abrirConfirmacao('resolver')}
        >
          {resolver.isPending ? 'Resolvendo...' : 'Resolver'}
        </Button>
      )}

      {isAtendente && status === 'Resolvido' && (
        <Button
          disabled={isPending}
          onClick={() => abrirConfirmacao('encerrar')}
        >
          {fechar.isPending ? 'Encerrando...' : 'Encerrar'}
        </Button>
      )}

      {(isAtendente || isSolicitante) && (status === 'Aberto' || status === 'EmAndamento') && (
        <Button
          variant="destructive"
          disabled={isPending}
          onClick={() => abrirConfirmacao('cancelar')}
        >
          {cancelar.isPending ? 'Cancelando...' : 'Cancelar'}
        </Button>
      )}

      {isAtendente && (status === 'Resolvido' || status === 'Fechado' || status === 'Cancelado') && (
        <Button variant="outline" disabled={isPending} onClick={() => abrirConfirmacao('reabrir')}>
          {reabrir.isPending ? 'Reabrindo...' : 'Reabrir'}
        </Button>
      )}

      {isAdmin && !statusFinal && (
        <Button variant="outline" onClick={() => setReatribuirAberto(true)}>
          Reatribuir
        </Button>
      )}

      {isAdmin && !statusFinal && (
        <Button variant="outline" onClick={() => setPrioridadeAberto(true)}>
          Alterar prioridade
        </Button>
      )}

      {isAdmin && !statusFinal && (
        <Button variant="destructive" onClick={() => setForcarEncerramentoAberto(true)}>
          Forçar Encerramento
        </Button>
      )}

      {(atribuir.isError || resolver.isError || fechar.isError || cancelar.isError || reabrir.isError) && (
        <Alert variant="destructive" className="w-full">
          <AlertDescription>
            {(atribuir.error || resolver.error || fechar.error || cancelar.error || reabrir.error)?.message ??
              'Erro ao executar a ação. Tente novamente.'}
          </AlertDescription>
        </Alert>
      )}

      <Dialog open={confirmarAcao !== null} onOpenChange={(open) => { if (!open) setConfirmarAcao(null) }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{tituloConfirmacao}</DialogTitle>
            <DialogDescription>{descricaoConfirmacao}</DialogDescription>
          </DialogHeader>
          {precisaMotivo && (
            <div className="flex flex-col gap-3">
              <div className="flex flex-col gap-1">
                <Label htmlFor="motivo-encerramento" className="text-sm">
                  Motivo de {confirmarAcao === 'encerrar' ? 'encerramento' : 'cancelamento'}
                </Label>
                <Select value={motivoSelecionado} onValueChange={(v) => setMotivoSelecionado(v as MotivoEncerramento)}>
                  <SelectTrigger id="motivo-encerramento">
                    <SelectValue placeholder="Selecione o motivo" />
                  </SelectTrigger>
                  <SelectContent>
                    {(Object.keys(MOTIVO_LABELS) as MotivoEncerramento[]).map((motivo) => (
                      <SelectItem key={motivo} value={motivo}>
                        {MOTIVO_LABELS[motivo]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              {motivoSelecionado === 'Outro' && (
                <div className="flex flex-col gap-1">
                  <Label htmlFor="motivo-outro" className="text-sm">
                    Descreva o motivo
                  </Label>
                  <Input
                    id="motivo-outro"
                    value={motivoOutroTexto}
                    onChange={(e) => setMotivoOutroTexto(e.target.value)}
                    placeholder="Ex: Chamado aberto por engano"
                  />
                </div>
              )}
              <div className="flex flex-col gap-1">
                <Label htmlFor="observacao-comentario" className="text-sm">
                  Comentário (opcional)
                </Label>
                <Textarea
                  id="observacao-comentario"
                  value={observacaoTexto}
                  onChange={(e) => setObservacaoTexto(e.target.value)}
                  placeholder="Escreva uma observação sobre o encerramento..."
                  rows={4}
                />
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmarAcao(null)}>
              Voltar
            </Button>
            <Button
              variant={confirmarAcao === 'cancelar' ? 'destructive' : 'default'}
              onClick={executarAcao}
              disabled={isPending || (motivoSelecionado === 'Outro' && !motivoOutroTexto.trim())}
            >
              {isPending ? 'Processando...' : 'Confirmar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ReatribuirModal
        open={reatribuirAberto}
        onOpenChange={setReatribuirAberto}
        chamadoId={chamado.id}
        responsavelAtualId={chamado.responsavelId}
      />
      <AlterarPrioridadeModal
        open={prioridadeAberto}
        onOpenChange={setPrioridadeAberto}
        chamadoId={chamado.id}
        prioridadeAtual={chamado.prioridade}
      />
      <ForcarEncerramentoModal
        open={forcarEncerramentoAberto}
        onOpenChange={setForcarEncerramentoAberto}
        chamadoId={chamado.id}
      />
    </div>
  )
}

export function ChamadoDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { perfil } = useAuth()
  const { data: chamado, isPending, error } = useChamado(id!)
  const location = useLocation()
  const [avisoAnexos, setAvisoAnexos] = useState<string | null>(
    (location.state as { avisoAnexos?: string } | null)?.avisoAnexos ?? null,
  )

  if (isPending) {
    return <p className="p-4 text-sm text-muted-foreground">Carregando...</p>
  }

  if (error) {
    if (error instanceof ApiError && error.status === 404) {
      return (
        <div className="flex flex-col items-center gap-3 p-8 text-center">
          <p className="text-sm text-muted-foreground">Chamado não encontrado.</p>
          <Button asChild variant="outline">
            <Link to="/chamados">Voltar para a lista</Link>
          </Button>
        </div>
      )
    }
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Erro ao carregar o chamado. Tente novamente.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  if (!chamado) return null

  const isAtendente = perfil?.tipo === 'Admin' || perfil?.tipo === 'Atendente'

  if (!isAtendente && chamado.solicitanteEmail !== perfil?.email) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Este chamado não pertence ao seu perfil ativo.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-8 p-8">
      <Button asChild variant="ghost" size="default" className="self-start">
        <Link to="/chamados">← Voltar</Link>
      </Button>

      <h1 className="text-3xl font-heading">
        <span className="mr-2 text-muted-foreground">{formatarNumeroChamado(chamado.numero)}</span>
        {chamado.titulo}
      </h1>

      {avisoAnexos && (
        <Alert variant="destructive" className="flex items-start justify-between gap-2">
          <AlertDescription>{avisoAnexos}</AlertDescription>
          <button
            type="button"
            onClick={() => setAvisoAnexos(null)}
            className="text-muted-foreground hover:text-foreground"
            aria-label="Dispensar aviso"
          >
            ✕
          </button>
        </Alert>
      )}

      <div className="flex flex-wrap items-center gap-4 text-lg">
        <StatusBadge status={chamado.status} />
        <PrioridadeBadge prioridade={chamado.prioridade} />
        <SlaBadge dataLimite={chamado.dataLimite} status={chamado.status} slaStatus={chamado.slaStatus} slaLabel={chamado.slaLabel} />
      </div>

      <BotoesAcao chamado={chamado} />

      <p className="text-lg leading-relaxed">{chamado.descricao}</p>

      <dl className="grid grid-cols-2 gap-6 text-lg text-muted-foreground">
        <div>
          <dt className="font-medium text-foreground">Categoria</dt>
          <dd>{chamado.categoriaNome ?? 'Sem categoria'}</dd>
        </div>
        <div>
          <dt className="font-medium text-foreground">Aberto em</dt>
          <dd>{new Date(chamado.dataCriacao).toLocaleString('pt-BR')}</dd>
        </div>
        {chamado.responsavelNome && (
          <div>
            <dt className="font-medium text-foreground">Responsável</dt>
            <dd>{chamado.responsavelNome}</dd>
          </div>
        )}
        {chamado.dataLimite && (
          <div>
            <dt className="font-medium text-foreground">Prazo (SLA)</dt>
            <dd>{new Date(chamado.dataLimite).toLocaleString('pt-BR')}</dd>
          </div>
        )}
        {chamado.dataConclusao && (
          <div>
            <dt className="font-medium text-foreground">Concluído em</dt>
            <dd>{new Date(chamado.dataConclusao).toLocaleString('pt-BR')}</dd>
          </div>
        )}
        {chamado.motivoEncerramento && (
          <div>
            <dt className="font-medium text-foreground">Motivo de encerramento</dt>
            <dd>
              {chamado.motivoEncerramento === 'CanceladoSolicitante' ? 'Cancelado pelo solicitante'
                : chamado.motivoEncerramento === 'AbertoIndevidamente' ? 'Aberto indevidamente'
                : chamado.motivoEncerramento}
              {chamado.motivoOutro && `: ${chamado.motivoOutro}`}
            </dd>
          </div>
        )}
      </dl>

      <AnexosList chamadoId={chamado.id} />

      <section className="space-y-4">
        <h2 className="text-xl font-heading">Comentários</h2>
        <ComentarioList chamadoId={chamado.id} />
        <ComentarioForm chamadoId={chamado.id} autor={perfil?.nome ?? ''} />
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-heading">Histórico</h2>
        <TimelineHistorico chamadoId={chamado.id} />
      </section>
    </div>
  )
}
