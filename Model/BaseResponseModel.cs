namespace Barunson.PublicApi.Model
{
    public class BaseResponseModel
    {
        public int status {  get; set; } = 200;
        public string? message { get; set; }
    }
}
