import React, { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/components/ui/use-toast';
import { useMutation, useQuery } from '@tanstack/react-query';
import chamadoApi from '@/services/chamadoApi';

interface ReatribuirModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  chamadoId: string;
  currentResponsavelNome: string;
  onReatribuirSuccess: () => void;
}

export function ReatribuirModal({
  open,
  onOpenChange,
  chamadoId,
  currentResponsavelNome,
  onReatribuirSuccess
}: ReatribuirModalProps) {
  const { toast } = useToast();
  const [selectedResponsavel, setSelectedResponsavel] = useState<string>('');
  const [motivo, setMotivo] = useState<string>('');

  // Buscar lista de atendentes
  const { data: atendentes = [] } = useQuery({
    queryKey: ['atendentes'],
    queryFn: () => chamadoApi.get('/atendentes').then(res => res.data)
  });

  // Mutação para reatribuir
  const reatribuirMutation = useMutation({
    mutationFn: (data: { novoResponsavelId: string; motivo: string }) =>
      chamadoApi.patch(`/chamados/${chamadoId}/reatribuir`, {
        responsavelId: data.novoResponsavelId,
        motivo: data.motivo
      }),
    onSuccess: () => {
      toast({
        title: 'Sucesso!',
        description: 'Chamado reatribuído com sucesso!',
        variant: 'default'
      });
      onReatribuirSuccess();
      onOpenChange(false);
      setSelectedResponsavel('');
      setMotivo('');
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Erro ao reatribuir chamado',
        variant: 'destructive'
      });
    }
  });

  const handleReatribuir = () => {
    if (!selectedResponsavel) {
      toast({
        title: 'Atenção',
        description: 'Selecione um atendente',
        variant: 'destructive'
      });
      return;
    }

    reatribuirMutation.mutate({
      novoResponsavelId: selectedResponsavel,
      motivo
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reatribuir Chamado</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium">Atendente Atual: {currentResponsavelNome}</label>
          </div>

          <div>
            <label className="text-sm font-medium mb-2 block">Novo Atendente *</label>
            <Select value={selectedResponsavel} onValueChange={setSelectedResponsavel}>
              <SelectTrigger>
                <SelectValue placeholder="Selecione um atendente" />
              </SelectTrigger>
              <SelectContent>
                {atendentes.map((atendente: any) => (
                  <SelectItem key={atendente.id} value={atendente.id}>
                    {atendente.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-sm font-medium mb-2 block">Motivo (opcional)</label>
            <Textarea
              placeholder="Ex: Especialista em email"
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
              rows={3}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button onClick={handleReatribuir} disabled={reatribuirMutation.isPending}>
            {reatribuirMutation.isPending ? 'Reatribuindo...' : 'Reatribuir'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
