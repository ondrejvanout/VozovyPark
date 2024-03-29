using System;
using System.Collections.Generic;
using System.Text;

namespace VozovyPark
{
    class Maintenance
    {
        private Guid _id;
        private Guid _carId;
        public Guid CarID { 
            private set => _carId = value;
            get => _carId;
        }
        private string service;
        private DateTime timeOfService;
        private decimal cost;
        private string invoiceNumber;

        public Maintenance(Guid id ,Guid carId, string service, DateTime time, decimal cost, string invoiceNum)
        {
            _id = id;
            _carId = carId;
            this.service = service;
            timeOfService = time;
            this.cost = cost;
            invoiceNumber = invoiceNum;
        }

        public string toFileFormat()
        {
            return $"<id>{_id}<id><cid>{_carId}<cid><s>{service}<s><t>{timeOfService.ToString()}<t><c>{cost}<c><ic>{invoiceNumber}<ic>";
        }

        public void print()
        {
            Console.WriteLine($"Servisní úkon: {service}\nČas provedení: {timeOfService}\nCena: {cost}\nČíslo faktury: {invoiceNumber}\n");
        }
    }

    class Vehicle
    {
        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => _id = value;
        }

        private string brand;
        public string Brand
        {
            get => brand;
            set => brand = value;
        }

        private string model;
        public string Model
        {
            get => model;
            set => model = value;
        }

        private string type;
        public string Type
        {
            get => type;
            set => type = value;
        }

        private double fuelConsumption;
        public double FuelConsumption
        {
            get => fuelConsumption;
            set => fuelConsumption = value;
        }

        public bool isReserved;

        public bool isDeleted;
        
        // Initialized in constructor empty. Add to list by method.
        private List<Maintenance> maintenancesWork;

        // Constructors
        public Vehicle(Guid id, string brand, string model, string type, double fuel_consumption, bool reserved)
        {
            _id = id;
            Brand = brand;
            Model = model;
            Type = type;
            FuelConsumption = fuel_consumption;
            isReserved = reserved;
            maintenancesWork = new List<Maintenance>();
            isDeleted = false;
        }
        
        public Vehicle(Guid id, string brand, string model, string type, double fuel_consumption, bool reserved, bool deleted)
        {
            _id = id;
            Brand = brand;
            Model = model;
            Type = type;
            FuelConsumption = fuel_consumption;
            isReserved = reserved;
            maintenancesWork = new List<Maintenance>();
            isDeleted = deleted;
        }

        public void addMaintenance(Maintenance maintenance)
        {
            maintenancesWork.Add(maintenance);
        }

        public int getMaintenaceCount()
        {
            return maintenancesWork.Count;
        }

        public Maintenance getMaintenanceByIndex(int index)
        {
            return maintenancesWork[index];
        }

        public string toFileFormat()
        {
            return $"<id>{Id}<id><b>{Brand}<b><m>{Model}<m><t>{Type}<t><f>{FuelConsumption}<f><r>{isReserved}<r><d>{isDeleted}<d>";
        }

        public void print()
        {
            Console.WriteLine($"{Brand} {Model}\n-typ: {Type}\n-spotřeba: {FuelConsumption} l/100 km");
            if (maintenancesWork.Count != 0)
            {
                Console.WriteLine("Servisní úkony vozidla:");
                for (int i = 0; i < maintenancesWork.Count; i++)
                    maintenancesWork[i].print();
            }
        }

        public int numberOfReservations(List<Reservation> reservations)
        {
            int count = 0;
            foreach (Reservation x in reservations)
            {
                if (Id.Equals(x.Vehicle.Id))
                    count++;
            }

            return count;
        }

        public override string ToString()
        {
            return $"{Brand} {Model} ({Type})";
        }
    }
}