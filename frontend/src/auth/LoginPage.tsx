import { useState, type FormEvent } from 'react'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAuth } from './AuthContext'
import { ApiError } from '@/lib/api'
import logoCamarj from '../assets/logo-camarj.png'

export function LoginPage() {
  const { login } = useAuth()
  const [email, setEmail] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [isPending, setIsPending] = useState(false)

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setErro(null)
    setIsPending(true)

    try {
      await login(email)
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setErro('E-mail não cadastrado — peça a um Admin para te cadastrar.')
      } else {
        setErro(err instanceof Error ? err.message : 'Ocorreu um erro inesperado.')
      }
    } finally {
      setIsPending(false)
    }
  }

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-8 p-6">
      <div className="flex flex-col items-center gap-3">
        <img src={logoCamarj} alt="Camarj" className="h-24 w-24 rounded-2xl shadow-lg" />
        <div className="text-center">
          <h1 className="text-2xl font-heading">Portal de Chamados</h1>
          <p className="text-sm text-muted-foreground">Entre com seu e-mail cadastrado</p>
        </div>
      </div>

      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Entrar</CardTitle>
          <CardDescription>Informe o e-mail cadastrado por um administrador</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="voce@camarj.com.br"
                required
              />
            </div>

            {erro && <p className="text-sm text-destructive">{erro}</p>}

            <Button type="submit" className="w-full" disabled={isPending || !email}>
              {isPending ? 'Entrando...' : 'Entrar'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
