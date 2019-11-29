using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public class ScooterService : IScooterService
    {
        private List<Scooter> _scooterList;
        decimal pricePerMinute = 0.10M;
        public ScooterService() {
            _scooterList = new List<Scooter>();
            _scooterList.Add(new Scooter("1", pricePerMinute));
            _scooterList.Add(new Scooter("2", pricePerMinute));
            _scooterList.Add(new Scooter("3", pricePerMinute));
        }

        public void AddScooter(string id, decimal pricePerMinute)
        {
            //Throw an exception if price per minute equal to negative or zero values
            if (pricePerMinute <= 0)
                throw new ArgumentException(
                    "Price per minute must be positive",nameof(pricePerMinute));

            var s = new Scooter(id, pricePerMinute);
            ScooterComparer scooterComparer = new ScooterComparer();

            if (_scooterList.Contains(s, scooterComparer))
            {
                throw new ArgumentException("Duplicated scooter id",id);
            } else
                _scooterList.Add(s);
            
        }

        public Scooter GetScooterById(string scooterId)
        {

            return _scooterList.Find(scooter => scooter.Id == scooterId);
        }

        public IList<Scooter> GetScooters()
        {
            return _scooterList;
        }

        public void RemoveScooter(string id)
        {
            var scooterToRemove = _scooterList.SingleOrDefault(scooter => scooter.Id == id);
            if (scooterToRemove != null)
                if (scooterToRemove.IsRented) {
                    throw new Exception("Rented scooter can't be removed");
                }
            _scooterList.Remove(scooterToRemove);
        }
    }
}
