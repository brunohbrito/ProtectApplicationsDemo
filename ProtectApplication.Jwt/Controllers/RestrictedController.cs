using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials.Interfaces;
using System.Linq;

namespace ProtectApplication.Jwt.Controllers
{
    [ApiController]
    [Route("restricted")]
    public class RestrictedController : Controller
    {
        private readonly IJsonWebKeySetService _jwksService;

        public RestrictedController(IJsonWebKeySetService jwksService)
        {
            _jwksService = jwksService;
        }
        [Authorize, HttpGet]
        public IActionResult Index()
        {
            return Ok("Must logged in to see this info");
        }

        [Authorize(Roles = "Managers"), HttpGet("role-based")]
        public IActionResult RoleBased()
        {
            return Ok("Only manager can see");
        }

        [Authorize(Policy = "ManagerOnly"), HttpGet("claim-based")]
        public IActionResult ClaimBased()
        {
            return Ok("Must have claim EmployeeFuction and value Manager");
        }

        [Authorize(Policy = "ManagerFromSalesDepartment"), HttpGet("policy-based")]
        public IActionResult PolicyBased()
        {
            return Ok("Must have claim EmployeeFuction and value Manager and from Sales Department");
        }

        [AllowAnonymous, HttpGet("decrypt-jwt")]
        public IActionResult DecryptAccessToken(string at)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var currentIssuer = $"{ControllerContext.HttpContext.Request.Scheme}://{ControllerContext.HttpContext.Request.Host}";
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidIssuer = currentIssuer,
                ValidateAudience = false,

            };
            var storedJwk = _jwksService.GetCurrent();
            tokenValidationParams.IssuerSigningKey = storedJwk.Key;
            var validationResult = tokenHandler.ValidateToken(at, tokenValidationParams);

            return Ok(validationResult.ClaimsIdentity.Claims.Select(s => new { type = s.Type, value = s.Value }));
        }

    }

}
