import { useIsFetching, useMutation, useQueryClient } from '@tanstack/react-query'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { reclassificarTipo } from '../api'
import { useTipos } from '../hooks/useTiposEAreas'
import { recarregarSeConflito } from '../hooks/useAcoesChamado'
import type { ChamadoResponse } from '@/types/api'

/**
 * Tipo do chamado no detalhe. Atendente/Admin podem reclassificar (spec area-e-tipo-do-chamado
 * AC-11 — ex.: chamados antigos "Não classificado"); Solicitante só vê.
 */
export function TipoChamadoCampo({ chamado }: { chamado: ChamadoResponse }) {
  const { perfil } = useAuth()
  const queryClient = useQueryClient()
  // Logo após a própria troca, a versão na tela ainda é a antiga: esperar a recarga evita 409 falso (AC-12).
  const recarregando = useIsFetching({ queryKey: ['chamado', chamado.id] }) > 0
  const podeReclassificar = perfil?.tipo === 'Admin' || perfil?.tipo === 'Atendente'
  const { data: tipos } = useTipos()
  const { mutate, isPending, error } = useMutation({
    mutationFn: (novoTipoId: string) => reclassificarTipo(chamado.id, novoTipoId, chamado.versao),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chamado', chamado.id] })
      queryClient.invalidateQueries({ queryKey: ['historico', chamado.id] })
      queryClient.invalidateQueries({ queryKey: ['chamados'] })
    },
    onError: (erro) => recarregarSeConflito(queryClient, chamado.id, erro),
  })

  if (!podeReclassificar) return <dd>{chamado.tipoNome ?? 'Não classificado'}</dd>

  // O tipo atual pode estar inativo (ex.: "Não classificado"): aparece como valor, mas não como opção.
  const tipoAtualAtivo = tipos?.some((t) => t.id === chamado.tipoId)

  return (
    <dd className="flex flex-col gap-2">
      <Select
        value={tipoAtualAtivo ? (chamado.tipoId ?? undefined) : undefined}
        onValueChange={(novoTipoId) => mutate(novoTipoId)}
        disabled={isPending || recarregando}
      >
        <SelectTrigger className="max-w-xs" aria-label="Tipo do chamado">
          <SelectValue placeholder={chamado.tipoNome ?? 'Não classificado'} />
        </SelectTrigger>
        <SelectContent>
          {tipos?.map((tipo) => (
            <SelectItem key={tipo.id} value={tipo.id}>
              {tipo.nome}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {error && (
        <Alert variant="destructive">
          <AlertDescription>{error.message || 'Não foi possível alterar o tipo.'}</AlertDescription>
        </Alert>
      )}
    </dd>
  )
}
