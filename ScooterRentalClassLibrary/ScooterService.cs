using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public class ScooterService : IScooterService
    {
        private List<Scooter> _scooterList;
        decimal _pricePerMinute = 0.10M;
        public ScooterService() {
            _scooterList = new List<Scooter>();
            _scooterList.Add(new Scooter("1", _pricePerMinute));
            _scooterList.Add(new Scooter("2", _pricePerMinute));
            _scooterList.Add(new Scooter("3", _pricePerMinute));
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
                throw new Exception("Duplicated scooter id");
            } else
                _scooterList.Add(s);
            
        }

        public Scooter GetScooterById(string scooterId)
        {
            //Find scooter by id
            return _scooterList.Find(scooter => scooter.Id == scooterId);
        }

        public IList<Scooter> GetScooters()
        {
            //Return list of scooters
            return _scooterList;
        }

        public void RemoveScooter(string id)
        {
            var scooterToRemove = _scooterList.SingleOrDefault(scooter => scooter.Id == id);
            if (scooterToRemove != null)
                //Check if scooter is rented and cannot be removed, then throw an exception
                if (scooterToRemove.IsRented) {
                    throw new Exception("Rented scooter can't be removed");
                }
            _scooterList.Remove(scooterToRemove);
        }
    }
}
