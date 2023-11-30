namespace Barunson.PublicApi.Config
{
    public class BarunApiJwt
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Authority { get; set; } = "";
       
        public string SecretKey { get; set; } = "";
    }
}
