import { test, expect, type Browser, type Page } from '@playwright/test'
import { api, contaDeTeste, credenciais, desativarContaDeTeste, login } from './helpers'

// Controle de acesso — spec controle-de-acesso (AC-01..AC-05, AC-07, AC-11, AC-12, AC-16, AC-19).
// Contas de teste com senha aleatória por rodada (nunca no código) e desativadas no fim (review-2 R-01).

async function entrarComo(browser: Browser, conta: { email: string; senha: string }) {
  const contexto = await browser.newContext()
  const pessoa = await contexto.newPage()
  await pessoa.goto('/login')
  await pessoa.locator('#email').fill(conta.email)
  await pessoa.locator('#senha').fill(conta.senha)
  await pessoa.getByRole('button', { name: /Entrar|Login/i }).click()
  await pessoa.waitForURL('**/chamados')
  return { contexto, pessoa }
}

async function atendenteDeTeste(page: Page) {
  return contaDeTeste(page, 'teste.acesso.e2e@camarj.com.br', 'Teste Acesso E2E', 'Atendente')
}

test('Admin tira o Relatório de um Atendente: some do menu na hora, a rota bloqueia e voltar ao padrão devolve', async ({ page, browser }) => {
  await login(page)
  const conta = await atendenteDeTeste(page)
  const { contexto, pessoa } = await entrarComo(browser, conta)

  try {
    // A pessoa vê o Relatório (padrão do Atendente — AC-16).
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

    // Na hora, sem sair e entrar: tirada da tela, com o aviso, e o item some do menu (AC-11).
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
  } finally {
    await contexto.close()
    await desativarContaDeTeste(page, conta)
  }
})

test('Atendente não entra no Controle de acesso pelo endereço', async ({ page, browser }) => {
  await login(page)
  const conta = await atendenteDeTeste(page)
  const { contexto, pessoa } = await entrarComo(browser, conta)

  try {
    await pessoa.goto('/admin/acessos') // AC-05
    await expect(pessoa).toHaveURL(/\/chamados$/)
    await expect(pessoa.getByText('Você não tem permissão para acessar esta área.')).toBeVisible()
    await expect(pessoa.getByRole('link', { name: 'Controle de acesso' })).toHaveCount(0)
  } finally {
    await contexto.close()
    await desativarContaDeTeste(page, conta)
  }
})

// review R-01: o Solicitante que GANHA o Dashboard abre a tela de verdade (antes a página o barrava pelo
// perfil); e um módulo que ele nunca teve dá "sem permissão", não "retirado" (review R-04).
test('Solicitante com Dashboard dado pelo Admin abre a tela; Kanban continua sem permissão', async ({ page, browser }) => {
  await login(page)
  const conta = await contaDeTeste(page, 'teste.acesso.sol.e2e@camarj.com.br', 'Teste Acesso Sol E2E', 'Solicitante')
  await api(page, 'PUT', `/acessos/${conta.id}`, { modulos: ['Arquivo', 'Dashboard'], chatPerfil: 'SemAcesso' })
  const { contexto, pessoa } = await entrarComo(browser, conta)

  try {
    await pessoa.getByRole('link', { name: 'Dashboard' }).click()
    await expect(pessoa).toHaveURL(/\/atendimento\/dashboard$/)
    await expect(pessoa.getByText('Esta área não está disponível para o seu perfil.')).toHaveCount(0)
    await expect(pessoa.getByRole('heading', { name: /Dashboard/i })).toBeVisible()

    await pessoa.goto('/atendimento/kanban')
    await expect(pessoa).toHaveURL(/\/chamados$/)
    await expect(pessoa.getByText('Você não tem permissão para acessar este módulo.')).toBeVisible()
  } finally {
    await contexto.close()
    await api(page, 'POST', `/acessos/${conta.id}/padrao`)
    await desativarContaDeTeste(page, conta)
  }
})

// AC-15 + review-2 R-03 (decisão de 2026-10-05): perfil mudado com o sistema aberto → a pessoa sai na
// hora e, ao entrar de novo, já vem com o perfil novo.
test('Admin muda o perfil de quem está com o sistema aberto: a pessoa sai com aviso e volta no perfil novo', async ({ page, browser }) => {
  await login(page)
  const conta = await atendenteDeTeste(page)
  const { contexto, pessoa } = await entrarComo(browser, conta)

  try {
    await expect(pessoa.getByRole('link', { name: 'Kanban' })).toBeVisible()

    await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: 'Solicitante', ativo: true })

    await expect(pessoa).toHaveURL(/\/login$/, { timeout: 15000 })
    await expect(pessoa.getByText('Seu perfil foi alterado. Entre novamente.')).toBeVisible()

    await pessoa.locator('#email').fill(conta.email)
    await pessoa.locator('#senha').fill(conta.senha)
    await pessoa.getByRole('button', { name: /Entrar|Login/i }).click()
    await pessoa.waitForURL('**/chamados')
    await expect(pessoa.getByRole('link', { name: 'Kanban' })).toHaveCount(0)
  } finally {
    await contexto.close()
    await desativarContaDeTeste(page, conta)
  }
})

