using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Moq;
using NUnit.Framework;

namespace ScooterRentalClassLibrary
{
    [TestFixture]
    public class ScooterRentalTests
    {

        private decimal pricePerMinute = 0.10M;
        private ScooterService _scooterService;
        private ScooterService CreateScooterService() {

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
            _scooterService.AddScooter("1", pricePerMinute);
            
            //Assert
            var result = _scooterService.GetScooters();
            int updateCountOfScooters = result.Count;
            Assert.Greater(updateCountOfScooters, initialCountOfScooters);
        }
        [Test]
        public void Test_Rent_Scooter() {

            //Arrange
            DateTime starter = new DateTime(2017, 11, 27, 12, 47, 05);
            var timeMock = new Mock<DateTimeProvider>();
            timeMock.SetupGet(tp => tp.Now).Returns(starter);
            //Create a RentalCompany instance
            RentalCompany rentalCompany = 
                new RentalCompany(_scooterService, timeMock.Object); {
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
            Assert.Greater(totalEndRent,0);
        }
       

    }
}
