import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAreas, useTipos } from '../hooks/useTiposEAreas'
import type { PrioridadeChamado, StatusChamado, SlaStatus, MotivoEncerramento } from '@/types/api'

export interface FiltroChamadosValue {
  status?: StatusChamado
  areaId?: string
  tipoId?: string
  busca?: string
  prioridade?: PrioridadeChamado
  dataInicio?: string
  dataFim?: string
  slaStatus?: SlaStatus
  motivoEncerramento?: MotivoEncerramento
}

const STATUS_OPTIONS_PADRAO: StatusChamado[] = ['Aberto', 'EmAndamento', 'Resolvido', 'Fechado', 'Cancelado']
const PRIORIDADE_OPTIONS: PrioridadeChamado[] = ['Baixa', 'Media', 'Alta', 'Urgente']
const SLA_OPTIONS: SlaStatus[] = ['DentroPrazo', 'Atencao', 'Atrasado']
const MOTIVO_OPTIONS: MotivoEncerramento[] = ['Resolvido', 'CanceladoSolicitante', 'AbertoIndevidamente', 'Duplicata', 'SemResposta', 'Outro']
const TODOS = 'todos'

interface FiltroChamadosProps {
  value: FiltroChamadosValue
  onChange: (value: FiltroChamadosValue) => void
  statusOptions?: StatusChamado[]
  mostrarPeriodo?: boolean
  mostrarSla?: boolean
  mostrarMotivo?: boolean
}

export function FiltroChamados({
  value,
  onChange,
  statusOptions = STATUS_OPTIONS_PADRAO,
  mostrarPeriodo = false,
  mostrarSla = false,
  mostrarMotivo = false,
}: FiltroChamadosProps) {
  const { data: areas } = useAreas()
  const { data: tipos } = useTipos()

  return (
    <div className="flex flex-wrap items-end gap-2">
      <Select
        value={value.status ?? TODOS}
        onValueChange={(status) =>
          onChange({ ...value, status: status === TODOS ? undefined : (status as StatusChamado) })
        }
      >
        <SelectTrigger>
          <SelectValue placeholder="Status" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={TODOS}>Todos os status</SelectItem>
          {statusOptions.map((status) => (
            <SelectItem key={status} value={status}>
              {status}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={value.prioridade ?? TODOS}
        onValueChange={(prioridade) =>
          onChange({ ...value, prioridade: prioridade === TODOS ? undefined : (prioridade as PrioridadeChamado) })
        }
      >
        <SelectTrigger>
          <SelectValue placeholder="Prioridade" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={TODOS}>Todas as prioridades</SelectItem>
          {PRIORIDADE_OPTIONS.map((prioridade) => (
            <SelectItem key={prioridade} value={prioridade}>
              {prioridade}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={value.areaId ?? TODOS}
        onValueChange={(areaId) => onChange({ ...value, areaId: areaId === TODOS ? undefined : areaId })}
      >
        <SelectTrigger>
          <SelectValue placeholder="Área" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={TODOS}>Todas as áreas</SelectItem>
          {areas?.map((area) => (
            <SelectItem key={area.id} value={area.id}>
              {area.nome}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={value.tipoId ?? TODOS}
        onValueChange={(tipoId) => onChange({ ...value, tipoId: tipoId === TODOS ? undefined : tipoId })}
      >
        <SelectTrigger>
          <SelectValue placeholder="Tipo" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={TODOS}>Todos os tipos</SelectItem>
          {tipos?.map((tipo) => (
            <SelectItem key={tipo.id} value={tipo.id}>
              {tipo.nome}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Input
        placeholder="Buscar por título, descrição ou número (CAM-42)..."
        value={value.busca ?? ''}
        onChange={(e) => onChange({ ...value, busca: e.target.value || undefined })}
      />

      {mostrarSla && (
        <Select
          value={value.slaStatus ?? TODOS}
          onValueChange={(sla) =>
            onChange({ ...value, slaStatus: sla === TODOS ? undefined : (sla as SlaStatus) })
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="SLA" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={TODOS}>Todos</SelectItem>
            {SLA_OPTIONS.map((sla) => (
              <SelectItem key={sla} value={sla}>
                {sla === 'DentroPrazo' ? 'No prazo' : sla === 'Atencao' ? 'Atenção' : 'Atrasado'}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      )}

      {mostrarMotivo && (
        <Select
          value={value.motivoEncerramento ?? TODOS}
          onValueChange={(motivo) =>
            onChange({ ...value, motivoEncerramento: motivo === TODOS ? undefined : (motivo as MotivoEncerramento) })
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Motivo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={TODOS}>Todos os motivos</SelectItem>
            {MOTIVO_OPTIONS.map((motivo) => (
              <SelectItem key={motivo} value={motivo}>
                {motivo === 'CanceladoSolicitante' ? 'Cancelado pelo solicitante'
                  : motivo === 'AbertoIndevidamente' ? 'Aberto indevidamente'
                  : motivo}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      )}

      {mostrarPeriodo && (
        <>
          <div className="flex flex-col gap-1">
            <Label htmlFor="filtro-data-inicio" className="text-xs text-muted-foreground">
              De
            </Label>
            <Input
              id="filtro-data-inicio"
              type="date"
              value={value.dataInicio ?? ''}
              onChange={(e) => onChange({ ...value, dataInicio: e.target.value || undefined })}
            />
          </div>

          <div className="flex flex-col gap-1">
            <Label htmlFor="filtro-data-fim" className="text-xs text-muted-foreground">
              Até
            </Label>
            <Input
              id="filtro-data-fim"
              type="date"
              value={value.dataFim ?? ''}
              onChange={(e) => onChange({ ...value, dataFim: e.target.value || undefined })}
            />
          </div>
        </>
      )}
    </div>
  )
}
