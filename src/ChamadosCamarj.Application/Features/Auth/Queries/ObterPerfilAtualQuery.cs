using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;

namespace ChamadosCamarj.Application.Features.Auth.Queries;

// review-fase9-independente.md #10 / AC-48: perfil.chatPerfil é hidratado só de localStorage no
// boot e nunca revalidado — quem foi revogado enquanto estava deslogado (ou com a aba fechada)
// voltava com o link "Chat" visível até logar de novo. Esta query dá ao frontend um jeito de
// revalidar o perfil atual no boot sem precisar de um token novo.
public record ObterPerfilAtualQuery(Guid UsuarioId) : IRequest<UsuarioPerfilResponse>;
