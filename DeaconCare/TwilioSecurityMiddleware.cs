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
        /// 🔒 Cryptographic Gatekeeper: Validates request payloads while 
        /// explicitly guarding against index out-of-bounds array crashes.
        /// </summary>
        public static async Task<bool> IsRequestValidAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            // 1. Array Bound Guard: Safely verify the signature header EXISTS before parsing it
            if (!request.Headers.ContainsKey("X-Twilio-Signature"))
            {
                Console.WriteLine("[Security Warning] Intercepted incoming payload missing 'X-Twilio-Signature' array data.");
                return false; // Safely drop connection without throwing an array index exception
            }

            // 2. Fetch the signature value securely
            string receivedSignature = request.Headers["X-Twilio-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(receivedSignature))
            {
                return false;
            }

            // 3. Extract the private auth token from hidden local environment variables
            string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? string.Empty;
            if (string.IsNullOrEmpty(authToken))
            {
                Console.WriteLine("[Security Alert] Critical Failure: TWILIO_AUTH_TOKEN is missing from memory variables.");
                return false;
            }

            // 4. Reconstruct the exact absolute URL path telemetry
            string absoluteUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            // 5. Stream and capture the raw string payload body asynchronously
            request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset internal stream index pointer
            }

            // 6. Compute the expected HMAC-SHA1 cryptographic verification token
            string signatureData = absoluteUrl + body;
            byte[] keyBytes = Encoding.UTF8.GetBytes(authToken);
            byte[] dataBytes = Encoding.UTF8.GetBytes(signatureData);

            using (var hmac = new HMACSHA1(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                string expectedSignature = Convert.ToBase64String(hashBytes);

                // 7. Binary comparison matching to confirm payload integrity
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
