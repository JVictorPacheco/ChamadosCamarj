import { useState } from 'react'
import { GoogleLogin, GoogleOAuthProvider, type CredentialResponse } from '@react-oauth/google'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from './AuthContext'
import { ApiError } from '@/lib/api'
import logoCamarj from '../assets/logo-camarj.png'

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? ''

export function LoginPage() {
  const { loginComGoogle } = useAuth()
  const [erro, setErro] = useState<string | null>(null)

  const onSuccess = async (credentialResponse: CredentialResponse) => {
    setErro(null)

    if (!credentialResponse.credential) {
      setErro('Não foi possível entrar com Google. Tente novamente.')
      return
    }

    try {
      await loginComGoogle(credentialResponse.credential)
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        setErro('E-mail não cadastrado — peça a um Admin para te cadastrar.')
      } else if (err instanceof ApiError && err.status === 401) {
        setErro('Conta Google não pertence ao domínio camarj.com.br.')
      } else {
        setErro(err instanceof Error ? err.message : 'Ocorreu um erro inesperado.')
      }
    }
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
            Use sua conta @camarj.com.br do Google Workspace
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col items-center gap-4">
          <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
            <GoogleLogin
              theme="filled_black"
              size="large"
              hosted_domain="camarj.com.br"
              onSuccess={onSuccess}
              onError={() => setErro('Não foi possível entrar com Google. Tente novamente.')}
            />
          </GoogleOAuthProvider>

          {erro && (
            <Alert variant="destructive">
              <AlertDescription>{erro}</AlertDescription>
            </Alert>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
