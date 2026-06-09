using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DeaconCare
{
    public static class TwilioSecurityMiddleware
    {
        /// <summary>
        /// 🔒 Cryptographic Gatekeeper: Rejects incoming payloads immediately if they fail signature matching.
        /// </summary>
        public static async Task<bool> IsRequestValidAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            // 1. Fetch the private validation token from your hidden local environment variables
            string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? string.Empty;
            if (string.IsNullOrEmpty(authToken))
            {
                Console.WriteLine("[Security Alert] Critical Failure: TWILIO_AUTH_TOKEN is missing from server memory.");
                return false;
            }

            // 2. Extract the signature header sent by the cellular gateway
            if (!request.Headers.TryGetValue("X-Twilio-Signature", out var receivedSignature))
            {
                Console.WriteLine("[Security Warning] Intercepted anonymous incoming request missing required signature.");
                return false;
            }

            // 3. Reconstruct the full absolute URL to verify identical path telemetry
            string absoluteUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            // 4. Stream and capture the raw string payload body asynchronously
            request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset internal stream index pointer for subsequent processing steps
            }

            // 5. Compute the expected HMAC-SHA1 cryptographic signature signature
            string signatureData = absoluteUrl + body;
            byte[] keyBytes = Encoding.UTF8.GetBytes(authToken);
            byte[] dataBytes = Encoding.UTF8.GetBytes(signatureData);

            using (var hmac = new HMACSHA1(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                string expectedSignature = Convert.ToBase64String(hashBytes);

                // 6. Execute binary evaluation match to confirm payload integrity
                if (receivedSignature.ToString() == expectedSignature)
                {
                    return true;
                }
            }

            Console.WriteLine("[Security Warning] Blocked malformed or forged spoof signature payload transaction.");
            return false;
        }
    }
}
