using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string message = "accessKey=klm05TvNBzhg7h7j&amount=4100000&extraData=1021&ipnUrl=https://webhook.site/your-test-id&orderId=HD1021_64197127&orderInfo=Thanh toan hoa don 1021&partnerCode=MOMOBKUN20180529&redirectUrl=https://localhost:7280/Invoice/MomoReturn&requestId=4fcf3690-2487-46e0-976d-1e619143bf5d&requestType=captureWallet";
        string secretKey = "at67qH6mk8w5Y1nAyMoTkAmdoRPcuCEn";

        byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
        using var hmacsha256 = new HMACSHA256(keyByte);
        byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));
        string hex1 = BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        Console.WriteLine($"Signature: {hex1}");
    }
}
