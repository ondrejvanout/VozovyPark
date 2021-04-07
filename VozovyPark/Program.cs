using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.IO;
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

        static Dictionary<int, string> userRoles;

        static Dictionary<int, string> userOperations;
        static Dictionary<int, string> adminOperations;

        static List<User> users;
        static List<Vehicle> vehicles;

        static Admin mainAdmin;
        static User currentUser;

        static void Main(string[] args)
        {
            userRoles = new Dictionary<int, string>();
            userRoles.Add(1, "Uživatel");
            userRoles.Add(2, "Administrátor");

            adminOperations = new Dictionary<int, string>();
            adminOperations.Add(0, "Odhlásit");
            adminOperations.Add(1, "Založit uživatele");
            adminOperations.Add(2, "Smazat uživatele");
            adminOperations.Add(3, "Vložit vozidlo");
            adminOperations.Add(4, "Odstranit vozidlo");
            adminOperations.Add(5, "Vložit rezervaci jménem uživatele");
            adminOperations.Add(6, "Vynutit změnu hesla");
            adminOperations.Add(7, "Zobrazit všechny uživatele");
            adminOperations.Add(8, "Zobrazit všechna auta");
            adminOperations.Add(9, "Zobrazit rezervace");

            // Main admin
            mainAdmin = new Admin(new Guid(), "Admin", "Admin", "heslo");

            // List of all users
            users = new List<User>();
            // List of all vehicles
            vehicles = new List<Vehicle>();

            string command = String.Empty;

            // Main loop - end if user command = "end" 
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
                                // User login


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
            File.WriteAllLines(VEHICLES_FILE_PATH, vehiclesInFileFormat);
            File.WriteAllLines(MAINTENACES_FILE_PATH, maintenancesInFileFormat);
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

        static string getAdminOpCode()
        {
            Console.WriteLine("Funkce:");

            foreach (KeyValuePair<int, string> operation in adminOperations)
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
                            users[i] = mainAdmin.deleteUser(users[i]);
                            break;
                        }
                    }
                    Console.WriteLine("Uživatel úspešně odstraněn.");
                    break;
                case 3:
                    // Create new vehicle //
                    Vehicle newVehicle = mainAdmin.createNewVehicle();

                    Console.WriteLine("Zadat záznam o údržbě vozidla [y/n]");
                    string userAction = Console.ReadLine();
                    
                    if (userAction.Equals("y") || userAction.Equals("Y"))
                    {
                        mainAdmin.addMaintenanceToVehicle(ref newVehicle);
                    }

                    vehicles.Add(newVehicle);
                    Console.WriteLine("Vozidlo úspešně přidáno.");
                    break;


                default:
                    Console.WriteLine($"Operace s kódem {opCode} neexistuje.");
                    break;
            }
        }
    }
}