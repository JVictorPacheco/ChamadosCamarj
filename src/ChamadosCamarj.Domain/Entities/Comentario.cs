using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class Comentario : BaseEntity
{
    private Comentario() { }

    public Comentario(Guid chamadoId, string autor, string conteudo, TipoComentario tipo = TipoComentario.Publico)
    {
        if (string.IsNullOrWhiteSpace(autor))
            throw new ArgumentException("Autor é obrigatório.", nameof(autor));
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo é obrigatório.", nameof(conteudo));

        ChamadoId = chamadoId;
        Autor = autor;
        Conteudo = conteudo;
        Tipo = tipo;
    }

    public Guid ChamadoId { get; private set; }
    public string Autor { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public TipoComentario Tipo { get; private set; }

    // Navegação EF
    public Chamado? Chamado { get; private set; }
}
