import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { ApiError } from '@/lib/api'
import { useAtualizarTipo, useCriarTipo } from '../hooks/useTipos'
import type { TipoChamadoResponse } from '@/types/api'

interface TipoFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  tipo: TipoChamadoResponse | null
}

interface FormValues {
  nome: string
  descricao: string
  ativo: boolean
}

const VALORES_PADRAO: FormValues = { nome: '', descricao: '', ativo: true }

export function TipoFormDialog({ open, onOpenChange, tipo }: TipoFormDialogProps) {
  const emEdicao = !!tipo
  const { mutate: criar, isPending: criando, error: erroCriar, reset: resetCriar } = useCriarTipo()
  const { mutate: atualizar, isPending: atualizando, error: erroAtualizar, reset: resetAtualizar } =
    useAtualizarTipo()

  const isPending = criando || atualizando
  const erro = erroCriar ?? erroAtualizar

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    setError,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: VALORES_PADRAO })

  const ativo = watch('ativo')

  useEffect(() => {
    if (!open) return

    reset(
      tipo
        ? { nome: tipo.nome, descricao: tipo.descricao, ativo: tipo.ativo }
        : VALORES_PADRAO,
    )
    resetCriar()
    resetAtualizar()
  }, [open, tipo, reset, resetCriar, resetAtualizar])

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
    if (emEdicao && tipo) {
      atualizar(
        { id: tipo.id, dados: { nome: values.nome, descricao: values.descricao, ativo: values.ativo } },
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
          <DialogTitle>{emEdicao ? 'Editar tipo' : 'Novo tipo'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="nome">Nome</Label>
            <Input id="nome" maxLength={100} {...register('nome', { required: 'Nome é obrigatório.' })} />
            {errors.nome && <p className="text-sm text-destructive">{errors.nome.message}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="descricao">Descrição</Label>
            <Input id="descricao" maxLength={300} {...register('descricao', { required: 'Descrição é obrigatória.' })} />
            {errors.descricao && <p className="text-sm text-destructive">{errors.descricao.message}</p>}
          </div>

          {emEdicao && (
            <div className="flex items-center gap-2">
              <Checkbox
                id="ativo"
                checked={ativo}
                onCheckedChange={(checked) => setValue('ativo', !!checked)}
              />
              <Label htmlFor="ativo">Ativo</Label>
            </div>
          )}

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
