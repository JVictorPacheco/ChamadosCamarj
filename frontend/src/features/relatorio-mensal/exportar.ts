import type { RelatorioMensalResponse } from '@/types/relatorio'

function linhaCsv(valores: (string | number)[]): string {
  return valores.map((v) => `"${String(v).replaceAll('"', '""')}"`).join(';')
}

export function exportarCsv(relatorio: RelatorioMensalResponse) {
  const linhas: string[] = []

  linhas.push(linhaCsv(['Relatório Mensal', `${relatorio.mes}/${relatorio.ano}`]))
  linhas.push('')
  linhas.push(linhaCsv(['Métrica', 'Valor']))
  linhas.push(linhaCsv(['Abertos', relatorio.totalAbertos]))
  linhas.push(linhaCsv(['Resolvidos', relatorio.totalResolvidos]))
  linhas.push(linhaCsv(['Cancelados', relatorio.totalCancelados]))
  linhas.push(linhaCsv(['Tempo médio de resolução (h)', relatorio.tempoMedioResolucaoHoras ?? '—']))
  linhas.push(linhaCsv(['SLA cumprido (%)', relatorio.sla.percentualCumprido ?? '—']))
  linhas.push('')

  linhas.push(linhaCsv(['Categoria', 'Quantidade']))
  for (const item of relatorio.porCategoria) {
    linhas.push(linhaCsv([item.categoriaNome, item.quantidade]))
  }

  if (relatorio.porAtendente) {
    linhas.push('')
    linhas.push(linhaCsv(['Atendente', 'Abertos', 'Resolvidos', 'Cancelados']))
    for (const item of relatorio.porAtendente) {
      linhas.push(linhaCsv([item.responsavelNome, item.abertos, item.resolvidos, item.cancelados]))
    }
  }

  const conteudo = '﻿' + linhas.join('\n')
  const blob = new Blob([conteudo], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `relatorio-mensal-${relatorio.ano}-${String(relatorio.mes).padStart(2, '0')}.csv`
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

export function imprimirRelatorio() {
  window.print()
}
