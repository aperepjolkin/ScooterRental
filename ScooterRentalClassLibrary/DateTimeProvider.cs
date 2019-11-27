using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public abstract class DateTimeProvider
    {
        private static DateTimeProvider current =
            DefaultTimeProvider.Instance;

        public static DateTimeProvider Current
        {
            get { return DateTimeProvider.current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                DateTimeProvider.current = value;
            }
        }

        public abstract DateTime Now { get; }

        public static void ResetToDefault()
        {
            DateTimeProvider.current = DefaultTimeProvider.Instance;
        }
    }

}
