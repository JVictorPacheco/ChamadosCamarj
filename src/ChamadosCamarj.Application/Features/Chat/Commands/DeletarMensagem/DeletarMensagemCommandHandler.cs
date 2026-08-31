using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DeletarMensagem;

public class DeletarMensagemCommandHandler : IRequestHandler<DeletarMensagemCommand>
{
    private const string PerfilAdmin = "Admin";

    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IMediator _mediator;

    public DeletarMensagemCommandHandler(
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IMediator mediator)
    {
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _mediator = mediator;
    }

    public async Task Handle(DeletarMensagemCommand request, CancellationToken cancellationToken)
    {
        var mensagem = await _mensagemRepository.ObterPorIdAsync(request.MensagemId, cancellationToken);
        if (mensagem is null)
            throw new NotFoundException("Mensagem", request.MensagemId);

        var ehAdmin = string.Equals(request.PerfilRequisitante, PerfilAdmin, StringComparison.OrdinalIgnoreCase);
        if (mensagem.AutorId != request.UsuarioId && !ehAdmin)
            throw new ForbiddenException("Você só pode deletar suas próprias mensagens.");

        if (mensagem.Deletada)
            return;

        var conteudoOriginal = mensagem.Conteudo ?? mensagem.NomeArquivo;
        mensagem.Deletar();
        await _mensagemRepository.AtualizarAsync(mensagem, cancellationToken);

        var detalhe = JsonSerializer.Serialize(new
        {
            conteudoOriginal,
            deletadaPorAdmin = ehAdmin && mensagem.AutorId != request.UsuarioId
        });

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, ChatAcao.MensagemDeletada, detalhe, mensagem.ConversaId, mensagem.Id),
            cancellationToken);

        await _mediator.Publish(new ChatMensagemDeletadaNotification(mensagem.ConversaId, mensagem.Id), cancellationToken);
    }
}
