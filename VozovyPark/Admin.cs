using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VozovyPark
{
    class Admin
    {
        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => _id = value;
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set => firstName = value;
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set => lastName = value;
        }

        private string password;
        public string Password
        {
            get => password;
            set => password = value;
        }

        private DateTime lastLoginDate;
        public DateTime LastLoginDate
        {
            get => lastLoginDate;
            set => lastLoginDate = value;
        }

        // Use when creating new admin
        public Admin(Guid id, string fName, string lName, string psswd)
        {
            _id = id;
            FirstName = fName;
            LastName = lName;
            Password = psswd;
        }

        // Use when login
        public Admin(string fName, string lName, string psswd)
        {
            FirstName = fName;
            LastName = lName;
            Password = psswd;
        }

        /*
         * Check if login admin is the main admin
         * Compare login credentials
         */
        public bool isThisAdmin(string fName, string lName, string psswd)
        {
            return (FirstName == fName && LastName == lName && Password == psswd);
        }

        /*
         * Create new user
         */
        public User createNewUser()
        {
            Console.WriteLine("Nový uživatel:");

            Console.Write("Jméno: ");
            string fName = Console.ReadLine();

            Console.Write("Příjmení: ");
            string lName = Console.ReadLine();

            Console.Write("Heslo: ");
            string psswd = Console.ReadLine();

            return new User(Guid.NewGuid(), fName, lName, psswd);
        }
        
        /*
         * Get credentials of user for deleting
         */
        public string[] getUserToDelete()
        {
            Console.WriteLine("Vymazat uživatele:");

            Console.Write("Jméno: ");
            string fName = Console.ReadLine();

            Console.Write("Příjmení: ");
            string lName = Console.ReadLine();

            return new string[] { fName, lName };
        }

        /*
         * Delete user
         */
        public User deleteUser(User user)
        {
            // Add to list of deleted users?

            return new User(user.Id, string.Empty, string.Empty, string.Empty);
        }

        /*
         * Add vehicle
         */
        public Vehicle createNewVehicle()
        {
            Console.WriteLine("Nové vozidlo");

            Console.Write("Značka: ");
            string brand = Console.ReadLine();

            Console.Write("Model: ");
            string model = Console.ReadLine();

            Console.Write("Typ: ");
            string type = Console.ReadLine();

            double fuelConsumption = 0.0;
            bool successfulConversion;
            do
            {
                successfulConversion = true;
                try
                {
                    Console.Write("Spotřeba [l/100km]: ");
                    string fuelConsumptionText = Console.ReadLine();
                    fuelConsumption = double.Parse(fuelConsumptionText);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Špatně zadaná spotřeba. Zadejte ve formě desetiného čísla.\n");
                    successfulConversion = false;
                }
            } while (!successfulConversion);

            return new Vehicle(Guid.NewGuid(), brand, model, type, fuelConsumption, false);
        }

        /*
         * Add vehicle maintenance
         */
        public void addMaintenanceToVehicle(ref Vehicle vehicle)
        {
            Console.WriteLine("Servisní úkon:");
            string service = Console.ReadLine();
 
            DateTime time;
            do
            {
                Console.WriteLine("Čas servisního úkonu. Formát: 14:15 21.05.2021");
            } while (!DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd.MM.yyyy", null, DateTimeStyles.None, out time));

            decimal cost;
            do
            {
                Console.WriteLine("Cena:");
            } while (!decimal.TryParse(Console.ReadLine(), out cost));

            int invoiceNumber;
            do
            {
                Console.WriteLine("Číslo faktury:");
            } while (!int.TryParse(Console.ReadLine(), out invoiceNumber));

            vehicle.addMaintenance(new Maintenance(vehicle.Id, service, time, cost, invoiceNumber));
        }


        public override string ToString()
        {
            return $"===Admin {Id}===:\nJméno: {FirstName}\nPříjmení: {LastName}\n/Heslo: {password}\nNaposled přihlášen: {LastLoginDate.ToString()}";
        }


    }
}