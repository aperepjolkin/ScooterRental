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
        private  DateTimeProvider _currentTime;
        decimal pricePerMinute = 0.10M;
        private DateTime _startRent;
        private DateTime _endRent;
        private decimal _finalRent = 0.00M;
        private const decimal _maxRentDailyIncomeLimit = 20.0M;

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

        public decimal CalculateRentForScooter(Scooter scooter)
        {
            _endRent = _currentTime.Now;
            var diffInMinutes = (_endRent - _startRent).TotalMinutes;
            _finalRent = Convert.ToInt32(diffInMinutes) * scooter.PricePerMinute;

            //If total amount per day reaches 20 EUR than timer must be stopped and restarted at beginning of next day at 0:00 am
            ScooterRentHistory(scooter);
            if (_finalRent >= (_maxRentDailyIncomeLimit))
            {
                _startRent = new DateTime(
                    _currentTime.Now.Year,
                    _currentTime.Now.Month,
                    _currentTime.Now.Day, 0, 0, 0).AddDays(1);
                return _finalRent;

            }
            return _finalRent;
        }

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
            var scooterRentIncome = CalculateRentForScooter(scooter);

            var scooterRented = _rentTotal.Find(x => x.ScooterId == scooter.Id);
            scooterRented.IsRented = false;

            scooter.IsRented = false;
            //_endRent = _currentTime.Now;
            //var diffInMinutes = (_endRent - _startRent).TotalMinutes;
            //_finalRent = Convert.ToInt32(diffInMinutes) * pricePerMinute;

            //if (scooterRented != null) {

            
            //}
            //else {
            //    totalScooterRentalIncome.Income = finalRent;
            //    totalScooterRentalIncome.IncomeYear = endRent.Year;
            //    _rentTotal.Add(totalScooterRentalIncome);
            //}


            //If total amount per day reaches 20 EUR than timer must be stopped and restarted at beginning of next day at 0:00 am

            //if (_finalRent >= (_maxRentDailyIncomeLimit)) {
            //    _startRent =  new DateTime(
            //        _currentTime.Now.Year,
            //        _currentTime.Now.Month,
            //        _currentTime.Now.Day, 0,0,0).AddDays(1);
            //    return _finalRent;

            //}


            
            //return _finalRent;
            return scooterRentIncome;
        }

        public void StartRent(string id)
        {
           var scooter = _iScooterService.GetScooterById(id);

           if (scooter == null)
               throw new Exception("Scooter does not exist");

           _startRent = _currentTime.Now;
          
           if (scooter.IsRented)
               throw new Exception("Scooter already rented");
          
           scooter.IsRented = true;
           RentalIncome scooterRentalIncome = new RentalIncome(scooter.Id, 0.00M) {
                StartedTime = _startRent,
                IsRented = true
           };
           _rentTotal.Add(scooterRentalIncome);
        }
    }
}
