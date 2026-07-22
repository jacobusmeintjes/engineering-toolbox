namespace EnterpriseFramework.Core.FitnessFunctions
{
    /// <summary>
    /// Marks a known, accepted violation of a fitness function during phased adoption
    /// (report-only -> blocking). Every waiver must reference a ticket and carry an expiry date;
    /// expired waivers should be treated as failures during the recurring waiver review.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class WaiverAttribute : Attribute
    {
        public WaiverAttribute(string ticketReference, string expiresOn)
        {
            TicketReference = ticketReference;
            ExpiresOn = expiresOn;
        }

        public string TicketReference { get; }
        /// <summary>ISO-8601 date (yyyy-MM-dd) after which this waiver is no longer valid.</summary>
        public string ExpiresOn { get; }
    }
}
