import { test, expect } from '@playwright/test'
import { api, login } from './helpers'

// Controle de acesso — spec controle-de-acesso (AC-01..AC-04, AC-11, AC-12, AC-16, AC-19).
// Usa uma conta de teste (teste.acesso.e2e) criada pelo Admin; a limpeza é feita com OK do usuário.
const EMAIL = 'teste.acesso.e2e@camarj.com.br'
const SENHA = 'TesteAcesso#2026'

test('Admin tira o Relatório de um Atendente: some do menu na hora, a rota bloqueia e voltar ao padrão devolve', async ({ page, browser }) => {
  await login(page)

  // Conta de teste (Atendente), criada se ainda não existir.
  const usuarios = await api<{ id: string; email: string }[]>(page, 'GET', '/usuarios')
  let alvo = usuarios.find((u) => u.email === EMAIL)
  if (!alvo) alvo = await api<{ id: string; email: string }>(page, 'POST', '/usuarios', { email: EMAIL, nome: 'Teste Acesso E2E', perfil: 'Atendente', senha: SENHA })
  await api(page, 'POST', `/acessos/${alvo.id}/padrao`)

  // A pessoa entra em outro navegador e vê o Relatório (padrão do Atendente — AC-16).
  const contexto = await browser.newContext()
  const pessoa = await contexto.newPage()
  await pessoa.goto('/login')
  await pessoa.locator('#email').fill(EMAIL)
  await pessoa.locator('#senha').fill(SENHA)
  await pessoa.getByRole('button', { name: /Entrar|Login/i }).click()
  await pessoa.waitForURL('**/chamados')
  await pessoa.goto('/atendimento/relatorio-mensal')
  await expect(pessoa.getByRole('link', { name: 'Relatório Mensal' })).toBeVisible()

  // Admin tira o Relatório pela tela de Controle de acesso (AC-01..AC-03).
  await page.goto('/admin/acessos')
  await page.getByLabel('Buscar pessoa').fill('teste.acesso.e2e')
  await page.getByRole('row', { name: /Teste Acesso E2E/ }).getByRole('button', { name: 'Ajustar acessos' }).click()
  const dialogo = page.getByRole('dialog')
  await dialogo.getByLabel('Relatório mensal').click()
  await dialogo.getByRole('button', { name: 'Salvar' }).click()
  await expect(dialogo).toBeHidden()

  // Na hora, sem sair e entrar: a pessoa é tirada da tela, vê o aviso e o item some do menu (AC-11).
  await expect(pessoa.getByText('Seu acesso a este módulo foi retirado.')).toBeVisible({ timeout: 15000 })
  await expect(pessoa).toHaveURL(/\/chamados$/)
  await expect(pessoa.getByRole('link', { name: 'Relatório Mensal' })).toHaveCount(0)
  await expect(pessoa.getByRole('link', { name: 'Dashboard' })).toBeVisible()

  // Pelo endereço também não entra (AC-12).
  await pessoa.goto('/atendimento/relatorio-mensal')
  await expect(pessoa).toHaveURL(/\/chamados$/)

  // Histórico de mudanças no painel e voltar ao padrão (AC-04, AC-14).
  await page.getByRole('row', { name: /Teste Acesso E2E/ }).getByRole('button', { name: 'Ajustar acessos' }).click()
  // .first(): rodadas anteriores com a mesma conta de teste deixam linhas iguais no histórico.
  await expect(dialogo.getByText(/Relatório mensal ligado → desligado/).first()).toBeVisible()
  await dialogo.getByRole('button', { name: 'Voltar ao padrão do perfil' }).click()
  await expect(dialogo).toBeHidden()
  await expect(pessoa.getByRole('link', { name: 'Relatório Mensal' })).toBeVisible({ timeout: 15000 })

  await contexto.close()
})

test('Atendente ou Solicitante não entra no Controle de acesso pelo endereço', async ({ page }) => {
  await login(page)
  const usuarios = await api<{ id: string; email: string }[]>(page, 'GET', '/usuarios')
  if (!usuarios.some((u) => u.email === EMAIL))
    await api(page, 'POST', '/usuarios', { email: EMAIL, nome: 'Teste Acesso E2E', perfil: 'Atendente', senha: SENHA })

  await page.evaluate(() => localStorage.clear())
  await page.goto('/login')
  await page.locator('#email').fill(EMAIL)
  await page.locator('#senha').fill(SENHA)
  await page.getByRole('button', { name: /Entrar|Login/i }).click()
  await page.waitForURL('**/chamados')

  await page.goto('/admin/acessos') // AC-05
  await expect(page).toHaveURL(/\/chamados$/)
  await expect(page.getByText('Você não tem permissão para acessar esta área.')).toBeVisible()
  await expect(page.getByRole('link', { name: 'Controle de acesso' })).toHaveCount(0)
})

// review R-01: o Solicitante que GANHA o Dashboard abre a tela de verdade (antes a página o barrava pelo
// perfil); e um módulo que ele nunca teve dá "sem permissão", não "retirado" (review R-04).
test('Solicitante com Dashboard dado pelo Admin abre a tela; Kanban continua sem permissão', async ({ page, browser }) => {
  const EMAIL_SOL = 'teste.acesso.sol.e2e@camarj.com.br'
  await login(page)
  const usuarios = await api<{ id: string; email: string }[]>(page, 'GET', '/usuarios')
  let sol = usuarios.find((u) => u.email === EMAIL_SOL)
  if (!sol) sol = await api<{ id: string; email: string }>(page, 'POST', '/usuarios', { email: EMAIL_SOL, nome: 'Teste Acesso Sol E2E', perfil: 'Solicitante', senha: SENHA })
  await api(page, 'PUT', `/acessos/${sol.id}`, { modulos: ['Arquivo', 'Dashboard'], chatPerfil: 'SemAcesso' })

  const contexto = await browser.newContext()
  const pessoa = await contexto.newPage()
  await pessoa.goto('/login')
  await pessoa.locator('#email').fill(EMAIL_SOL)
  await pessoa.locator('#senha').fill(SENHA)
  await pessoa.getByRole('button', { name: /Entrar|Login/i }).click()
  await pessoa.waitForURL('**/chamados')

  await pessoa.getByRole('link', { name: 'Dashboard' }).click()
  await expect(pessoa).toHaveURL(/\/atendimento\/dashboard$/)
  await expect(pessoa.getByText('Esta área não está disponível para o seu perfil.')).toHaveCount(0)
  await expect(pessoa.getByRole('heading', { name: /Dashboard/i })).toBeVisible()

  await pessoa.goto('/atendimento/kanban')
  await expect(pessoa).toHaveURL(/\/chamados$/)
  await expect(pessoa.getByText('Você não tem permissão para acessar este módulo.')).toBeVisible()

  await api(page, 'POST', `/acessos/${sol.id}/padrao`)
  await contexto.close()
})
