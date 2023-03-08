using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.Users
{
    public class LoginModel : PageModel
    {
        public UsersInfo usersInfo = new UsersInfo();
        public string errorMessage = "";
        //public const string myComputer = "DESKTOP-CEUMQH8";
        //public const string myComputer = "LAPTOP-FMOR13KE";
        public int PermissionLevel = 0;
        public string FirstName = "";
        public static string serverName = System.Environment.MachineName;//gets the pc name
        public string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//קוד לכניסה למדע נתונים       

        public void OnGet()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            UsersInformation usersInfo = GroupManagementWithDB.Pages.Groups.MainModel.GetUserPermission(connection);
            try
            {
                if (usersInfo != null)
                {
                    PermissionLevel = usersInfo.PermissionLevel;
                    FirstName = usersInfo.firstName;
                }
                if (PermissionLevel != 0)
                {
                    Response.Redirect("/Groups/Main");
                    return;
                }

            }
            catch (Exception)
            {
                Response.Redirect("/Users/Login");
            }
        

        }



        public void OnPost()
        {
            usersInfo.email = Request.Form["email"];
            usersInfo.password = Request.Form["password"];

            //Regex regexCheck = new Regex(cityRegex);

            if (usersInfo.email.Length == 0 || usersInfo.password.Length == 0)
            {
                errorMessage = "somting worng";
                return;
            }



            // save the new client into the database

            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataReader? reader = null;

            try
            {
                connection.Open();
                string sqlSelect = $"select * from users where email = @email and password = @password";
                SqlCommand command = new SqlCommand(sqlSelect, connection);

                command.Parameters.AddWithValue("@email", usersInfo.email);
                string subEmail = usersInfo.email.Substring(0, usersInfo.email.IndexOf("@"));
                command.Parameters.AddWithValue("@password", EncodingPassword(usersInfo.password, subEmail));

                reader = command.ExecuteReader();
                usersInfo.password = "";


                if (!(reader != null && reader.HasRows))
                {
                    errorMessage = $"email or password is incorrect";
                    return;
                }


                while (reader.Read())
                {
                    usersInfo.id = "" + reader.GetInt32(reader.GetOrdinal("id"));
                    usersInfo.email = reader.GetString(reader.GetOrdinal("email"));
                    usersInfo.firstName = reader.GetString(reader.GetOrdinal("first_name"));
                    usersInfo.lastName = reader.GetString(reader.GetOrdinal("last_name"));
                    usersInfo.permissions = reader.GetInt32(reader.GetOrdinal("permission_code"));
                }



                reader.Close();
                SqlCommand proc_userloggedin = new SqlCommand("Proc_User_loggedin", connection);
                proc_userloggedin.CommandType = CommandType.StoredProcedure;

                proc_userloggedin.Parameters.AddWithValue("@email", usersInfo.email);
                proc_userloggedin.Parameters.AddWithValue("@id", usersInfo.id);
                proc_userloggedin.Parameters.AddWithValue("@first_name", usersInfo.firstName);
                proc_userloggedin.Parameters.AddWithValue("@last_name", usersInfo.lastName);
                proc_userloggedin.Parameters.AddWithValue("@permission_code", usersInfo.permissions);
                proc_userloggedin.ExecuteNonQuery();

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

           


            


            //errorMessage = $"{usersInfo.email} - {usersInfo.password} - {usersInfo.permissions}";

            //usersInfo.name = "";

            Response.Redirect("/Groups/Main");
        }
        private static string EncodingPassword(string password, string subEmail)
        {

            string newPass = "";
            int indexSubEmail = 0;
            int subEmailLength = subEmail.Length;
            for (int i = 0; i < password.Length; i++)
            {
                if (indexSubEmail >= subEmailLength)
                    indexSubEmail = 0;
                newPass += password[i] + subEmail[indexSubEmail++] + 100;
            }

            byte[] messageBytes = Encoding.UTF8.GetBytes(newPass);
            string encodedMessage = Convert.ToBase64String(messageBytes);

            return encodedMessage;

        }

        public class UsersInfo
        {
            public string id = "";
            public string email = "";
            public string firstName = "";
            public string lastName = "";
            public string password = "";
            public int permissions = 1;
        }
    }
}
