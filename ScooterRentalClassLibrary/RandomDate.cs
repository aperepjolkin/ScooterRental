using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public static class RandomDate
    {
        private static readonly Random Gen = new Random();
        public static DateTime RandomDay()
        {
            DateTime start = new DateTime(2017, 11, 29);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(Gen.Next(range)).AddHours(Gen.Next(0, 24)).AddMinutes(Gen.Next(0, 60)).AddSeconds(Gen.Next(0, 60));
        }
    }
}
