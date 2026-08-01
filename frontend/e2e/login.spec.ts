import { test, expect } from '@playwright/test'

test('login com email e senha - sucesso', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByRole('heading', { name: /Portal de Chamados|Entrar/i })).toBeVisible()

  await page.locator('#email').fill('suporte@camarj.com.br')
  await page.locator('#senha').fill('Akira.321')
  await page.getByRole('button', { name: /Entrar|Login/i }).click()

  await page.waitForURL('**/chamados')
  await expect(page.getByRole('link', { name: 'Meus Chamados' })).toBeVisible()
})

test('login com credenciais invalidas - erro', async ({ page }) => {
  await page.goto('/login')
  await page.locator('#email').fill('invalido@camarj.com.br')
  await page.locator('#senha').fill('senhaerrada')
  await page.getByRole('button', { name: /Entrar|Login/i }).click()

  await expect(page.getByText(/inválidos|inválido|erro/i)).toBeVisible()
})