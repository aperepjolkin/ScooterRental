using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Moq;
using NUnit.Framework;

namespace ScooterRentalClassLibrary
{
    [TestFixture]
    public class RentalCompanyTests
    {
        decimal pricePerMinute = 0.10M;
        private ScooterService _scooterService;
        private RentalCompany _rentalCompany;
        private Mock<DateTimeProvider> _timeMock;
        private DateTime _starter;
        private DateTime _stopped;
        private decimal totalPriceOfRental = 0.00M;
       [SetUp]
        public void SetUp()
        {
            _scooterService = new  ScooterService();
            _starter = new DateTime(2017, 11, 27, 12, 47, 05);
            
            _timeMock = new Mock<DateTimeProvider>();
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany = new RentalCompany(_scooterService, _timeMock.Object);
            
        }

        [Test]
        public void Test_Calculate_Rental_Price_When_Rental_Ends()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById("1");
            // Act
            _rentalCompany.StartRent(scooter.Id);
            //Stop scooter rent after 1 hour from start rent
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter.AddHours(1));
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);
            // Assert
            Assert.That(totalPriceOfRental, Is.EqualTo(6));
        }

        //Test If total amount per day reaches 20 EUR than timer must be stopped and restarted at beginning of next day at 0:00 am.
        [Test]
        public void Test_If_Total_Amount_Per_Day_Reaches_A_Limit()
        {
            // Arrange
            //DateTime nextDay = new DateTime(_starter(),0,0,0);;
            
            var scooter = _scooterService.GetScooterById("2");
            // Act
            _rentalCompany.StartRent(scooter.Id);
            // Scooter has been started stopped 4 time per day rent after
            _stopped = _starter.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);

            _rentalCompany.StartRent(scooter.Id);
            _stopped = _stopped.AddHours(0.33);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);

            _rentalCompany.StartRent(scooter.Id);
            _stopped = _stopped.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);

            _rentalCompany.StartRent(scooter.Id);
            _stopped = _stopped.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);

            var time =_starter.ToShortTimeString();
            // Assert
            //Total amount per day reaches 20
            Assert.Multiple(() =>
            {
                Assert.That(totalPriceOfRental, Is.EqualTo(20));
            });
        }

        [Test]
        public void Test_Calculate_Rental_Income_From_Rentals()
        {
            // Arrange
            var listOfScooters = new List<Scooter>()
            {
                new Scooter("4", pricePerMinute) { IsRented = true },
                new Scooter("5", pricePerMinute) { IsRented = true },
                new Scooter("6", pricePerMinute) { IsRented = false },
                new Scooter("7", pricePerMinute) { IsRented = false },
                new Scooter("8", pricePerMinute) { IsRented = false },
                new Scooter("9", pricePerMinute) { IsRented = true },
                new Scooter("10", pricePerMinute) { IsRented = true },
                new Scooter("11", pricePerMinute) { IsRented = true },
                new Scooter("12", pricePerMinute) { IsRented = true }
            };
           
           
            decimal totalIncome = _rentalCompany.CalculateIncome(null,true);
            // Act

            // Assert
        }

        [Test]
        public void Test_Scooter_Is_In_Rent()
        {
            // Arrange
            _scooterService.AddScooter("4",pricePerMinute);
            var scooter = _scooterService.GetScooterById("4");
            // Act
            _rentalCompany.StartRent(scooter.Id);
            // Assert
            Assert.IsTrue(scooter.IsRented);
        }

        [Test]
        public void Test_Scooter_Is_Not_In_Rent()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById("1");

            // Act
            _rentalCompany.StartRent(scooter.Id);
            _stopped = _starter.AddHours(0.26);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(scooter.Id);
            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(scooter.IsRented);
                Assert.That(totalPriceOfRental, Is.GreaterThanOrEqualTo(0.00M));
            });
            
        }
    }

   
}
