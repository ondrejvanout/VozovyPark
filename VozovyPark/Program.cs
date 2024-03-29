﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace VozovyPark
{
    class Program
    {
        private const string BARRIER = "====================================================";
        private const string END_COMMAND = "end";
        
        private const string USERS_FILE_PATH = @"/home/ondra/RiderProjects/VozovyPark/VozovyPark/Users.txt";
        private const string VEHICLES_FILE_PATH = @"/home/ondra/RiderProjects/VozovyPark/VozovyPark/Vehicles.txt";
        private const string MAINTENACES_FILE_PATH = @"/home/ondra/RiderProjects/VozovyPark/VozovyPark/Maintenances.txt";
        private const string RESERVATIONS_FILE_PATH = @"/home/ondra/RiderProjects/VozovyPark/VozovyPark/Reservations.txt";

        private static DateTime todaysDate;
        
        static Dictionary<int, string> userRoles;

        static Dictionary<int, string> userOperations;
        static Dictionary<int, string> adminOperations;
        static Dictionary<int, string> reservationCategories;

        static List<User> users;
        static List<User> deletedUsers;
        static List<Vehicle> vehicles;
        static List<Vehicle> deletedVehicles;
        static List<Reservation> reservations;

        static Admin mainAdmin;
        static User mainUser;

        static void Main(string[] args)
        {
            todaysDate = DateTime.Today;

            userRoles = new Dictionary<int, string>();
            userRoles.Add(1, "Uživatel");
            userRoles.Add(2, "Administrátor");

            // Admin operations //
            adminOperations = new Dictionary<int, string>();
            adminOperations.Add(0, "Odhlásit");
            adminOperations.Add(1, "Založit uživatele");
            adminOperations.Add(2, "Smazat uživatele");
            adminOperations.Add(3, "Vložit vozidlo");
            adminOperations.Add(4, "Přidání servisního úkonu");
            adminOperations.Add(5, "Odstranit vozidlo");
            adminOperations.Add(6, "Vložit rezervaci jménem uživatele");
            adminOperations.Add(7, "Vynutit změnu hesla");
            adminOperations.Add(8, "Zobrazit všechny uživatele");
            adminOperations.Add(9, "Zobrazit všechna auta");
            adminOperations.Add(10, "Zobrazit rezervace");
            
            // Reservation categories to select from when showing all reservations
            reservationCategories = new Dictionary<int, string>();
            reservationCategories.Add(0, "Po autech");
            reservationCategories.Add(1, "Po osobách");
            reservationCategories.Add(2, "Historické rezervace");
            reservationCategories.Add(3, "Aktuální rezervace");
            
            // User operations //
            userOperations = new Dictionary<int, string>();
            userOperations.Add(0, "Odhlásit");
            userOperations.Add(1, "Změnit heslo");
            userOperations.Add(2, "Zadat rezervaci");
            userOperations.Add(3, "Zrušit rezervaci");
            userOperations.Add(4, "Zobrazit aktuální rezervace");
            
            // Main admin
            mainAdmin = new Admin(new Guid(), "Admin", "Admin", "heslo");

            // List of all users
            users = new List<User>();
            // List of all deleted users
            deletedUsers = new List<User>();
            // List of all vehicles
            vehicles = new List<Vehicle>();
            // List of all deleted vehicles
            deletedVehicles = new List<Vehicle>();
            // List of all reservations
            reservations = new List<Reservation>();
            

            string command = String.Empty;
            
            // LOAD FROM FILES //
            string[] usersFromFile = File.ReadAllLines(USERS_FILE_PATH);
            string[] vehiclesFromFile = File.ReadAllLines(VEHICLES_FILE_PATH);
            string[] maintenancesFromFile = File.ReadAllLines(MAINTENACES_FILE_PATH);
            string[] reservationsFromFile = File.ReadAllLines(RESERVATIONS_FILE_PATH);
            
            Regex regex;
            GroupCollection groups;
            
            // Users
            foreach (string user in usersFromFile)
            {
                // ID
                regex = new Regex("<id>(.*)<id>");
                Match idMatch = regex.Match(user);
                groups = idMatch.Groups;
                Guid id = new Guid(groups[1].ToString());
                
                // NAME
                regex = new Regex("<n>(.*)<n>");
                Match nameMatch = regex.Match(user);
                groups = nameMatch.Groups;
                string name = groups[1].ToString();
                
                // LAST NAME
                regex = new Regex("<l>(.*)<l>");
                Match lastNameMatch = regex.Match(user);
                groups = lastNameMatch.Groups;
                string lastName = groups[1].ToString();
                
                // PASSWORD 
                regex = new Regex("<p>(.*)<p>");
                Match passwordMatch = regex.Match(user);
                groups = passwordMatch.Groups;
                string[] encodedPassword = groups[1].ToString().Split(":");

                // LAST LOGIN DATE 
                regex = new Regex("<d>(.*)<d>");
                Match dateMatch = regex.Match(user);
                groups = dateMatch.Groups;
                DateTime lastLoginDate = DateTime.Parse(groups[1].ToString());
                
                // FORCED PASSWORD CHANGE
                regex = new Regex("<pc>(.*)<pc>");
                Match passwordChangeMatch = regex.Match(user);
                groups = passwordChangeMatch.Groups;
                bool requiredPasswordChange = Boolean.Parse(groups[1].ToString());

                User currentUser = new User(id, name, lastName, encodedPassword[1], int.Parse(encodedPassword[0]), lastLoginDate, requiredPasswordChange);
                
                if (name.Equals(string.Empty) && lastName.Equals(string.Empty) && encodedPassword[1].Equals(string.Empty))
                    deletedUsers.Add(currentUser);
                else
                    users.Add(currentUser);
            }

            // Vehicles
            foreach (string vehicle in vehiclesFromFile)
            {
                // ID
                regex = new Regex("<id>(.*)<id>");
                Match idMatch = regex.Match(vehicle);
                groups = idMatch.Groups;
                Guid id = new Guid(groups[1].ToString());
                
                // BRAND
                regex = new Regex("<b>(.*)<b>");
                Match brandMatch = regex.Match(vehicle);
                groups = brandMatch.Groups;
                string brand = groups[1].ToString();
                
                // MODEL
                regex = new Regex("<m>(.*)<m>");
                Match modelMatch = regex.Match(vehicle);
                groups = modelMatch.Groups;
                string model = groups[1].ToString();
                
                // TYPE
                regex = new Regex("<t>(.*)<t>");
                Match typeMatch = regex.Match(vehicle);
                groups = typeMatch.Groups;
                string type = groups[1].ToString();
                
                // FUEL CONSUMPTION
                regex = new Regex("<f>(.*)<f>");
                Match fuelMatch = regex.Match(vehicle);
                groups = fuelMatch.Groups;
                double fuelConsumption = double.Parse(groups[1].ToString());
                
                // IS RESERVED
                regex = new Regex("<r>(.*)<r>");
                Match isReservedMatch = regex.Match(vehicle);
                groups = isReservedMatch.Groups;
                bool isReserved = Boolean.Parse(groups[1].ToString());
                
                // IS RESERVED
                regex = new Regex("<d>(.*)<d>");
                Match isDeletedMatch = regex.Match(vehicle);
                groups = isDeletedMatch.Groups;
                bool isDeleted = Boolean.Parse(groups[1].ToString());
                
                if (isDeleted)
                    deletedVehicles.Add(new Vehicle(id, brand, model, type, fuelConsumption, isReserved, isDeleted));
                else
                    vehicles.Add(new Vehicle(id, brand, model, type, fuelConsumption, isReserved));
            }
            
            // Maintenances
            foreach (string maintenance in maintenancesFromFile)
            {
                // ID
                regex = new Regex("<id>(.*)<id>");
                Match idMatch = regex.Match(maintenance);
                groups = idMatch.Groups;
                Guid id = new Guid(groups[1].ToString());
                
                // CAR ID
                regex = new Regex("<cid>(.*)<cid>");
                Match carIdMatch = regex.Match(maintenance);
                groups = carIdMatch.Groups;
                Guid carId = new Guid(groups[1].ToString());
                
                // SERVICE
                regex = new Regex("<s>(.*)<s>");
                Match serviceMatch = regex.Match(maintenance);
                groups = serviceMatch.Groups;
                string service = groups[1].ToString();
                
                // DATE OF SERVICE
                regex = new Regex("<t>(.*)<t>");
                Match dateMatch = regex.Match(maintenance);
                groups = dateMatch.Groups;
                DateTime dateOfService = DateTime.Parse(groups[1].ToString());
                
                // COST
                regex = new Regex("<c>(.*)<c>");
                Match costMatch = regex.Match(maintenance);
                groups = costMatch.Groups;
                decimal cost = Decimal.Parse(groups[1].ToString());
                
                // INVOICE NUMBER
                regex = new Regex("<ic>(.*)<ic>");
                Match invoiceNumMatch = regex.Match(maintenance);
                groups = invoiceNumMatch.Groups;
                string invoiceNumber = groups[1].ToString();

                Maintenance currentMaintenance = new Maintenance(id, carId, service, dateOfService, cost, invoiceNumber);
                
                // Assign maintenance to vehicle
                for (int i = 0; i < vehicles.Count; i++)
                {
                    if (vehicles[i].Id == currentMaintenance.CarID)
                        vehicles[i].addMaintenance(currentMaintenance);
                }

                for (int i = 0; i < deletedVehicles.Count; i++)
                {
                    if (deletedVehicles[i].Id == currentMaintenance.CarID)
                        deletedVehicles[i].addMaintenance(currentMaintenance);
                }
            }
  
            // Reservations
            foreach (string reservation in reservationsFromFile)
            {
                // ID
                regex = new Regex("<id>(.*)<id>");
                Match idMatch = regex.Match(reservation);
                groups = idMatch.Groups;
                Guid id = new Guid(groups[1].ToString());
                
                // USER ID
                regex = new Regex("<uid>(.*)<uid>");
                Match uidMatch = regex.Match(reservation);
                groups = uidMatch.Groups;
                Guid userId = new Guid(groups[1].ToString());
                
                // VEHICLE ID
                regex = new Regex("<vid>(.*)<vid>");
                Match vidMatch = regex.Match(reservation);
                groups = vidMatch.Groups;
                Guid vehicleId = new Guid(groups[1].ToString());
                
                // DATE FROM
                regex = new Regex("<df>(.*)<df>");
                Match dateFromMatch = regex.Match(reservation);
                groups = dateFromMatch.Groups;
                DateTime dateFrom = DateTime.Parse(groups[1].ToString());
                
                // DATE TO
                regex = new Regex("<dt>(.*)<dt>");
                Match dateToMatch = regex.Match(reservation);
                groups = dateToMatch.Groups;
                DateTime dateTo = DateTime.Parse(groups[1].ToString());
                
                // Find correct user by id
                User correctUser = null;
                foreach (User user in users)
                {
                    if (userId.Equals(user.Id))
                        correctUser = user;
                }
                
                // Find correct vehicle by id
                Vehicle correctVehicle = null;
                foreach (Vehicle vehicle in vehicles)
                {
                    if (vehicleId.Equals(vehicle.Id))
                        correctVehicle = vehicle;
                }
                
                reservations.Add(new Reservation(id, correctUser, correctVehicle, dateFrom, dateTo));
            }
            
            
            // Main loop - end if user command = "end" //
            while (true)
            {
                // Default window
                command = getRole();
                if (command == END_COMMAND)
                {
                    break;
                }
                else
                {
                    int selectedRole;
                    if (int.TryParse(command, out selectedRole))
                    {
                        switch (selectedRole)
                        {
                            case 1:
                                /*
                                 * User login
                                 */
                                Console.WriteLine(BARRIER);

                                bool successLogin = false;
                                int failedLoginCount = -1;
                                do
                                {
                                    failedLoginCount++;
                                    if (failedLoginCount > 0)
                                        Console.WriteLine("Špatné přihlašovací údaje");

                                    Console.WriteLine("Login");
                                    mainUser = loginUser();
                                } while (mainUser == null);
                                
                                /*
                                 * Required password change
                                 */
                                if (mainUser.forcedPsswdChange)
                                {
                                    Console.WriteLine("==Vyžádána změna hesla==");
                                    Console.WriteLine("Nové heslo:");
                                    mainUser.Password = Console.ReadLine();
                                    mainUser.forcedPsswdChange = false;
                                }

                                /*
                                 * User´s operations 
                                 */
                                Console.WriteLine($"{BARRIER}\n{BARRIER}");
                                Console.WriteLine("Příhlášen jako uživatel");
                                Console.WriteLine($"Jméno: {mainUser.FirstName}\nPříjmení: {mainUser.LastName}\n" +
                                                  $"Poslední přihlášení: {mainUser.LastLoginDate.ToString("HH:mm dd.MM.yyyy")}\n");

                                // Update last login date
                                mainUser.LastLoginDate = DateTime.Now;
                                
                                int operationCode;
                                do
                                {
                                    if (int.TryParse(getUserOpCode(), out operationCode))
                                        executeUserOperation(operationCode);
                                    else
                                    {
                                        Console.WriteLine("Špatně zadaná instrukce. Zadejte číslo přiřazené k instrukci, kterou chcete provést.");
                                        operationCode = int.MaxValue;
                                    }
                                } while (operationCode != 0);

                                break;
                            case 2:
                                /*
                                 * Admin login
                                 */
                                Console.WriteLine(BARRIER);

                                bool successfulLogin = false;
                                int failedLoginCounter = -1; // If > 0 print error message
                                do
                                {
                                    failedLoginCounter++;
                                    if (failedLoginCounter > 0)
                                        Console.WriteLine("Špatné přihlašovací údaje.\n");

                                    Console.WriteLine("Login jako admin:");
                                    successfulLogin = loginAdmin();
                                } while (!successfulLogin);
                                Console.WriteLine("Úspěšně přihlášen");

                                /*
                                 * Admin´s operations 
                                 */
                                Console.WriteLine($"{BARRIER}\n{BARRIER}");
                                Console.WriteLine("Příhlášen jako Administrátor");
                                Console.WriteLine($"Jméno: {mainAdmin.FirstName}\nPříjmení: {mainAdmin.LastName}\n" +
                                                $"Poslední přihlášení: {mainAdmin.LastLoginDate}\n");

                                int opCode;
                                do
                                {
                                    if (int.TryParse(getAdminOpCode(), out opCode))
                                        executeAdminOperation(opCode);
                                    else
                                    {
                                        Console.WriteLine("Špatně zadaná instrukce. Zadejte číslo přiřazené k instrukci, kterou chcete provést.");
                                        opCode = int.MaxValue;
                                    }
                                } while (opCode != 0);

                                break;
                            default:
                                Console.WriteLine($"Číslo {selectedRole} neodkazuje na žádnou roli\n");
                                break;
                        }
                    }
                }
            }

            /*
             * SAVE TO FILES
             */
            // Users
            List<string> usersInFileFormat = new List<string>();
            foreach (User x in users)
            {
                usersInFileFormat.Add(x.toFileFormat());
            }

            foreach (User x in deletedUsers)
            {
                usersInFileFormat.Add(x.toFileFormat());
            }
            File.WriteAllLines(USERS_FILE_PATH, usersInFileFormat);

            // Vehicles
            List<string> vehiclesInFileFormat = new List<string>();
            // Maintenances
            List<string> maintenancesInFileFormat = new List<string>();
            foreach (Vehicle x in vehicles)
            {
                vehiclesInFileFormat.Add(x.toFileFormat());
                for (int i = 0; i < x.getMaintenaceCount(); i++)
                    maintenancesInFileFormat.Add(x.getMaintenanceByIndex(i).toFileFormat());
            }

            foreach (Vehicle deletedVehicle in deletedVehicles)
            {
                vehiclesInFileFormat.Add(deletedVehicle.toFileFormat());
                for (int i = 0; i < deletedVehicle.getMaintenaceCount(); i++)
                    maintenancesInFileFormat.Add(deletedVehicle.getMaintenanceByIndex(i).toFileFormat());
            }
            File.WriteAllLines(VEHICLES_FILE_PATH, vehiclesInFileFormat);
            File.WriteAllLines(MAINTENACES_FILE_PATH, maintenancesInFileFormat);
            
            // Reservations
            List<string> reservationsInFileFormat = new List<string>();
            foreach (Reservation x in reservations)
            {
                reservationsInFileFormat.Add(x.toFileFormat());
            }
            File.WriteAllLines(RESERVATIONS_FILE_PATH, reservationsInFileFormat);
        }

        static string getRole()
        {
            Console.WriteLine("Zvolte roli:");
            foreach (KeyValuePair<int, string> role in userRoles)
                Console.WriteLine($"[{role.Key}] - {role.Value}");

            Console.WriteLine("\n(Pro ukončení programu zadejte \"end\")");

            return Console.ReadLine();
        }

        static bool loginAdmin()
        {
            Console.WriteLine("Zadejte jméno:");
            string name = Console.ReadLine();

            Console.WriteLine("Zadejte příjmení:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Zadejte heslo:");
            string password = Console.ReadLine();

            Admin admin = new Admin(name, lastName, password);

            // Check if admin exists
            if (mainAdmin.isThisAdmin(name, lastName, password))
                return true;
            else
                return false;
        }

        static User loginUser()
        {
            Console.WriteLine("Zadejte jméno:");
            string name = Console.ReadLine();

            Console.WriteLine("Zadejte příjmení:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Zadejte heslo:");
            string password = Console.ReadLine();

            User user = new User(name, lastName, password);

            foreach (User currentUser in users)
            {
                if (currentUser.isThisUser(user))
                    return currentUser;
            }

            return null;
        }

        static string getAdminOpCode()
        {
            Console.WriteLine("Funkce:");

            foreach (KeyValuePair<int, string> operation in adminOperations)
                Console.WriteLine($"[{operation.Key}] - {operation.Value}");

            Console.Write("Operace: ");
            return Console.ReadLine();
        }

        static string getUserOpCode()
        {
            Console.WriteLine("Funkce:");

            foreach (KeyValuePair<int, string> operation in userOperations)
                Console.WriteLine($"[{operation.Key}] - {operation.Value}");    
                
            Console.Write("Operace: ");
            return Console.ReadLine();
        }

        static void executeAdminOperation(int opCode)
        {
            Console.WriteLine(BARRIER);

            switch(opCode)
            {
                case 0:
                    // Logout //
                    Console.WriteLine("\nAdministrátor odhlášen");
                    break;
                case 1:
                    // Create new user //
                    users.Add(mainAdmin.createNewUser());
                    Console.WriteLine("Uživatel úspešně přidán.");
                    break;
                case 2:
                    // Delete user //
                    string[] credentials = mainAdmin.getUserToDelete();

                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].FirstName == credentials[0] && users[i].LastName == credentials[1])
                        {
                            bool removeReservation;
                            List<int> deleteReservationIndexes = new List<int>();
                            
                            // Check user's reservations
                            for (int j = 0; j < reservations.Count; j++)
                            {
                                Reservation actReservation = reservations[j];
                                if (actReservation.User.Id.Equals(users[i].Id))
                                {
                                    int numberOfReservations = actReservation.Vehicle.numberOfReservations(reservations);
                                    if (numberOfReservations == 1)
                                    {
                                        int indexOfVehicle = vehicles.IndexOf(actReservation.Vehicle);
                                        if (indexOfVehicle != -1)
                                            vehicles[indexOfVehicle].isReserved = false;
                                    }

                                    deleteReservationIndexes.Add(i);
                                }
                            }

                            foreach (int x in deleteReservationIndexes)
                            {
                                reservations.RemoveAt(x);
                            }

                            deletedUsers.Add(mainAdmin.deleteUser(users[i]));
                            users.RemoveAt(i);
                            break;
                        }
                    }
                    Console.WriteLine("Uživatel úspešně odstraněn.");
                    break;
                case 3:
                    // Create new vehicle //
                    Vehicle newVehicle = mainAdmin.createNewVehicle();

                    string userAction;
                    do
                    {
                        Console.WriteLine("Zadat záznam o údržbě vozidla [y/n]");
                        userAction = Console.ReadLine();
                        
                        if (userAction.Equals("y") || userAction.Equals("Y"))
                            mainAdmin.addMaintenanceToVehicle(ref newVehicle);
                        
                    } while (userAction.Equals("y") || userAction.Equals("Y"));

                    vehicles.Add(newVehicle);
                    Console.WriteLine("Vozidlo úspešně přidáno.");
                    break;
                case 4:
                    Console.WriteLine("Dostupná vozidla:");
                    for (int i = 0; i < vehicles.Count; i++) 
                        Console.WriteLine($"[{i}] - {vehicles[i]}");
                    
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
                    
                    if (vehicles.ElementAt(vehicleIndex) != null)
                    {
                        Vehicle currentVehicle = vehicles[vehicleIndex];
                        string userSelector;
                        do
                        {
                            mainAdmin.addMaintenanceToVehicle(ref currentVehicle);
                            Console.WriteLine("Zadat záznam o údržbě vozidla [y/n]");
                            userSelector = Console.ReadLine();
                        } while (userSelector.Equals("y") || userSelector.Equals("Y"));

                        vehicles[vehicleIndex] = currentVehicle;
                    }

                    break;
                case 5:
                    // Delete vehicle
                    Console.WriteLine("Dostupná vozidla:");
                    for (int i = 0; i < vehicles.Count; i++) 
                        Console.WriteLine($"[{i}] - {vehicles[i]}");
                    
                    int index;
                    int parseAttempt = 0;
                    bool successful;
                    do
                    {
                        if (parseAttempt > 0)
                            Console.WriteLine("Zadejte číslo přiřazené k vozu, které si přejete vybrat");
                        
                        Console.Write("Vozidlo: ");
                        parseAttempt++;
                        successful = int.TryParse(Console.ReadLine(), out index);
                    } while (!successful || index < 0 || index >= vehicles.Count);

                    if (vehicles.ElementAt(index) != null)
                    {
                        Vehicle deletedVehicle = vehicles[index];
                        
                        // Delete reservations with this vehicle
                        for (int i = 0; i < reservations.Count; i++)
                        {
                            if (reservations[i].Vehicle.Id.Equals(deletedVehicle.Id))
                                reservations.RemoveAt(i);
                        }
                        
                        deletedVehicle.isDeleted = true;
                        deletedVehicles.Add(deletedVehicle);
                        vehicles.RemoveAt(index);
                        
                        Console.WriteLine("\nVozidlo úspěšně odstraněno\n");
                    }
                    
                    break;
                case 6:
                    // Add reservation as user
                    
                    // Select user
                    Console.WriteLine("==Uživatelé==:");
                    for (int i = 0; i < users.Count; i++)
                    {
                        Console.Write($"[{i}] - ");
                        users[i].print();
                    }
                    
                    int userIndex;
                    int indexParseAttempt = 0;
                    bool parseSuccess;
                    do
                    {
                        if (indexParseAttempt > 0)
                            Console.WriteLine("Zadejte číslo přiřazené k vozu, které si přejete vybrat");
                        
                        Console.Write("Vozidlo: ");
                        indexParseAttempt++;
                        parseSuccess = int.TryParse(Console.ReadLine(), out userIndex);
                    } while (!parseSuccess || userIndex < 0 || userIndex >= vehicles.Count);

                    User selectedUser = users[userIndex];
                    Console.WriteLine();

                    // Reservation
                    reservations.Add(selectedUser.createNewReservation(reservations, vehicles, todaysDate));
                    break;
                case 7:
                    // Forced password change
                    int selectIndex = selectUser(users);
                    users[selectIndex].forcedPsswdChange = true;
                    
                    Console.WriteLine("\nZměna hesla vynucena.\n");
                    break;
                case 8:
                    Console.WriteLine("==Uživatelé==:");
                    for (int i = 0; i < users.Count; i++) 
                        users[i].print();
                    
                    Console.WriteLine();
                    break;
                case 9:
                    Console.WriteLine("==Vozidla==:");
                    for (int i = 0; i < vehicles.Count; i++) 
                        vehicles[i].print();
                    
                    Console.WriteLine("\n==Odstraněná vozidla==:");
                    for (int i = 0; i < deletedVehicles.Count; i++)
                        deletedVehicles[i].print();
                    
                    break;
                case 10:
                    // Show all reservations
                    Console.WriteLine("==Zobrazit rezervace==");
                    foreach (KeyValuePair<int, string> category in reservationCategories)
                    {
                        Console.WriteLine($"[{category.Key}] - {category.Value}");
                    }
                    Console.WriteLine();
                    
                    int categoryIndex;
                    int parseAtt = 0;
                    bool success;
                    do
                    {
                        if (parseAtt > 0)
                            Console.WriteLine("Zadejte číslo přiřazené ke kategorii, podle které si přejete zobrazit rezervace");
                        
                        Console.Write("Kategorie: ");
                        parseAtt++;
                        success = int.TryParse(Console.ReadLine(), out categoryIndex);
                    } while (!success || !reservationCategories.ContainsKey(categoryIndex));

                    switch (categoryIndex)
                    {
                        case 0:
                            // Reservations by vehicle
                            int carIndex = selectVehicle(vehicles);

                            Console.Write($"Rezervace vozidla {vehicles[carIndex]}\n");
                            int vCount = 0;
                            foreach (Reservation reservation in reservations)
                            {
                                if (reservation.Vehicle.Id.Equals(vehicles[carIndex].Id))
                                {
                                    reservation.User.print();
                                    reservation.print();
                                    vCount++;
                                }
                            }
                            
                            if (vCount == 0)
                                Console.WriteLine("Vozidlo nemá žádné rezervace.\n");
                            
                            break;
                        case 1:
                            // Reservations by user
                            int uIndex = selectUser(users);
                            
                            Console.Write($"Rezervace uživatele ");
                            users[uIndex].print();

                            int uCount = 0;
                            foreach (Reservation reservation in reservations)
                            {
                                if (reservation.User.Id.Equals(users[uIndex].Id))
                                {
                                    reservation.print();
                                    uCount++;
                                }
                            }
                            
                            if (uCount == 0)
                                Console.WriteLine("Uživatel nemá žádné rezervace.\n");
                            
                            break;
                        case 2:
                            // Historic reservations
                            Console.WriteLine("==Historické rezervace==:");
                            int hCount = 0;
                            foreach (Reservation reservation in reservations)
                            {
                                if (DateTime.Compare(reservation.DateTo, todaysDate) < 0)
                                {
                                    reservation.User.print();
                                    reservation.print();
                                    hCount++;
                                }
                            }
                            
                            if (hCount == 0)
                                Console.WriteLine("Nejsou žádné historické rezervace.\n");
                            
                            break;
                        case 3:
                            // Actual reservations
                            Console.WriteLine("==Aktuální rezervace==:");
                            int aCount = 0;
                            foreach (Reservation reservation in reservations)
                            {
                                if (DateTime.Compare(todaysDate, reservation.DateTo) < 0)
                                {
                                    reservation.User.print();
                                    reservation.print();
                                    aCount++;
                                }
                            }

                            if (aCount == 0) 
                                Console.WriteLine("Nejsou žádné aktuální rezervace.\n");
                            
                            break;
                        default:
                            Console.WriteLine("Nastala chyba.");
                            break;
                    }
                    
                    break;
                default:
                    Console.WriteLine($"Operace s kódem {opCode} neexistuje.");
                    break;
            }
        }

        static void executeUserOperation(int opCode)
        {
            Console.WriteLine(BARRIER);

            switch (opCode)
            {
                case 0:
                    // Log out
                    Console.WriteLine("Uživatel odhlášen");
                    break;
                case 1:
                    // Change password
                    Console.WriteLine("Nové heslo:");
                    mainUser.Password = Console.ReadLine();
                    break;
                case 2:
                    // Create reservation
                    reservations.Add(mainUser.createNewReservation(reservations, vehicles, todaysDate));
                    
                    Console.WriteLine("\nRezervace úspěšně přidána\n");
                    break;
                case 3:
                    // Delete reservation
                    int reservationIndex = selectUsersReservation(reservations, mainUser);

                    Vehicle vehicle = reservations[reservationIndex].Vehicle;
                    int numberOfReservations = 0;
                    foreach (Reservation x in reservations)
                    {
                        if (vehicle.Id.Equals(x.Vehicle.Id))
                            numberOfReservations++;
                    }

                    if (numberOfReservations == 1)
                        vehicle.isReserved = false;
                    
                    reservations.RemoveAt(reservationIndex);
                    Console.WriteLine("\nRezervace úpěšně odstraněna\n");
                    break;
                case 4:
                    // Show all reservations
                    Console.WriteLine("Moje rezervace:");
                    foreach (Reservation reservation in reservations)
                    {
                        if (mainUser.Id.Equals(reservation.User.Id))
                            reservation.print();
                    }
                    
                    break;
                default:
                    Console.WriteLine($"Operace s kódem {opCode} neexistuje.");
                    break;
            }
        }


        public static DateTime findNextAvailableDate(List<Reservation> lReservations)
        {
            DateTime nextDate = lReservations[0].DateTo.AddDays(1.0);
            
            if (lReservations.Count == 1)
                return lReservations[0].DateTo.AddDays(1.0);
            else
            {
                for (int i = 1; i < lReservations.Count; i++)
                {
                    DateTime currentDate = lReservations[i].DateTo;
                    if (DateTime.Compare(nextDate, currentDate) < 0)
                        nextDate = currentDate;
                }
            }

            return nextDate;
        }
        
        /*
         * Select vehicle fro vehicles list
         */
        public static int selectVehicle(List<Vehicle> vehiclesList)
        {
            // Select vehicle
            Console.WriteLine("\n==Vozidla==:");
            for (int i = 0; i < vehiclesList.Count; i++)
            {
                Console.Write($"[{i}] - ");
                Console.WriteLine(vehiclesList[i]);
            }
                    
            int vehicleIndex;
            int parseAttempt = 0;
            bool successfulParse;
            do
            {
                if (parseAttempt > 0)
                    Console.WriteLine("Zadejte číslo přiřazené k vozidlu, které si přejete vybrat");
                        
                Console.Write("Vozidlo: ");
                parseAttempt++;
                successfulParse = int.TryParse(Console.ReadLine(), out vehicleIndex);
            } while (!successfulParse || vehicleIndex < 0 || vehicleIndex >= vehiclesList.Count);

            return vehicleIndex;
        }
        
        /*
         * Select user from users list
         */
        public static int selectUser(List<User> userList)
        {
            // Select user
            Console.WriteLine("\n==Uživatelé==:");
            for (int i = 0; i < userList.Count; i++)
            {
                Console.Write($"[{i}] - ");
                userList[i].print();
            }
                    
            int userIndex;
            int parseAttempt = 0;
            bool successfulParse;
            do
            {
                if (parseAttempt > 0)
                    Console.WriteLine("Zadejte číslo přiřazené k uživateli, kterého si přejete vybrat");
                        
                Console.Write("Uživatel: ");
                parseAttempt++;
                successfulParse = int.TryParse(Console.ReadLine(), out userIndex);
            } while (!successfulParse || userIndex < 0 || userIndex >= userList.Count);

            return userIndex;
        }
        
        /*
         * Select reservation from reservations list
         */
        public static int selectUsersReservation(List<Reservation> resevationsList, User user)
        {
            Console.WriteLine("\n==Rezervace==:");

            // Populate users reservation list
            List<int> usersReservationIndexes = new List<int>();
            for (int i = 0; i < resevationsList.Count; i++)
            {
                if (resevationsList[i].User.Id.Equals(user.Id))
                {
                    Console.Write($"[{i}] - ");
                    resevationsList[i].print();
                    usersReservationIndexes.Add(i);
                }
            }

            int reservationIndex;
            int parseAttempt = 0;
            bool successfulParse;
            do
            {
                if (parseAttempt > 0)
                    Console.WriteLine("Zadejte číslo přiřazené k rezervaci, kterou si přejete vybrat");
                        
                Console.Write("Uživatel: ");
                parseAttempt++;
                successfulParse = int.TryParse(Console.ReadLine(), out reservationIndex);
            } while (!successfulParse || !usersReservationIndexes.Contains(reservationIndex));

            return reservationIndex;
        }
    }
}