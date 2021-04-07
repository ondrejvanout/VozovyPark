using System;
using System.Collections.Generic;
using System.Text;

namespace VozovyPark
{
    class Reservation
    {
        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => _id = value;
        }

        private User user;
        public User User
        {
            get => user;
            set => user = value;
        }

        private Vehicle vehicle;
        public Vehicle Vehicle
        {
            get => vehicle;
            set => vehicle = value;
        }

        private DateTime dateFrom;
        public DateTime DateFrom
        {
            get => dateFrom;
            set => dateFrom = value;
        }

        private DateTime dateTo;
        public DateTime DateTo
        {
            get => dateTo;
            set => dateTo = value;
        }

        public Reservation(Guid id, User user, Vehicle car, DateTime dateFrom, DateTime dateTo)
        {
            Id = id;
            User = user;
            Vehicle = car;
            DateFrom = dateFrom;
            DateTo = dateTo;
        }

        public string toFileFormat()
        {
            return $"<id>{Id}<id><uid>{User.Id}<uid><vid>{Vehicle.Id}<vid><df>{DateFrom}<df><dt>{DateTo}<dt>";
        }
    }
}