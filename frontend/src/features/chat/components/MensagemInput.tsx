import { useState, useRef, useCallback, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useEnviarMensagem, useEnviarArquivo } from '../hooks/useChat'
import type { ChatMensagemResponse } from '@/types/api'
import { Send, Paperclip, X, Smile, Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

// Bug #11: lista original tinha só 24 emojis — ampliada pra cobrir as categorias mais usadas em
// conversa de trabalho (reações, gestos, status, objetos comuns), sem virar um seletor completo.
const EMOJIS_COMPOSER = [
  '😀', '😃', '😄', '😁', '😆', '🙂', '😉', '😊',
  '😍', '🥰', '😘', '😋', '😎', '🤓', '🧐', '🤔',
  '😐', '😑', '🙄', '😴', '😪', '😢', '😭', '😮',
  '😲', '😳', '🥺', '😅', '😬', '🙁', '😞', '😡',
  '🤬', '🥳', '🤯', '🤗', '🤝', '👋', '👍', '👎',
  '👏', '🙏', '💪', '✌️', '🤞', '👌', '🤙', '👊',
  '☝️', '👀', '❤️', '🧡', '💛', '💚', '💙', '💜',
  '🖤', '🤍', '💯', '🔥', '✨', '🎉', '🎊', '🎂',
  '☕', '🍕', '🍺', '⚽', '🏆', '📌', '📎', '📅',
  '⏰', '📞', '💻', '📧', '✅', '❌', '⚠️', '❓',
  '❗', '💡', '🚀', '🙌',
]

// review-fase9-independente.md #7: erro único compartilhado entre texto e arquivo fazia o texto de
// retry ("Toque em enviar...") aparecer também em falha de upload, onde o botão de enviar nem
// reenvia o arquivo. `origem` deixa o texto e o destaque visual do botão condizentes com o que de
// fato pode ser reenviado.
interface ErroEnvio {
  mensagem: string
  origem: 'texto' | 'arquivo'
}

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
  digitandoRef: React.RefObject<boolean>,
  delay = 3000
) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const agendar = useCallback(() => {
    if (timerRef.current) clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => {
      // Sem isso, retomar a digitação depois de uma pausa >3s nunca reenviava o sinal de
      // "começou a digitar" — o guard em handleChange (if (!digitandoRef.current)) ficava
      // travado em true pra sempre depois do primeiro keystroke da mensagem inteira, mesmo
      // já tendo avisado ao servidor que parou. Resultado: o indicador aparecia uma vez e
      // nunca mais, mesmo com a pessoa continuando a digitar a mesma mensagem.
      digitandoRef.current = false
      onPararDigitar(conversaId)
    }, delay)
  }, [conversaId, onPararDigitar, delay, digitandoRef])

  // Sem isso, trocar de conversa em menos de `delay`ms de digitação deixava o timer antigo
  // vivo — ele disparava depois, chamando onPararDigitar com o ID da conversa ANTERIOR
  // (evento supérfluo indo pro grupo SignalR de uma conversa que o componente já não olha mais).
  const cancelar = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current)
      timerRef.current = null
    }
  }, [])

  return [agendar, cancelar] as const
}

