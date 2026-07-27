import { useState } from 'react'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { useAuth } from './AuthContext'
import { ApiError } from '@/lib/api'
import { esqueciSenha } from './api'
import logoCamarj from '../assets/logo-camarj.png'

export function LoginPage() {
  const { loginComSenha } = useAuth()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [pendente, setPendente] = useState(false)
  const [esqueciSenhaAberto, setEsqueciSenhaAberto] = useState(false)
  const [emailRecuperacao, setEmailRecuperacao] = useState('')
  const [recuperacaoPendente, setRecuperacaoPendente] = useState(false)
  const [recuperacaoEnviado, setRecuperacaoEnviado] = useState(false)
  const [recuperacaoErro, setRecuperacaoErro] = useState<string | null>(null)

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErro(null)
    setPendente(true)

    try {
      await loginComSenha(email, senha)
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        setErro('E-mail não cadastrado — peça a um Admin para te cadastrar.')
      } else {
        setErro(err instanceof Error ? err.message : 'E-mail ou senha inválidos.')
      }
    } finally {
      setPendente(false)
    }
  }

  const onEsqueciSenha = async (e: React.FormEvent) => {
    e.preventDefault()
    setRecuperacaoErro(null)
    setRecuperacaoPendente(true)

    try {
      await esqueciSenha(emailRecuperacao)
      setRecuperacaoEnviado(true)
    } catch (err) {
      setRecuperacaoErro(err instanceof Error ? err.message : 'Erro ao enviar e-mail.')
    } finally {
      setRecuperacaoPendente(false)
    }
  }

  const fecharEsqueciSenha = (aberto: boolean) => {
    if (!aberto) {
      setEmailRecuperacao('')
      setRecuperacaoEnviado(false)
      setRecuperacaoErro(null)
    }
    setEsqueciSenhaAberto(aberto)
  }

  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center gap-10 overflow-hidden p-6">
      <div className="flex flex-col items-center gap-5">
        <img
          src={logoCamarj}
          alt="Camarj"
          className="h-44 w-44 rounded-2xl shadow-xl"
        />
        <div className="text-center">
          <h1 className="font-serif text-5xl tracking-tight">Portal de Chamados</h1>
          <p className="mt-1 text-lg text-muted-foreground">Entre com sua conta corporativa CAMARJ</p>
        </div>
      </div>

      <Card className="w-full max-w-md border-border/60 shadow-2xl">
        <CardHeader>
          <CardTitle className="font-serif text-2xl">Entrar</CardTitle>
          <CardDescription className="text-base">
            Use seu e-mail e senha cadastrados
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                placeholder="seu@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="senha">Senha</Label>
              <Input
                id="senha"
                type="password"
                placeholder="Sua senha"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
              />
            </div>

            {erro && (
              <Alert variant="destructive">
                <AlertDescription>{erro}</AlertDescription>
              </Alert>
            )}

            <Button type="submit" disabled={pendente} className="w-full">
              {pendente ? 'Entrando...' : 'Entrar'}
            </Button>

            <button
              type="button"
              className="text-sm text-muted-foreground underline underline-offset-4 hover:text-foreground"
              onClick={() => setEsqueciSenhaAberto(true)}
            >
              Esqueci minha senha?
            </button>
          </form>
        </CardContent>
      </Card>

      <Dialog open={esqueciSenhaAberto} onOpenChange={fecharEsqueciSenha}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Esqueci minha senha</DialogTitle>
            <DialogDescription>
              {recuperacaoEnviado
                ? 'Um link de redefinição foi enviado para o seu e-mail. Verifique sua caixa de entrada em instantes.'
                : 'Digite seu e-mail e enviaremos um link de redefinição.'}
            </DialogDescription>
          </DialogHeader>

          {recuperacaoEnviado ? (
            <DialogFooter>
              <Button variant="outline" onClick={() => fecharEsqueciSenha(false)} className="w-full">
                Fechar
              </Button>
            </DialogFooter>
          ) : (
            <form onSubmit={onEsqueciSenha} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="email-recuperacao">E-mail</Label>
                <Input
                  id="email-recuperacao"
                  type="email"
                  placeholder="seu@email.com"
                  value={emailRecuperacao}
                  onChange={(e) => setEmailRecuperacao(e.target.value)}
                  required
                  autoFocus
                />
              </div>

              {recuperacaoErro && (
                <Alert variant="destructive">
                  <AlertDescription>{recuperacaoErro}</AlertDescription>
                </Alert>
              )}

              <DialogFooter>
                <Button type="submit" disabled={recuperacaoPendente} className="w-full">
                  {recuperacaoPendente ? 'Enviando...' : 'Enviar'}
                </Button>
              </DialogFooter>
            </form>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
