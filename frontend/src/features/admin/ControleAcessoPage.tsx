import { useMemo, useState } from 'react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Checkbox } from '@/components/ui/checkbox'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAcessos, useAcessoUsuario, useSalvarAcessos, useVoltarAoPadrao } from './hooks/useAcessos'
import type { AcessoUsuarioDetalheResponse, AcessoUsuarioResumoResponse, ChatPerfil, ModuloSistema } from '@/types/api'

const NOME_MODULO: Record<ModuloSistema, string> = {
  Arquivo: 'Arquivo',
  Kanban: 'Kanban',
  Fila: 'Fila',
  Dashboard: 'Dashboard',
  RelatorioMensal: 'Relatório mensal',
}

const OPCOES_CHAT: { value: ChatPerfil; label: string }[] = [
  { value: 'SemAcesso', label: 'Sem acesso' },
  { value: 'Participante', label: 'Participante' },
  { value: 'CriadorDeGrupo', label: 'Pode criar grupos' },
]

/**
 * Controle de acesso por módulo (spec controle-de-acesso). Só Admin (a rota já exige; o servidor também).
 * Cada pessoa herda o padrão do perfil; aqui o Admin liga/desliga módulos só dela e ajusta o Chat.
 */
export function ControleAcessoPage() {
  const { data: pessoas, isPending, isError } = useAcessos()
  const [busca, setBusca] = useState('')
  const [selecionada, setSelecionada] = useState<AcessoUsuarioResumoResponse | null>(null)

  const filtradas = useMemo(() => {
    const termo = busca.trim().toLowerCase()
    return (pessoas ?? []).filter((p) => !termo || p.nome.toLowerCase().includes(termo) || p.email.toLowerCase().includes(termo))
  }, [pessoas, busca])

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex flex-col gap-1">
        <h1 className="text-xl font-heading">Controle de acesso</h1>
        <p className="text-sm text-muted-foreground">
          Cada pessoa começa com o padrão do perfil. Ajuste aqui os módulos de quem precisa de mais ou de menos.
        </p>
      </div>

      <Input
        placeholder="Buscar por nome ou e-mail"
        value={busca}
        onChange={(e) => setBusca(e.target.value)}
        className="max-w-sm"
        aria-label="Buscar pessoa"
      />

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Não foi possível carregar os acessos. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}
      {isPending && <p className="text-sm text-muted-foreground">Carregando...</p>}

      {!isPending && pessoas && (
        <div className="rounded-lg border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Perfil</TableHead>
                <TableHead>Módulos</TableHead>
                <TableHead>Chat</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtradas.map((p) => (
                <TableRow key={p.id}>
                  <TableCell>
                    <div className="font-medium">{p.nome}</div>
                    <div className="text-xs text-muted-foreground">{p.email}</div>
                  </TableCell>
                  <TableCell>
                    {p.perfil}
                    {!p.ativo && <Badge variant="secondary" className="ml-2">Inativo</Badge>}
                  </TableCell>
                  <TableCell>
                    {p.acessoTotal ? (
                      <Badge>Admin — acesso total</Badge>
                    ) : (
                      <div className="flex flex-wrap items-center gap-1">
                        {p.modulos.map((m) => (
                          <Badge key={m} variant="outline">{NOME_MODULO[m]}</Badge>
                        ))}
                        {p.modulos.length === 0 && <span className="text-xs text-muted-foreground">Só abrir e acompanhar chamados</span>}
                        {p.temAjuste && <Badge variant="secondary">ajustado</Badge>}
                      </div>
                    )}
                  </TableCell>
                  <TableCell>{OPCOES_CHAT.find((o) => o.value === p.chatPerfil)?.label}</TableCell>
                  <TableCell className="text-right">
                    {!p.acessoTotal && (
                      <Button variant="outline" size="sm" onClick={() => setSelecionada(p)}>
                        Ajustar acessos
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {filtradas.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="py-6 text-center text-sm text-muted-foreground">
                    Ninguém encontrado.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      )}

      {selecionada && <PainelAcessos pessoa={selecionada} onClose={() => setSelecionada(null)} />}
    </div>
  )
}

function PainelAcessos({ pessoa, onClose }: { pessoa: AcessoUsuarioResumoResponse; onClose: () => void }) {
  const { data: detalhe, isPending, isError } = useAcessoUsuario(pessoa.id)

  return (
    <Dialog open onOpenChange={(aberto) => { if (!aberto) onClose() }}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Acessos de {pessoa.nome}</DialogTitle>
          <DialogDescription>
            Perfil {pessoa.perfil}. Abrir chamado e Meus chamados são sempre liberados.
          </DialogDescription>
        </DialogHeader>
        {isPending && <p className="text-sm text-muted-foreground">Carregando...</p>}
        {isError && (
          <Alert variant="destructive">
            <AlertDescription>Não foi possível carregar os acessos desta pessoa.</AlertDescription>
          </Alert>
        )}
        {detalhe && <FormularioAcessos detalhe={detalhe} onClose={onClose} />}
      </DialogContent>
    </Dialog>
  )
}

function FormularioAcessos({ detalhe, onClose }: { detalhe: AcessoUsuarioDetalheResponse; onClose: () => void }) {
  const [marcados, setMarcados] = useState<ModuloSistema[]>(detalhe.modulos.filter((m) => m.efetivo).map((m) => m.modulo))
  const [chat, setChat] = useState<ChatPerfil>(detalhe.chatPerfil)
  const salvar = useSalvarAcessos(detalhe.id)
  const voltar = useVoltarAoPadrao(detalhe.id)
  const ocupado = salvar.isPending || voltar.isPending
  const erro = salvar.error ?? voltar.error
  const temAjuste = detalhe.modulos.some((m) => m.efetivo !== m.padrao)

  const alternar = (modulo: ModuloSistema, ligado: boolean) =>
    setMarcados((atual) => (ligado ? [...atual, modulo] : atual.filter((m) => m !== modulo)))

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-2">
        <p className="text-sm font-medium">Módulos</p>
        {detalhe.modulos.map((m) => {
          const ligado = marcados.includes(m.modulo)
          return (
            <div key={m.modulo} className="flex items-center gap-3">
              <Checkbox
                id={`modulo-${m.modulo}`}
                checked={ligado}
                disabled={!m.ajustavel || ocupado}
                onCheckedChange={(v) => alternar(m.modulo, v === true)}
              />
              <Label htmlFor={`modulo-${m.modulo}`} className="font-normal">{NOME_MODULO[m.modulo]}</Label>
              <span className="text-xs text-muted-foreground">
                {ligado === m.padrao ? 'padrão do perfil' : 'ajustado'}
              </span>
            </div>
          )
        })}
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="acesso-chat">Chat</Label>
        <Select value={chat} onValueChange={(v) => setChat(v as ChatPerfil)} disabled={ocupado}>
          <SelectTrigger id="acesso-chat" className="max-w-xs">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {OPCOES_CHAT.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {erro && (
        <Alert variant="destructive">
          <AlertDescription>{erro.message}</AlertDescription>
        </Alert>
      )}

      <div className="flex flex-col gap-1">
        <p className="text-sm font-medium">Histórico de mudanças</p>
        {detalhe.auditoria.length === 0 ? (
          <p className="text-xs text-muted-foreground">Nenhuma mudança de acesso registrada.</p>
        ) : (
          <ul className="max-h-40 overflow-y-auto text-xs text-muted-foreground">
            {detalhe.auditoria.map((a, i) => (
              <li key={i}>
                {new Date(a.dataHora).toLocaleString('pt-BR')} — {a.alteradoPorNome}: {a.item} {a.anterior} → {a.novo}
              </li>
            ))}
          </ul>
        )}
      </div>

      <DialogFooter className="gap-2 sm:justify-between">
        <Button variant="outline" disabled={!temAjuste || ocupado} onClick={() => voltar.mutate(undefined, { onSuccess: onClose })}>
          Voltar ao padrão do perfil
        </Button>
        <div className="flex gap-2">
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button disabled={ocupado} onClick={() => salvar.mutate({ modulos: marcados, chatPerfil: chat }, { onSuccess: onClose })}>
            {salvar.isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </div>
      </DialogFooter>
    </div>
  )
}
