using Barunson.DbContext;
using Barunson.DbContext.DbModels.BarShop;
using Barunson.DbContext.DbModels.DearDeer;
using Barunson.PublicApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Barunson.PublicApi.Controllers
{
    /// <summary>
    /// 멤플러스 전용
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MemPlusController : ControllerBase
    {
        private readonly BarShopContext _barShopContext;
        private readonly string AllowClientId = "MemPlus";

        public MemPlusController(BarShopContext barShopContext)
        {
            _barShopContext = barShopContext;
        }

        /// <summary>
        /// 제휴 회원 조회
        /// </summary>
        /// <param name="date">
        /// 조회 날짜: yyyyMMdd, yyyy-MM-dd 모두 허용
        /// </param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MemPlusReponse>> Get(string? date)
        {
            ActionResult response = BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = "잘못된 요청." });
            try
            {
                var clientId = ((ClaimsIdentity)User.Identity!).Claims.Single(m => m.Type == JwtRegisteredClaimNames.Sid).Value;
                if (clientId == AllowClientId)
                {
                    if (date == null)
                        return BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = "날짜 정보가 없습니다." });

                    string[] formats = new []{ "yyyyMMdd", "yyyy-MM-dd" };
                    DateTime toDate;
                    if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                        return BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = $"날짜 정보가 잘못 되었습니다. {date}" });

                    var fromDate = toDate.AddDays(-1);

                    var model = new MemPlusReponse
                    {
                        RequestDate = toDate.ToString("yyyy-MM-dd")
                    };
                    //오늘 부터  1년 전까지 조회 가능
                    if (toDate <= DateTime.Now.Date && toDate > DateTime.Now.Date.AddYears(-1))
                    {
                        #region 바른손

                        var SbQuery = from r in _barShopContext.S4_Event_Raina
                                      join b in _barShopContext.S2_UserInfo_BHands on r.uid equals b.uid
                                      join c in _barShopContext.EVENT_MARKETING_AGREEMENT on r.uid equals c.uid into cc
                                      from d in cc.DefaultIfEmpty()
                                      where r.reg_date >= fromDate && r.reg_date < toDate
                                      && r.event_div == "MKevent"
                                      && !_barShopContext.S4_Event_Raina.Any(x => x.event_div == "MKevent" && x.uid == r.uid && x.reg_date < fromDate)
                                      select new
                                      {
                                          r.uid,
                                          b.uname,
                                          b.umail,
                                          b.BirthDate,
                                          b.Gender,
                                          b.phone1,
                                          b.phone2,
                                          b.phone3,
                                          b.hand_phone1,
                                          b.hand_phone2,
                                          b.hand_phone3,
                                          b.zip1,
                                          b.zip2,
                                          b.address,
                                          b.addr_detail,
                                          b.reg_date,
                                          b.wedd_year,
                                          b.wedd_month,
                                          b.wedd_day,
                                          b.SELECT_SALES_GUBUN,
                                          b.REFERER_SALES_GUBUN,
                                          b.ConnInfo,
                                          SettleAgreeDate = _barShopContext.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Where(x => x.UID == r.uid).Max(m => m.REG_DATE),
                                          CommYN = _barShopContext.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Any(x => x.UID == r.uid && x.MARKETING_TYPE_CODE == "119001"),
                                          Shinhan = _barShopContext.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Any(x => x.UID == r.uid && x.MARKETING_TYPE_CODE == "119006"),
                                          Kyobo = _barShopContext.S2_USERINFO_THIRD_PARTY_MARKETING_AGREEMENT.Any(x => x.UID == r.uid && x.MARKETING_TYPE_CODE == "119008"),
                                          mkAgreeId = (string?)d.uid,
                                          mkGubun = d.gubun
                                      };

                        var SbItems = await SbQuery.ToListAsync();
                        foreach (var item in SbItems)
                        {
                            var bday = string.Concat(item.BirthDate, item.Gender);
                            if (!string.IsNullOrEmpty(bday) && bday.Length == 9)
                                bday = bday.Substring(2, 7);

                            model.Agreements.Add(new MemPlusCustomer
                            {
                                Name = item.uname,
                                Bday = bday,
                                TelNo = $"{item.phone1}-{item.phone2}-{item.phone3}",
                                MobileTelNo = $"{item.hand_phone1}-{item.hand_phone2}-{item.hand_phone3}",
                                ZipCode = $"{item.zip1}-{item.zip2}",
                                Address = item.address,
                                AddressDetail = item.addr_detail,
                                RegisterDate = (item.SettleAgreeDate ?? item.reg_date).ToString("yyyyMMdd"),
                                RegisterTime = (item.SettleAgreeDate ?? item.reg_date).ToString("HHmmss"),
                                EMail = item.umail,
                                ci = item.ConnInfo,
                                UserId = item.uid,
                                RegisterSite = SiteName(string.IsNullOrEmpty(item.SELECT_SALES_GUBUN) ? item.REFERER_SALES_GUBUN : item.SELECT_SALES_GUBUN),
                                BirthDay = item.BirthDate,
                                WeddYear = item.wedd_year,
                                WeddMonth = item.wedd_month,
                                WeddDay = item.wedd_day,
                                MarketYN = "Y",
                                CommYN = item.CommYN ? "Y" : "N",
                                JoinType = JoinType(item.CommYN, item.mkAgreeId, item.mkGubun),
                                Shinhan = item.Shinhan ? "Y" : "N",
                                Kyobo = item.Kyobo ? "Y" : "N"
                            });
                        }

                        #endregion

                        #region 디얼디어
                        var DDQuery = from m in _barShopContext.MEMPLUS_DAILY_INFO
                                      join b in _barShopContext.S2_UserInfo_Deardeer on m.uid equals b.uid
                                      join c in _barShopContext.EVENT_MARKETING_AGREEMENT on m.uid equals c.uid into cc
                                      from d in cc.DefaultIfEmpty()
                                      where m.file_dt == toDate.ToString("yyyy-MM-dd") && m.site_div == "SD"
                                      select new
                                      {
                                          m.uid,
                                          b.uname,
                                          b.umail,
                                          b.birth,
                                          b.gender,
                                          b.phone1,
                                          b.phone2,
                                          b.phone3,
                                          b.hand_phone1,
                                          b.hand_phone2,
                                          b.hand_phone3,
                                          b.zip1,
                                          b.zip2,
                                          b.address,
                                          b.addr_detail,
                                          b.reg_date,
                                          b.wedd_year,
                                          b.wedd_month,
                                          b.wedd_day,
                                          b.conninfo,
                                          SettleAgreeDate = _barShopContext.S2_UserInfo_Deardeer_Marketing.Where(x => x.uid == m.uid && x.agreement_type == "MEMPLUS").Max(m => m.agree_date),
                                          CommYN = _barShopContext.S2_UserInfo_Deardeer_Marketing.Any(x => x.uid == m.uid && x.agreement_type == "MEMPLUS_COMM"),
                                          Shinhan = _barShopContext.S2_UserInfo_Deardeer_Marketing.Any(x => x.uid == m.uid && x.agreement_type == "MEMPLUS_INSURA"),
                                          mkGubun = d.gubun
                                      };
                        var ddItems = await DDQuery.ToListAsync();

                        foreach (var item in ddItems)
                        {
                            var bday = string.Concat(item.birth, item.gender);
                            if (!string.IsNullOrEmpty(bday) && bday.Length == 9)
                                bday = bday.Substring(2, 7);

                            model.Agreements.Add(new MemPlusCustomer
                            {
                                Name = item.uname,
                                Bday = bday,
                                TelNo = $"{item.phone1}-{item.phone2}-{item.phone3}",
                                MobileTelNo = $"{item.hand_phone1}-{item.hand_phone2}-{item.hand_phone3}",
                                ZipCode = $"{item.zip1}-{item.zip2}",
                                Address = item.address,
                                AddressDetail = item.addr_detail,
                                RegisterDate = item.SettleAgreeDate!.Value.ToString("yyyyMMdd"),
                                RegisterTime = item.SettleAgreeDate!.Value.ToString("HHmmss"),
                                EMail = item.umail,
                                ci = item.conninfo,
                                UserId = item.uid,
                                RegisterSite = SiteName("SD"),
                                BirthDay = item.birth,
                                WeddYear = item.wedd_year,
                                WeddMonth = item.wedd_month,
                                WeddDay = item.wedd_day,
                                MarketYN = "Y",
                                CommYN = item.CommYN ? "Y" : "N",
                                JoinType = JoinType(item.CommYN, null, item.mkGubun),
                                Shinhan = item.Shinhan ? "Y" : "N",
                                Kyobo = "N"
                            });
                        }

                        #endregion

                        #region 취소
                        var cancelQuery = from m in _barShopContext.MEMPLUS_DAILY_INFO_CANCEL
                                          where m.file_dt == toDate.ToString("yyyy-MM-dd")
                                          select new MemPlusCancel
                                          {
                                              MobileTelNo = m.hphone,
                                              UserId = m.uid
                                          };
                        model.Cancels = await cancelQuery.Distinct().ToListAsync();
                        #endregion
                    }
                    if (model.Agreements.Count == 0 && model.Cancels.Count == 0)
                        return NotFound(model);
                    else
                        return model;
                }
                else
                {
                    response = Unauthorized();
                }
            }
            catch
            {
                response = BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = "잘못된 요청." });
            }
            return response;
        }
        private string JoinType(bool commYn, string? mkAgreeId, string? mkGubun)
        {
            var jtype = "";
            if (commYn)
            {
                if (string.IsNullOrEmpty(mkAgreeId))
                    jtype = "1";
                else
                {
                    if (mkGubun == "S")
                        jtype = "3";
                    else if (string.IsNullOrEmpty(mkGubun)) 
                        jtype = "2";
                }
            }
            return jtype;
        }

        private string SiteName(string siteCode)
        {
            var siteName = "";
            switch (siteCode)
            {
                case "SA":
                    siteName = "비핸즈";
                    break;
                case "SB":
                    siteName = "바른손";
                    break;
                case "ST":
                    siteName = "더카드";
                    break;
                case "SS":
                    siteName = "프리미어페이퍼";
                    break;
                case "SD":
                    siteName = "디얼디어";
                    break;
                default:
                    siteName = "바른손몰";
                    break;

            }
            return siteName;
        }
    }
}
