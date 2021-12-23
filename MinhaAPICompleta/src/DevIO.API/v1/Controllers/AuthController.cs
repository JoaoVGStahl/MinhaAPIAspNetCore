using DevIO.API.Controllers;
using DevIO.API.Extensions;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.API.v1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class AuthController : MainController
    {

        private readonly SignInManager<IdentityUser> _signManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        public AuthController(INotificador notificador,
                              SignInManager<IdentityUser> signManager,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings,
                              IUser user, 
                              ILogger<AuthController> logger) : base(notificador, user)
        {
            _signManager = signManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        [HttpPost("Resgiter")]
        public async Task<ActionResult> Register(RegisterUserViewModel newUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = newUser.Email,
                Email = newUser.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, newUser.Password);

            if (result.Succeeded)
            {
                await _signManager.SignInAsync(user, false);
                return CustomResponse(await GerarJWT(user.Email));
            }
            foreach (var erro in result.Errors)
            {
                NotificarErro(erro.Description);
            }

            return CustomResponse(newUser);
        }
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginUserViewModel login)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signManager.PasswordSignInAsync(login.Email, login.Password, false, true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário " + login.Email + " logado com sucesso!");
                return CustomResponse(await GerarJWT(login.Email));
            }
            if (result.IsLockedOut)
            {
                NotificarErro("Usuário Temporariamente bloqueador por execesso de tentativas");
                return CustomResponse(login);
            }
            NotificarErro("Usuário E/ou senha incorretos");
            return CustomResponse(login);
        }

        private async Task<LoginResponseViewModel> GerarJWT(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var identityClaims = IdentityClaimsUser(user, claims, userRoles);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);
            return CreateResponseObject(user, claims, encodedToken);
        }
        private ClaimsIdentity IdentityClaimsUser(IdentityUser user, IList<Claim> claims, IList<string> userRoles)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Id do Token
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString())); // Not Valida Before => Não é valido antes desta data
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)); // Quando foi emitido
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }
            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);
            return identityClaims;
        }
        private LoginResponseViewModel CreateResponseObject(IdentityUser user, IList<Claim> claims, string encodedToken)
        {
            var response = new LoginResponseViewModel
            {
                AcessToken = encodedToken,//Jwt
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel
                    {
                        Type = c.Type,
                        Value = c.Value
                    })
                }
            };
            return response;
        }
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

    }
}
