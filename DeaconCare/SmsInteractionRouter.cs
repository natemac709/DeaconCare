using System;
using System.Threading.Tasks;

namespace DeaconCare
{
    public class SmsInteractionRouter
    {
        private readonly DatabaseService _database;

        public SmsInteractionRouter(DatabaseService database)
        {
            _database = database;
        }

        /// <summary>
        /// 📬 Text Command Router: Parses a deacon's raw inbound SMS text command string 
        /// while explicitly guarding against array bounds or indexing crashes.
        /// </summary>
        public async Task<string> ProcessIncomingSmsAsync(string incomingText, string volunteerId)
        {
            if (string.IsNullOrWhiteSpace(incomingText))
            {
                return "Error: Empty payload.";
            }

            // Clean up the text input to ensure safe, predictable string pattern matching
            string cleanCommand = incomingText.Trim().ToUpper();

            // 🟢 BLOCK CADENCE PARSER: Processes weekly 'ACK' confirmations for Sunday teams
            if (cleanCommand == "ACK")
            {
                Console.WriteLine($"[Roster Gateway] Volunteer {volunteerId} confirmed their Sunday stationary block roster slot.");
                await _database.UpdateLedgerStateAsync("sunday_block_slot", volunteerId, "1");
                return "Thank you. Your Sunday service slot is officially confirmed. Stand by for service.";
            }

            // 🟠 SERIES CADENCE PARSER: Processes multi-day care claims (e.g., 'CLAIM 402' for a post-birth meal)
            if (cleanCommand.StartsWith("CLAIM"))
            {
                // Safe string isolation: slice out the text following "CLAIM" directly, avoiding arrays entirely
                string taskId = cleanCommand.Replace("CLAIM", "").Trim();

                if (string.IsNullOrEmpty(taskId))
                {
                    return "Error: Missing Task ID. Please reply using the format: CLAIM [TaskNumber]";
                }

                Console.WriteLine($"[Series Gateway] Volunteer {volunteerId} claimed Care Series Slot: {taskId}");
                await _database.UpdateLedgerStateAsync(taskId, volunteerId, "1");
                return $"Success! You have officially claimed Task {taskId}. The logistical details are locked to your track.";
            }

            // 🔵 ESCALATION ROUTER: If a deacon text flags a care crisis, route it out of the diaconate to the Elders
            if (cleanCommand == "PASTOR" || cleanCommand == "ESCALATE")
            {
                Console.WriteLine($"[Polity Valve] CRITICAL: Volunteer {volunteerId} initiated an explicit Elder Escalation trigger.");
                await _database.UpdateLedgerStateAsync("active_mercy_need", volunteerId, "2");
                return "SECURED. This task has been immediately escalated to the Elder Session. Please contact Pastor John directly to debrief offline.";
            }

            // Fallback for unrecognized text strings to guide the user safely
            return "Command not recognized. Reply ACK to confirm a roster slot, CLAIM [Number] to accept a care series task, or PASTOR to escalate an emergency.";
        }
    }
}
