import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Separator } from '@/components/ui/separator'
import { useAuth } from '@/auth/AuthContext'
import { useMarcarComoLido } from './hooks/useChat'
import { useChatSignalR } from './hooks/useChatSignalR'
import { ConversaList } from './components/ConversaList'
import { MensagemList } from './components/MensagemList'
import { MensagemInput } from './components/MensagemInput'
import { PresencaPanel } from './components/PresencaPanel'
import type { ChatMensagemResponse } from '@/types/api'
import { MessageSquare } from 'lucide-react'

export function ChatPage() {
  const { perfil } = useAuth()
  const navigate = useNavigate()
  const [conversaAtivaId, setConversaAtivaId] = useState<string | null>(null)
  const [respostaParaMensagem, setRespostaParaMensagem] = useState<ChatMensagemResponse | null>(null)
  const [digitandoNome, setDigitandoNome] = useState<string | null>(null)
  const [acessoRevogadoAlerta, setAcessoRevogadoAlerta] = useState(false)
  const [mostrarPresenca, setMostrarPresenca] = useState(false)

  const marcarLido = useMarcarComoLido()

  const handleAcessoRevogado = useCallback(() => {
    setAcessoRevogadoAlerta(true)
    setTimeout(() => navigate('/chamados'), 3000)
  }, [navigate])

  const handleDigitando = useCallback(
    (conversaId: string, nome: string) => {
      if (conversaId === conversaAtivaId) {
        setDigitandoNome(nome)
      }
    },
    [conversaAtivaId]
  )

  const handlePararDigitar = useCallback(
    (conversaId: string) => {
      if (conversaId === conversaAtivaId) {
        setDigitandoNome(null)
      }
    },
    [conversaAtivaId]
  )

  const { emitirDigitando, emitirPararDigitar } = useChatSignalR({
    conversaAtiva: conversaAtivaId,
    onAcessoRevogado: handleAcessoRevogado,
    onDigitando: handleDigitando,
    onPararDigitar: handlePararDigitar,
  })

  // Verifica acesso ao chat
  if (perfil?.chatPerfil === 'SemAcesso' || !perfil?.chatPerfil) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Você não tem acesso ao chat corporativo.</AlertDescription>
        </Alert>
      </div>
    )
  }

  const selecionarConversa = (id: string) => {
    setConversaAtivaId(id)
    setRespostaParaMensagem(null)
    setDigitandoNome(null)
    marcarLido.mutate(id)
  }

  return (
    <div className="flex h-screen flex-col">
      {acessoRevogadoAlerta && (
        <Alert variant="destructive" className="m-2">
          <AlertDescription>
            Seu acesso ao chat foi revogado. Redirecionando...
          </AlertDescription>
        </Alert>
      )}

      <div className="flex flex-1 overflow-hidden">
        {/* Coluna esquerda: Lista de conversas + Presença */}
        <div className="flex w-full flex-col border-r border-border md:w-80 lg:w-96">
          {/* Tabs: Conversas / Presença */}
          <div className="flex border-b border-border">
            <button
              type="button"
              onClick={() => setMostrarPresenca(false)}
              className={`flex-1 py-2 text-xs font-medium transition-colors ${
                !mostrarPresenca
                  ? 'border-b-2 border-primary text-foreground'
                  : 'text-muted-foreground hover:text-foreground'
              }`}
            >
              Conversas
            </button>
            <button
              type="button"
              onClick={() => setMostrarPresenca(true)}
              className={`flex-1 py-2 text-xs font-medium transition-colors ${
                mostrarPresenca
                  ? 'border-b-2 border-primary text-foreground'
                  : 'text-muted-foreground hover:text-foreground'
              }`}
            >
              Presença
            </button>
          </div>

          <div className="flex-1 overflow-hidden">
            {!mostrarPresenca ? (
              <ConversaList
                conversaAtivaId={conversaAtivaId}
                onSelectConversa={selecionarConversa}
              />
            ) : (
              <div className="h-full overflow-y-auto py-2">
                <PresencaPanel />
              </div>
            )}
          </div>
        </div>

        {/* Coluna direita: conversa ativa */}
        <div className="hidden flex-1 flex-col md:flex">
          {conversaAtivaId ? (
            <>
              <MensagemList
                conversaId={conversaAtivaId}
                digitandoNome={digitandoNome}
                onResponder={(m) => setRespostaParaMensagem(m)}
              />
              <Separator />
              <MensagemInput
                conversaId={conversaAtivaId}
                respostaParaMensagem={respostaParaMensagem}
                onCancelarResposta={() => setRespostaParaMensagem(null)}
                onDigitando={emitirDigitando}
                onPararDigitar={emitirPararDigitar}
              />
            </>
          ) : (
            <div className="flex flex-1 flex-col items-center justify-center gap-2 text-muted-foreground">
              <MessageSquare className="h-12 w-12 opacity-20" />
              <p className="text-sm">Selecione uma conversa</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
