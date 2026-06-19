using System.Net;
using System.Text.Json;
using FluentValidation;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.NotFound, new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            var erros = ex.Errors.Select(e => new { campo = e.PropertyName, erro = e.ErrorMessage });
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, new { errors = erros });
        }
        catch (InvalidOperationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao processar a requisição {Path}", context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, new { message = "Ocorreu um erro interno." });
        }
    }

    private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, object body)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
