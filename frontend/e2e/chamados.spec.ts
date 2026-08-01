import { test, expect } from '@playwright/test'

test.beforeEach(async ({ page }) => {
  await page.goto('/login')
  await page.locator('#email').fill('suporte@camarj.com.br')
  await page.locator('#senha').fill('Akira.321')
  await page.getByRole('button', { name: /Entrar|Login/i }).click()
  await page.waitForURL('**/chamados')
})

test('abrir chamado', async ({ page }) => {
  await page.getByRole('link', { name: 'Abrir Chamado' }).click()
  await page.waitForURL('**/chamados/novo')

  const titulo = `Teste E2E ${Date.now()}`
  await page.locator('#titulo').fill(titulo)
  await page.locator('#descricao').fill('Descrição criada pelo teste E2E.')
  await page.locator('button:has-text("Selecione uma categoria")').click()
  await page.locator('[role="option"]').first().click()
  await page.getByRole('button', { name: 'Abrir chamado' }).click()

  await page.waitForURL(/\/chamados\/[0-9a-f-]+$/)
  await expect(page.getByRole('heading', { name: titulo })).toBeVisible()
  await expect(page.getByText('Aberto', { exact: true })).toBeVisible()
})

test('listar chamados', async ({ page }) => {
  await expect(page.getByText('Meus Chamados')).toBeVisible()
})

test('arquivo de chamados finalizados', async ({ page }) => {
  await page.getByRole('link', { name: 'Arquivo' }).click()
  await page.waitForURL('**/chamados/arquivo')
  await expect(page.getByText('Arquivo')).toBeVisible()
})