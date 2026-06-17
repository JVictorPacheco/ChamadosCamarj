using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriasController(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    /// <summary>
    /// Lista todas as categorias ativas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Categoria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Categoria>>> Listar(CancellationToken cancellationToken)
    {
        var categorias = await _categoriaRepository.ObterAtivasAsync(cancellationToken);
        return Ok(categorias);
    }
}
