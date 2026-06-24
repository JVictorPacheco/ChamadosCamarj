import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useAuth, type TipoPerfil } from './AuthContext'

const PERFIS: { tipo: TipoPerfil; descricao: string }[] = [
  { tipo: 'Admin', descricao: 'Acesso administrativo completo' },
  { tipo: 'Atendente', descricao: 'Atende e resolve chamados' },
  { tipo: 'Solicitante', descricao: 'Abre e acompanha chamados' },
]

export function ProfileSelector() {
  const { login } = useAuth()

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-6 p-6">
      <h1 className="text-2xl font-heading">Selecione um perfil</h1>
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
