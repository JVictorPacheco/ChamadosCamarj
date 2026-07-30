import { test, expect } from '@playwright/test'

test.beforeEach(async ({ page }) => {
  await page.goto('/login')
  await page.locator('#email').fill('suporte@camarj.com.br')
  await page.locator('#senha').fill('Akira.321')
  await page.getByRole('button', { name: /Entrar|Login/i }).click()
  await page.waitForURL('**/chamados')
})

test('pagina de usuarios - acesso admin', async ({ page }) => {
  await page.goto('/admin/usuarios')
  await page.waitForURL('**/admin/usuarios')
  await expect(page.getByText(/Usuários/i).first()).toBeVisible()
})

test('pagina de categorias - acesso admin', async ({ page }) => {
  await page.goto('/admin/categorias')
  await expect(page.getByRole('heading', { name: /Categorias/i })).toBeVisible()
})

test('pagina de grupos - acesso admin', async ({ page }) => {
  await page.goto('/admin/grupos')
  await expect(page.getByRole('heading', { name: /Grupos/i })).toBeVisible()
})