import React from 'react';
import { useQuery } from '@tanstack/react-query';
import chamadoApi from '@/services/chamadoApi';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface TimelineHistoricoProps {
  chamadoId: string;
}

const ICONES_ACAO: Record<string, string> = {
  Criado: '📌',
  Assumido: '👤',
  Reatribuido: '↩️',
  PrioridadeAlterada: '🔴',
  Resolvido: '✅',
  Fechado: '🔒',
  Cancelado: '❌',
  ComentarioAdicionado: '💬'
};

export function TimelineHistorico({ chamadoId }: TimelineHistoricoProps) {
  const { data: historico = [], isLoading } = useQuery({
    queryKey: ['historico', chamadoId],
    queryFn: () =>
      chamadoApi.get(`/chamados/${chamadoId}/historico`).then(res => res.data)
  });

  if (isLoading) {
    return <div className="text-center py-4">Carregando histórico...</div>;
  }

  if (historico.length === 0) {
    return <div className="text-center py-4 text-gray-500">Sem histórico</div>;
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">📜 Histórico</h3>

      <div className="relative">
        {/* Linha vertical */}
        <div className="absolute left-4 top-0 bottom-0 w-1 bg-gray-300"></div>

        {/* Entradas do histórico */}
        <div className="space-y-4 ml-16">
          {historico.map((entrada: any, index: number) => (
            <div key={entrada.id} className="flex items-start">
              {/* Ponto da timeline */}
              <div className="absolute -left-2 w-9 h-9 bg-white border-4 border-blue-500 rounded-full flex items-center justify-center">
                <span className="text-sm">{ICONES_ACAO[entrada.acao] || '•'}</span>
              </div>

              {/* Card da ação */}
              <div className="flex-1 bg-white p-4 rounded-lg border border-gray-200 shadow-sm">
                <div className="flex justify-between items-start mb-2">
                  <div>
                    <p className="font-semibold text-gray-900">{entrada.acao}</p>
                    <p className="text-sm text-gray-600">por {entrada.usuarioNome}</p>
                  </div>
                  <span className="text-xs text-gray-500">
                    {format(new Date(entrada.dataHora), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
                  </span>
                </div>

                {/* Descrição detalhada */}
                {entrada.descricao && (
                  <p className="text-sm text-gray-700 mt-2">{entrada.descricao}</p>
                )}

                {/* Detalhes específicos (ex: mudança de prioridade) */}
                {entrada.dadosAnteriores && entrada.dadosNovos && (
                  <div className="text-xs text-gray-600 mt-2 p-2 bg-gray-50 rounded">
                    <p>
                      <strong>Antes:</strong> {JSON.stringify(entrada.dadosAnteriores)}
                    </p>
                    <p>
                      <strong>Depois:</strong> {JSON.stringify(entrada.dadosNovos)}
                    </p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
