using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace GroupManagementWithDB.Pages.Groups
{
    public class MainModel : PageModel
    {
        public int PermissionLevel = 0;
        public string FirstName = "";
        public List<GroupsInfo> listGroups = new List<GroupsInfo>();// list that will hold in all groups

        public static string serverName = System.Environment.MachineName;//gets the pc name

        public string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//defines a connection string for a SQL Server 

        // Manager creates groups | A normal user can join and disconnect from groups
        // | Those who do not have permission cannot connect

        public class UsersInformation
        {
            public string id = "";
            public string email = "";
            public string firstName = "";
            public string lastName = "";
            public string password = "";
            public int PermissionLevel = 0;
        }


        static public UsersInformation GetUserPermission(SqlConnection connection)
        {
            //SqlConnection connections = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                UsersInformation userinfo = new UsersInformation();
                SqlCommand getPermFromDB = new SqlCommand("SELECT * FROM user_loggedin", connection);
                SqlDataReader reader = getPermFromDB.ExecuteReader();
                while (reader.Read())
                {
                    userinfo.email = reader.GetString(0);
                    userinfo.id = "" + reader.GetInt32(1);
                    userinfo.firstName = reader.GetString(2);
                    userinfo.lastName = reader.GetString(3);
                    userinfo.PermissionLevel = reader.GetInt32(4);

                }
                reader.Close();
                connection.Close();
                return userinfo;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public void OnGet()
        {

            SqlDataReader? reader = null;
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
         


            try
            {
                //isLoggedin = true;
                connection.Open();
                SqlCommand getPermFromDB = new SqlCommand("SELECT permission_code FROM user_loggedin", connection);
                reader = getPermFromDB.ExecuteReader();
                while (reader.Read())
                {
                    PermissionLevel = reader.GetInt32(0);
                }


                if (PermissionLevel == 0)
                {
                    Response.Redirect("/Users/Login");
                    return;
                }

                reader.Close();


                 if (PermissionLevel == 1)
                {
                    SqlCommand Proc_Groups_To_Join = new SqlCommand("Proc_Groups_To_Join", connection);
                    Proc_Groups_To_Join.CommandType = CommandType.StoredProcedure;
                    reader = Proc_Groups_To_Join.ExecuteReader();
                }
                else 
                {
                    string sqlSelect = "SELECT * FROM Groups"; // SQL query to retrieve all data from the Groups table
                    SqlCommand command = new SqlCommand(sqlSelect, connection);//object with the SQL query and a database connection
                    reader = command.ExecuteReader();
                }
                // Execute the query and Create a object to read the data

                // loop through each row 
                while (reader.Read())
                {
                    GroupsInfo groupInfo = new GroupsInfo();
                    groupInfo.id = "" + reader.GetInt32(0);
                    groupInfo.name = reader.GetString(1);
                    groupInfo.created_at = reader.GetDateTime(2).ToString();
                    listGroups.Add(groupInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exeption : " + ex.ToString());///////////////////
            }
            finally
            {
                if (reader != null) reader.Close();
                if (connection != null) connection.Close();
            }
        }
    }


    public class GroupsInfo
    {
        public string id = "";
        public string name = "";
        public string created_at = "";
    }
}
