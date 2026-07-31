import { test, expect } from '@playwright/test'

test.beforeEach(async ({ page }) => {
  await page.goto('/login')
  await page.locator('#email').fill('suporte@camarj.com.br')
  await page.locator('#senha').fill('Akira.321')
  await page.getByRole('button', { name: /Entrar|Login/i }).click()
  await page.waitForURL('**/chamados')
})

test('dashboard carrega metricas', async ({ page }) => {
  await page.getByRole('link', { name: 'Dashboard' }).click()
  await page.waitForURL('**/dashboard')
  await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible()
})

test('fila de atendimento', async ({ page }) => {
  await page.getByRole('link', { name: 'Fila' }).click()
  await page.waitForURL('**/fila')
  await expect(page.getByRole('heading', { name: /Fila/i })).toBeVisible()
})

test('relatorio mensal', async ({ page }) => {
  await page.getByRole('link', { name: /Relatório/i }).click()
  await page.waitForURL('**/relatorio-mensal')
  await expect(page.getByRole('heading', { name: /Relatório/i })).toBeVisible()
})