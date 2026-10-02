import { test, expect, type Page } from '@playwright/test'
import { api, login, PREFIXO } from './helpers'

// Edição simultânea — spec correcoes-pre-deploy AC-10, AC-12, AC-14. "Outra pessoa" é simulada por
// uma chamada direta à API (sem If-Match) depois que a tela já mostrou o chamado.
const MENSAGEM = 'Outra pessoa alterou este chamado. Os dados foram atualizados; confira e refaça a ação.'

async function criarChamado(page: Page, titulo: string) {
  const [area] = await api<{ id: string }[]>(page, 'GET', '/grupos')
  const [tipo] = await api<{ id: string }[]>(page, 'GET', '/tipos')
  return api<{ id: string }>(page, 'POST', '/chamados', {
    titulo, descricao: 'Teste E2E de edição simultânea.', areaId: area.id, tipoId: tipo.id, prioridade: 'Baixa',
  })
}

test.beforeEach(async ({ page }) => {
  await login(page)
})

test('detalhe: ação sobre versão desatualizada avisa, recarrega e a nova tentativa funciona', async ({ page }) => {
  const chamado = await criarChamado(page, `${PREFIXO} Conflito detalhe ${Date.now()}`)
  await page.goto(`/chamados/${chamado.id}`)
  await expect(page.getByRole('button', { name: 'Assumir' })).toBeVisible()

  // Outra pessoa muda a prioridade depois que a tela carregou.
  await api(page, 'PATCH', `/chamados/${chamado.id}/prioridade`, { novaPrioridade: 'Alta' })

  await page.getByRole('button', { name: 'Assumir' }).click()
  await expect(page.getByText(MENSAGEM)).toBeVisible() // AC-10
  await expect(page.getByText('Alta', { exact: true }).first()).toBeVisible() // dados recarregados

  // AC-12: com os dados atualizados, a mesma ação funciona.
  await page.getByRole('button', { name: 'Assumir' }).click()
  await expect(page.getByText('Em andamento', { exact: true }).first()).toBeVisible()
})

test('kanban: mover cartão desatualizado avisa e o cartão volta à coluna', async ({ page }) => {
  const titulo = `${PREFIXO} Conflito kanban ${Date.now()}`
  const chamado = await criarChamado(page, titulo)
  await page.goto('/atendimento/kanban')
  const cartao = page.getByText(titulo)
  await expect(cartao).toBeVisible()

  await api(page, 'PATCH', `/chamados/${chamado.id}/prioridade`, { novaPrioridade: 'Alta' })

  const origem = await cartao.boundingBox()
  const destino = await page.getByRole('heading', { name: 'Em Andamento', exact: true }).boundingBox()
  if (!origem || !destino) throw new Error('cartão ou coluna não encontrados')
  await page.mouse.move(origem.x + origem.width / 2, origem.y + origem.height / 2)
  await page.mouse.down()
  await page.mouse.move(origem.x + origem.width / 2 + 20, origem.y + origem.height / 2, { steps: 5 })
  await page.mouse.move(destino.x + destino.width / 2, destino.y + 60, { steps: 20 })
  await page.mouse.up()

  await expect(page.getByText(MENSAGEM)).toBeVisible() // AC-14
  // O cartão continua na coluna Aberto: o status real não mudou.
  const atual = await api<{ status: string }>(page, 'GET', `/chamados/${chamado.id}`)
  expect(atual.status).toBe('Aberto')
})
