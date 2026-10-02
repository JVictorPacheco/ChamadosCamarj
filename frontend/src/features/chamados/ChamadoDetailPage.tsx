import { useCallback, useState } from 'react'
import { useIsFetching } from '@tanstack/react-query'
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
import { UploadAnexoForm } from './components/UploadAnexoForm'
import { TipoChamadoCampo } from './components/TipoChamadoCampo'
import { EditarChamadoModal } from './components/EditarChamadoModal'
import { podeEditarChamado } from './lib/permissoes'
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
  const atribuir = useAtribuirChamado(chamado.id, chamado.versao)
  // Versão de quando o diálogo/modal foi aberto: uma recarga em segundo plano com ele aberto não
  // pode trocar a versão por baixo, senão a alteração de outra pessoa seria sobrescrita sem aviso
  // (spec correcoes-pre-deploy AC-09; review-2 R-01).
  const [versaoAoAbrir, setVersaoAoAbrir] = useState(chamado.versao)
  const resolver = useResolverChamado(chamado.id, versaoAoAbrir)
  const fechar = useFecharChamado(chamado.id, versaoAoAbrir)
  const cancelar = useCancelarChamado(chamado.id, versaoAoAbrir)
  const reabrir = useReabrirChamado(chamado.id, versaoAoAbrir)
  // Enquanto o chamado recarrega (ex.: logo após a própria ação), a versão na tela ainda é a antiga:
  // bloquear os botões evita um 409 falso (spec correcoes-pre-deploy AC-12).
  const recarregando = useIsFetching({ queryKey: ['chamado', chamado.id] }) > 0
  const [reatribuirAberto, setReatribuirAberto] = useState(false)
  const [prioridadeAberto, setPrioridadeAberto] = useState(false)
  const [forcarEncerramentoAberto, setForcarEncerramentoAberto] = useState(false)
  const [editarAberto, setEditarAberto] = useState(false)
  const [confirmarAcao, setConfirmarAcao] = useState<'resolver' | 'encerrar' | 'cancelar' | 'reabrir' | null>(null)
  const [motivoSelecionado, setMotivoSelecionado] = useState<MotivoEncerramento>('Resolvido')
  const [motivoOutroTexto, setMotivoOutroTexto] = useState('')
  const [observacaoTexto, setObservacaoTexto] = useState('')

  const isAdmin = perfil?.tipo === 'Admin'
  const isAtendente = perfil?.tipo === 'Admin' || perfil?.tipo === 'Atendente'
  const isSolicitante = perfil?.tipo === 'Solicitante'
  // Solicitante do grupo vê o chamado do colega, mas só quem abriu pode cancelar (spec autorizacao-chamados AC-06)
  const abriuOChamado = chamado.solicitanteEmail.toLowerCase() === (perfil?.email ?? '').toLowerCase()
  const status = chamado.status
  const statusFinal = status === 'Fechado' || status === 'Cancelado'

  const isPending =
    atribuir.isPending || resolver.isPending || fechar.isPending || cancelar.isPending || reabrir.isPending || recarregando

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
    setVersaoAoAbrir(chamado.versao)
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
      {podeEditarChamado(chamado, perfil) && (
        <Button variant="outline" disabled={recarregando} onClick={() => setEditarAberto(true)}>
          Editar
        </Button>
      )}

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

      {(isAtendente || (isSolicitante && abriuOChamado)) && (status === 'Aberto' || status === 'EmAndamento') && (
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
        <Button variant="outline" disabled={recarregando} onClick={() => { setVersaoAoAbrir(chamado.versao); setReatribuirAberto(true) }}>
          Reatribuir
        </Button>
      )}

      {isAdmin && !statusFinal && (
        <Button variant="outline" disabled={recarregando} onClick={() => { setVersaoAoAbrir(chamado.versao); setPrioridadeAberto(true) }}>
          Alterar prioridade
        </Button>
      )}

      {isAdmin && !statusFinal && (
        <Button variant="destructive" disabled={recarregando} onClick={() => { setVersaoAoAbrir(chamado.versao); setForcarEncerramentoAberto(true) }}>
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
        versao={versaoAoAbrir}
      />
      <AlterarPrioridadeModal
        open={prioridadeAberto}
        onOpenChange={setPrioridadeAberto}
        chamadoId={chamado.id}
        prioridadeAtual={chamado.prioridade}
        versao={versaoAoAbrir}
      />
      {editarAberto && <EditarChamadoModal chamado={chamado} onClose={() => setEditarAberto(false)} />}
      <ForcarEncerramentoModal
        open={forcarEncerramentoAberto}
        onOpenChange={setForcarEncerramentoAberto}
        chamadoId={chamado.id}
        versao={versaoAoAbrir}
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
  const [enviandoAnexos, setEnviandoAnexos] = useState(false)
  const onUploadChange = useCallback((uploading: boolean) => setEnviandoAnexos(uploading), [])

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

  // Quem pode ver o chamado é decidido pelo servidor: sem acesso, a API responde 404 (tratado acima).
  if (!chamado) return null

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
          <dt className="font-medium text-foreground">Área</dt>
          <dd>{chamado.areaNome ?? 'Sem área'}</dd>
        </div>
        <div>
          <dt className="font-medium text-foreground">Tipo</dt>
          <TipoChamadoCampo chamado={chamado} />
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

      <section className="space-y-4">
        <h2 className="text-xl font-heading">Anexos</h2>
        <UploadAnexoForm chamadoId={chamado.id} />
        <AnexosList chamadoId={chamado.id} isUploading={enviandoAnexos} />
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-heading">Comentários</h2>
        <ComentarioList chamadoId={chamado.id} />
        <ComentarioForm chamadoId={chamado.id} autor={perfil?.nome ?? ''} onUploadChange={onUploadChange} />
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-heading">Histórico</h2>
        <TimelineHistorico chamadoId={chamado.id} />
      </section>
    </div>
  )
}
