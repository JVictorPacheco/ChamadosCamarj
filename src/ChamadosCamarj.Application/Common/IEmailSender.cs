namespace ChamadosCamarj.Application.Common;

public interface IEmailSender
{
    Task EnviarAsync(string para, string assunto, string corpoHtml);
}
