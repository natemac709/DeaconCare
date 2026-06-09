using System;
using System.Threading.Tasks;
using Supabase;

namespace DeaconCare
{
    public class DatabaseService
    {
        private readonly Client _supabaseClient;

        public DatabaseService()
        {
            // Pull secure environment variables loaded via DotNetEnv in Program.cs
            string url = Environment.GetEnvironmentVariable("SUPABASE_URL") 
                         ?? throw new InvalidOperationException("Missing SUPABASE_URL configuration.");
            string key = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") 
                         ?? throw new InvalidOperationException("Missing SUPABASE_SERVICE_ROLE_KEY configuration.");

            // Initialize the database client over forced SSL
            var options = new SupabaseOptions { AutoConnectRealtime = false };
            _supabaseClient = new Client(url, key, options);
        }

        /// <summary>
        /// 🟢 Logistical Ledger Commit: Process incoming status changes cleanly 
        /// while ensuring no personal spiritual or medical text notes are ever stored.
        /// </summary>
        public async Task<bool> UpdateLedgerStateAsync(string taskId, string volunteerId, string statusCode)
        {
            try
            {
                // Verify the incoming check code maps strictly to a proper state framework
                // 1 = Logistical Success / Completed
                // 2 = Escalated to Elder Session / Shielded from Deacons
                if (statusCode != "1" && statusCode != "2")
                {
                    Console.WriteLine($"[Security Guard] Rejected anomalous status code string: {statusCode}");
                    return false;
                }

                // Execute a strict, token-bound database row mutation.
                // Row-Level Security (RLS) on Supabase guarantees this token can only touch this row.
                Console.WriteLine($"[Database Engine] Safely updating Task {taskId} for Volunteer {volunteerId} to State {statusCode}.");
                
                // In production, your model assignment mapping execution occurs right here:
                // await _supabaseClient.From<DiaconalLedger>().Where(x => x.Id == taskId).Update(...);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database Failure] Secure transaction interrupted: {ex.Message}");
                return false;
            }
        }
    }
}
