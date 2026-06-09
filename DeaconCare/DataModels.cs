using System;

namespace DeaconCare
{
    /// <summary>
    /// 🧱 The Diaconal Ledger Entry Model:
    /// Maps directly to your schema.sql tables while keeping personal text fields strictly offline.
    /// </summary>
    public class DiaconalTaskEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Maps the task to one of our three universal cadences (Block, Series, FluidPulse)
        public string CadenceType { get; set; } = "Block"; 
        
        public string StatusCode { get; set; } = "0"; // 0=Dispatched, 1=Completed, 2=Escalated to Elders
        
        public DateTime ExecutionDate { get; set; }

        // 🔗 SYSTEM INTEGRATION TRACKERS:
        // These ID fields let DeaconCare link background checks and scheduling logs directly 
        // back to their existing profiles without saving any sensitive personal text files.
        public string? PlanningCenterPersonId { get; set; }
        public string? ChurchDirectoryAppMemberId { get; set; }
        
        public Guid? AssignedTrustCircleId { get; set; } // Ties tasks to small groups or married couples
    }
}