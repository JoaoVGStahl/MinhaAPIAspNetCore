using AutoMapper;
using DevIO.API.Controllers;
using DevIO.API.Extensions;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevIO.API.v1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _fornecedorRespository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public FornecedoresController(IFornecedorRepository fornecedorRepository,
                                      IMapper mapper,
                                      IFornecedorService fornecedorService,
                                      IEnderecoRepository enderecoRepository,
                                      INotificador notificador,
                                      IUser user) : base(notificador, user)
        {
            _fornecedorRespository = fornecedorRepository;
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _enderecoRepository = enderecoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos()
        {
            var fornecedor = _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRespository.ObterTodos());
            return Ok(fornecedor);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> ObterPorId(Guid id)
        {
            /*   
               if (User.Identity.IsAuthenticated)
               {
                   var userName = User.Identity.Name;
               }

               if (UsuarioAutenticado)
               {
                   var userName = UsuarioID;
               }
            */

            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if (fornecedor == null) return NotFound();

            return Ok(fornecedor);
        }
        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorViewModel>> NovoFornecedor(FornecedorViewModel fornecedorVM)
        {

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorVM);

            var result = await _fornecedorService.Adicionar(fornecedor);

            if (!result) return BadRequest();

            return CustomResponse(fornecedorVM);
        }
        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> EditarFornecedor(Guid id, FornecedorViewModel fornecedorViewModel)
        {
            if (id != fornecedorViewModel.Id)
            {
                NotificarErro("O Id informado não é o mesmo que foi informado na Query!");
                return CustomResponse(fornecedorViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);

            await _fornecedorService.Atualizar(fornecedor);

            return CustomResponse(fornecedorViewModel);
        }
        [ClaimsAuthorize("Fornecedor", "Remover")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> DeletarFornecedor(Guid id)
        {
            var fornecedorVM = await ObterFornecedorEndereco(id);

            if (fornecedorVM == null) return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse();
        }
        [HttpGet("endereco/{id:guid}")]
        public async Task<EnderecoViewModel> ObterEnderecoporId(Guid id)
        {
            var enderecoVM = _mapper.Map<EnderecoViewModel>(await _enderecoRepository.ObterEnderecoPorFornecedor(id));
            return enderecoVM;
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<ActionResult> AtuaizarEndereco(Guid id, EnderecoViewModel enderecoVM)
        {
            if (id != enderecoVM.Id)
            {
                NotificarErro("O Id informado não é o mesmo informado na Query!");
                return CustomResponse(enderecoVM);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var endereco = _mapper.Map<Endereco>(enderecoVM);

            await _fornecedorService.AtualizarEndereco(endereco);

            return CustomResponse(enderecoVM);
        }

        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorRespository.ObterFornecedorEndereco(id));
        }
        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorRespository.ObterFornecedorProdutosEndereco(id));
        }
    }
}
