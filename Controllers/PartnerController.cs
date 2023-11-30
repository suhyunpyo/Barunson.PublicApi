using Barunson.DbContext;
using Barunson.DbContext.DbModels.BarShop;
using Barunson.PublicApi.Config;
using Barunson.PublicApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Barunson.PublicApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PartnerController : ControllerBase
    {
        private readonly BarunApiJwt _barunApiJwt;
        private readonly BarShopContext _barShopContext;
        public PartnerController(BarunApiJwt barunApiJwt, BarShopContext barShopContext) 
        {
            _barunApiJwt = barunApiJwt;
            _barShopContext = barShopContext;
        }
        /// <summary>
        /// 클라이언트 인증 및 Access Token 발급
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PartnerClientAcessToken>> Authenticate(PartnerClientLogin login)
        {
            ActionResult response = Unauthorized();
            try
            {
                var ipAddr = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString() ?? "UnKnown";
                var client = await AuthenticateClient(login);
                if (client != null)
                {
                    return await GenerateJWTToken(client, ipAddr);
                }
            }
            catch 
            {
                response = BadRequest();
            }
            return response;
        }
        /// <summary>
        /// 클라이언트 인증 및 Access Token 발급
        /// 새로 고침 토큰 사용
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("refresh-token")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PartnerClientAcessToken>> RefreshToken(PartnerClientLoginToken login)
        {
            ActionResult response = Unauthorized();
            try
            {
                var ipAddr = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString() ?? "UnKnown";
                var client = await AuthenticateClient(login);
                if (client != null)
                {
                    return await GenerateJWTToken(client, ipAddr);
                }
            }
            catch
            {
                response = BadRequest();
            }
            return response;
        }

        /// <summary>
        /// 클라이언트 Secret 키 변경(Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("change-secret")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeSecret(PartnerClientChangeSecret model)
        {
            IActionResult response = BadRequest();
            try
            {
                var client = await _barShopContext.PartnerClient
                    .FromSql($"SELECT * FROM PartnerClient Where ClientId = {model.ClientId} and  PWDCOMPARE({model.BeforeClientSecret}, ClientSecret) = 1 and Active = 1")
                    .FirstOrDefaultAsync();
                if (client != null)
                {
                    await _barShopContext.Database.ExecuteSqlAsync($"Update PartnerClient Set ClientSecret = PWDENCRYPT({model.AfterClientSecret}) Where ClientId = {model.ClientId}");
                    response = Ok();
                }
            }
            catch {}

            return response;
        }

        /// <summary>
        /// 클라이언트 정보
        /// </summary>
        /// <returns></returns>
        [Route("info")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PartnerClientInfo>> Info()
        {
            ActionResult response = Unauthorized();
            try
            {
                var clientId = ((ClaimsIdentity)User.Identity!).Claims.Single(m => m.Type == JwtRegisteredClaimNames.Sid).Value;
                var client = await (from a in _barShopContext.PartnerClient
                                    where a.ClientId == clientId && a.Active == true
                                    select new PartnerClientInfo
                                    {
                                        ClientId = a.ClientId,
                                        ClientName = a.ClientName,
                                        Active = a.Active,
                                        RegisterDate = a.RegisterDate
                                    }).FirstOrDefaultAsync();

                return (client != null) ? Ok(client) : NotFound();
            }
            catch
            {
                response = BadRequest();
            }
            return response;
        }
        #region Private Functions
        /// <summary>
        /// 인증 확인, client id & secret 방식
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private async Task<PartnerClient?> AuthenticateClient(PartnerClientLogin login) {

            return await _barShopContext.PartnerClient
                .FromSql($"SELECT * FROM PartnerClient Where ClientId = {login.ClientId} and  PWDCOMPARE({login.ClientSecret}, ClientSecret) = 1 and Active = 1")
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 인증 확인, 새로고침 토큰 방식
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private async Task<PartnerClient?> AuthenticateClient(PartnerClientLoginToken login)
        {
            PartnerClient? client = null;

            var now = DateTime.UtcNow;
            var query = from a in _barShopContext.PartnerClientToken
                        where a.ClientId == login.ClientId && a.RefreshToken == login.RefreshToken
                        && a.Used == false && a.Expiress > now
                        select a;
            var tokenItem = await query.FirstOrDefaultAsync();
            if (tokenItem != null)
            {
                tokenItem.Used = true;
               
                await _barShopContext.SaveChangesAsync();

                client = await (from a in _barShopContext.PartnerClient
                                        where a.ClientId == login.ClientId && a.Active == true
                                        select a).FirstOrDefaultAsync();
                
            }
            return client;
        }

        /// <summary>
        /// Access token 생성
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<PartnerClientAcessToken> GenerateJWTToken(PartnerClient model, string clientIp)
        {
            var now = DateTimeOffset.UtcNow;
            var exp = now.AddHours(1);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_barunApiJwt.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid, model.ClientId),
                new Claim(JwtRegisteredClaimNames.Name, model.ClientName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            var token = new JwtSecurityToken(
                issuer: _barunApiJwt.Issuer,
                audience: _barunApiJwt.Audience,
                claims: claims,
                expires: exp.LocalDateTime,
                signingCredentials: credentials
            );

            return new PartnerClientAcessToken
            {
                ClientId = model.ClientId,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = exp,
                Created = now,
                RefreshToken = await GenerateRefreshToken(model.ClientId, clientIp, now),
                RefreshTokenExpires = now.AddDays(7)
            };
        }

        /// <summary>
        /// 새로고침 토큰 생성
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientIp"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private async Task<string> GenerateRefreshToken(string clientId, string clientIp, DateTimeOffset now)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenInfo = new PartnerClientToken
            {
                ClientId = clientId,
                Created = now.DateTime,
                RefreshToken = token,
                Expiress = now.AddDays(7).DateTime,
                ClientIp = clientIp,
                Used = false
            };
            _barShopContext.PartnerClientToken.Add(refreshTokenInfo);

            //만료된 토큰 제거 (데이터 유지 불필요)
            var rmTokens = from m in _barShopContext.PartnerClientToken
                           where m.ClientId == clientId && (m.Used == true || m.Expiress < now)
                           select m;
            _barShopContext.PartnerClientToken.RemoveRange(rmTokens);

            await _barShopContext.SaveChangesAsync();

            return token;
        }
        #endregion
    }
}
