using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VozovyPark
{
    class User
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

        public User(Guid id, string fName, string lName, string psswd)
        {
            _id = id;
            FirstName = fName;
            LastName = lName;
            Password = psswd;
        }
        
        // Use when ogin
        public User(string fName, string lName, string psswd)
        {
            FirstName = fName;
            LastName = lName;
            Password = psswd;
        }

        // Call to initialize if loading Users from file
        public User(Guid id, string fName, string lName, string encodedPsswd, int shift, DateTime lastLogDate)
        {
            _id = id;
            FirstName = fName;
            LastName = lName;
            Password = DecodePassword(shift, encodedPsswd);
            LastLoginDate = lastLogDate;
        }

        /*
         * Check if login user is this user
         * Compare login credentials
         */
        public bool isThisUser(User user)
        {
            return (FirstName == user.FirstName && LastName == user.LastName && Password == user.Password);
        }
        
        /*
         * Create new registration
         */
        public Reservation createNewReservation(List<Reservation> reservations, List<Vehicle> vehicles, DateTime todaysDate)
        {
            Console.WriteLine("Dostupná vozidla:");
                    for (int i = 0; i < vehicles.Count; i++)
                    {
                        if (!vehicles[i].isReserved)
                            Console.WriteLine($"[{i}] - {vehicles[i]}");
                    }
                    
                    List<int> reservedVehicleIndexes = new List<int>();
                    Console.WriteLine("\nRezervovaná vozidla");
                    for (int i = 0; i < vehicles.Count; i++)
                    {
                        if (vehicles[i].isReserved)
                        {
                            Console.WriteLine($"[{i}] - {vehicles[i]}");
                            reservedVehicleIndexes.Add(i);
                        }
                    }
                    
                    // VEHICLE
                    int vehicleIndex;
                    int attempt = 0;
                    bool successfulParse;
                    do
                    {
                        if (attempt > 0)
                            Console.WriteLine("Zadejte číslo přiřazené k vozu, které si přejete vybrat");
                        
                        Console.Write("Vozidlo: ");
                        attempt++;
                        successfulParse = int.TryParse(Console.ReadLine(), out vehicleIndex);
                    } while (!successfulParse || vehicleIndex < 0 || vehicleIndex >= vehicles.Count);
                    
                    vehicles[vehicleIndex].isReserved = true;
                    Vehicle selectedVehicle = vehicles[vehicleIndex];

                    DateTime nextAvailableDate;
                    bool selectedReservedVehicle = false;
                    List<Reservation> selectedVehicleReservations = new List<Reservation>();
                    if (reservedVehicleIndexes.Contains(vehicleIndex))
                    {
                        selectedReservedVehicle = true;
                        
                        foreach (Reservation x in reservations)
                        {
                            if (x.Vehicle.Id.Equals(selectedVehicle.Id))
                                selectedVehicleReservations.Add(x);
                        }
                    }
                    
                    // DATE FROM
                    DateTime dateFrom;
                    if (selectedReservedVehicle)
                    {
                        nextAvailableDate = Program.findNextAvailableDate(selectedVehicleReservations);
                        Console.WriteLine($"\nNejbližší volné datum rezervace ke zvolenému vozidlu je {nextAvailableDate.ToString("dd.MM.yyyy")}\n");
                        int mistakeCountDF = 0;
                        do
                        {
                            if (mistakeCountDF > 0)
                                Console.WriteLine($"Nejbližší volné datum rezervace ke zvolenému vozidlu je {nextAvailableDate.ToString("dd.MM.yyyy")}");
                            mistakeCountDF++;
                        
                            do
                            {
                                Console.WriteLine("Datum začátku rezervace (formát: 01.12.2020):");
                            } while (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, DateTimeStyles.None,
                                out dateFrom));

                        } while (DateTime.Compare(nextAvailableDate, dateFrom) > 0);
                    }
                    else
                    {
                        int mistakeCountDF = 0;
                        do
                        {
                            if (mistakeCountDF > 0)
                                Console.WriteLine("Datum začátku rezervace musí být minimálně zítřejší datum.");
                            mistakeCountDF++;
                        
                            do
                            {
                                Console.WriteLine("Datum začátku rezervace (formát: 01.12.2020):");
                            } while (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, DateTimeStyles.None,
                                out dateFrom));

                        } while (DateTime.Compare(todaysDate, dateFrom) >= 0);
                    }

                    // DATE TO
                    DateTime dateTo;
                    int dateCompareResult;
                    int mistakeCountDT = 0;
                    do
                    {
                        if (mistakeCountDT > 0)
                            Console.WriteLine("Datum konce rezervace musí být později než datum začátku");
                        mistakeCountDT++;
                        
                        do
                        {
                            Console.WriteLine("Datum konce rezervace (formát: 01.12.2020):");
                        } while (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, DateTimeStyles.None,
                            out dateTo));

                        dateCompareResult = DateTime.Compare(dateFrom, dateTo);
                    } while (dateCompareResult >= 0);
                    
                    
                    return new Reservation(Guid.NewGuid(), this, selectedVehicle, dateFrom, dateTo);
        }

        // For testing purpose 
        public override string ToString()
        {
            return $"===Admin {Id}===:\nId: {Id}\nJméno: {FirstName}\nPříjmení: {LastName}\n/Heslo: {password}\nNaposled přihlášen: {LastLoginDate.ToString()}";
        }

        public void print()
        {
            Console.WriteLine($"{FirstName} {LastName}");
        }

        /*
         * Encode and decode password with caesar cipher
         */
        private char Cipher(char ch, int shift)
        {
            if (!char.IsLetter(ch))
                return ch;

            char offset = char.IsUpper(ch) ? 'A' : 'a';
            return (char)((((ch + shift) - offset) % 26) + offset);
        }

        // Return password in format shift:encodedPassword to save in file
        public string encodePassword()
        {
            Random shiftGenerator = new Random();
            int shift = shiftGenerator.Next(1, 15);

            string password = Password;
            StringBuilder encodedPassword = new StringBuilder();

            foreach (char x in password)
                encodedPassword.Append(Cipher(x, shift));

            return $"{shift}:{encodedPassword}";
        }

        // Decode password from file
        public string DecodePassword(int shift, string encodedPassword)
        {
            StringBuilder decodedPassword = new StringBuilder();

            foreach (char x in encodedPassword)
                decodedPassword.Append(Cipher(x, 26 - shift));

            return decodedPassword.ToString();
        }

        /*
         * Parse user to right format for file
         */
        public string toFileFormat()
        {
            return $"<id>{Id}<id><n>{FirstName}<n><l>{LastName}<l><p>{encodePassword()}<p><d>{LastLoginDate.ToString()}<d>";
        }
    }
}