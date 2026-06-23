const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api'

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

export async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${BASE_URL}${path}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
    })
  } catch {
    throw new ApiError('Serviço indisponível. Verifique sua conexão.')
  }

  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new ApiError(body?.message ?? 'Ocorreu um erro inesperado.', response.status, body?.errors)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
