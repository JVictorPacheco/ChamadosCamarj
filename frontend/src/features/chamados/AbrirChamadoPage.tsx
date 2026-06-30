import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { useQueryClient } from '@tanstack/react-query'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { ApiError } from '@/lib/api'
import { useAbrirChamado } from './hooks/useAbrirChamado'
import { useCategorias } from './hooks/useCategorias'
import type { PrioridadeChamado } from '@/types/api'

interface FormValues {
  titulo: string
  descricao: string
  categoriaId: string
  prioridade: PrioridadeChamado
}

const PRIORIDADES: PrioridadeChamado[] = ['Baixa', 'Media', 'Alta', 'Urgente']

export function AbrirChamadoPage() {
  const { perfil } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: categorias } = useCategorias()
  const { mutate, isPending, error } = useAbrirChamado()
  const {
    register,
    handleSubmit,
    control,
    setError,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: { prioridade: 'Media', categoriaId: '' } })

  const onSubmit = (values: FormValues) => {
    if (!perfil) return

    mutate(
      {
        titulo: values.titulo,
        descricao: values.descricao,
        categoriaId: values.categoriaId,
        prioridade: values.prioridade,
        solicitanteNome: perfil.nome,
        solicitanteEmail: perfil.email,
      },
      {
        onSuccess: (chamado) => navigate(`/chamados/${chamado.id}`),
        onError: (err) => {
          if (!(err instanceof ApiError)) return

          if (err.status === 404) {
            queryClient.invalidateQueries({ queryKey: ['categorias'] })
            setError('categoriaId', { message: 'Categoria não existe mais. Lista atualizada, selecione outra.' })
            return
          }

          for (const { campo, erro } of err.errors ?? []) {
            const field = (campo.charAt(0).toLowerCase() + campo.slice(1)) as keyof FormValues
            setError(field, { message: erro })
          }
        },
      },
    )
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex max-w-xl flex-col gap-4 p-4">
      <h1 className="text-xl font-heading">Abrir chamado</h1>

      <div className="flex flex-col gap-1.5">
        <Label htmlFor="titulo">Título</Label>
        <Input id="titulo" {...register('titulo', { required: 'Título é obrigatório.' })} />
        {errors.titulo && <p className="text-sm text-destructive">{errors.titulo.message}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <Label htmlFor="descricao">Descrição</Label>
        <Textarea id="descricao" {...register('descricao', { required: 'Descrição é obrigatória.' })} />
        {errors.descricao && <p className="text-sm text-destructive">{errors.descricao.message}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Categoria</Label>
        <Controller
          control={control}
          name="categoriaId"
          rules={{ required: 'Categoria é obrigatória.' }}
          render={({ field }) => (
            <Select onValueChange={field.onChange} value={field.value}>
              <SelectTrigger>
                <SelectValue placeholder="Selecione uma categoria" />
              </SelectTrigger>
              <SelectContent>
                {categorias?.map((categoria) => (
                  <SelectItem key={categoria.id} value={categoria.id}>
                    {categoria.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
        {errors.categoriaId && <p className="text-sm text-destructive">{errors.categoriaId.message}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Prioridade</Label>
        <Controller
          control={control}
          name="prioridade"
          render={({ field }) => (
            <Select onValueChange={field.onChange} value={field.value}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {PRIORIDADES.map((prioridade) => (
                  <SelectItem key={prioridade} value={prioridade}>
                    {prioridade}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
      </div>

      {error && !(error instanceof ApiError && (error.errors?.length || error.status === 404)) && (
        <Alert variant="destructive">
          <AlertDescription>{error.message}</AlertDescription>
        </Alert>
      )}

      <Button type="submit" disabled={isPending} className="self-end">
        Abrir chamado
      </Button>
    </form>
  )
}
