using AutoMapper;
using DevIO.Api.DTO;
using DevIO.Api.Extensions;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;
using SoftwareTech.IO.Api.Controllers;

namespace SoftwareTech.IO.Api.V1.Controllers;


[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/produtos")]
public class ProdutoController : MainController
{

    private readonly IProdutoRepository _produtoRepository;
    private readonly IProdutoService _produtoService;
    private readonly IMapper _mapper;
    public ProdutoController(INotificador notificador, IProdutoRepository produtoRepository, IProdutoService produtoService, IMapper mapper, IUser user) : base(notificador, user)
    {
        _produtoRepository = produtoRepository;
        _produtoService = produtoService;
        _mapper = mapper;
    }



    [HttpGet]
    public async Task<IEnumerable<ProdutoDTO>> ObterTodos()
    {
        return _mapper.Map<IEnumerable<ProdutoDTO>>(await _produtoRepository.ObterProdutosFornecedores());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProdutoDTO>> ObterPorId(Guid id)
    {
        var produtoDto = await ObterProduto(id);

        if (produtoDto == null) return NotFound();

        return produtoDto;
    }

    [ClaimsAuthorize("Produto", "Adicionar")]
    [HttpPost]
    [RequestSizeLimit(25000000)]
    public async Task<ActionResult<ProdutoDTO>> Adicionar(
        // Binder personalizado para envio de IFormFile e ViewModel dentro de um FormData compatível com .NET Core 3.1 ou superior (system.text.json)
        [ModelBinder(BinderType = typeof(ProdutoModelBinder))]
        ProdutoDTO produtoDto)
    {


        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var imgPrefix = Guid.NewGuid() + "_";

        if (!await UploadArquivo(produtoDto.ImagemUpload, imgPrefix))
        {
            return CustomResponse(produtoDto);
        }

        produtoDto.Imagem = imgPrefix + produtoDto.ImagemUpload.FileName;

        await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

        return CustomResponse(produtoDto);
    }


    [ClaimsAuthorize("Produto", "Atualizar")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.Id)
        {
            NotificarError("Os ids informados não são iguais!");
            return CustomResponse();
        }

        var produtoAtualizacao = await ObterProduto(id);

        if (string.IsNullOrEmpty(produtoDto.Imagem))
            produtoDto.Imagem = produtoAtualizacao.Imagem;

        if (!ModelState.IsValid) return CustomResponse(ModelState);

        if (produtoDto.ImagemUpload != null)
        {
            var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;
            if (!await UploadArquivo(produtoDto.ImagemUpload, imagemNome))
            {
                return CustomResponse(ModelState);
            }

            produtoAtualizacao.Imagem = imagemNome;
        }

        produtoAtualizacao.FornecedorId = produtoDto.FornecedorId;
        produtoAtualizacao.Nome = produtoDto.Nome;
        produtoAtualizacao.Descricao = produtoDto.Descricao;
        produtoAtualizacao.Valor = produtoDto.Valor;
        produtoAtualizacao.Ativo = produtoDto.Ativo;

        await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

        return CustomResponse(produtoDto);
    }



    [ClaimsAuthorize("Produto", "Excluir")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ProdutoDTO>> Excluir(Guid id)
    {
        var produto = await ObterProduto(id);

        if (produto == null) return NotFound();

        await _produtoService.Remover(id);

        return CustomResponse(produto);
    }


    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpHead("{id:guid}")]
    private async Task<ProdutoDTO> ObterProduto(Guid id)
    {
        return _mapper.Map<ProdutoDTO>(await _produtoRepository.ObterProdutoFornecedor(id));
    }

    private async Task<bool> UploadArquivo(IFormFile arquivo, string imgPrefixo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            NotificarError("Forneça uma imagem para este produto!");
            return false;
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

        if (System.IO.File.Exists(filePath))
        {
            NotificarError("Já existe um arquivo com este nome!");
            return false;
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        return true;
    }
}