// review-2 R-02: o painel de Chat de um Admin também mostra o histórico (só abre e fecha — não muda nada).
test('Painel de Chat de um Admin mostra o histórico de mudanças', async ({ page }) => {
  await login(page)
  await page.goto('/admin/acessos')
  await page.getByLabel('Buscar pessoa').fill(credenciais().email)
  await page.getByRole('row', { name: new RegExp(credenciais().email) }).getByRole('button', { name: 'Ajustar Chat' }).click()
  const dialogo = page.getByRole('dialog')
  await expect(dialogo.getByText('Histórico de mudanças')).toBeVisible()
  await dialogo.getByRole('button', { name: 'Cancelar' }).click()
  await expect(dialogo).toBeHidden()
})

// review-3 R-02: a pessoa estava FORA (aba fechada) quando o perfil mudou — ao reabrir, sai e pede login.
test('Perfil mudado com a pessoa fora do sistema: ao reabrir, pede login de novo com o aviso', async ({ page, browser }) => {
  await login(page)
  const conta = await atendenteDeTeste(page)
  const { contexto } = await entrarComo(browser, conta)
  const sessaoSalva = await contexto.storageState() // token e perfil salvos no navegador dela
  await contexto.close()

  await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: 'Solicitante', ativo: true })

  const reaberto = await browser.newContext({ storageState: sessaoSalva })
  try {
    const tela = await reaberto.newPage()
    await tela.goto('/chamados')
    await expect(tela).toHaveURL(/\/login$/, { timeout: 15000 })
    await expect(tela.getByText('Seu perfil foi alterado. Entre novamente.')).toBeVisible()
  } finally {
    await reaberto.close()
    await desativarContaDeTeste(page, { ...conta, perfil: 'Solicitante' })
  }
})

// review-3 R-01: pessoa COM Área — salvar módulo e Chat juntos dava 500 (e o bug antigo de trocar/tirar Área
// em Editar usuário não gravava). Só API: o que importa é a gravação.
test('Pessoa com Área: trocar a Área grava, e salvar módulo + Chat juntos funciona e audita', async ({ page }) => {
  await login(page)
  const conta = await atendenteDeTeste(page)
  try {
    const grupos = await api<{ id: string }[]>(page, 'GET', '/grupos')
    expect(grupos.length).toBeGreaterThan(0)
    await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: 'Atendente', ativo: true, grupoId: grupos[0].id })
    const comArea = (await api<{ id: string; grupoId: string | null }[]>(page, 'GET', '/usuarios')).find((u) => u.id === conta.id)
    expect(comArea?.grupoId).toBe(grupos[0].id)

    await api(page, 'PUT', `/acessos/${conta.id}`, {
      modulos: ['Arquivo', 'Kanban', 'Fila', 'Dashboard'], chatPerfil: 'Participante', perfilEsperado: 'Atendente',
    })
    const detalhe = await api<{ modulos: { modulo: string; efetivo: boolean }[]; chatPerfil: string; auditoria: { item: string }[] }>(
      page, 'GET', `/acessos/${conta.id}`)
    expect(detalhe.chatPerfil).toBe('Participante')
    expect(detalhe.modulos.find((m) => m.modulo === 'RelatorioMensal')?.efetivo).toBe(false)
    expect(detalhe.auditoria.map((a) => a.item)).toEqual(expect.arrayContaining(['Relatório mensal', 'Chat']))

    await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: 'Atendente', ativo: true, grupoId: null })
    const semArea = (await api<{ id: string; grupoId: string | null }[]>(page, 'GET', '/usuarios')).find((u) => u.id === conta.id)
    expect(semArea?.grupoId ?? null).toBeNull()
  } finally {
    await desativarContaDeTeste(page, conta)
  }
})

// review-4 R-01: aba aberta, mas sem conexão no instante da mudança — o aviso em tempo real se perde. Ao a
// conexão voltar, o sistema relê o cadastro e desconecta do mesmo jeito.
test('Perfil mudado durante uma queda de conexão: ao reconectar, a pessoa sai com o aviso', async ({ page, browser }) => {
  test.setTimeout(120_000)
  await login(page)
  const conta = await atendenteDeTeste(page)
  const { contexto, pessoa } = await entrarComo(browser, conta)

  try {
    await expect(pessoa.getByRole('link', { name: 'Kanban' })).toBeVisible()
    await contexto.setOffline(true)
    await api(page, 'PUT', `/usuarios/${conta.id}`, { nome: conta.nome, perfil: 'Solicitante', ativo: true })
    await pessoa.waitForTimeout(3000) // o aviso em tempo real sai enquanto ela está sem conexão
    await expect(pessoa).not.toHaveURL(/\/login$/)

    await contexto.setOffline(false)
    await expect(pessoa).toHaveURL(/\/login$/, { timeout: 60_000 })
    await expect(pessoa.getByText('Seu perfil foi alterado. Entre novamente.')).toBeVisible()
  } finally {
    await contexto.close()
    await desativarContaDeTeste(page, { ...conta, perfil: 'Solicitante' })
  }
})
