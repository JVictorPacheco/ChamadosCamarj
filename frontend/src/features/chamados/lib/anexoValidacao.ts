export const TAMANHO_MAXIMO_BYTES = 10 * 1024 * 1024
export const EXTENSOES_PERMITIDAS = ['.pdf', '.jpg', '.jpeg', '.png', '.gif', '.doc', '.docx', '.xls', '.xlsx', '.zip']

function extensaoPermitida(nomeArquivo: string): boolean {
  const extensao = nomeArquivo.slice(nomeArquivo.lastIndexOf('.')).toLowerCase()
  return EXTENSOES_PERMITIDAS.includes(extensao)
}

export function validarArquivo(arquivo: File): string | null {
  if (arquivo.size > TAMANHO_MAXIMO_BYTES) {
    return 'Arquivo excede o tamanho máximo de 10MB.'
  }
  if (!extensaoPermitida(arquivo.name)) {
    return 'Tipo de arquivo não permitido. Tipos aceitos: PDF, imagens, Word, Excel, ZIP.'
  }
  return null
}
