using AutoMapper;
using DevIO.API.ViewModels;
using DevIO.Business.Models;

namespace DevIO.API.Extensions
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<ProdutoViewModel,Produto>();    
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap(); 
            CreateMap<ProdutoImagemViewModel, Produto>().ReverseMap();

            CreateMap<Produto, ProdutoViewModel>().ForMember(dest =>dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}
