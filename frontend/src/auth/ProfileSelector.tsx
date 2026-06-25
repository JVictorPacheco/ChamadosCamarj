import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useAuth, type TipoPerfil } from './AuthContext'
import logoCamarj from '../assets/logo-camarj.png'

const PERFIS: { tipo: TipoPerfil; descricao: string }[] = [
  { tipo: 'Admin', descricao: 'Acesso administrativo completo' },
  { tipo: 'Atendente', descricao: 'Atende e resolve chamados' },
  { tipo: 'Solicitante', descricao: 'Abre e acompanha chamados' },
]

export function ProfileSelector() {
  const { login } = useAuth()

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-8 p-6">
      <div className="flex flex-col items-center gap-3">
        <img src={logoCamarj} alt="Camarj" className="h-24 w-24 rounded-2xl shadow-lg" />
        <div className="text-center">
          <h1 className="text-2xl font-heading">Portal de Chamados</h1>
          <p className="text-sm text-muted-foreground">Selecione um perfil para entrar</p>
        </div>
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        {PERFIS.map(({ tipo, descricao }) => (
          <Card key={tipo} className="w-64">
            <CardHeader>
              <CardTitle>{tipo}</CardTitle>
              <CardDescription>{descricao}</CardDescription>
            </CardHeader>
            <CardContent>
              <Button className="w-full" onClick={() => login(tipo)}>
                Entrar como {tipo}
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
