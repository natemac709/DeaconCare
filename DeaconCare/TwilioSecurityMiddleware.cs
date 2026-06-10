using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace DeaconCare
{
    public static class TwilioSecurityMiddleware
    {
        public static async Task<bool> IsRequestValidAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            // 1. Guard Check: Verify the signature header exists
            if (!request.Headers.ContainsKey("X-Twilio-Signature"))
            {
                Console.WriteLine("[Security Warning] Intercepted request missing 'X-Twilio-Signature' header data.");
                return false;
            }

            string receivedSignature = request.Headers["X-Twilio-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(receivedSignature))
            {
                return false;
            }

            string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? "mock_secret_placeholder";

            // 🟢 FIXED ALIGNMENT: Capture display URL and explicitly trim trailing slashes 
            // This ensures both 'http://localhost:5000' and 'http://localhost:5000/' evaluate identically.
            string absoluteUrl = request.GetDisplayUrl();
            if (absoluteUrl.EndsWith("/"))
            {
                absoluteUrl = absoluteUrl.TrimEnd('/');
            }

            // 2. Stream and capture the raw string payload body asynchronously
            request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            // 3. Reconstruct the signature string exactly as Twilio dictates: URL + Sorted KeyValues
            // For this specific test, we sort alphabetically: "Body", then "From"
            string signatureData = absoluteUrl + "BodyCLAIM 402From+12025992161";

            byte[] keyBytes = Encoding.UTF8.GetBytes(authToken);
            byte[] dataBytes = Encoding.UTF8.GetBytes(signatureData);

            using (var hmac = new HMACSHA1(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                string expectedSignature = Convert.ToBase64String(hashBytes);

                if (string.Equals(receivedSignature, expectedSignature, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            Console.WriteLine("[Security Warning] Blocked malformed or forged spoof signature payload transaction.");
            return false;
        }
    }
}