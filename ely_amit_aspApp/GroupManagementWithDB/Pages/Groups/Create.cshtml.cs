using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.Groups
{
    public class CreateModel : PageModel
    {

        private const string cityRegex = @"^[a-zA-Z\s]+$";


        public GroupsInfo groupsInfo = new GroupsInfo();
        public string errorMessage = "";
        public string successMessage = "";
        public int PermissionLevel = 0;
        public string FirstName = "";
        public static string serverName = System.Environment.MachineName;//gets the pc name
                                                                         //public const string myComputer = "DESKTOP-CEUMQH8";
                                                                         //public const string myComputer = "LAPTOP-FMOR13KE";



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
                if (PermissionLevel == 0)
                {
                    Response.Redirect("/Users/Login");
                    return;
                }

            }
            catch (Exception )
            {
                Response.Redirect("/Users/Login");
            }
        }

        public void OnPost()
        {
            groupsInfo.name = Request.Form["name"];

            Regex regexCheck = new Regex(cityRegex);

            if (groupsInfo.name.Length == 0)
            {
                errorMessage = "All the fields are required";
                return;
            }
            else if (!regexCheck.IsMatch(groupsInfo.name))
            {
                errorMessage = "City name format is incorrect!!";
                return;
            }


            // save the new client into the database

            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                //SetUserPermission(connection);

                connection.Open();
                string sql = "INSERT INTO groups" + "(name) VALUES " + "(@name);";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", groupsInfo.name);
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

            groupsInfo.name = "";
            successMessage = "New Group added successfuly";

            Response.Redirect("/Groups/Main");
        }
    }
}
