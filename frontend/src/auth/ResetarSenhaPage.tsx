import { useState } from 'react'
import { useSearchParams, Link } from 'react-router'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { PasswordInput } from '@/components/PasswordInput'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Sun, Moon } from 'lucide-react'
import { useTheme } from '@/hooks/useTheme'
import { resetarSenha } from './api'
import logoCamarj from '../assets/logo-camarj.png'

export function ResetarSenhaPage() {
  const { theme, toggleTheme } = useTheme()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''

  const [novaSenha, setNovaSenha] = useState('')
  const [confirmarSenha, setConfirmarSenha] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [pendente, setPendente] = useState(false)
  const [sucesso, setSucesso] = useState(false)

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErro(null)

    if (novaSenha.length < 8) {
      setErro('A senha deve ter no mínimo 8 caracteres.')
      return
    }

    if (novaSenha !== confirmarSenha) {
      setErro('As senhas não conferem.')
      return
    }

    setPendente(true)

    try {
      await resetarSenha(token, novaSenha)
      setSucesso(true)
    } catch (err) {
      setErro(err instanceof Error ? err.message : 'Erro ao redefinir senha.')
    } finally {
      setPendente(false)
    }
  }

  if (!token) {
    return (
      <div className="relative flex min-h-svh items-center justify-center p-6">
        <button
          type="button"
          onClick={toggleTheme}
          className="absolute top-4 right-4 rounded-lg p-2 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          aria-label={theme === 'dark' ? 'Alternar para tema claro' : 'Alternar para tema escuro'}
        >
          {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </button>
        <Card className="w-full max-w-md border-border/60 shadow-2xl">
          <CardHeader>
            <CardTitle className="font-serif text-2xl">Link inválido</CardTitle>
            <CardDescription>
              O link de redefinição de senha é inválido ou expirou.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild className="w-full">
              <Link to="/login">Voltar para o login</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center gap-10 overflow-hidden p-6">
      <button
        type="button"
        onClick={toggleTheme}
        className="absolute top-4 right-4 rounded-lg p-2 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
        aria-label={theme === 'dark' ? 'Alternar para tema claro' : 'Alternar para tema escuro'}
      >
        {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
      </button>
      <div className="flex flex-col items-center gap-5">
        <img
          src={logoCamarj}
          alt="Camarj"
          className="h-44 w-44 rounded-2xl shadow-xl"
        />
        <div className="text-center">
          <h1 className="font-serif text-5xl tracking-tight">Portal de Chamados</h1>
          <p className="mt-1 text-lg text-muted-foreground">Redefinir senha</p>
        </div>
      </div>

      <Card className="w-full max-w-md border-border/60 shadow-2xl">
        <CardHeader>
          <CardTitle className="font-serif text-2xl">Nova senha</CardTitle>
          <CardDescription className="text-base">
            {sucesso
              ? 'Senha redefinida com sucesso!'
              : 'Digite sua nova senha abaixo'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {sucesso ? (
            <Button asChild className="w-full">
              <Link to="/login">Ir para o login</Link>
            </Button>
          ) : (
            <form onSubmit={onSubmit} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="nova-senha">Nova senha</Label>
                <PasswordInput
                  id="nova-senha"
                  placeholder="Mínimo 8 caracteres"
                  value={novaSenha}
                  onChange={(e) => setNovaSenha(e.target.value)}
                  required
                  autoFocus
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <Label htmlFor="confirmar-senha">Confirmar senha</Label>
                <PasswordInput
                  id="confirmar-senha"
                  placeholder="Digite novamente"
                  value={confirmarSenha}
                  onChange={(e) => setConfirmarSenha(e.target.value)}
                  required
                />
              </div>

              {erro && (
                <Alert variant="destructive">
                  <AlertDescription>{erro}</AlertDescription>
                </Alert>
              )}

              <Button type="submit" disabled={pendente} className="w-full">
                {pendente ? 'Redefinindo...' : 'Redefinir senha'}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
