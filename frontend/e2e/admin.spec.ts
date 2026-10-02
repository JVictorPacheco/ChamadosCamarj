import { test, expect } from '@playwright/test'
import { login } from './helpers'

test.beforeEach(async ({ page }) => {
  await login(page)
})

test('pagina de usuarios - acesso admin', async ({ page }) => {
  await page.goto('/admin/usuarios')
  await page.waitForURL('**/admin/usuarios')
  await expect(page.getByText(/Usuários/i).first()).toBeVisible()
})

test('pagina de tipos de chamado - acesso admin', async ({ page }) => {
  await page.goto('/admin/tipos')
  await expect(page.getByRole('heading', { name: /Tipos de chamado/i })).toBeVisible()
})

test('pagina de grupos - acesso admin', async ({ page }) => {
  await page.goto('/admin/grupos')
  await expect(page.getByRole('heading', { name: /Grupos/i })).toBeVisible()
})