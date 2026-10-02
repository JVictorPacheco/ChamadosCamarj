import { test, expect } from '@playwright/test'
import { login } from './helpers'

test.beforeEach(async ({ page }) => {
  await login(page)
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