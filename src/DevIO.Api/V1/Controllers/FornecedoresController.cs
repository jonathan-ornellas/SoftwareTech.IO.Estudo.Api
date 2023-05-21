using System.Runtime.CompilerServices;
using AutoMapper;
using DevIO.Api.DTO;
using DevIO.Api.Extensions;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using DevIO.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwareTech.IO.Api.Controllers;

namespace SoftwareTech.IO.Api.V1.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/fornecedores")]
public class FornecedoresController : MainController
{

    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly IMapper _mapper;
    private readonly IFornecedorService _fornecedorService;
    private readonly IEnderecoRepository _enderecoRepository;
    public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper, IFornecedorService fornecedorService, INotificador notificador, IEnderecoRepository enderecoRepository, IUser user) : base(notificador, user)
    {
        _fornecedorRepository = fornecedorRepository;
        _mapper = mapper;
        _fornecedorService = fornecedorService;
        _enderecoRepository = enderecoRepository;
    }

    [HttpGet]
    public async Task<IEnumerable<FornecedorDTO>> ObterTodos()
    {
        var fornecedor = _mapper.Map<IEnumerable<FornecedorDTO>>(await _fornecedorRepository.ObterTodos());


        return fornecedor;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FornecedorDTO>> ObterPorId(Guid id)
    {
        var fornecedor = await ObtFornecedorProdutoEndereco(id);

        if (fornecedor == null) return NotFound();

        return Ok(fornecedor);
    }

    [ClaimsAuthorize("Fornecedor", "Adicionar")]
    [HttpPost]
    public async Task<ActionResult<FornecedorDTO>> Adicionar(FornecedorDTO fornecedorDTO)
    {


        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var fornecedor = _mapper.Map<Fornecedor>(fornecedorDTO);
        await _fornecedorService.Adicionar(fornecedor);

        return CustomResponse(fornecedor);

    }

    [ClaimsAuthorize("Fornecedor", "Atualizar")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FornecedorDTO>> Atualizar(Guid id, FornecedorDTO fornecedorDTO)
    {
        if (id != fornecedorDTO.Id)
        {
            NotificarError("O id informado não é o mesmo que foi passado na query");
            return CustomResponse(fornecedorDTO);
        }

        if (!ModelState.IsValid) return CustomResponse(ModelState);


        await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorDTO));

        return CustomResponse(fornecedorDTO);

    }

    [ClaimsAuthorize("Fornecedor", "Excluir")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<FornecedorDTO>> Excluir(Guid id)
    {
        var fornecedor = await ObtFornecedorEndereco(id);

        if (fornecedor == null) return NotFound();

        await _fornecedorService.Remover(id);

        return CustomResponse();
    }

    [HttpGet("endereco/{id:guid}")]
    public async Task<EnderecoDTO> ObterEnderecoPorId(Guid id)
    {
        return _mapper.Map<EnderecoDTO>(await _enderecoRepository.ObterPorId(id));
    }

    [ClaimsAuthorize("Fornecedor", "Atualizar")]
    [HttpPut("endereco/{id:guid}")]
    public async Task<IActionResult> AtualizarEndereco(Guid id, EnderecoDTO enderecoDto)
    {
        if (id != enderecoDto.Id)
        {
            NotificarError("O id informado não é o mesmo que foi passado na query");
            return CustomResponse(enderecoDto);
        }

        if (!ModelState.IsValid) return CustomResponse(ModelState);

        await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoDto));

        return CustomResponse(enderecoDto);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpHead("{id:guid}")]
    private async Task<FornecedorDTO> ObtFornecedorProdutoEndereco(Guid id)
    {
        return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpHead("{id:guid}")]
    private async Task<FornecedorDTO> ObtFornecedorEndereco(Guid id)
    {
        return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorEndereco(id));
    }
}