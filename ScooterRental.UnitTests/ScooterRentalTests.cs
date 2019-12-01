using System;
using NUnit.Framework;
using ScooterRentalClassLibrary;
using Moq;

namespace ScooterRental.UnitTests
{
    [TestFixture]
    public class ScooterRentalTests
    {

        private decimal pricePerMinute = 0.10M;
        private ScooterService _scooterService;
        private ScooterService CreateScooterService()
        {

            return new ScooterService();
        }

        [SetUp]
        public void SetUp()
        {
            _scooterService = CreateScooterService();
        }

        [Test]
        public void Test_Update_Available_Scooters_List()
        {

            //Arrange

            var listOfScooters = _scooterService.GetScooters();
            int initialCountOfScooters = listOfScooters.Count;
            //Act
            _scooterService.AddScooter("4", pricePerMinute);

            //Assert
            var result = _scooterService.GetScooters();
            int updateCountOfScooters = result.Count;
            Assert.Greater(updateCountOfScooters, initialCountOfScooters);
        }
        [Test]
        public void Test_Rent_Scooter()
        {

            // Arrange
            DateTime starter = new DateTime(2017, 11, 27, 12, 47, 05);
            var timeMock = new Mock<DateTimeProvider>();
            timeMock.SetupGet(tp => tp.Now).Returns(starter);
            //Create a RentalCompany instance
            RentalCompany rentalCompany =
                new RentalCompany(_scooterService, timeMock.Object);
            {
                rentalCompany.Name = "AP'";
            };
            var scooterList = _scooterService.GetScooters();
            var random = new Random();

            int index = random.Next(scooterList.Count);
            string scooterId = scooterList[index].Id.ToString();
            int randomRentedScooterHours = random.Next(24);     // creates a number between 0 and 51
            //Act
            rentalCompany.StartRent(scooterId);
            timeMock.SetupGet(tp => tp.Now).Returns(starter.AddHours(randomRentedScooterHours));
            decimal totalEndRent = rentalCompany.EndRent(scooterId);
            //Assert
            Assert.Greater(totalEndRent, 0);
        }

        [Test]
        public void Test_Should_Not_Allow_Negative_Price_Per_Minute()
        {
            Assert.Multiple(() =>
            {
                var ex = Assert.Throws<ArgumentException>(
                    () => _scooterService.AddScooter("456", -0.10M));
                StringAssert.StartsWith("Price per minute must be positive", ex.Message);

                ex = Assert.Throws<ArgumentException>(
                    () => _scooterService.AddScooter("444", -0.00M));
                StringAssert.StartsWith("Price per minute must be positive", ex.Message);
            });
        }

        [Test]
        public void Test_Should_Not_Allow_Duplicated_ScooterIds()
        {
            _scooterService.AddScooter("456", 0.10M);

            var ex = Assert.Throws<Exception>(
                () => { _scooterService.AddScooter("456", 0.10M); }
            );
            StringAssert.StartsWith("Duplicated scooter id", ex.Message);

        }

        [Test]
        public void Test_Should_Return_Null_If_Scooter_Not_Found()
        {
            // Arrange
            _scooterService.AddScooter("Test", 0.10M);

            // Act
            var notFound = _scooterService.GetScooterById("5696");

            // Assert
            Assert.IsNull(notFound);
        }

        [Test]
        public void Test_Remove_Scooter_If_Found()
        {
            // Arrange
            string scooterIdToBeRemoved = "6";
            _scooterService.AddScooter("7", 0.10M);
            _scooterService.AddScooter(scooterIdToBeRemoved, 0.10M);
            // Act
            _scooterService.RemoveScooter(scooterIdToBeRemoved);
            var actualResult = _scooterService.GetScooterById(scooterIdToBeRemoved);
            // Assert
            Assert.IsNull(actualResult);
        }

        [Test]
        public void Test_Should_Not_Remove_a_Rented_Scooter()
        {
            // Arrange
            string scooterIdToBeRemoved = "10";
            _scooterService.AddScooter(scooterIdToBeRemoved, 0.10M);
            var scooter = _scooterService.GetScooterById(scooterIdToBeRemoved);
            scooter.IsRented = true;

            // Assert
            var ex = Assert.Throws<Exception>(
                () => { _scooterService.RemoveScooter(scooterIdToBeRemoved); }
            );
            StringAssert.StartsWith("Rented scooter can not be removed", ex.Message);
        }
    }
}
