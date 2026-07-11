import React, { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import { useMutation } from '@tanstack/react-query';
import chamadoApi from '@/services/chamadoApi';

interface AlterarPrioridadeModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  chamadoId: string;
  prioridadeAtual: string;
  onAlterarSuccess: () => void;
}

const PRIORIDADES = [
  { value: 'Urgente', label: 'Urgente', color: 'text-red-600', bgColor: 'bg-red-50' },
  { value: 'Alta', label: 'Alta', color: 'text-orange-600', bgColor: 'bg-orange-50' },
  { value: 'Media', label: 'Média', color: 'text-yellow-600', bgColor: 'bg-yellow-50' },
  { value: 'Baixa', label: 'Baixa', color: 'text-green-600', bgColor: 'bg-green-50' }
];

export function AlterarPrioridadeModal({
  open,
  onOpenChange,
  chamadoId,
  prioridadeAtual,
  onAlterarSuccess
}: AlterarPrioridadeModalProps) {
  const { toast } = useToast();
  const [selectedPrioridade, setSelectedPrioridade] = useState<string>(prioridadeAtual);

  const alterarMutation = useMutation({
    mutationFn: (novaPrioridade: string) =>
      chamadoApi.patch(`/chamados/${chamadoId}/prioridade`, {
        prioridade: novaPrioridade
      }),
    onSuccess: () => {
      toast({
        title: 'Sucesso!',
        description: 'Prioridade alterada com sucesso!',
        variant: 'default'
      });
      onAlterarSuccess();
      onOpenChange(false);
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Erro ao alterar prioridade',
        variant: 'destructive'
      });
    }
  });

  const handleSalvar = () => {
    if (selectedPrioridade === prioridadeAtual) {
      toast({
        title: 'Atenção',
        description: 'Selecione uma prioridade diferente',
        variant: 'destructive'
      });
      return;
    }

    alterarMutation.mutate(selectedPrioridade);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Alterar Prioridade</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <p className="text-sm text-gray-600">
            Prioridade atual: <strong>{prioridadeAtual}</strong>
          </p>

          <RadioGroup value={selectedPrioridade} onValueChange={setSelectedPrioridade}>
            <div className="space-y-3">
              {PRIORIDADES.map((prioridade) => (
                <div key={prioridade.value} className={`flex items-center space-x-2 p-3 rounded border ${prioridade.bgColor}`}>
                  <RadioGroupItem value={prioridade.value} id={prioridade.value} />
                  <Label htmlFor={prioridade.value} className="flex-1 cursor-pointer">
                    <span className={prioridade.color}>{prioridade.label}</span>
                  </Label>
                </div>
              ))}
            </div>
          </RadioGroup>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button onClick={handleSalvar} disabled={alterarMutation.isPending}>
            {alterarMutation.isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
