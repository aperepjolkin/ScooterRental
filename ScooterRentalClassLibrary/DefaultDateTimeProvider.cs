using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalClassLibrary
{
    public class DefaultTimeProvider : DateTimeProvider
    {
        private readonly static DefaultTimeProvider instance =
           new DefaultTimeProvider();

        private DefaultTimeProvider() { }

        public override DateTime Now
        {
            get { return DateTime.Now; }
        }

        public static DefaultTimeProvider Instance
        {
            get { return DefaultTimeProvider.instance; }
        }
    }
}
