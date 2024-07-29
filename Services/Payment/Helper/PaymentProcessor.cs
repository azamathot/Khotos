using System.Security.Cryptography;
using System.Text;

namespace Payment.Helper
{
    public class PaymentProcessor
    {
        public static void OnSuccessPayment(int invoiceId, decimal sum, string name, string login, string email)
        {
            Console.WriteLine($"Success: {invoiceId}, {sum}, {name}, {login}, {email}");
        }

        public static void OnFailPayment(int invoiceId, decimal sum, string name, string login, string email)
        {
            Console.WriteLine($"Fail: {invoiceId}, {sum}, {name}, {login}, {email}");
        }

        public static string ComputeMD5(string s)
        {
            StringBuilder sb = new StringBuilder();

            // Инициализировать хеш-объект MD5
            using (MD5 md5 = MD5.Create())
            {
                // Вычислить хэш заданной строки
                byte[] hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(s));

                // Преобразование массива байтов в строковый формат
                foreach (byte b in hashValue)
                {
                    sb.Append($"{b:X2}");
                }
            }

            return sb.ToString();
        }
    }
}
