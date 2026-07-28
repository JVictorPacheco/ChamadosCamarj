import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/PasswordInput'
import { Label } from '@/components/ui/label'
import { Checkbox } from '@/components/ui/checkbox'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { ApiError } from '@/lib/api'
import { useAtualizarUsuario, useCriarUsuario } from '../hooks/useUsuarios'
import { useGrupos } from '../hooks/useGrupos'
import type { TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

interface UsuarioFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  usuario: UsuarioPerfilResponse | null
}

interface FormValues {
  nome: string
  email: string
  perfil: TipoPerfil
  ativo: boolean
  senha: string
  grupoId: string
}

const PERFIS: TipoPerfil[] = ['Admin', 'Atendente', 'Solicitante']

const VALORES_PADRAO: FormValues = { nome: '', email: '', perfil: 'Solicitante', ativo: true, senha: '', grupoId: '' }

export function UsuarioFormDialog({ open, onOpenChange, usuario }: UsuarioFormDialogProps) {
  const emEdicao = !!usuario
  const { mutate: criar, isPending: criando, error: erroCriar, reset: resetCriar } = useCriarUsuario()
  const { mutate: atualizar, isPending: atualizando, error: erroAtualizar, reset: resetAtualizar } =
    useAtualizarUsuario()
  const { data: grupos } = useGrupos()

  const isPending = criando || atualizando
  const erro = erroCriar ?? erroAtualizar

  const {
    register,
    handleSubmit,
    control,
    setError,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: VALORES_PADRAO })

  useEffect(() => {
    if (!open) return

    reset(
      usuario
        ? { nome: usuario.nome, email: usuario.email, perfil: usuario.perfil, ativo: usuario.ativo, senha: '', grupoId: usuario.grupoId ?? '' }
        : VALORES_PADRAO,
    )
    resetCriar()
    resetAtualizar()
  }, [open, usuario, reset, resetCriar, resetAtualizar])

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
      setError('email', { message: err.message })
    }
  }

  const onSubmit = (values: FormValues) => {
    const grupoId = values.grupoId || null

    if (emEdicao && usuario) {
      atualizar(
        { id: usuario.id, dados: { nome: values.nome, perfil: values.perfil, ativo: values.ativo, grupoId } },
        { onSuccess: () => fechar(false), onError: tratarErro },
      )
      return
    }

    criar(
      { email: values.email, nome: values.nome, perfil: values.perfil, senha: values.senha, grupoId },
      { onSuccess: () => fechar(false), onError: tratarErro },
    )
  }

  const erroGenerico =
    erro && !(erro instanceof ApiError && (erro.errors?.length || erro.status === 409)) ? erro : null

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{emEdicao ? 'Editar usuário' : 'Novo usuário'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="nome">Nome</Label>
            <Input id="nome" {...register('nome', { required: 'Nome é obrigatório.' })} />
            {errors.nome && <p className="text-sm text-destructive">{errors.nome.message}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">E-mail</Label>
            <Input
              id="email"
              type="email"
              disabled={emEdicao}
              {...register('email', { required: 'E-mail é obrigatório.' })}
            />
            {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
          </div>

          {!emEdicao && (
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="senha">Senha</Label>
              <PasswordInput
                id="senha"
                {...register('senha', { required: !emEdicao && 'Senha é obrigatória.', minLength: { value: 8, message: 'Mínimo 8 caracteres.' } })}
              />
              {errors.senha && <p className="text-sm text-destructive">{errors.senha.message}</p>}
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <Label>Perfil</Label>
            <Controller
              control={control}
              name="perfil"
              rules={{ required: 'Perfil é obrigatório.' }}
              render={({ field }) => (
                <Select onValueChange={field.onChange} value={field.value}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione um perfil" />
                  </SelectTrigger>
                  <SelectContent>
                    {PERFIS.map((perfilOpcao) => (
                      <SelectItem key={perfilOpcao} value={perfilOpcao}>
                        {perfilOpcao}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {errors.perfil && <p className="text-sm text-destructive">{errors.perfil.message}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Grupo</Label>
            <Controller
              control={control}
              name="grupoId"
              render={({ field }) => (
                <Select onValueChange={field.onChange} value={field.value}>
                  <SelectTrigger>
                    <SelectValue placeholder="Nenhum" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">Nenhum</SelectItem>
                    {(grupos ?? []).map((g) => (
                      <SelectItem key={g.id} value={g.id}>
                        {g.nome}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </div>

          {emEdicao && (
            <Controller
              control={control}
              name="ativo"
              render={({ field }) => (
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="usuario-ativo"
                    checked={field.value}
                    onCheckedChange={(checked) => field.onChange(!!checked)}
                  />
                  <Label htmlFor="usuario-ativo" className="text-sm font-normal">
                    Ativo
                  </Label>
                </div>
              )}
            />
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
