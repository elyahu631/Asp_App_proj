using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.Users
{
    public class RegisterModel : PageModel
    {
        //[a-zA-Z0-9\.\-_]+ : Matches one or more of the following characters
        //
        private const string emailRegex = @"^[a-zA-Z0-9\.\-_]+@[a-zA-Z]{2,15}(?:\.[a-zA-Z]+){1,2}$";
        private const string nameRegex = @"^[a-zA-Z\s]+$";
       

        //: בין 7 עד 20 תווים (כולל), כאשר חייבת להכיל: אות גדולה אחת לפחות, ספרה אחת לפחות ותו מיוחד אחד לפחות.
        private const string passRegex = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{7,20}$";
        
        public UsersInformation registrationInfo = new UsersInformation();
        public string errorMessage = "";
        public string successMessage = "";
        public int PermissionLevel = 0;
        public string FirstName = "";

        public static string serverName = System.Environment.MachineName;//gets the pc name

        public string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//קוד לכניסה למדע נתונים
 
        public void OnGet()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            UsersInformation usersInfo = GroupManagementWithDB.Pages.Groups.MainModel.GetUserPermission(connection);
            PermissionLevel = usersInfo.PermissionLevel;
            FirstName = usersInfo.firstName;

        }


        public static bool InputValidation(Regex regexCheck, string input)
        {
            return regexCheck.IsMatch(input);
        }


        public int IsEmailExists(string userEmail)
        { 
            SqlConnection? connection=null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                string sqlSelect = $"select * from users where email = @email";
                SqlCommand command = new SqlCommand(sqlSelect, connection);

                command.Parameters.AddWithValue("@email", userEmail);
                SqlDataReader reader = command.ExecuteReader();

                if (!(reader != null && reader.HasRows))
                {
                    return 0;
                }
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
                if (connection != null) connection.Close();
            }
        }
        public void OnPost()
        {
            registrationInfo.firstName = Request.Form["firstName"];
            registrationInfo.lastName = Request.Form["lastName"];
            registrationInfo.email = Request.Form["email"];
            registrationInfo.password = Request.Form["password"];

            if (registrationInfo.firstName.Length == 0 || registrationInfo.lastName.Length == 0 ||
                    registrationInfo.email.Length == 0 || registrationInfo.password.Length == 0)
            {
                errorMessage = "All the fields are required!!";
                return;
            }

            if (!InputValidation(new Regex(nameRegex), registrationInfo.firstName) ||
                !InputValidation(new Regex(nameRegex), registrationInfo.lastName))
            {
                errorMessage = "First name or Last name in a bad format!!";
                return;
            }


            if (!InputValidation(new Regex(emailRegex), registrationInfo.email))
            {
                errorMessage = "Email in a bad format!!";
                return;
            }

            if (!InputValidation(new Regex(passRegex), registrationInfo.password))
            {
                errorMessage = "Password in a bad format!!";
                return;
            }

            string to = registrationInfo.email;
            string subject = "Welcome";
            string body = "Body";

            int emailExistsStatus = IsEmailExists(to);
            if (emailExistsStatus == -1)
            {
                errorMessage = "Failed to connect to the database.";
                return;
            }
            else if (emailExistsStatus == 1)
            {
                errorMessage = "There is some problem with your details.";
                return;
            }

            try
            {
                SendEmail(to, subject, body);
            }
            catch (Exception)
            {
                errorMessage = "Failed to send the email.";
                return;
            }
            // save the new client into the database



            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                string sql = "INSERT INTO users " + "(first_name, last_name, email, password) VALUES " + "(@firsName, @lastName ,@email ,@password);";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@firsName", registrationInfo.firstName);
                command.Parameters.AddWithValue("@lastName", registrationInfo.lastName);
                command.Parameters.AddWithValue("@email", registrationInfo.email);
                string subEmail = registrationInfo.email.Substring(0, registrationInfo.email.IndexOf("@"));
                command.Parameters.AddWithValue("@password", EncodingPassword(registrationInfo.password, subEmail));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                errorMessage = ex.Message;
                return;
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            Response.Redirect("/Users/Login");
        }

        private static void SendEmail(string to, string subject, string body)
        {
            MailMessage message = new MailMessage("elynivwpf@gmail.com", to, subject, body);
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);//587, 465, 25, 2525
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("elynivwpf@gmail.com", DecodingPassword("MzA4MzExMzE2MzE2MzA5MzIzMzIwMzA1MzAzMzIyMzA4MzExMjk1MzI2MzI3MzEw", "email"));
            smtp.Send(message);
        }

        private static string EncodingPassword(string password, string subEmail)
        {

            string newPass = "";
            int indexSubEmail = 0;
            int subEmailLength = subEmail.Length;

            // Loop through each character in the password string
            for (int i = 0; i < password.Length; i++)
            {
                // If we've reached the end of the subEmail string, start over from the beginning
                if (indexSubEmail >= subEmailLength)
                    indexSubEmail = 0;

                // Append the current password character, current subEmail character, and 100 to the newPass string
                newPass += password[i] + subEmail[indexSubEmail++] + 100;
            }

            // TF-8 is a variable-length encoding, meaning that each character can be encoded using
            // 1 to 4 bytes, depending on the character's Unicode code point
            //  In UTF-8, ASCII characters (code points 0-127) are represented using a single byte,
            // while other characters are represented using multiple bytes.
            //exmple Hello
            //01001000 01100101 01101100 01101100 01101111
            //for the first set of 3 bytes (01001000 01100101 01101100)
            //010010 000110 010101 101100 |||| 011011 000110 111101 -> padding character
            //"Hello" would be "S2VsbG8="
            byte[] messageBytes = Encoding.UTF8.GetBytes(newPass); // כל תו יהיה באקסה של באסקי
            string encodedMessage = Convert.ToBase64String(messageBytes); // 

            return encodedMessage;

        }
        private static string DecodingPassword(string encodedPassword, string subEmail)
        {
            byte[] decodedBytes = Convert.FromBase64String(encodedPassword);
            string originalMessage = Encoding.UTF8.GetString(decodedBytes);

            string decodPass = "";
            int indexSubEmail = 0;
            int subEmailLength = subEmail.Length;
            int ascciValue = 0;
            string part = "";

            for (int i = 0; i < originalMessage.Length; i += 3)
            {
                part = $"{originalMessage[i]}{originalMessage[i + 1]}{originalMessage[i + 2]}";
                ascciValue = int.Parse(part) - 100;

                if (indexSubEmail >= subEmailLength)
                    indexSubEmail = 0;
                decodPass += (char)(ascciValue - subEmail[indexSubEmail++]);
            }
            return decodPass;
        }
    }
}
