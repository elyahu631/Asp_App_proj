using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.Groups
{
    public class JoinModel : PageModel
    {
        public int PermissionLevel = 0;
        public string FirstName = "";
        public void OnGet()
        {


            string serverName = System.Environment.MachineName;//gets the pc name
                                                               //const string myComputer = "DESKTOP-CEUMQH8";
                                                               //const string myComputer = "LAPTOP-FMOR13KE";
            string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//קוד לכניסה למדע נתונים
            SqlConnection connection = new SqlConnection(connectionString);
            UsersInformation usersInfo = GroupManagementWithDB.Pages.Groups.MainModel.GetUserPermission(connection);
            try
            {
                if (usersInfo != null)
                {
                    PermissionLevel = usersInfo.PermissionLevel;
                    FirstName = usersInfo.firstName;
                }
                if (PermissionLevel == 0)
                {
                    Response.Redirect("/Users/Login");
                    return;
                }

            }
            catch (Exception)
            {
                Response.Redirect("/Users/Login");
            }
            SqlDataReader? reader = null;
            try
            {
                string id = Request.Query["id"];

                connection.Open();

                string email = "";

                SqlCommand getPermFromDB = new SqlCommand("SELECT email FROM user_loggedin", connection);
                reader = getPermFromDB.ExecuteReader();
                while (reader.Read())
                {
                    email = reader.GetString(0);
                }

                reader.Close();

                string sql = "INSERT INTO users_groups " + "(email, id_group) VALUES " + "(@email, @id);";

                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {

                
            }
            finally
            {
                if (connection != null) connection.Close();
            }


            Response.Redirect("/Groups/Main");
        }
    }
}
