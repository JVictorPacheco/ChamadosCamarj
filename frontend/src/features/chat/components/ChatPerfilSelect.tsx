import { useState } from 'react'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useQueryClient } from '@tanstack/react-query'
import { definirChatPerfil } from '../api'
import type { ChatPerfil } from '@/types/api'

const OPCOES: { value: ChatPerfil; label: string }[] = [
  { value: 'SemAcesso', label: 'Sem Acesso' },
  { value: 'Participante', label: 'Participante' },
  { value: 'CriadorDeGrupo', label: 'Criador de Grupo' },
]

interface ChatPerfilSelectProps {
  usuarioId: string
  valorAtual: ChatPerfil
}

export function ChatPerfilSelect({ usuarioId, valorAtual }: ChatPerfilSelectProps) {
  const [pendente, setPendente] = useState(false)
  const queryClient = useQueryClient()

  const handleChange = async (valor: string) => {
    const chatPerfil = valor as ChatPerfil
    setPendente(true)
    try {
      await definirChatPerfil(usuarioId, chatPerfil)
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    } catch {
      // Falha silenciosa — o valor no select não muda (a query invalida e recupera o real)
    } finally {
      setPendente(false)
    }
  }

  return (
    <Select value={valorAtual} onValueChange={handleChange} disabled={pendente}>
      <SelectTrigger size="sm" className="w-36">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {OPCOES.map((op) => (
          <SelectItem key={op.value} value={op.value}>
            {op.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
