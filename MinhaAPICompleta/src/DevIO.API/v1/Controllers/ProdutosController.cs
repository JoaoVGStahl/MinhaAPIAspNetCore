using AutoMapper;
using DevIO.API.Controllers;
using DevIO.API.Extensions;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DevIO.API.v1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  INotificador notificador,
                                  IUser user) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            var produtoVM = _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());

            return produtoVM;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoVM = await ObterProduto(id);

            if (produtoVM == null) return NotFound();

            return produtoVM;
        }
        [ClaimsAuthorize("Produto", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var produto = _mapper.Map<Produto>(produtoVM);

            var imgNome = Guid.NewGuid() + "_" + produtoVM.Imagem;

            if (!UploadImagem(produtoVM.ImagemUpload, imgNome))
            {
                return CustomResponse();
            }


            produtoVM.Imagem = imgNome;

            await _produtoService.Adicionar(produto);

            return CustomResponse(produtoVM);
        }

        [ClaimsAuthorize("Produto", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
            {
                NotificarErro("Os Id informados são diferentes!");
                return CustomResponse();
            }


            var produtoAtualizacao = await ObterProduto(id);

            produtoViewModel.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoViewModel.ImagemUpload != null)
            {
                var imgNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
                if (!UploadImagem(produtoViewModel.ImagemUpload, imgNome))
                {
                    return CustomResponse(ModelState);
                }
                produtoViewModel.Imagem = imgNome;
            }
            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoAtualizacao);

        }
        [ClaimsAuthorize("Produto", "Adicionar")]
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarImagemFormFile(ProdutoImagemViewModel produtoVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var produto = _mapper.Map<Produto>(produtoVM);

            var imgPrefix = Guid.NewGuid() + "_";

            if (!await UploadoImagemFormFile(produtoVM.ImagemUpload, imgPrefix))
            {
                return CustomResponse();
            }

            produtoVM.Imagem = imgPrefix + produtoVM.ImagemUpload.FileName;

            await _produtoService.Adicionar(produto);

            return CustomResponse(produtoVM);
        }
        [ClaimsAuthorize("Produto", "Adicionar")]
        [RequestSizeLimit(40000000)]
        [HttpPost("Imagem")]
        public ActionResult AdicionarImagemForm(IFormFile file)
        {
            return Ok(file);
        }

        private bool UploadImagem(string arquivo, string arquivoNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto");
                return false;
            }

            var imageByteArray = Convert.FromBase64String(arquivo);

            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Produtos/Imagens", arquivoNome);

            if (System.IO.File.Exists(filepath))
            {
                NotificarErro("Ja Existe uma arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filepath, imageByteArray);

            return true;

        }
        private async Task<bool> UploadoImagemFormFile(IFormFile arquivo, string imgPrefix)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto");
                return false;
            }

            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Produtos/Imagens", imgPrefix + arquivo.FileName);

            if (System.IO.File.Exists(filepath))
            {
                NotificarErro("Já Existe uma arquivo com este nome!");
                return false;
            }
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }
            return true;
        }
        [ClaimsAuthorize("Produto", "Remover")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> DeletarProduto(Guid id)
        {
            var produtoVM = await ObterProduto(id);

            if (produtoVM == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produtoVM);
        }
        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }
    }
}
