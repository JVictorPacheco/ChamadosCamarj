using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ChamadosCamarj.WebApi.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : ActionFilterAttribute
{
    private const string HeaderName = "Idempotency-Key";
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            await next();
            return;
        }

        var key = $"{context.HttpContext.Request.Method}:{keyValues.First()!}";

        if (_cache.TryGetValue(key, out _))
        {
            context.Result = new ConflictObjectResult(new
            {
                message = "Requisicao duplicada detectada. Aguarde e tente novamente."
            });
            return;
        }

        _cache.Set(key, true, TimeSpan.FromMinutes(5));

        await next();
    }
}
