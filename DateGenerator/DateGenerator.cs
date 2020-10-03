using GeneratorInterface;
using System;

namespace DateGenerator
{
    public class DateGenerator : IGenerator
    {
        private static readonly DateTime MIN_DATE = new DateTime(1970, 1, 1);

        private readonly Random random;

        public DateGenerator()
        {
            random = new Random();
        }

        public object Generate()
        {
            int daysRange = (DateTime.Now - MIN_DATE).Days;
            return MIN_DATE.AddDays(random.Next(daysRange)).AddHours(random.Next(0, 24)).
                AddMinutes(random.Next(0, 60)).AddSeconds(random.Next(0, 60));
        }
    }
}
