import { useState, useCallback } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
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
import { useAreas, useTipos } from './hooks/useTiposEAreas'
import { SeletorArquivosMultiplo } from './components/SeletorArquivosMultiplo'
import { uploadAnexo, sugerirTriagem, type TriagemSugestao } from './api'
import type { PrioridadeChamado } from '@/types/api'

interface FormValues {
  titulo: string
  descricao: string
  areaId: string
  tipoId: string
  prioridade: PrioridadeChamado
}

const PRIORIDADES: PrioridadeChamado[] = ['Baixa', 'Media', 'Alta', 'Urgente']

export function AbrirChamadoPage() {
  const { perfil } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: areas } = useAreas()
  const { data: tipos } = useTipos()
  const { mutate, isPending, error } = useAbrirChamado()
  const [arquivos, setArquivos] = useState<File[]>([])
  const [enviandoAnexos, setEnviandoAnexos] = useState(false)
  const [sugestao, setSugestao] = useState<TriagemSugestao | null>(null)
  const [sugerindo, setSugerindo] = useState(false)
  const {
    register,
    handleSubmit,
    control,
    setValue,
    setError,
    formState: { errors },
  } = useForm<FormValues>({
    // Área já vem com o grupo de quem abre; pode trocar (spec area-e-tipo-do-chamado AC-02).
    defaultValues: { prioridade: 'Media', areaId: perfil?.grupoId ?? '', tipoId: '' },
  })

  const titulo = useWatch({ control, name: 'titulo' })
  const descricao = useWatch({ control, name: 'descricao' })

  const handleSugerir = useCallback(async () => {
    const texto = (titulo ?? '').trim() + ' ' + (descricao ?? '').trim()
    if (texto.length < 5) return

    setSugerindo(true)
    try {
      const resultado = await sugerirTriagem(titulo ?? '', descricao ?? '')
      if (resultado.temSugestao) {
        setSugestao(resultado)
        if (resultado.areaId) setValue('areaId', resultado.areaId)
        if (resultado.tipoId) setValue('tipoId', resultado.tipoId)
      }
    } catch {
      setSugestao(null)
    } finally {
      setSugerindo(false)
    }
  }, [titulo, descricao, setValue])

  const onSubmit = (values: FormValues) => {
    if (!perfil) return

    mutate(
      {
        titulo: values.titulo,
        descricao: values.descricao,
        areaId: values.areaId,
        tipoId: values.tipoId,
        prioridade: values.prioridade,
      },
      {
        onSuccess: async (chamado) => {
          if (arquivos.length === 0) {
            navigate(`/chamados/${chamado.id}`)
            return
          }

          setEnviandoAnexos(true)
          const resultados = await Promise.allSettled(arquivos.map((arquivo) => uploadAnexo(chamado.id, arquivo)))
          setEnviandoAnexos(false)
          const falhas = resultados.filter((r) => r.status === 'rejected').length

          navigate(`/chamados/${chamado.id}`, {
            state:
              falhas > 0
                ? {
                    avisoAnexos: `Chamado criado, mas ${falhas} de ${arquivos.length} anexo(s) não foram enviados. Tente novamente aqui na tela do chamado.`,
                  }
                : undefined,
          })
        },
        onError: (err) => {
          if (!(err instanceof ApiError)) return

          if (err.status === 404) {
            // Área ou tipo desativado entre abrir a tela e enviar.
            queryClient.invalidateQueries({ queryKey: ['areas'] })
            queryClient.invalidateQueries({ queryKey: ['tipos'] })
            setError('tipoId', { message: 'A área ou o tipo escolhido não está mais disponível. Lista atualizada, selecione de novo.' })
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

      <div className="flex items-center justify-between">
        <span className="text-sm text-muted-foreground">Área e tipo</span>
        <Button type="button" variant="ghost" size="sm" onClick={handleSugerir} disabled={sugerindo || isPending}>
          {sugerindo ? 'Sugerindo...' : 'Sugerir área e tipo'}
        </Button>
      </div>
      {sugestao?.temSugestao && (
        <p className="text-xs text-muted-foreground">
          Sugestão:{' '}
          {[sugestao.areaNome && `Área: ${sugestao.areaNome}`, sugestao.tipoNome && `Tipo: ${sugestao.tipoNome}`]
            .filter(Boolean)
            .join(' / ')}
        </p>
      )}

      <div className="flex flex-col gap-1.5">
        <Label>Área</Label>
        <Controller
          control={control}
          name="areaId"
          rules={{ required: 'Área é obrigatória.' }}
          render={({ field }) => (
            <Select onValueChange={field.onChange} value={field.value}>
              <SelectTrigger>
                <SelectValue placeholder="Selecione a área" />
              </SelectTrigger>
              <SelectContent>
                {areas?.map((area) => (
                  <SelectItem key={area.id} value={area.id}>
                    {area.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
        {errors.areaId && <p className="text-sm text-destructive">{errors.areaId.message}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <Label>Tipo</Label>
        <Controller
          control={control}
          name="tipoId"
          rules={{ required: 'Tipo é obrigatório.' }}
          render={({ field }) => (
            <Select onValueChange={field.onChange} value={field.value}>
              <SelectTrigger>
                <SelectValue placeholder="Incidente, dúvida, solicitação..." />
              </SelectTrigger>
              <SelectContent>
                {tipos?.map((tipo) => (
                  <SelectItem key={tipo.id} value={tipo.id}>
                    {tipo.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
        {errors.tipoId && <p className="text-sm text-destructive">{errors.tipoId.message}</p>}
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

      <SeletorArquivosMultiplo arquivos={arquivos} onChange={setArquivos} disabled={isPending || enviandoAnexos} />

      {error && !(error instanceof ApiError && (error.errors?.length || error.status === 404)) && (
        <Alert variant="destructive">
          <AlertDescription>{error.message}</AlertDescription>
        </Alert>
      )}

      <Button type="submit" disabled={isPending || enviandoAnexos} className="self-end">
        {enviandoAnexos ? 'Enviando anexos...' : 'Abrir chamado'}
      </Button>
    </form>
  )
}
