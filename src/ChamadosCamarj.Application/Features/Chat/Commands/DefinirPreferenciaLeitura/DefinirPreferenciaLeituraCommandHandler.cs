using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirPreferenciaLeitura;

public class DefinirPreferenciaLeituraCommandHandler : IRequestHandler<DefinirPreferenciaLeituraCommand>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public DefinirPreferenciaLeituraCommandHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task Handle(DefinirPreferenciaLeituraCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        if (usuario.MostrarConfirmacaoLeitura == request.Mostrar)
            return;

        usuario.DefinirPreferenciaLeitura(request.Mostrar);
        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);
    }
}
