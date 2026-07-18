import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useCategorias } from '../hooks/useCategorias'
import type { PrioridadeChamado, StatusChamado } from '@/types/api'

export interface FiltroChamadosValue {
  status?: StatusChamado
  categoriaId?: string
  busca?: string
  prioridade?: PrioridadeChamado
  dataInicio?: string
  dataFim?: string
}

const STATUS_OPTIONS_PADRAO: StatusChamado[] = ['Aberto', 'EmAndamento', 'Resolvido', 'Fechado', 'Cancelado']
const PRIORIDADE_OPTIONS: PrioridadeChamado[] = ['Baixa', 'Media', 'Alta', 'Urgente']
const TODOS = 'todos'

interface FiltroChamadosProps {
  value: FiltroChamadosValue
  onChange: (value: FiltroChamadosValue) => void
  statusOptions?: StatusChamado[]
  mostrarPeriodo?: boolean
}

export function FiltroChamados({
  value,
  onChange,
  statusOptions = STATUS_OPTIONS_PADRAO,
  mostrarPeriodo = false,
}: FiltroChamadosProps) {
  const { data: categorias } = useCategorias()

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
        value={value.categoriaId ?? TODOS}
        onValueChange={(categoriaId) =>
          onChange({ ...value, categoriaId: categoriaId === TODOS ? undefined : categoriaId })
        }
      >
        <SelectTrigger>
          <SelectValue placeholder="Categoria" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={TODOS}>Todas as categorias</SelectItem>
          {categorias?.map((categoria) => (
            <SelectItem key={categoria.id} value={categoria.id}>
              {categoria.nome}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Input
        placeholder="Buscar..."
        value={value.busca ?? ''}
        onChange={(e) => onChange({ ...value, busca: e.target.value || undefined })}
      />

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
