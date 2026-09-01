import { useState, useCallback, useEffect } from 'react'
import { useNavigate } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { useAuth } from '@/auth/AuthContext'
import { useCriarConversa, useMarcarComoLido } from './hooks/useChat'
import { useConversaDetalhe } from './hooks/useConversas'
import { useChatSignalR } from './hooks/useChatSignalR'
import { ConversaList } from './components/ConversaList'
import { MensagemList } from './components/MensagemList'
import { MensagemInput } from './components/MensagemInput'
import { PresencaPanel } from './components/PresencaPanel'
import { MembrosGrupoDialog } from './components/MembrosGrupoDialog'
import type { ChatMensagemResponse } from '@/types/api'
import { MessageSquare, PanelLeftClose, PanelLeftOpen, Users } from 'lucide-react'

export function ChatPage() {
  const { perfil } = useAuth()
  const navigate = useNavigate()
  const [conversaAtivaId, setConversaAtivaId] = useState<string | null>(null)
  const [respostaParaMensagem, setRespostaParaMensagem] = useState<ChatMensagemResponse | null>(null)
  const [digitandoNome, setDigitandoNome] = useState<string | null>(null)
  const [acessoRevogadoAlerta, setAcessoRevogadoAlerta] = useState(false)
  const [mostrarPresenca, setMostrarPresenca] = useState(false)
  const [painelColapsado, setPainelColapsado] = useState(false)
  const [mostrarMembros, setMostrarMembros] = useState(false)

  const marcarLido = useMarcarComoLido()
  const criarConversa = useCriarConversa()
  const { data: conversaDetalhe } = useConversaDetalhe(conversaAtivaId)

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

  const { emitirDigitando, emitirPararDigitar, status: statusConexao } = useChatSignalR({
    conversaAtiva: conversaAtivaId,
    onAcessoRevogado: handleAcessoRevogado,
    onDigitando: handleDigitando,
    onPararDigitar: handlePararDigitar,
  })

  // Só mostra o aviso de reconexão se ficar sem conexão por mais de 1.5s — evita
  // piscar a cada blip rápido de rede (reconexão automática costuma resolver na hora).
  const [mostrarAvisoConexao, setMostrarAvisoConexao] = useState(false)
  useEffect(() => {
    if (statusConexao === 'conectado') {
      setMostrarAvisoConexao(false)
      return
    }
    const timer = setTimeout(() => setMostrarAvisoConexao(true), 1500)
    return () => clearTimeout(timer)
  }, [statusConexao])

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

  const iniciarConversa = (usuarioId: string) => {
    criarConversa.mutate(usuarioId, {
      onSuccess: (conversa) => {
        selecionarConversa(conversa.id)
        setMostrarPresenca(false)
      },
    })
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

      {mostrarAvisoConexao && !acessoRevogadoAlerta && (
        <Alert className="m-2">
          <AlertDescription>
            {statusConexao === 'offline'
              ? 'Sem conexão em tempo real — tentando reconectar...'
              : 'Reconectando ao chat em tempo real...'}
          </AlertDescription>
        </Alert>
      )}

      <div className="flex flex-1 overflow-hidden">
        {/* Coluna esquerda: Lista de conversas + Presença */}
        {!painelColapsado ? (
          <div className="flex w-full flex-col border-r border-border md:w-80 lg:w-96">
            {/* Tabs: Conversas / Presença */}
            <div className="flex items-center border-b border-border">
              <div className="flex flex-1">
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
              <button
                type="button"
                onClick={() => setPainelColapsado(true)}
                title="Recolher painel de conversas"
                className="border-l border-border px-2 py-2 text-muted-foreground hover:text-foreground transition-colors"
              >
                <PanelLeftClose className="h-4 w-4" />
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
                  <PresencaPanel onIniciarConversa={iniciarConversa} />
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="hidden w-10 flex-col items-center border-r border-border py-2 md:flex">
            <button
              type="button"
              onClick={() => setPainelColapsado(false)}
              title="Mostrar painel de conversas"
              className="rounded-md p-1 text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
            >
              <PanelLeftOpen className="h-4 w-4" />
            </button>
          </div>
        )}

        {/* Coluna direita: conversa ativa */}
        <div className="hidden flex-1 flex-col md:flex">
          {conversaAtivaId ? (
            <>
              {conversaDetalhe?.tipo === 'Grupo' && (
                <div className="flex items-center justify-between border-b border-border px-4 py-2">
                  <span className="truncate text-sm font-medium">{conversaDetalhe.nome}</span>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setMostrarMembros(true)}
                    className="h-8 gap-1.5 px-2 text-xs text-muted-foreground"
                  >
                    <Users className="h-3.5 w-3.5" />
                    {conversaDetalhe.participantes.length} membros
                  </Button>
                </div>
              )}
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

      {conversaAtivaId && conversaDetalhe?.tipo === 'Grupo' && (
        <MembrosGrupoDialog
          conversaId={conversaAtivaId}
          open={mostrarMembros}
          onOpenChange={setMostrarMembros}
          onIniciarConversa={iniciarConversa}
        />
      )}
    </div>
  )
}
