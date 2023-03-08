using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.UserGroups
{
    public class MyGroupsModel : PageModel
    {
        public List<GroupsInfo> listGroups = new List<GroupsInfo>();// list that will hold in all groups

        public static string serverName = System.Environment.MachineName;//gets the pc name

        public string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//defines a connection string for a SQL Server 

        // Manager creates groups | A normal user can join and disconnect from groups
        // | Those who do not have permission cannot connect
        public int PermissionLevel = 0;
        public string FirstName = "";


 

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


       

            //string sqlSelect = "SELECT * FROM Groups"; // SQL query to retrieve all data from the Groups table
            SqlCommand proc_User_Groups_List = new SqlCommand("Proc_User_Groups_List", connection);//object with the SQL query and a database connection
              proc_User_Groups_List.CommandType = CommandType.StoredProcedure;

            reader = proc_User_Groups_List.ExecuteReader();// Execute the query and Create a object to read the data

                // loop through each row 
                while (reader.Read())
                {
                    GroupsInfo groupInfo = new GroupsInfo();
                    groupInfo.id = "" + reader.GetInt32(0);
                    groupInfo.name = reader.GetString(1);
                    groupInfo.created_at = reader.GetDateTime(2).ToString();
                    if (PermissionLevel == 1)

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