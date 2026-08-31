using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EditarMensagem;

public class EditarMensagemCommandHandler : IRequestHandler<EditarMensagemCommand>
{
    private static readonly TimeSpan LimiteEdicao = TimeSpan.FromHours(24);

    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IMediator _mediator;

    public EditarMensagemCommandHandler(
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IMediator mediator)
    {
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _mediator = mediator;
    }

    public async Task Handle(EditarMensagemCommand request, CancellationToken cancellationToken)
    {
        var mensagem = await _mensagemRepository.ObterPorIdAsync(request.MensagemId, cancellationToken);
        if (mensagem is null)
            throw new NotFoundException("Mensagem", request.MensagemId);

        if (mensagem.AutorId != request.UsuarioId)
            throw new ForbiddenException("Você só pode editar suas próprias mensagens.");

        if (mensagem.Deletada)
            throw new BadRequestException("Não é possível editar uma mensagem removida.");

        if (mensagem.Tipo != ChatMensagemTipo.Texto)
            throw new BadRequestException("Apenas mensagens de texto podem ser editadas.");

        if (DateTime.UtcNow - mensagem.DataCriacao > LimiteEdicao)
            throw new BadRequestException("Mensagens só podem ser editadas em até 24 horas após o envio.");

        var conteudoAnterior = mensagem.Conteudo;
        mensagem.Editar(request.NovoConteudo);
        await _mensagemRepository.AtualizarAsync(mensagem, cancellationToken);

        var detalhe = JsonSerializer.Serialize(new
        {
            conteudoAnterior,
            conteudoNovo = request.NovoConteudo
        });

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, ChatAcao.MensagemEditada, detalhe, mensagem.ConversaId, mensagem.Id),
            cancellationToken);

        var response = mensagem.ToResponse(request.UsuarioId);
        await _mediator.Publish(new ChatMensagemEditadaNotification(mensagem.ConversaId, response), cancellationToken);
    }
}
