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
        private decimal _pricePerMinute = 0.10M;

        private const string _scooterId1001 = "ID1001";
        private List<Scooter> listOfScooters;
        private const string _scooterId1 = "1";
        private const string _scooterId4 = "4";
        private const string _scooterId5 = "5";
        private const string _scooterId6 = "6";
        private const string _scooterId7 = "7";

        [SetUp]
        public void SetUp()
        {
            _scooterService = new ScooterService();
            _starter = new DateTime(2017, 11, 27, 12, 47, 05);

            _timeMock = new Mock<DateTimeProvider>();
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany = new RentalCompany(_scooterService, _timeMock.Object);

            listOfScooters = new List<Scooter>()
            {
                new Scooter("4", pricePerMinute),
                new Scooter("5", pricePerMinute),
                new Scooter("6", pricePerMinute),
                new Scooter("7", pricePerMinute),
                new Scooter("8", pricePerMinute),
                new Scooter("9", pricePerMinute),
                new Scooter("10", pricePerMinute),
                new Scooter("11", pricePerMinute),
                new Scooter("12", pricePerMinute)
            };

            //InitMockYear(_starter, _scooterService, _rentalCompany);
            foreach (var scooter in listOfScooters)
            {
                _scooterService.AddScooter(scooter.Id, pricePerMinute);

            }
        }

        public void InitMockYear(DateTime randomDateTime)
        {
            
            _timeMock = new Mock<DateTimeProvider>();
            _timeMock.SetupGet(tp => tp.Now).Returns(randomDateTime);

        }

        [Test]
        public void Test_Start_Rent_Should_Fail_If_Scooter_Not_Found()
        {
            // Arrange

            // Act

            // Assert
            var ex = Assert.Throws<Exception>(
                () =>
                {
                    _rentalCompany.StartRent(_scooterId1001);
                }
           );
            StringAssert.StartsWith("Scooter does not exist", ex.Message);
        }

        [Test]
        public void Test_Throws_Exception_Rent_Should_Fail_If_Scooter_Is_Rented()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById(_scooterId1);
            // Act
            scooter.IsRented = true;
            // Assert
            var ex = Assert.Throws<Exception>(
                () =>
                {
                    _rentalCompany.StartRent(_scooterId1);
                }
            );
            StringAssert.StartsWith("Scooter already rented", ex.Message);
        }

        [Test]
        public void Test_Should_Return_Daily_Limit_For_Same_Day_Rent()
        {
            // Arrange
            var expected = 20;

            _timeMock.SetupGet(d => d.Now).Returns(new DateTime(2019, 11, 24, 11, 23, 18));
            _rentalCompany.StartRent(_scooterId5);

            _timeMock.SetupGet(d => d.Now).Returns(new DateTime(2019, 11, 24, 18, 00, 53));
            _rentalCompany.EndRent(_scooterId5);

            // Act
            var actual = _rentalCompany.CalculateIncome(2019, false);

            // Assert
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void Test_Should_Fail_If_Scooter_Is_Rented()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById(_scooterId1);

            // Act
            _rentalCompany.StartRent(_scooterId1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsTrue(scooter.IsRented);
                var ex = Assert.Throws<Exception>(
                    () => { _rentalCompany.StartRent(_scooterId1); }
                );
                StringAssert.StartsWith("Scooter already rented", ex.Message);
            });

        }

        [Test]
        public void Test_Scooter_Does_Not_Exists()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById(_scooterId1001);

            // Act
            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNull(scooter);
                var ex = Assert.Throws<Exception>(
                    () => { _rentalCompany.StartRent(_scooterId1001); }
                );
                StringAssert.StartsWith("Scooter does not exist", ex.Message);
            });

        }

        [Test]
        public void Test_Calculate_Rental_Price_When_Rental_Ends()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById(_scooterId1);
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
        public void Test_If_Total_Amount_Per_Day_Reaches_A_Limit_For_One_Scooter_Rented_Many_Times()
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
            var expectedIncomeAll = 24.9M;

            
            //InitMockYear(_starter, _scooterService, _rentalCompany);
            foreach (var scooter in listOfScooters)
            {
                _scooterService.GetScooterById(scooter.Id);

                _starter = RandomDate.RandomDay();
                _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
                _rentalCompany.StartRent(scooter.Id);

                _stopped = _starter.AddHours(0.45);
                _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
                //Exclude two sooters from stops to calculated
                if (!scooter.Id.Equals("8") && !scooter.Id.Equals("10"))
                {
                    _rentalCompany.EndRent(scooter.Id);

                }
                else
                {
                    _stopped = _starter.AddHours(0.50);
                    _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
                    _rentalCompany.CalculateRentForScooter(scooter);

                }
            }


            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(2018, true);
            // Assert
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }

        [Test]
        public void Test_Calculate_Total_Income_Rented_Year_IncludeNotCompletedRentals_At_The_Report_Time()
        {
            // Arrange
            var expectedIncomeAll = 11.70M;

            // Act
            _starter = new DateTime(2019, 11, 30, 15, 39, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId4);

            //Scooter 4 is rented for 30 minutes
            _stopped = _starter.AddHours(0.50);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId4);

            //Scooter 5 is rented for 1 hours
            _starter = new DateTime(2019, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId5);

            _stopped = _starter.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId5);

            //Scooter 6 is rented for 27 minutes, but not stopped
            _starter = new DateTime(2019, 11, 28, 14, 00, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId6);

            _stopped = _starter.AddHours(0.45);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            _rentalCompany.CalculateRentForScooter(_scooterService.GetScooterById(_scooterId6));

            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(2019, true);
            // Assert
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }

        [Test]
        public void Test_Calculate_Total_Income_Rented_One_Year_All_Stopped_Scooters_Report_Time()
        {
            // Arrange
            var expectedIncomeAll = 9.00M;

            // Act
            _starter = new DateTime(2019, 11, 30, 15, 39, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId4);

            //Scooter 4 is rented for 30 minutes
            _stopped = _starter.AddHours(0.50);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId4);

            //Scooter 5 is rented for 1 hours
            _starter = new DateTime(2019, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId5);

            _stopped = _starter.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId5);

            //Scooter 6 is rented for 27 minutes, but not stopped
            _starter = new DateTime(2019, 11, 28, 14, 00, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId6);

            _stopped = _starter.AddHours(0.45);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            _rentalCompany.CalculateRentForScooter(_scooterService.GetScooterById(_scooterId6));

            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(2019,false);
            // Assert
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }

        [Test]
        public void Test_Calculate_Total_Income_Rented_Multiple_Years_All_Stopped_Scooters()
        {
            // Arrange
            var expectedIncomeAll = 9.00M;
            
            // Act
            _starter = new DateTime(2019, 11, 30, 15, 39, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId4);

            //Scooter 4 is rented for 30 minutes
            _stopped = _starter.AddHours(0.50);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId4);

            //Scooter 5 is rented for 1 hours
            _starter = new DateTime(2019, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId5);

            _stopped = _starter.AddHours(1);

            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId5);

            //Scooter 6 is rented for 27 minutes, but not stopped
            _starter = new DateTime(2019, 11, 28, 14, 00, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId6);

            _stopped = _starter.AddHours(0.45);

            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            _rentalCompany.CalculateRentForScooter(_scooterService.GetScooterById(_scooterId6));

            //Scooter 7 is rented for 2 hours
            _starter = new DateTime(2018, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId7);

            _stopped = _starter.AddHours(2);

            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId7);

            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(2019, false);
            // Assert
           
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }

        [Test]
        public void Test_Calculate_Total_Rented_Income_With_Empty_Year_IncludeNotCompletedRentals_Scooter()
        {
            // Arrange
            var expectedIncomeAll = 23.7M;

            // Act
            _starter = new DateTime(2019, 11, 30, 15, 39, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId4);

            //Scooter 4 is rented for 30 minutes
            _stopped = _starter.AddHours(0.50);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId4);

            //Scooter 5 is rented for 1 hours
            _starter = new DateTime(2019, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId5);

            _stopped = _starter.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId5);

            //Scooter 6 is rented for 27 minutes, but not stopped
            _starter = new DateTime(2019, 11, 28, 14, 00, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId6);

            _stopped = _starter.AddHours(0.45);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            _rentalCompany.CalculateRentForScooter(_scooterService.GetScooterById(_scooterId6));

            //Scooter 7 is rented for 2 hours
            _starter = new DateTime(2018, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId7);

            _stopped = _starter.AddHours(2);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId7);

            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(null, true);
            // Assert
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }

        [Test]
        public void Test_Calculate_Total_Rented_Income_With_Empty_Year_All_Stopped_Scooter()
        {
            // Arrange
            var expectedIncomeAll = 21.0M;

            // Act
            _starter = new DateTime(2019, 11, 30, 15, 39, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId4);

            //Scooter 4 is rented for 30 minutes
            _stopped = _starter.AddHours(0.50);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId4);

            //Scooter 5 is rented for 1 hours
            _starter = new DateTime(2019, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId5);

            _stopped = _starter.AddHours(1);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId5);

            //Scooter 6 is rented for 27 minutes, but not stopped
            _starter = new DateTime(2019, 11, 28, 14, 00, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId6);

            _stopped = _starter.AddHours(0.45);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            _rentalCompany.CalculateRentForScooter(_scooterService.GetScooterById(_scooterId6));

            //Scooter 7 is rented for 2 hours
            _starter = new DateTime(2018, 11, 29, 12, 30, 00);
            _timeMock.SetupGet(tp => tp.Now).Returns(_starter);
            _rentalCompany.StartRent(_scooterId7);

            _stopped = _starter.AddHours(2);
            _timeMock.SetupGet(tp => tp.Now).Returns(_stopped);
            totalPriceOfRental = _rentalCompany.EndRent(_scooterId7);

            // Act
            decimal totalIncome = _rentalCompany.CalculateIncome(null, false);
            // Assert
            Assert.AreEqual(expectedIncomeAll, totalIncome);
        }
        [Test]
        public void Test_Scooter_Is_Set_To_Rented_Status()
        {
            // Arrange
          
            var scooter = _scooterService.GetScooterById("3");
            // Act
            _rentalCompany.StartRent(scooter.Id);
            // Assert
            Assert.IsTrue(scooter.IsRented);
        }

        [Test]
        public void Test_Should_Reset_Rented_Scooter()
        {
            // Arrange
            var scooter = _scooterService.GetScooterById(_scooterId1);
            _rentalCompany.StartRent(scooter.Id);

            // Act
            _rentalCompany.EndRent(scooter.Id);

            // Assert
            Assert.IsFalse(scooter.IsRented);
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
