import { test, expect } from '@playwright/test'
import { abrirChamadoPelaTela, login, PREFIXO } from './helpers'

test.beforeEach(async ({ page }) => {
  await login(page)
})

test('abrir chamado', async ({ page }) => {
  await abrirChamadoPelaTela(page, `${PREFIXO} Abrir ${Date.now()}`)
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
