using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _repository;

    public CategoriasController(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Lista todas as categorias ativas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var categorias = await _repository.ObterAtivasAsync(cancellationToken);
        return Ok(categorias.Select(c => new
        {
            c.Id,
            c.Nome,
            c.Descricao
        }));
    }
}
