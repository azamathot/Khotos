using Microsoft.AspNetCore.Mvc;
using Payment.Helper;
using Robokassa.NET;
using Robokassa.NET.Enums;
using Robokassa.NET.Models;
using SharedModels.Payment;
using System.Collections.ObjectModel;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Payment.Controllers
{
    [Route("payment/robokassapayment")]
    [ApiController]
    public class RobokassaPaymentController : ControllerBase
    {
        readonly IRobokassaService _service;
        readonly ILogger<RobokassaPaymentController> _logger;
        IConfiguration _conf;
        public RobokassaPaymentController(IRobokassaService service, ILogger<RobokassaPaymentController> logger, IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _conf = configuration;
        }

        [HttpGet("success")]
        public IActionResult Success(
            decimal outSum,
            int invId,
            string signatureValue,
            [FromQuery(Name = "Shp_login")] string login,
            [FromQuery(Name = "Shp_name")] string name,
            [FromQuery(Name = "Shp_email")] string email,
            [FromQuery(Name = "Shp_oplata")] string payed)
            =>
                Ok(@$"
                    Payment successfully completed! 
                    Sum: {outSum}, 
                    InvoiceID : {invId}, 
                    Signature: {signatureValue}, 
                    Login: {login},
                    Payed count: {payed},
                    Name: {HttpUtility.UrlDecode(name)},
                    We send invoice to your email: {HttpUtility.UrlDecode(email)}"
                );

        [HttpGet("fail")]
        public IActionResult Fail() => Ok("Payment failed");


        // GET: api/<RobokassaPaymentController>
        [HttpPost("paymentorder")]
        public IActionResult PaymentOrder(PaymentRequest request)
        {
            //Создаем чек 
            var receipt = new RobokassaReceiptRequest(
                SnoType.Patent,
                new Collection<ReceiptOrderItem>()
                {
                    new ReceiptOrderItem(request.ProductName, request.Quantity, request.Price, Tax.Vat110, PaymentMethod.FullPayment,
                        PaymentObject.Payment)
                }
            );

            //Опциональные пользовательские параметры shp_ https://docs.robokassa.ru/#1205
            var customFields = new CustomShpParameters();
            //Можно указать поля с префиксом Shp_
            customFields.Add("Shp_login", request.Email);
            //Можно без него, метод Add поправит ключ
            customFields.Add("email", request.Email);
            customFields.Add("name", request.Username);
            customFields.Add("oplata", request.Price.ToString());
            customFields.Add("orderid", request.OrderId.ToString());
            customFields.Add("productname", request.ProductName);

            try
            {
                //Основной сценарий получения платежной ссылки
                var paymentUrl = _service
                    .GenerateAuthLink(receipt.TotalPrice, request.OrderId, receipt, customFields);
                return Ok(paymentUrl.Link);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // GET: api/<RobokassaPaymentController>
        [HttpGet("paymentcreatebtnlink")]
        public IActionResult PaymentCreateBtnLink(PaymentRequest request)
        {
            var merchant_login = _conf["RobokassaOptions:ShopName"];
            var password_1 = _conf["RobokassaOptionsTest:Password1"];
            var invid = request.OrderId;
            var description = request.ProductDescription;
            var out_sum = request.Price.ToString();
            var shp_item = "1";
            var culture = "ru";
            var encoding = "utf-8";
            var in_curr = "";

            var signature_value = PaymentProcessor.ComputeMD5($"{merchant_login}:{out_sum}:{invid}:{password_1}");
            var paymentUrl1 = "<html><script "
                + "src='https://auth.robokassa.ru/Merchant/PaymentForm/FormMS.js?"
                + $"isTest=1&MerchantLogin={merchant_login}&OutSum={out_sum}&InvoiceID={invid}"
                + $"&Description={description}&SignatureValue={signature_value}'></script></html>";

            signature_value = PaymentProcessor.ComputeMD5($"{merchant_login}:{out_sum}:{invid}:{password_1}:Shp_item={shp_item}");
            var paymentUrl2 = "<html>" + "<script " +
                "src='https://auth.robokassa.ru/Merchant/PaymentForm/FormFLS.js?MerchantLogin=" + merchant_login +
                "&OutSum=" + out_sum + "&InvId=" + invid + "&IncCurrLabel=" + in_curr +
                "&Description=" + description + "&SignatureValue=" + signature_value + "&Shp_item=" + shp_item +
                "&Culture=" + culture + "&Encoding=" + encoding + "'>" +
                "</script></html>";

            try
            {
                //Основной сценарий получения платежной ссылки
                return base.Content(paymentUrl1 + paymentUrl2, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test")]
        public IActionResult test()
        {
            PaymentRequest request = new PaymentRequest()
            {
                Email = "azamathot@gmail.com",
                OrderId = 1,
                Price = 51,
                ProductDescription = "test",
                ProductId = 1,
                ProductName = "test",
                Username = "test",
                Quantity = 2
            };
            try
            {
                return PaymentOrder(request);
                //return RedirectToAction("PaymentOrder", request);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
