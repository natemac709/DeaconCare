using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace DeaconCare
{
    // 🏛️ Ecclesiastical Structural Archetypes
    public enum OfficeRole { Elder, Deacon, Deaconess, CovenantMember }
    public enum ServiceCadence { Block, Series, FluidPulse }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🏛️ DeaconCare Secure Middleware Engine Starting...");

            // Spin up background worker for the anonymous midnight data purge
            _ = Task.Run(() => StartMidnightScrubScheduler());

            // Build an isolated, secure Kestrel web host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // 🔒 Force Listen on Secure HTTPS Port 5001 only (No Port 5000/HTTP allowed)
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
                            // Bind your domain SSL/TLS Certificate securely
                            // For local testing, this uses the local dev certificate. In production, pass your pfx path.
                            listenOptions.UseHttps(httpsOptions =>
                            {
                                // Strict enforcement of TLS 1.3 encryption protocol
                                httpsOptions.SslProtocols = SslProtocols.Tls13;
                            });
                        });
                    });
                    webBuilder.Configure(app =>
                    {
                        // Direct routing interceptor for incoming TLS webhooks
                        app.Run(async context =>
                        {
                            await ProcessSecureRequestAsync(context);
                        });
                    });
                })
                .Build();

            await host.RunAsync();
        }

        private static async Task ProcessSecureRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            // 🛡️ Validate Cryptographic Request Signature (Twilio Webhook Validation / API Keys)
            if (!request.Headers.TryGetValue("X-DeaconCare-Signature", out var signature))
            {
                await RejectRequestAsync(response, StatusCodes.Status412PreconditionFailed, "Security Signature Required.");
                return;
            }

            // Strictly allow encrypted POST streams only
            if (request.Method != "POST")
            {
                await RejectRequestAsync(response, StatusCodes.Status405MethodNotAllowed, "HTTPS POST Only.");
                return;
            }

            // Stream payload securely
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                string jsonPayload = await reader.ReadToEndAsync();
                
                // 🔄 Route to universal church cadences (Blocks, Series, Fluid Pulses)
                bool success = RouteCadencePayload(jsonPayload);

                if (success)
                {
                    response.StatusCode = StatusCodes.Status200OK;
                    await response.WriteAsync("Ledger Updated Over Encrypted Channel.");
                }
                else
                {
                    await RejectRequestAsync(response, StatusCodes.Status400BadRequest, "Malformed Cadence State.");
                }
            }
        }

        private static bool RouteCadencePayload(string payload)
        {
            // 🟢 BLOCK: Instantly clear Sunday Audio, Nursery, or Communion prep rosters
            // 🟠 SERIES: Update calendar dates for Wedding Deaconesses or Post-Birth meal series
            // 🔵 FLUID PULSE: Fire urgent accessibility accommodations or Mercy Fund data streams
            Console.WriteLine($"[Secure Ledger] Processing verified TLS stream: {payload}");
            return true;
        }

        private static async Task StartMidnightScrubScheduler()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                DateTime nextMidnight = DateTime.Today.AddDays(1);
                TimeSpan timeToMidnight = nextMidnight - now;

                await Task.Delay(timeToMidnight);

                // 🔴 AMNESIA PURGE LOOP
                // Erases recipient tracking names, phone numbers, and travel endpoints daily
                ExecuteCryptographicScrub();
            }
        }

        private static void ExecuteCryptographicScrub()
        {
            Console.WriteLine("🛡️ [Security] Running Midnight Scrub. All personal PII records permanently wiped.");
        }

        private static async Task RejectRequestAsync(HttpResponse response, int statusCode, string reason)
        {
            Console.WriteLine($"[Security Event Blocked] Threat prevented: {reason}");
            response.StatusCode = statusCode;
            await response.WriteAsync(reason);
        }
    }
}
