using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
namespace ScooterRentalClassLibrary
{
    public class RentalCompany : IRentalCompany
    {
        public string Name { get; set; }
        private readonly IScooterService _iScooterService;
        private readonly DateTimeProvider _currentTime;
     
        //Global variable for storing scooter's start time 
        private DateTime _startRent;
        private DateTime _endRent;
        private decimal _finalRent = 0.00M;
        private const decimal MaxRentDailyIncomeLimit = 20.0M;

        //Variable to store all rent income for all rented scooters
        private readonly List<RentalIncome> _rentTotal = new List<RentalIncome>();


        public RentalCompany(IScooterService iScooterService, DateTimeProvider currentTime)
        {
            _iScooterService = iScooterService;
            _currentTime = currentTime;
        }

        public void TimeDiff(DateTime diff)
        {
            
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {

            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }
            List<RentalIncome> incomeForCalculation = null;

            //Calculate all scooters by concrete year
            if (year.HasValue )
            {
                if (includeNotCompletedRentals) { 
                     incomeForCalculation = _rentTotal.Where(income => income.IncomeYear >= year)
                        .ToList();
                }
                else
                {
                    incomeForCalculation = _rentTotal.Where(income => income.IsRented.Equals(false) && income.IncomeYear >= year)
                        .ToList();
                }

            }
            else
            {

                if (includeNotCompletedRentals)
                {
                    incomeForCalculation = _rentTotal.ToList();
                }
                else
                {
                    incomeForCalculation = _rentTotal.Where(income => income.IsRented.Equals(false))
                        .ToList();
                }
            }

            return incomeForCalculation.Sum(yearIncome => yearIncome.Income);
            
        }

        public decimal DiffMinutes( DateTime e, DateTime s)
        {
            return (decimal)(e - s).TotalMinutes;
        }
        public decimal CalculateRentForScooter(Scooter scooter)
        {
            _endRent = _currentTime.Now;
            //Calculate scooter time in minutes
            var diffInMinutes = (decimal)(_endRent - _startRent).TotalMinutes;

            // Max minutes per day = how many minutes scooter can be rented each day before 20 eur limit is achieved
            var maxMinutesAllowedPerDay = MaxRentDailyIncomeLimit / scooter.PricePerMinute;
            //if scooter rent minutes reached maximum allowed per day, then total income must be stopped at 20
            if (diffInMinutes <= maxMinutesAllowedPerDay)
              _finalRent = Convert.ToInt32(diffInMinutes) * scooter.PricePerMinute;
            else
               _finalRent = Convert.ToInt32(maxMinutesAllowedPerDay) * scooter.PricePerMinute;
            ScooterRentHistory(scooter);
            //If total amount per day reaches 20 EUR than timer must be stopped and restarted at beginning of next day at 0:00 am
            if (_finalRent >= (MaxRentDailyIncomeLimit))
            {
                _startRent = new DateTime(
                    _currentTime.Now.Year,
                    _currentTime.Now.Month,
                    _currentTime.Now.Day, 0, 0, 0).AddDays(1);
                return _finalRent;

            }
            return _finalRent;
        }
        /// <summary>
        /// Update scooter rent history.
        /// </summary>
        /// <param name="scooter">Scooter instance</param>
        private void ScooterRentHistory(Scooter scooter)
        {
            //Get rented scooter from rental history
            var scooterRented = _rentTotal.Find(x => x.ScooterId == scooter.Id);
            scooterRented.StoppedTime = _endRent;
                scooterRented.Income += _finalRent;
                scooterRented.IncomeYear = _endRent.Year;
        }

        public decimal EndRent(string id)
        {
            var scooter = _iScooterService.GetScooterById(id);
            //Calculate rent for a particular scooter and return current income
            var scooterRentIncome = CalculateRentForScooter(scooter);
           //Find a scooter added to rental list
            var scooterRented = _rentTotal.Find(x => x.ScooterId == scooter.Id);
            //Check if scooter was used many time during different time periods
            var scooterRentedTimesCount = _rentTotal.FindAll(x => x.ScooterId == scooter.Id).Count;
            //if scooter count used more then 1 time, then sum all rent incomes
            if (scooterRentedTimesCount > 1)
            {
                scooterRentIncome = _rentTotal.Sum(x => x.Income);
            }
            //Set scooter is rented to false
            scooterRented.IsRented = false;
            scooter.IsRented = false;
            
            return scooterRentIncome;
        }

        public void StartRent(string id)
        {
           var scooter = _iScooterService.GetScooterById(id);
            //Check if scooter exists
           if (scooter == null)
               throw new Exception("Scooter does not exist");
           //Set a rent start time
           _startRent = _currentTime.Now;
           // Check if scooter is already rented
           if (scooter.IsRented)
               throw new Exception("Scooter already rented");
          
           scooter.IsRented = true;
           // Add scooter to rental class and list for total income calculation
           RentalIncome scooterRentalIncome = new RentalIncome(scooter.Id, 0.00M) {
                StartedTime = _startRent,
                IsRented = true
           };
           _rentTotal.Add(scooterRentalIncome);
        }
    }
}
