namespace FulfillmentService.Domain
{
    // Domain/DateTimeOffsetExtensions.cs
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset AddBusinessDays(this DateTimeOffset date, int days)
        {
            var current = date;
            var added = 0;

            while (added < days)
            {
                current = current.AddDays(1);
                if (current.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                    added++;
            }

            return current;
        }
    }
}
