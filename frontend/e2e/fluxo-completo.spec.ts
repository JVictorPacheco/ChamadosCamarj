import { test, expect } from '@playwright/test'
import { abrirChamadoPelaTela, login, PREFIXO } from './helpers'

test('fluxo completo: login -> abrir chamado -> detalhe -> comentar -> listar', async ({ page }) => {
  await login(page)

  const titulo = `${PREFIXO} Fluxo ${Date.now()}`
  await abrirChamadoPelaTela(page, titulo)
  await expect(page.getByText('Aberto', { exact: true })).toBeVisible()

  const comentario = `Comentário E2E ${Date.now()}`
  await page.getByPlaceholder('Escreva um comentário...').fill(comentario)
  await page.getByRole('button', { name: 'Comentar' }).click()
  await expect(page.getByText(comentario)).toBeVisible()

  await page.getByRole('link', { name: 'Meus Chamados' }).click()
  await page.waitForURL('**/chamados')
  await expect(page.getByText(titulo)).toBeVisible()

  await page.getByText(titulo).click()
  await page.waitForURL(/\/chamados\/[0-9a-f-]+$/)
  await expect(page.getByRole('heading', { name: titulo })).toBeVisible()
})
