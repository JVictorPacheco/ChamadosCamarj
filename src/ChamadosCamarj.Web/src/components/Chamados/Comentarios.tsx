import React, { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import chamadoApi from '@/services/chamadoApi';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/use-toast';
import { Badge } from '@/components/ui/badge';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface ComentariosProps {
  chamadoId: string;
  perfilUsuario?: string; // 'Admin', 'Cliente', etc
}

export function Comentarios({ chamadoId, perfilUsuario }: ComentariosProps) {
  const { toast } = useToast();
  const [novoComentario, setNovoComentario] = useState('');
  const [ehInterno, setEhInterno] = useState(false);

  // Buscar comentários
  const { data: comentarios = [], refetch: refetchComentarios } = useQuery({
    queryKey: ['comentarios', chamadoId, perfilUsuario],
    queryFn: () =>
      chamadoApi.get(`/chamados/${chamadoId}/comentarios`, {
        params: { perfilUsuario }
      }).then(res => res.data)
  });

  // Adicionar comentário
  const adicionarMutation = useMutation({
    mutationFn: (dados: { conteudo: string; tipo: string }) =>
      chamadoApi.post(`/chamados/${chamadoId}/comentarios`, dados),
    onSuccess: () => {
      toast({
        title: 'Sucesso!',
        description: 'Comentário adicionado com sucesso!',
        variant: 'default'
      });
      setNovoComentario('');
      setEhInterno(false);
      refetchComentarios();
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Erro ao adicionar comentário',
        variant: 'destructive'
      });
    }
  });

  const handleAdicionar = () => {
    if (!novoComentario.trim()) {
      toast({
        title: 'Atenção',
        description: 'Adicione um texto ao comentário',
        variant: 'destructive'
      });
      return;
    }

    adicionarMutation.mutate({
      conteudo: novoComentario,
      tipo: ehInterno ? 'Interno' : 'Publico'
    });
  };

  return (
    <div className="space-y-6">
      <h3 className="text-lg font-semibold">💬 Comentários ({comentarios.length})</h3>

      {/* Lista de comentários */}
      <div className="space-y-4">
        {comentarios.length === 0 ? (
          <p className="text-gray-500 text-center py-4">Sem comentários</p>
        ) : (
          comentarios.map((comentario: any) => (
            <div
              key={comentario.id}
              className={`p-4 rounded-lg border ${
                comentario.tipo === 'Interno'
                  ? 'bg-red-50 border-red-200'
                  : 'bg-green-50 border-green-200'
              }`}
            >
              <div className="flex justify-between items-start mb-2">
                <div className="flex items-center gap-2">
                  <p className="font-semibold text-gray-900">{comentario.autor}</p>
                  <Badge variant={comentario.tipo === 'Interno' ? 'destructive' : 'default'}>
                    {comentario.tipo === 'Interno' ? '🔒 Interno' : '✅ Público'}
                  </Badge>
                </div>
                <span className="text-xs text-gray-500">
                  {format(new Date(comentario.dataCriacao), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
                </span>
              </div>
              <p className="text-gray-700">{comentario.conteudo}</p>
            </div>
          ))
        )}
      </div>

      {/* Formulário para adicionar comentário */}
      <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 space-y-3">
        <p className="font-semibold text-sm">Adicionar Comentário</p>

        <Textarea
          placeholder="Escreva um comentário..."
          value={novoComentario}
          onChange={(e) => setNovoComentario(e.target.value)}
          rows={3}
          className="resize-none"
        />

        <div className="flex items-center gap-2">
          <Checkbox
            id="ehInterno"
            checked={ehInterno}
            onCheckedChange={(checked) => setEhInterno(checked as boolean)}
          />
          <label htmlFor="ehInterno" className="text-sm cursor-pointer">
            Este é um comentário interno (apenas atendentes podem ver)
          </label>
        </div>

        <Button
          onClick={handleAdicionar}
          disabled={adicionarMutation.isPending}
          className="w-full"
        >
          {adicionarMutation.isPending ? 'Adicionando...' : 'Adicionar Comentário'}
        </Button>
      </div>
    </div>
  );
}
