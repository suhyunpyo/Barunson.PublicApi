namespace Barunson.PublicApi.Model
{
    /// <summary>
    /// 멤플러스 응답
    /// </summary>
    public class MemPlusReponse
    {
        public string RequestDate { get; set; } = "";
        public List<MemPlusCustomer> Agreements { get; set; } = new List<MemPlusCustomer>();
        public List<MemPlusCancel> Cancels { get; set; } = new List<MemPlusCancel>();

    }
    /// <summary>
    /// 멤플러스 회원 모델
    /// </summary>
    public class MemPlusCustomer
    {
        /// <summary>
        /// 고객명 1
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 생년월일(birth+Gender) 2
        /// SUBSTRING(birth+Gender,3,7)
        /// </summary>
        public string Bday { get; set; } = "";
        /// <summary>
        /// 전화번호 3
        /// </summary>
        public string TelNo { get; set; } = "";
        /// <summary>
        /// 휴대폰번호 4
        /// </summary>
        public string MobileTelNo { get; set; } = "";
        /// <summary>
        /// 우편번호 5
        /// </summary>
        public string ZipCode { get; set; } = "";
        /// <summary>
        /// 주소 6
        /// </summary>
        public string Address { get; set; } = "";
        /// <summary>
        /// 상세주소 7
        /// </summary>
        public string AddressDetail { get; set; } = "";
        /// <summary>
        /// 등록일자 8
        /// yyyyMMdd
        /// </summary>
        public string RegisterDate { get; set; } = "";
        /// <summary>
        /// 등록시간 9
        /// HHmmss
        /// </summary>
        public string RegisterTime { get; set; } = "";
        /// <summary>
        /// 이메일 10
        /// </summary>
        public string EMail { get; set; } = "";
        /// <summary>
        /// ci 11
        /// </summary>
        public string ci { get; set; } = "";
        /// <summary>
        /// User ID 12
        /// </summary>
        public string UserId { get; set; } = "";
        /// <summary>
        /// 가입사이트 13
        /// </summary>
        public string RegisterSite { get; set; } = "";
        /// <summary>
        /// 생년월일 14
        /// yyyyMMdd
        /// </summary>
        public string BirthDay { get; set; } = "";
        /// <summary>
        /// 예식일자(년) 15
        /// </summary>
        public string WeddYear { get; set; } = "";
        /// <summary>
        /// 예식일자(월) 16
        /// </summary>
        public string WeddMonth { get; set; } = "";
        /// <summary>
        /// 예식일자(일) 17
        /// </summary>
        public string WeddDay { get; set; } = "";
        /// <summary>
        /// 마케팅여부 18
        /// </summary>
        public string MarketYN { get; set; } = "";
        /// <summary>
        /// 통신여부 19
        /// </summary>
        public string CommYN { get; set; } = "";
        /// <summary>
        /// 참여영역구분 20
        /// 1,2,3 or empty
        /// </summary>
        public string JoinType { get; set;} = "";
        /// <summary>
        /// 신한생명 21
        /// Y or N
        /// </summary>
        public string Shinhan { get; set; } = "";
        /// <summary>
        /// 교보생명 22
        /// </summary>
        public string Kyobo { get; set; } = "";
    }

    public class MemPlusCancel
    {
        /// <summary>
        /// 휴대폰번호 1
        /// </summary>
        public string MobileTelNo { get; set; } = "";
        /// <summary>
        /// User ID 2
        /// </summary>
        public string UserId { get; set; } = "";
    }
}
