using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public class RentalCompany : IRentalCompany
    {
        public string Name { get; set; }
        private readonly IScooterService _iScooterService;
        private readonly DateTimeProvider _currentTime;
        decimal pricePerMinute = 0.10M;
        private DateTime _startRent;
        private decimal finalRent = 0.00M;

        public RentalCompany(IScooterService iScooterService, DateTimeProvider currentTime)
        {
            _iScooterService = iScooterService;
            _currentTime = currentTime;
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {
            throw new NotImplementedException();
        }

        public decimal EndRent(string id)
        {
            var scooter = _iScooterService.GetScooterById(id);
            scooter.IsRented = false;
            var endRent = _currentTime.Now;
            var diffInMinutes = (endRent - _startRent).TotalMinutes;
            finalRent += Convert.ToInt32(diffInMinutes) * pricePerMinute;
            //If total amount per day reaches 20 EUR than timer must be stopped and restarted at beginning of next day at 0:00 am

            if (finalRent >= (20M))
            {
                _startRent =  new DateTime(
                    _currentTime.Now.Year,
                    _currentTime.Now.Month,
                    _currentTime.Now.Day, 0,0,0).AddDays(1);
                return finalRent;

            }
            return finalRent;
        }

        public void StartRent(string id)
        {
           var scooter = _iScooterService.GetScooterById(id);
          _startRent = _currentTime.Now;
          scooter.IsRented = true;
        }
    }
}
