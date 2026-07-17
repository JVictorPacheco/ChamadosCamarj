export type StatusChamado = "Aberto" | "EmAndamento" | "Resolvido" | "Fechado" | "Cancelado";
export type PrioridadeChamado = "Baixa" | "Media" | "Alta" | "Urgente";
export type TipoComentario = "Publico" | "Interno";
export type AcaoHistorico =
  | "Criado"
  | "Assumido"
  | "Reatribuido"
  | "Resolvido"
  | "Fechado"
  | "Cancelado"
  | "ComentarioAdicionado"
  | "PrioridadeAlterada";

export interface ChamadoResponse {
  id: string;
  titulo: string;
  descricao: string;
  status: StatusChamado;
  prioridade: PrioridadeChamado;
  solicitanteNome: string;
  solicitanteEmail: string;
  responsavelId: string | null;
  responsavelNome: string | null;
  categoriaId: string;
  categoriaNome: string | null;
  dataLimite: string | null; // ISO 8601
  dataConclusao: string | null;
  dataCriacao: string;
  dataAtualizacao: string | null;
  quantidadeComentarios: number;
  quantidadeAnexos: number;
}

export interface ComentarioResponse {
  id: string;
  autor: string;
  conteudo: string;
  tipo: TipoComentario;
  dataCriacao: string;
}

export interface CategoriaResponse {
  id: string;
  nome: string;
  descricao: string;
  ativa: boolean;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
  totalPaginas: number;
  temProxima: boolean;
  temAnterior: boolean;
}

export interface AbrirChamadoRequest {
  titulo: string;
  descricao: string;
  solicitanteNome: string;
  solicitanteEmail: string;
  categoriaId: string;
  prioridade?: PrioridadeChamado;
}

export interface ComentarChamadoRequest {
  autor: string;
  conteudo: string;
  interno: boolean;
}

export interface HistoricoResponse {
  id: string;
  chamadoId: string;
  usuarioNome: string;
  usuarioId: string | null;
  acao: AcaoHistorico;
  detalheAnterior: string | null;
  detalheNovo: string | null;
  dataHora: string;
}

export type TipoPerfil = "Admin" | "Atendente" | "Solicitante";

export interface UsuarioPerfilResponse {
  id: string;
  email: string;
  nome: string;
  perfil: TipoPerfil;
  ativo: boolean;
}
