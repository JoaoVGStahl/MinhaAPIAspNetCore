using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevIO.API.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "O Campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O Campo {0} está em um formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage ="O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage ="O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength =8)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage ="As senhas não conferem")]
        public string confirmPassword { get; set; }
    }
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "O Campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O Campo {0} está em um formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 8)]
        public string Password { get; set; }
    }
    public class LoginResponseViewModel
    {
        public string AcessToken { get; set; }

        public double ExpiresIn { get; set; }

        public UserTokenViewModel UserToken { get; set; }
    }
    public class UserTokenViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public IEnumerable<ClaimViewModel> Claims { get; set; }
    }
    public class ClaimViewModel
    {
        public string Value { get; set; }

        public string Type { get; set; }
    }
}
