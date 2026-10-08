import { expect, type Page } from '@playwright/test'

// Credenciais NUNCA ficam no código: E2E_EMAIL / E2E_SENHA (conta Admin) vêm do ambiente.
export function credenciais() {
  const email = process.env.E2E_EMAIL
  const senha = process.env.E2E_SENHA
  if (!email || !senha) throw new Error('Defina E2E_EMAIL e E2E_SENHA (conta Admin) para rodar os testes E2E.')
  return { email, senha }
}

export const API = process.env.E2E_API ?? 'http://localhost:5000/api'

// Os testes rodam contra o banco real (dev = prod): tudo o que criam leva este prefixo, para a
// limpeza encontrar (spec correcoes-pre-deploy AC-16).
export const PREFIXO = '[TESTE-E2E]'

export async function login(page: Page) {
  const { email, senha } = credenciais()
  await page.goto('/login')
  await page.locator('#email').fill(email)
  await page.locator('#senha').fill(senha)
  await page.getByRole('button', { name: /Entrar|Login/i }).click()
  await page.waitForURL('**/chamados')
}

/** Escolhe a primeira opção de um select do formulário pelo rótulo (Área, Tipo...). */
export async function escolherPrimeiraOpcao(page: Page, rotulo: string) {
  await page.locator('div.flex.flex-col', { has: page.locator(`label:text-is("${rotulo}")`) })
    .getByRole('combobox').first().click()
  await page.getByRole('option').first().click()
}

/** Abre um chamado pela tela, preenchendo Área e Tipo, e espera o detalhe. */
export async function abrirChamadoPelaTela(page: Page, titulo: string) {
  await page.getByRole('link', { name: 'Abrir Chamado' }).click()
  await page.waitForURL('**/chamados/novo')
  await page.locator('#titulo').fill(titulo)
  await page.locator('#descricao').fill('Descrição criada pelo teste E2E.')
  await escolherPrimeiraOpcao(page, 'Área')
  await escolherPrimeiraOpcao(page, 'Tipo')
  await page.getByRole('button', { name: 'Abrir chamado' }).click()
  await page.waitForURL(/\/chamados\/[0-9a-f-]+$/)
  await expect(page.getByRole('heading', { name: titulo })).toBeVisible()
}

/** Chamada direta à API com o token da sessão logada na página. */
export async function api<T = unknown>(page: Page, method: string, path: string, body?: unknown): Promise<T> {
  const token = await page.evaluate(() => localStorage.getItem('chamados-camarj:token'))
  const resposta = await page.request.fetch(`${API}${path}`, {
    method,
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: body === undefined ? undefined : JSON.stringify(body),
  })
  expect(resposta.ok(), `${method} ${path} → ${resposta.status()}`).toBeTruthy()
  return (resposta.status() === 204 ? undefined : await resposta.json()) as T
}

/**
 * Conta de teste com SENHA ALEATÓRIA gerada a cada rodada — nunca escrita no código (o repositório é
 * público). Cria, ou reativa e redefine a senha de uma já existente, sempre no padrão do perfil.
 * Chamar `desativarContaDeTeste` no fim (review-2 R-01 de controle-de-acesso).
 */
export async function contaDeTeste(page: Page, email: string, nome: string, perfil: 'Atendente' | 'Solicitante') {
  const senha = `Tst-${crypto.randomUUID()}-Aa1!`
  const usuarios = await api<{ id: string; email: string; ativo: boolean }[]>(page, 'GET', '/usuarios')
  const existente = usuarios.find((u) => u.email === email)
  if (existente?.ativo) {
    await api(page, 'PUT', `/usuarios/${existente.id}`, { nome, perfil, ativo: true })
    await api(page, 'PATCH', `/usuarios/${existente.id}/senha`, { novaSenha: senha })
    await api(page, 'POST', `/acessos/${existente.id}/padrao`)
    return { id: existente.id, email, nome, perfil, senha }
  }
  // Nova, ou desativada (o cadastro reativa, define a senha e volta ao padrão de módulos).
  const criada = await api<{ id: string }>(page, 'POST', '/usuarios', { email, nome, perfil, senha })
  return { id: criada.id, email, nome, perfil, senha }
}

export async function desativarContaDeTeste(page: Page, conta: { id: string; nome: string; perfil: string }) {
  await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: conta.perfil, ativo: false })
}
