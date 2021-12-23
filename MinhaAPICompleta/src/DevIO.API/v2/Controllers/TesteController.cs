using DevIO.API.Controllers;
using DevIO.Business.Intefaces;
using Elmah.Io.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DevIO.API.v2.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/teste")]
    public class TesteController : MainController
    {
        private readonly ILogger _logger;
        public TesteController(INotificador notificador, IUser appUser, ILogger<TesteController> logger) : base(notificador, appUser)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Valor()
        {
            throw new Exception("Erro");
            /*
            try
            {
                var i = 0;
                var result = 500 / i;
            }
            catch (DivideByZeroException e)
            {
                e.Ship(HttpContext);                
            }
            */
            _logger.LogTrace("Log de Trace"); // Usado em ambiente de desevolvimento ex: Processo x começou em y horas e terminou z horas. - Desabilitado por padrão.
            _logger.LogDebug("Log Debug");  // Usado em ambiente de desevolvimento para debug.
            _logger.LogInformation("Log de informação"); // Usado para gravar qualquer coisa que você queria registrar.
            _logger.LogWarning("Log Warning");// Usado para informar algum erro que ocorreu ex: 404
            _logger.LogError("Log Erro");//usado para registrar um erro que aconteceu durante a execução da aplicação
            _logger.LogCritical("Log Critico"); // Usado para registrar um erro critico que afeta diretamente a saude e responsividade da aplicação
            return "Sou a V2";
        }
    }
}