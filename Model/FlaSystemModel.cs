using System.Text.Json.Serialization;

namespace Barunson.PublicApi.Model
{
    /// <summary>
    /// 주문정보
    /// </summary>
    public class FlaOrderInfo
    {
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 신랑정보
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public FlaPersonInfo? GroomInfo { get; set; } 

        /// <summary>
        /// 신부정보
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public FlaPersonInfo? BrideInfo { get; set; }

        /// <summary>
        /// 결혼 날짜시간
        /// yyyy-MM-dd {오전:오후} hh:mm
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? WeddingDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? WeddingHall { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? WeddingHallDetal { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? WeddingHallAddress { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? WeddingHallPhone { get; set; }

        public string Message { get; set; } = string.Empty;
    }
    /// <summary>
    /// 인물 정보
    /// </summary>
    public class FlaPersonInfo
    {
        /// <summary>
        /// 호칭
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// 이름
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 전화번호
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 부모정보
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<FlaPersonInfo>? Parents { get; set; }
    }
}
