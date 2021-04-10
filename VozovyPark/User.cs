using System;
using System.Collections.Generic;
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