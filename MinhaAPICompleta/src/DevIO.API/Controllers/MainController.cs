using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;

namespace DevIO.API.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;
        public readonly IUser _appUser;

        public Guid UsuarioID { get; set; }

        protected bool UsuarioAutenticado { get; set; }

        public MainController(INotificador notificador,
                              IUser appUser)
        {
            _notificador = notificador;
            _appUser = appUser;

            if (UsuarioAutenticado = appUser.IsAuthenticated())
            {
                UsuarioID = appUser.GetUserId();
            }
        }
        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }
        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }

        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)
            {
                var erroMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(erroMsg);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem));
        }


        //Validação de notificações e erros,

        //Validação de Model State

        //Validação da operação de negócios

    }
}
