using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions; // 🟢 Required namespace for safe URL reconstruction

namespace DeaconCare
{
    public static class TwilioSecurityMiddleware
    {
        /// <summary>
        /// 🔒 Cryptographic Gatekeeper: Validates request payloads while 
        /// explicitly avoiding internal host array string splitting crashes.
        /// </summary>
        public static async Task<bool> IsRequestValidAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            // 1. Safe Header Guard Check
            if (!request.Headers.ContainsKey("X-Twilio-Signature"))
            {
                return false;
            }

            string receivedSignature = request.Headers["X-Twilio-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(receivedSignature))
            {
                return false;
            }

            string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? "mock_secret_placeholder";

            // 🟢 FIXED BOUNDARY LINE: Uses native UriHelper to construct the path string.
            // Bypasses manual string array formatting completely to eliminate index exceptions.
            string absoluteUrl = request.GetDisplayUrl();

            // 2. Stream and capture the raw string payload body asynchronously
            request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            // 3. Compute the expected HMAC-SHA1 verification token
            string signatureData = absoluteUrl + body;
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

            return false;
        }
    }
}
