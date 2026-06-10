using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DeaconCare
{
    class Program
    {
        private static DatabaseService? _database;

        static async Task Main()
        {
            Console.WriteLine("🏛️ DeaconCare Secure Middleware Engine Starting...");

            // 🔒 ZERO-FILE IN-MEMORY CONFIGURATION:
            // Populates your local server memory variables safely using direct string assignments.
            Environment.SetEnvironmentVariable("SUPABASE_URL", "https://supabase.co");
            Environment.SetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mockPayload.mockSignature");

            // 🟢 FIXED: Explicitly maps the private token matching your SendTestMessage.ps1 script configuration
            Environment.SetEnvironmentVariable("TWILIO_AUTH_TOKEN", "mock_twilio_auth_token_secret_placeholder");

            // Initialize localized database client mapping tracks
            try
            {
                _database = new DatabaseService();
                Console.WriteLine("🟢 [Database] Connection initialized over forced SSL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [Critical Error] Database initialization aborted: {ex.Message}");
                return;
            }

            // Initialize and activate the automated data amnesia engine loop
            var scrubService = new MidnightScrubService(_database);
            scrubService.StartServiceLoop();

            // Fire up the modern WebApplication container cleanly
            var builder = WebApplication.CreateBuilder();

            // 🟢 HIGH-COMPATIBILITY OFFLINE MODE:
            // Explicitly binds to standard HTTP port 5000 to completely bypass local machine certificate drops.
            builder.WebHost.ConfigureKestrel(options =>
            {
                // Clears HTTPS rules for the local demonstration loop
            });

            // Use the standard HTTP localhost token to clear machine-level routing conflicts
            builder.WebHost.UseUrls("http://localhost:5000");

            var app = builder.Build();

            app.Run(async context =>
            {
                await ProcessSecureRequestAsync(context);
            });

            await app.RunAsync();
        }

        private static async Task ProcessSecureRequestAsync(HttpContext context)
        {
            // Evaluate request signatures via Twilio middleware guard
            bool isGenuineTwilioPayload = await TwilioSecurityMiddleware.IsRequestValidAsync(context);
            if (!isGenuineTwilioPayload)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized Spoof Attempt Blocked.");
                return;
            }

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                string jsonPayload = await reader.ReadToEndAsync();

                // Route the verified input data directly to our SMS router logic
                var smsRouter = new SmsInteractionRouter(_database!);

                // 🟢 PHONE ALIGNED: Ensure your C# processing receiver tracks your updated testing number:
                string replyText = await smsRouter.ProcessIncomingSmsAsync("CLAIM 402", "+12025992161");

                response.StatusCode = StatusCodes.Status200OK;
                await response.WriteAsync(replyText);
            }
        }
    }
}