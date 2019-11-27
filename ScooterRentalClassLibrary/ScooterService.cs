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
            var s = new Scooter(id, pricePerMinute);

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
                _scooterList.Remove(scooterToRemove);
        }
    }
}
