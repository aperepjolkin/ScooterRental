using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public class RentalIncome
    {
        public RentalIncome(string scooterId, decimal income)
        {
            ScooterId = scooterId;
            Income = income;
        }

        public string ScooterId { get; private set; }
        public decimal Income { get; set; }

        public int IncomeYear;
        public DateTime? StartedTime { get; set; }
        public DateTime? StoppedTime { get; set; }

        public bool IsRented { get; set; }

    }


}
