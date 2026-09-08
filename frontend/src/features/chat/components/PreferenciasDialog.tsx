import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'

interface PreferenciasDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

// AC-55 a AC-58: primeira entrada de um menu de preferências do usuário — hoje só tem o toggle de
// confirmação de leitura, mas o dialog já fica pronto pra crescer com outras preferências futuras.
export function PreferenciasDialog({ open, onOpenChange }: PreferenciasDialogProps) {
  const { perfil, atualizarPreferenciaLeitura } = useAuth()
  const [pendente, setPendente] = useState(false)
  const [erro, setErro] = useState<string | null>(null)

  const onToggle = async (mostrar: boolean) => {
    setErro(null)
    setPendente(true)
    try {
      await atualizarPreferenciaLeitura(mostrar)
    } catch (err) {
      setErro(err instanceof Error ? err.message : 'Não foi possível salvar a preferência.')
    } finally {
      setPendente(false)
    }
  }

  // CONVENTIONS.md 3.6: resetar estados relacionados ao fechar — sem isso, um erro de uma tentativa
  // anterior (ex: falha de rede) ficava exibido ao reabrir o dialog, antes de qualquer nova tentativa.
  const aoMudarAbertura = (novoOpen: boolean) => {
    if (!novoOpen) setErro(null)
    onOpenChange(novoOpen)
  }

  return (
    <Dialog open={open} onOpenChange={aoMudarAbertura}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Preferências</DialogTitle>
          <DialogDescription>Configurações pessoais, salvas no seu perfil.</DialogDescription>
        </DialogHeader>

        <div className="flex items-center justify-between gap-4 py-2">
          <div className="space-y-1">
            <Label htmlFor="preferencia-leitura">Mostrar confirmação de leitura</Label>
            <p className="text-sm text-muted-foreground">
              Exibe "Visto" nas suas mensagens do chat. Se desligar, você também deixa de ver quando
              os outros leem as suas.
            </p>
          </div>
          <Switch
            id="preferencia-leitura"
            checked={perfil?.mostrarConfirmacaoLeitura ?? true}
            disabled={pendente}
            onCheckedChange={onToggle}
          />
        </div>

        {erro && (
          <Alert variant="destructive">
            <AlertDescription>{erro}</AlertDescription>
          </Alert>
        )}
      </DialogContent>
    </Dialog>
  )
}
