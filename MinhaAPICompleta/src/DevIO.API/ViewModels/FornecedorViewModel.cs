using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevIO.API.ViewModels
{
    public class FornecedorViewModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O campo {0} precisa ser preenchido!")]
        [StringLength(100, ErrorMessage ="O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 2)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O campo {0} precisa ser preenchido!")]
        [StringLength(14, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 11)]
        public string Documento { get; set; }
        public int TipoFornecedor { get; set; }
        public EnderecoViewModel Endereco { get; set; }
        public bool Ativo { get; set; }

        /* EF Relations */
        public IEnumerable<ProdutoViewModel> Produtos { get; set; }
    }
}
