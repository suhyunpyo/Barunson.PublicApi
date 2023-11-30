using Barunson.PublicApi.Filter;
using System.Text.Json.Serialization;

namespace Barunson.PublicApi.Model
{
    /// <summary>
    /// 접속 파트너 클라이언트 로그인모델
    /// </summary>
    public class PartnerClientLogin
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
    /// <summary>
    /// 접속 파트너 클라이언트 로그인모델, 새로고침 토큰
    /// </summary>
    public class PartnerClientLoginToken
    {
        public string ClientId { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
    /// <summary>
    ///  접속 파트너 클라이언트 비밀번호 변경
    /// </summary>
    public class PartnerClientChangeSecret
    {
        public string ClientId { get; set; } = string.Empty;
        /// <summary>
        /// 이전 비번
        /// </summary>
        public string BeforeClientSecret { get; set; } = string.Empty;
        /// <summary>
        /// 변경 비번
        /// </summary>
        public string AfterClientSecret { get; set; } = string.Empty;
    }
    /// <summary>
    /// 접속 파트너 클라이언트
    /// </summary>
    public class PartnerClientInfo
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;

        [JsonConverter(typeof(CustomDateTimeOffsetConverter))]
        public DateTimeOffset? RegisterDate { get; set; }
        public bool Active { get; set; }
    }

    /// <summary>
    /// 파트너 클라이언트 AcessToken
    /// </summary>
    public class PartnerClientAcessToken
    {
        public string ClientId { get; set; } = string.Empty;
        /// <summary>
        /// AcessToken 문자
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 만료 일자
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeOffsetConverter))]
        public DateTimeOffset Expires { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 생성 일자
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeOffsetConverter))]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 새로고침 토큰
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// 새로고침 토큰 만료 일자
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeOffsetConverter))]
        public DateTimeOffset RefreshTokenExpires { get; set; } = DateTimeOffset.UtcNow;
    }
}
