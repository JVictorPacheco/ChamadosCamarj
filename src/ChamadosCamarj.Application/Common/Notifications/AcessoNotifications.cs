using ChamadosCamarj.Domain.Enums;
using MediatR;

namespace ChamadosCamarj.Application.Common.Notifications;

/// <summary>
/// Os acessos de uma pessoa mudaram (spec controle-de-acesso AC-11/AC-13): avisa só ela, em tempo real,
/// para o menu se atualizar sem sair e entrar. Mesmo mecanismo do ChatPerfilAtualizado. Leva o perfil do
/// cadastro: se ele mudou, a tela da pessoa sai e pede para entrar de novo (review-2 R-03).
/// </summary>
public record AcessosAtualizadosNotification(Guid UsuarioId, IReadOnlyList<string> Modulos, ChatPerfil ChatPerfil, Perfil Perfil) : INotification;
