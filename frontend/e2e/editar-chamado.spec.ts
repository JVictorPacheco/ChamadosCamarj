import { test, expect, type Page } from '@playwright/test'
import { api, login, PREFIXO } from './helpers'

// Editar chamado — spec editar-chamado (AC-12, AC-15, AC-17, AC-10, AC-19, AC-20, AC-25).
const MENSAGEM_CONFLITO = 'Outra pessoa alterou este chamado. Os dados foram atualizados; confira e refaça a ação.'

async function criarChamado(page: Page, titulo: string) {
  const [area] = await api<{ id: string }[]>(page, 'GET', '/grupos')
  const [tipo] = await api<{ id: string }[]>(page, 'GET', '/tipos')
  return api<{ id: string }>(page, 'POST', '/chamados', {
    titulo, descricao: 'Descrição original do teste E2E.', areaId: area.id, tipoId: tipo.id,
  })
}

test.beforeEach(async ({ page }) => {
  await login(page)
})

test('Admin edita título e descrição e o histórico mostra antes e depois', async ({ page }) => {
  const titulo = `${PREFIXO} Editar ${Date.now()}`
  const chamado = await criarChamado(page, titulo)
  await page.goto(`/chamados/${chamado.id}`)

  await page.getByRole('button', { name: 'Editar' }).click()
  const dialogo = page.getByRole('dialog')
  await expect(dialogo.getByLabel('Título')).toHaveValue(titulo) // AC-12
  await dialogo.getByLabel('Título').fill(`${titulo} (corrigido)`)
  await dialogo.getByLabel('Descrição').fill('Descrição corrigida pelo teste E2E.')
  await dialogo.getByRole('button', { name: 'Salvar' }).click()

  await expect(dialogo).toBeHidden() // AC-15
  await expect(page.getByRole('heading', { name: `${titulo} (corrigido)` })).toBeVisible()
  await expect(page.getByText('Chamado editado')).toBeVisible() // AC-17
  await expect(page.getByText('Descrição corrigida pelo teste E2E.').first()).toBeVisible()
})

test('chamado encerrado não mostra o botão Editar', async ({ page }) => {
  const chamado = await criarChamado(page, `${PREFIXO} Encerrado ${Date.now()}`)
  await api(page, 'PATCH', `/chamados/${chamado.id}/atribuir`)
  await api(page, 'PATCH', `/chamados/${chamado.id}/resolver`)

  await page.goto(`/chamados/${chamado.id}`)
  await expect(page.getByText('Resolvido', { exact: true }).first()).toBeVisible()
  await expect(page.getByRole('button', { name: 'Editar' })).toHaveCount(0) // AC-10
})

test('conflito mantém o texto digitado no modal e a segunda tentativa grava', async ({ page }) => {
  const titulo = `${PREFIXO} Conflito edição ${Date.now()}`
  const chamado = await criarChamado(page, titulo)
  await page.goto(`/chamados/${chamado.id}`)

  await page.getByRole('button', { name: 'Editar' }).click()
  const dialogo = page.getByRole('dialog')
  await dialogo.getByLabel('Título').fill(`${titulo} (meu texto)`)

  // Outra pessoa muda o título com o modal aberto.
  await api(page, 'PUT', `/chamados/${chamado.id}`, { titulo: `${titulo} (da outra pessoa)`, descricao: 'Descrição original do teste E2E.' })

  await dialogo.getByRole('button', { name: 'Salvar' }).click()
  await expect(dialogo.getByText(MENSAGEM_CONFLITO)).toBeVisible() // AC-19
  await expect(dialogo.getByLabel('Título')).toHaveValue(`${titulo} (meu texto)`)
  await expect(dialogo.getByText('Texto atual no chamado')).toBeVisible()
  await expect(dialogo.getByText(`${titulo} (da outra pessoa)`)).toBeVisible()

  await dialogo.getByRole('button', { name: 'Salvar' }).click() // AC-20
  await expect(dialogo).toBeHidden()
  await expect(page.getByRole('heading', { name: `${titulo} (meu texto)` })).toBeVisible()
})
