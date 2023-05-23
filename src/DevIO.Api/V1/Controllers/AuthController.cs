using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevIO.Api.DTO;
using DevIO.Api.Extensions;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoftwareTech.IO.Api.Controllers;

namespace SoftwareTech.IO.Api.V1.Controllers;


[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class AuthController : MainController
{

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppSetting _appSetting;
    private readonly ILogger _logger;
    public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppSetting> appSetting, IUser user, ILogger<AuthController> logger) : base(notificador, user)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _appSetting = appSetting.Value;
    }


    [HttpPost("nova-conta")]
    public async Task<ActionResult> Registrar(RegisterUserDTO registerUser)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var user = new IdentityUser
        {
            UserName = registerUser.Email,
            Email = registerUser.Email,
            EmailConfirmed = true
        };


        var result = await _userManager.CreateAsync(user, registerUser.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, false);
            return CustomResponse(await GerarJwt(user.Email));
        }
        foreach (var error in result.Errors)
        {
            NotificarError(error.Description);
        }

        return CustomResponse(registerUser);
    }


    [HttpPost("entrar")]
    public async Task<ActionResult> Login(LoginUserDTO loginUser)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario " + loginUser.Email + " logado com sucesso!");
            return CustomResponse(await GerarJwt(loginUser.Email));
        }

        if (result.IsLockedOut)
        {
            NotificarError("Usuário temporariamente bloqueado por tentativas inválidas");
            return CustomResponse(loginUser);
        }

        NotificarError("Usuário ou senha incorretos");
        return CustomResponse(loginUser);
    }

    private async Task<LoginResponseDTO> GerarJwt(string email)
    {

        var user = await _userManager.FindByEmailAsync(email);
        var claims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

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

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_appSetting.Secret);
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _appSetting.Emissor,
            Audience = _appSetting.ValidoEm,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(_appSetting.ExpiracaoHoras),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

        });

        var encodedToken = tokenHandler.WriteToken(token);

        var response = new LoginResponseDTO
        {
            AccessToken = encodedToken,
            ExpiresIn = TimeSpan.FromHours(_appSetting.ExpiracaoHoras).TotalSeconds,
            UserToken = new UserTokenDTO
            {
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(c => new ClaimDTO { Type = c.Type, Value = c.Value })
            }
        };

        return response;
    }

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}