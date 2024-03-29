﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Interfaces;
using NSE.Core.Messages.Integration;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Models;
using NSE.MessageBus;
using NSE.WebAPI.Core.Controllers;
using NSE.WebAPI.Core.Identidade;
using NSE.WebAPI.Core.Usuario;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NSE.Identidade.API.Controllers
{
    [Route("api/identidade")]
    public class AuthController : MainController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppTokenSettings _appTokenSettings;
        private readonly AppSettings _appSettings;
        private readonly IMessageBus _bus;
        private readonly IAspNetUser _aspNetUser;
        private readonly IJsonWebKeySetService _jwksService;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AppSettings appSettings, IMessageBus bus, IAspNetUser aspNetUser, IJsonWebKeySetService jwksService, AppTokenSettings appTokenSettings, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings;
            _bus = bus;
            _aspNetUser = aspNetUser;
            _jwksService = jwksService;
            _appTokenSettings = appTokenSettings;
            _context = context;
        }

        [HttpPost("nova-conta")]
        public async Task<IActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha);

            if (result.Succeeded)
            {
                var clienteResult = await RegistrarCliente(usuarioRegistro);

                if (!clienteResult.ValidationResult.IsValid)
                {
                    await _userManager.DeleteAsync(user);
                    return CustomResponse(clienteResult);
                }

                //await _signInManager.SignInAsync(user, isPersistent: false); //nao existe necessidade de fazer login na criacao de usuario
                return CustomResponse(await GerarJwt(usuarioRegistro.Email));
            }

            foreach (var erro in result.Errors)
            {
                AdicionarErroProcessamento(erro.Description);
            }

            return CustomResponse();

        }

        //Implementando o Request para o RabbitMQ
        private async Task<ResponseMessage> RegistrarCliente(UsuarioRegistro usuarioRegistro)
        {
            var usuario = await _userManager.FindByEmailAsync(usuarioRegistro.Email);

            var usuarioRegistrado = new UsuarioRegistradoIntegrationEvent(
                Guid.Parse(usuario.Id), usuarioRegistro.Nome, usuarioRegistro.Email, usuarioRegistro.Cpf);

            try
            {
                return await _bus.RequestAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(usuarioRegistrado);
            }
            catch
            {
                await _userManager.DeleteAsync(usuario);
                throw;
            }
        }

        [HttpPost("autenticar")]
        public async Task<IActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, false, true);

            if (result.Succeeded)
            {
                return CustomResponse(await GerarJwt(usuarioLogin.Email));
            }

            if (result.IsLockedOut)
            {
                AdicionarErroProcessamento("Usuario bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            AdicionarErroProcessamento("Usuario ou Senha incorretos");
            return CustomResponse();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                AdicionarErroProcessamento("Refresh Token inválido");
                return CustomResponse();
            }

            RefreshToken? token = await ObterRefreshToken(refreshToken);

            if (token is null)
            {
                AdicionarErroProcessamento("Refresh Token expirado");
                return CustomResponse();
            }

            return CustomResponse(await GerarJwt(token.Username));
        }

        private async Task<RefreshToken> ObterRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens.AsNoTracking()
           .FirstOrDefaultAsync(u => u.Token == Guid.Parse(refreshToken));

            token = token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now
               ? token
               : null;
            return token;
        }

        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            var user = await _userManager.FindByNameAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);

            var identityClaims = await ObterClaimsUsuarios(user, claims);
            var encodedToken = CodificarToken(identityClaims);

            var refreshToken = GerarRefreshToken(email);

            return ObterRespostaToken(encodedToken, user, claims, refreshToken.Result);
        }

        private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, IdentityUser user, IList<Claim> claims, RefreshToken refreshToken)
        {
            var response = new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = TimeSpan.FromHours(2).TotalSeconds,
                UsuarioToken = new UsuarioToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        private string CodificarToken(ClaimsIdentity identityClaims)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(_appSettings?.Secret) ? "MYSUPERMEGAHIDDENSECRETKEY" : _appSettings?.Secret);
            var key = _jwksService.GetCurrentSecurityKey();

            var currentIssuer = $"{_aspNetUser.ObterHttpContext().Request.Scheme}://{_aspNetUser.ObterHttpContext().Request.Host}";

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = string.IsNullOrEmpty(currentIssuer) ? "NerdStoreEnterprise" : currentIssuer,
                //Audience = string.IsNullOrEmpty(_appSettings.ValidoEm) ? "https://localhost" : _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(2),
                //SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                SigningCredentials = new SigningCredentials(key.Result, SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        private async Task<ClaimsIdentity> ObterClaimsUsuarios(IdentityUser user, IList<Claim> claims)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            //tipos especificos de claims para JWT
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));


            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private async Task<RefreshToken> GerarRefreshToken(string email)
        {
            var refreshToken = new RefreshToken
            {
                Username = email,
                ExpirationDate = DateTime.UtcNow.AddHours(_appTokenSettings.RefreshTokenExpiration)
            };

            _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(u => u.Username == email));
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();

            return refreshToken;
        }

        //public async Task<RefreshToken> ObterRefreshToken(Guid refreshToken)
        //{
        //    var token = await _context.RefreshTokens.AsNoTracking()
        //        .FirstOrDefaultAsync(u => u.Token == refreshToken);

        //    return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now
        //        ? token
        //        : null;
        //}
    }
}
