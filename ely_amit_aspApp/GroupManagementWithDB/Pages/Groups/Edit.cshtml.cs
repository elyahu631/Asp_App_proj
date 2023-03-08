using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using GroupManagementWithDB.Pages.Groups;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.Groups
{
    public class EditModel : PageModel
    {
        private const string cityRegex = @"^[a-zA-Z\s]+$";



        public GroupsInfo groupInfo = new GroupsInfo();
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
            catch (Exception)
            {
                Response.Redirect("/Users/Login");
            }

            string id = Request.Query["id"];
            SqlDataReader? reader = null;
            try
            {
                connection.Open();
                string sqlSelect = "SELECT * FROM Groups WHERE id_group=@id";
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    groupInfo.id = "" + reader.GetInt32(0);
                    groupInfo.name = reader.GetString(1);
                }
            }
            catch (Exception ex)
            {

                errorMessage = ex.Message;
                return;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (connection != null) connection.Close();
            }
        }
        public void OnPost()
        {
            groupInfo.id = Request.Form["id"];
            groupInfo.name = Request.Form["name"];
            Regex regexCheck = new Regex(cityRegex);

            if (groupInfo.id.Length == 0 || groupInfo.name.Length == 0)
            {
                errorMessage = "All fields are required!!!!!!!";
                return;
            }
            else if (!regexCheck.IsMatch(groupInfo.name))
            {
                errorMessage = "City name format is incorrect!!";
                return;
            }

            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                string sql = "UPDATE Groups " + "Set name =@name " + "where id_group=@id;";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", groupInfo.name);
                command.Parameters.AddWithValue("@id", groupInfo.id);
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

            Response.Redirect("/Groups/Main");
        }
    }
}
