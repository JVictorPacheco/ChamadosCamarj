const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api'

const TOKEN_STORAGE_KEY = 'chamados-camarj:token'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY)
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token)
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY)
}

// Chamado pelo AuthContext quando a API devolve 401 (token expirado/inválido) — evita
// a pessoa ficar vendo erros genéricos com uma sessão morta; redireciona pro login.
let aoDeslogarPorTokenInvalido: (() => void) | null = null
export function registrarLogoutAutomatico(callback: () => void): void {
  aoDeslogarPorTokenInvalido = callback
}

export interface ApiFieldError {
  campo: string
  erro: string
}

export class ApiError extends Error {
  status?: number
  errors?: ApiFieldError[]

  constructor(message: string, status?: number, errors?: ApiFieldError[]) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}

export function gerarIdempotencyKey(): string {
  return crypto.randomUUID()
}

export async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getToken()

  // FormData (upload de arquivo) não pode ter Content-Type fixado manualmente — o
  // browser precisa gerar o boundary do multipart sozinho.
  const isFormData = options?.body instanceof FormData

  let response: Response
  try {
    response = await fetch(`${BASE_URL}${path}`, {
      ...options,
      headers: {
        ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...options?.headers,
      },
    })
  } catch {
    throw new ApiError('Serviço indisponível. Verifique sua conexão.')
  }

  if (!response.ok) {
    if (response.status === 401) {
      clearToken()
      aoDeslogarPorTokenInvalido?.()
    }

    const body = await response.json().catch(() => null)
    throw new ApiError(body?.message ?? 'Ocorreu um erro inesperado.', response.status, body?.errors)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
