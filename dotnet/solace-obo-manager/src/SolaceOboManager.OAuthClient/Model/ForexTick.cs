namespace SolaceOboManager.OAuthClient.Model
{
    public sealed class ForexTick
    {
        public string Pair { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public DateTime Timestamp { get; set; }

        public ForexTick()
        {
        }

        public void Reset()
        {
            Pair = string.Empty;
            Bid = 0;
            Ask = 0;
            Timestamp = DateTime.MinValue;
        }
    }
}
