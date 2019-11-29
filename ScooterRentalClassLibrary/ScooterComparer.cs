using System.Collections.Generic;

namespace ScooterRentalClassLibrary
{
    public  class ScooterComparer : IEqualityComparer<Scooter>
    {
        public bool Equals(Scooter x, Scooter y)
        {
            //If both object refernces are equal then return true
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }
            //If one of the object refernce is null then return false
            if (x is null || y is null)
            {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(Scooter obj)
        {
            //If obj is null then return 0
            if (obj is null)
            {
                return 0;
            }

            int IDHashCode = obj.Id.GetHashCode();

            return IDHashCode;
        }
    }
    }
