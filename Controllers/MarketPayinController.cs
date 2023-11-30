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
    /// <summary>
    /// MarketPayin 전용 API
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MarketPayinController : ControllerBase
    {
        private readonly BarShopContext _barShopContext;
        private readonly string AllowClientId = "MarketPayin";

        public MarketPayinController(BarShopContext barShopContext)
        {
            _barShopContext = barShopContext;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponseModel>> CheckUser(ReuqestUserModel model)
        {
            ActionResult response = BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = "잘못된 요청." });
            try
            {
                var clientId = ((ClaimsIdentity)User.Identity!).Claims.Single(m => m.Type == JwtRegisteredClaimNames.Sid).Value;
                if (clientId == AllowClientId)
                {
                    //청첩장 주문완료 및 결제완료 상태, 주문타입은 청첩장이며, 구매사이트는 (바른손, 바른손몰, 프페)
                    var query = from o in _barShopContext.custom_order
                                where o.order_seq == model.OrderNo 
                                && o.order_name == model.Name
                                && o.status_seq >= 9 && o.settle_status == 2 && o.sales_Gubun != "SD"
                                && (o.order_type == "1" || o.order_type == "6" || o.order_type == "7")
                                select new { seq = o.order_seq, hphone = o.order_hphone, name = o.order_name};
                    //샘플주문
                    //STATUS_SEQ: 0-시작, 1-주문완료/입금대기중, 3or5-주문취소, 4-결제완료,10-제품준비/포장중
                    //  12-발송완료
                    var query1 = from s in _barShopContext.CUSTOM_SAMPLE_ORDER
                                 where s.sample_order_seq == model.OrderNo
                                 && s.MEMBER_NAME == model.Name
                                 && s.STATUS_SEQ >= 4 && s.STATUS_SEQ != 5
                                 && s.SALES_GUBUN != "SD"
                                 select new { seq = s.sample_order_seq, hphone = s.MEMBER_HPHONE, name = s.MEMBER_NAME };

                    var item = await query.FirstOrDefaultAsync();
                    if (item == null) //주문 정보가 없으면 sample 주문 정보
                    { 
                        item = await query1.FirstOrDefaultAsync();
                    }
                    if (item != null && item.hphone.Replace("-", "") == model.Phone.Replace("-", ""))
                    {
                        return new BaseResponseModel { status = (int)HttpStatusCode.OK, message = "OK" };
                    }
                    else
                    {
                        response = NotFound(new BaseResponseModel { status = (int)HttpStatusCode.NotFound, message = "조회실패: 결과가 없습니다." });
                    }
                }
                
            }
            catch
            {
                response = BadRequest(new BaseResponseModel { status = (int)HttpStatusCode.BadRequest, message = "잘못된 요청." });
            }
            return response;
        }
        public class ReuqestUserModel
        {
            public string Name { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public int OrderNo { get; set; } = -1;
        }
    }
    
}
