import { useState, useRef, useCallback, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useEnviarMensagem, useEnviarArquivo } from '../hooks/useChat'
import type { ChatMensagemResponse } from '@/types/api'
import { Send, Paperclip, X, Smile } from 'lucide-react'

interface MensagemInputProps {
  conversaId: string
  respostaParaMensagem: ChatMensagemResponse | null
  onCancelarResposta: () => void
  onDigitando: (conversaId: string) => void
  onPararDigitar: (conversaId: string) => void
}

// Debounce simples para o evento de "parar de digitar"
function usePararDigitarDebounce(
  conversaId: string,
  onPararDigitar: (id: string) => void,
  delay = 3000
) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  return useCallback(() => {
    if (timerRef.current) clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => {
      onPararDigitar(conversaId)
    }, delay)
  }, [conversaId, onPararDigitar, delay])
}

export function MensagemInput({
  conversaId,
  respostaParaMensagem,
  onCancelarResposta,
  onDigitando,
  onPararDigitar,
}: MensagemInputProps) {
  const [conteudo, setConteudo] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement | null>(null)
  const emojiInputRef = useRef<HTMLInputElement | null>(null)
  const digitandoRef = useRef(false)
  const agendarPararDigitar = usePararDigitarDebounce(conversaId, onPararDigitar)

  const enviarMensagem = useEnviarMensagem(conversaId)
  const enviarArquivo = useEnviarArquivo(conversaId)

  // Limpar estado ao trocar de conversa
  useEffect(() => {
    setConteudo('')
    setErro(null)
    digitandoRef.current = false
  }, [conversaId])

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      enviar()
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setConteudo(e.target.value)

    if (!digitandoRef.current) {
      digitandoRef.current = true
      onDigitando(conversaId)
    }
    agendarPararDigitar()
  }

  const enviar = () => {
    const texto = conteudo.trim()
    if (!texto) return
    setErro(null)
    enviarMensagem.mutate(
      {
        conteudo: texto,
        respostaParaMensagemId: respostaParaMensagem?.id,
      },
      {
        onSuccess: () => {
          setConteudo('')
          onCancelarResposta()
          digitandoRef.current = false
          onPararDigitar(conversaId)
        },
        onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao enviar mensagem.'),
      }
    )
  }

  const handleArquivo = (e: React.ChangeEvent<HTMLInputElement>) => {
    const arquivo = e.target.files?.[0]
    if (!arquivo) return
    setErro(null)
    enviarArquivo.mutate(arquivo, {
      onSuccess: () => {
        if (fileInputRef.current) fileInputRef.current.value = ''
      },
      onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao enviar arquivo.'),
    })
  }

  // Truque para abrir o seletor de emoji nativo via input[type=text] com inputmode=none
  // O foco nesse input abre o painel de emoji em dispositivos compatíveis
  const abrirEmojiNativo = () => {
    emojiInputRef.current?.focus()
    emojiInputRef.current?.click()
  }

  const handleEmojiInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const emoji = e.target.value
    if (emoji) {
      setConteudo((prev) => prev + emoji)
      e.target.value = ''
    }
  }

  const isPending = enviarMensagem.isPending || enviarArquivo.isPending

  return (
    <div className="flex flex-col gap-1 border-t border-border px-4 py-3">
      {/* Preview de citação */}
      {respostaParaMensagem && (
        <div className="flex items-start gap-2 rounded-md border-l-4 border-primary bg-muted px-3 py-2">
          <div className="flex-1 min-w-0">
            <p className="text-xs font-medium text-muted-foreground">
              Respondendo a {respostaParaMensagem.autorNome}
            </p>
            <p className="truncate text-xs text-muted-foreground">
              {respostaParaMensagem.conteudo ?? '[arquivo]'}
            </p>
          </div>
          <button
            type="button"
            onClick={onCancelarResposta}
            className="flex-shrink-0 rounded p-0.5 hover:bg-muted-foreground/20"
          >
            <X className="h-3.5 w-3.5 text-muted-foreground" />
          </button>
        </div>
      )}

      {/* Erro */}
      {erro && (
        <Alert variant="destructive">
          <AlertDescription className="text-xs">{erro}</AlertDescription>
        </Alert>
      )}

      {/* Input principal */}
      <div className="flex items-end gap-2">
        {/* Emoji picker (input oculto para truque nativo) */}
        <input
          ref={emojiInputRef}
          type="text"
          className="sr-only"
          aria-hidden="true"
          onChange={handleEmojiInput}
          tabIndex={-1}
        />
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="h-9 w-9 flex-shrink-0 p-0"
          onClick={abrirEmojiNativo}
          title="Emoji"
          disabled={isPending}
        >
          <Smile className="h-5 w-5 text-muted-foreground" />
        </Button>

        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="h-9 w-9 flex-shrink-0 p-0"
          onClick={() => fileInputRef.current?.click()}
          title="Enviar arquivo"
          disabled={isPending}
        >
          <Paperclip className="h-5 w-5 text-muted-foreground" />
        </Button>

        <input
          ref={fileInputRef}
          type="file"
          className="hidden"
          accept=".pdf,.jpg,.jpeg,.png,.gif,.webp,.docx,.xlsx,.pptx,.zip"
          onChange={handleArquivo}
        />

        <Textarea
          value={conteudo}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder="Digite uma mensagem..."
          className="min-h-[2.25rem] max-h-32 flex-1 resize-none py-2 text-sm"
          rows={1}
          disabled={isPending}
        />

        <Button
          type="button"
          size="sm"
          className="h-9 w-9 flex-shrink-0 p-0"
          onClick={enviar}
          disabled={isPending || !conteudo.trim()}
          title="Enviar"
        >
          <Send className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
