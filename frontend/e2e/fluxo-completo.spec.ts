import { test, expect } from '@playwright/test'

test('fluxo completo: login mock -> abrir chamado -> detalhe -> comentar -> listar', async ({ page }) => {
  await page.goto('/login')
  await page.getByRole('button', { name: 'Entrar como Solicitante' }).click()
  await page.waitForURL('**/chamados')

  await page.getByRole('link', { name: 'Abrir Chamado' }).click()
  await page.waitForURL('**/chamados/novo')

  const titulo = `Chamado E2E ${Date.now()}`
  await page.locator('#titulo').fill(titulo)
  await page.locator('#descricao').fill('Descrição do chamado criado pelo teste E2E.')
  await page.locator('button:has-text("Selecione uma categoria")').click()
  await page.locator('[role="option"]').first().click()
  await page.getByRole('button', { name: 'Abrir chamado' }).click()

  await page.waitForURL(/\/chamados\/[0-9a-f-]+$/)
  await expect(page.getByRole('heading', { name: titulo })).toBeVisible()
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