export function MensagemInput({
  conversaId,
  respostaParaMensagem,
  onCancelarResposta,
  onDigitando,
  onPararDigitar,
}: MensagemInputProps) {
  const [conteudo, setConteudo] = useState('')
  const [erro, setErro] = useState<ErroEnvio | null>(null)
  const [mostrarEmojiPicker, setMostrarEmojiPicker] = useState(false)
  const fileInputRef = useRef<HTMLInputElement | null>(null)
  const emojiPickerRef = useRef<HTMLDivElement | null>(null)
  const emojiBotaoRef = useRef<HTMLButtonElement | null>(null)
  const digitandoRef = useRef(false)
  const [agendarPararDigitar, cancelarPararDigitar] = usePararDigitarDebounce(conversaId, onPararDigitar, digitandoRef)

  const enviarMensagem = useEnviarMensagem(conversaId)
  const enviarArquivo = useEnviarArquivo(conversaId)

  // Limpar estado ao trocar de conversa
  useEffect(() => {
    setConteudo('')
    setErro(null)
    setMostrarEmojiPicker(false)
    digitandoRef.current = false
    // review-fase9-independente.md #6: cancelarPararDigitar() era chamado só no CORPO do efeito, o
    // que cobria a troca de conversa (efeito reroda) mas não o desmonte do componente — sair de
    // /chat com o timer pendente deixava um PararDigitar disparar até 3s depois sobre uma conexão
    // já parada. Devolver a função de cleanup cobre os dois casos.
    return cancelarPararDigitar
  }, [conversaId, cancelarPararDigitar])

  // AC-49: fecha o picker de emoji ao clicar fora dele ou pressionar Esc — antes só fechava
  // escolhendo um emoji ou trocando de conversa, o que incomodava no uso real.
  useEffect(() => {
    if (!mostrarEmojiPicker) return

    const aoClicarFora = (e: MouseEvent) => {
      const alvo = e.target as Node
      if (emojiPickerRef.current?.contains(alvo)) return
      if (emojiBotaoRef.current?.contains(alvo)) return
      setMostrarEmojiPicker(false)
    }

    const aoPressionarTecla = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMostrarEmojiPicker(false)
    }

    document.addEventListener('mousedown', aoClicarFora)
    document.addEventListener('keydown', aoPressionarTecla)
    return () => {
      document.removeEventListener('mousedown', aoClicarFora)
      document.removeEventListener('keydown', aoPressionarTecla)
    }
  }, [mostrarEmojiPicker])

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      enviar()
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setConteudo(e.target.value)
    if (erro) setErro(null)

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
          cancelarPararDigitar()
          onPararDigitar(conversaId)
        },
        onError: (err) =>
          setErro({ mensagem: err instanceof Error ? err.message : 'Erro ao enviar mensagem.', origem: 'texto' }),
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
      onError: (err) => {
        // review-fase9-independente.md #7: sem limpar aqui, reescolher o MESMO arquivo depois de
        // uma falha não dispara onChange (o value do input não mudou) — a pessoa ficava sem
        // conseguir tentar de novo sem escolher um arquivo diferente antes.
        if (fileInputRef.current) fileInputRef.current.value = ''
        setErro({ mensagem: err instanceof Error ? err.message : 'Erro ao enviar arquivo.', origem: 'arquivo' })
      },
    })
  }

  const inserirEmoji = (emoji: string) => {
    setConteudo((prev) => prev + emoji)
    setMostrarEmojiPicker(false)
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

      {/* Erro — AC-52: mensagem específica, texto digitado preservado, reenvio em um clique
          (o botão de enviar destaca em vermelho enquanto o erro de TEXTO não é resolvido) */}
      {erro && (
        <Alert variant="destructive">
          <AlertDescription className="text-xs">
            {erro.mensagem}{' '}
            {erro.origem === 'texto'
              ? 'Toque em enviar para tentar de novo.'
              : 'Escolha o arquivo novamente para tentar de novo.'}
          </AlertDescription>
        </Alert>
      )}

      {/* Picker de emojis do composer */}
      {mostrarEmojiPicker && (
        <div
          ref={emojiPickerRef}
          className="mb-1 grid max-h-56 grid-cols-8 gap-1 overflow-y-auto rounded-md border border-border bg-popover p-2 shadow-md"
        >
          {EMOJIS_COMPOSER.map((emoji) => (
            <button
              key={emoji}
              type="button"
              onClick={() => inserirEmoji(emoji)}
              className="rounded p-1 text-lg hover:bg-muted transition-colors"
            >
              {emoji}
            </button>
          ))}
        </div>
      )}

      {/* Input principal */}
      <div className="flex items-end gap-2">
        <Button
          ref={emojiBotaoRef}
          type="button"
          variant="ghost"
          size="sm"
          className="h-9 w-9 flex-shrink-0 p-0"
          onClick={() => setMostrarEmojiPicker((v) => !v)}
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
          className={cn('h-9 w-9 flex-shrink-0 p-0', erro?.origem === 'texto' && !isPending && 'ring-2 ring-destructive')}
          onClick={enviar}
          disabled={isPending || !conteudo.trim()}
          title={erro?.origem === 'texto' && !isPending ? 'Tentar enviar novamente' : 'Enviar'}
        >
          {isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
        </Button>
      </div>
    </div>
  )
}
