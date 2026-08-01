import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { ApiError } from '@/lib/api'
import { useAtualizarGrupo, useCriarGrupo } from '../hooks/useGrupos'
import type { GrupoResponse } from '@/types/api'

interface GrupoFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  grupo: GrupoResponse | null
}

interface FormValues {
  nome: string
  descricao: string
}

const VALORES_PADRAO: FormValues = { nome: '', descricao: '' }

export function GrupoFormDialog({ open, onOpenChange, grupo }: GrupoFormDialogProps) {
  const emEdicao = !!grupo
  const { mutate: criar, isPending: criando, error: erroCriar, reset: resetCriar } = useCriarGrupo()
  const { mutate: atualizar, isPending: atualizando, error: erroAtualizar, reset: resetAtualizar } =
    useAtualizarGrupo()

  const isPending = criando || atualizando
  const erro = erroCriar ?? erroAtualizar

  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: VALORES_PADRAO })

  useEffect(() => {
    if (!open) return

    reset(
      grupo
        ? { nome: grupo.nome, descricao: grupo.descricao }
        : VALORES_PADRAO,
    )
    resetCriar()
    resetAtualizar()
  }, [open, grupo, reset, resetCriar, resetAtualizar])

  const fechar = (proximoEstado: boolean) => {
    onOpenChange(proximoEstado)
  }

  const tratarErro = (err: Error) => {
    if (!(err instanceof ApiError)) return

    for (const { campo, erro: mensagem } of err.errors ?? []) {
      const field = (campo.charAt(0).toLowerCase() + campo.slice(1)) as keyof FormValues
      setError(field, { message: mensagem })
    }

    if (err.status === 409) {
      setError('nome', { message: err.message })
    }
  }

  const onSubmit = (values: FormValues) => {
    if (emEdicao && grupo) {
      atualizar(
        { id: grupo.id, dados: { nome: values.nome, descricao: values.descricao } },
        { onSuccess: () => fechar(false), onError: tratarErro },
      )
      return
    }

    criar(
      { nome: values.nome, descricao: values.descricao },
      { onSuccess: () => fechar(false), onError: tratarErro },
    )
  }

  const erroGenerico =
    erro && !(erro instanceof ApiError && (erro.errors?.length || erro.status === 409)) ? erro : null

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{emEdicao ? 'Editar grupo' : 'Novo grupo'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="nome">Nome</Label>
            <Input id="nome" {...register('nome', { required: 'Nome é obrigatório.' })} />
            {errors.nome && <p className="text-sm text-destructive">{errors.nome.message}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="descricao">Descrição</Label>
            <Input id="descricao" {...register('descricao', { required: 'Descrição é obrigatória.' })} />
            {errors.descricao && <p className="text-sm text-destructive">{errors.descricao.message}</p>}
          </div>

          {erroGenerico && (
            <Alert variant="destructive">
              <AlertDescription>{erroGenerico.message}</AlertDescription>
            </Alert>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => fechar(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Salvando...' : 'Salvar'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
