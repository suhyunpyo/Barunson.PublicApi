using Azure;
using Barunson.DbContext;
using Barunson.PublicApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Barunson.PublicApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FlaSystemController : ControllerBase
    {
        private readonly BarunsonContext _dbContext;
        private readonly DearDeerContext _deardeerDbContext;
        private readonly string AllowClientId = "flasystem";

        public FlaSystemController(BarunsonContext barunsonContext, DearDeerContext deardeerDbContext)
        {
            _dbContext = barunsonContext;
            _deardeerDbContext = deardeerDbContext;
        }

        /// <summary>
        /// 모초 주문 정보
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Mcard/{id}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FlaOrderInfo>> McardOrderInfo(string id)
        {
            var result = new FlaOrderInfo
            {
                OrderId = id,
                Message = "인증되지 않은 접근입니다."
            };

            //flasystem 만 접근
            var clientId = ((ClaimsIdentity)User.Identity!).Claims.Single(m => m.Type == JwtRegisteredClaimNames.Sid).Value;
            if (clientId != AllowClientId)
                return Unauthorized();

            try
            {
                var orderid = int.Parse(id);
                var query = from m in _dbContext.TB_Invitation_Detail
                            join i in _dbContext.TB_Invitation on m.Invitation_ID equals i.Invitation_ID
                            join o in _dbContext.TB_Order on i.Order_ID equals o.Order_ID
                            where o.Order_ID == orderid
                            select new { m, o.Order_Status_Code, o.Payment_Status_Code };
                var item = await query.FirstOrDefaultAsync();

                //주문 상태확인 주문완료, 결제완료: Order_Status_Code = OSC01, PAYMENT_STATUS_CODE=PSC02
                //화환 서비스 이용 여부 확인 필요
                if (item != null && (item.Order_Status_Code == "OSC01" && item.Payment_Status_Code == "PSC02" && item.m.Flower_gift_YN == "Y"))
                {
                    result.GroomInfo = new FlaPersonInfo();
                    result.GroomInfo.Title = "신랑";
                    result.GroomInfo.Name = item.m.Groom_Name;
                    result.GroomInfo.Phone = item.m.Groom_Phone;
                    result.GroomInfo.Parents = new List<FlaPersonInfo>
                    {
                        new FlaPersonInfo
                        {
                            Name = item.m.Groom_Parents1_Name,
                            Phone = item.m.Groom_Parents1_Phone,
                            Title = item.m.Groom_Parents1_Title
                        },
                        new FlaPersonInfo
                        {
                            Name = item.m.Groom_Parents2_Name,
                            Phone = item.m.Groom_Parents2_Phone,
                            Title = item.m.Groom_Parents2_Title
                        }
                    };

                    result.BrideInfo = new FlaPersonInfo();
                    result.BrideInfo.Title = "신부";
                    result.BrideInfo.Name = item.m.Bride_Name;
                    result.BrideInfo.Phone = item.m.Bride_Phone;
                    result.BrideInfo.Parents = new List<FlaPersonInfo>
                    {
                        new FlaPersonInfo
                        {
                            Name = item.m.Bride_Parents1_Name,
                            Phone = item.m.Bride_Parents1_Phone,
                            Title = item.m.Bride_Parents1_Title
                        },
                        new FlaPersonInfo
                        {
                            Name = item.m.Bride_Parents2_Name,
                            Phone = item.m.Bride_Parents2_Phone,
                            Title = item.m.Bride_Parents2_Title
                        }
                    };

                    var wdate = DateTime.Parse(item.m.WeddingDate);
                    if (!string.IsNullOrEmpty(item.m.WeddingHour))
                    {
                        var h = int.Parse(item.m.WeddingHour.Trim());
                        if (item.m.Time_Type_Code == "오후" && h < 12)
                            h += 12;

                        wdate = wdate.AddHours(h);
                    }
                    if (!string.IsNullOrEmpty(item.m.WeddingMin))
                        wdate = wdate.AddMinutes(int.Parse(item.m.WeddingMin.Trim()));

                    result.WeddingDate = wdate.ToString("yyyy-MM-dd HH:mm:ss");
                    result.WeddingHall = item.m.Weddinghall_Name;
                    result.WeddingHallDetal = item.m.WeddingHallDetail;
                    result.WeddingHallAddress = item.m.Weddinghall_Address;
                    result.WeddingHallPhone = item.m.Weddinghall_PhoneNumber;

                    result.Message = "";
                }
                else
                {
                    result.Message = "주문 정보를 찾을 수 없습니다.";
                    return NotFound(result);
                }
            }
            catch
            {
                result.Message = "잘못된 요청.";
                return BadRequest(result);
            }
            return result;
        }


        [Route("Dcard/{id}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FlaOrderInfo>> DdOrderInfo(string id)
        {
            var result = new FlaOrderInfo
            {
                OrderId = id,
                Message = "인증되지 않은 접근입니다."
            };

            //flasystem 만 접근
            var clientId = ((ClaimsIdentity)User.Identity!).Claims.Single(m => m.Type == JwtRegisteredClaimNames.Sid).Value;
            if (clientId != AllowClientId)
                return Unauthorized();

            try
            {
                var orderid = int.Parse(id);
                var query = from o in _deardeerDbContext.orders
                            join m in _deardeerDbContext.mobile_cards on o.id equals (uint)m.order_id
                            where o.id == orderid
                            && o.order_base == "mcard"
                            && o.order_state == "E"
                            && m.is_disabled == "F"
                            select new { m };
                var item = await query.FirstOrDefaultAsync();

                if (item != null && item.m.use_flower_gift == "T")
                {
                    result.GroomInfo = new FlaPersonInfo();
                    result.GroomInfo.Title = "신랑";
                    result.GroomInfo.Name = item.m.groom_name;
                    result.GroomInfo.Phone = item.m.groom_phone;
                    result.GroomInfo.Parents = new List<FlaPersonInfo>
                 {
                     new FlaPersonInfo
                     {
                         Name = item.m.groom_father,
                         Phone = item.m.groom_father_phone,
                         Title = "아버지"
                     },
                     new FlaPersonInfo
                     {
                         Name = item.m.groom_mother,
                         Phone = item.m.groom_mother_phone,
                         Title = "어머니"
                     }
                 };

                    result.BrideInfo = new FlaPersonInfo();
                    result.BrideInfo.Title = "신부";
                    result.BrideInfo.Name = item.m.bride_name;
                    result.BrideInfo.Phone = item.m.bride_phone;
                    result.BrideInfo.Parents = new List<FlaPersonInfo>
                 {
                      new FlaPersonInfo
                     {
                         Name = item.m.bride_father,
                         Phone = item.m.bride_father_phone,
                         Title = "아버지"
                     },
                     new FlaPersonInfo
                     {
                         Name = item.m.bride_mother,
                         Phone = item.m.bride_mother_phone,
                         Title = "어머니"
                     }
                 };

                    DateTime wdate;

                    if (!string.IsNullOrEmpty(item.m.wedd_date))
                    {
                        string wedd_date = item.m.wedd_date;
                        if (item.m.wedd_date.Contains("요일"))
                        {
                            wedd_date = item.m.wedd_date.Substring(0, item.m.wedd_date.Length - 4);
                        }
                        wdate = DateTime.Parse(wedd_date);

                        if (!string.IsNullOrEmpty(item.m.wedd_hour))
                        {
                            var arr = item.m.wedd_hour.Split('#');
                            var h = int.Parse(arr[1].Trim());
                            if (!arr[0].Contains("오전") && h < 12)
                            {
                                h += 12;
                            }
                            wdate = wdate.AddHours(h);
                        }

                        if (!string.IsNullOrEmpty(item.m.wedd_minute))
                        {
                            wdate = wdate.AddMinutes(int.Parse(item.m.wedd_minute.Trim()));
                        }

                        result.WeddingDate = wdate.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    result.WeddingHall = item.m.map_title;
                    result.WeddingHallDetal = item.m.map_detail;
                    result.WeddingHallAddress = item.m.map_new_address;
                    result.WeddingHallPhone = item.m.map_phone;

                    result.Message = "";
                }
                else
                {
                    result.Message = "주문 정보를 찾을 수 없습니다.";
                    return NotFound(result);
                }

            }
            catch (Exception)
            {
                return BadRequest(new FlaOrderInfo
                {
                    OrderId = id,
                    Message = "잘못된 요청."
                });
            }

            return result;
        }
    }
}
