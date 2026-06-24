import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useCategorias } from '../hooks/useCategorias'
import type { StatusChamado } from '@/types/api'

export interface FiltroChamadosValue {
  status?: StatusChamado
  categoriaId?: string
  busca?: string
}

const STATUS_OPTIONS: StatusChamado[] = ['Aberto', 'EmAndamento', 'Resolvido', 'Fechado', 'Cancelado']
const TODOS = 'todos'

interface FiltroChamadosProps {
  value: FiltroChamadosValue
  onChange: (value: FiltroChamadosValue) => void
}

export function FiltroChamados({ value, onChange }: FiltroChamadosProps) {
  const { data: categorias } = useCategorias()

  return (
    <div className="flex flex-wrap gap-2">
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
          {STATUS_OPTIONS.map((status) => (
            <SelectItem key={status} value={status}>
              {status}
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
    </div>
  )
}
