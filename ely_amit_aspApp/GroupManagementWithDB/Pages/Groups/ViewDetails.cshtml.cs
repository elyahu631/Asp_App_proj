using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static GroupManagementWithDB.Pages.Groups.MainModel;
using System.Data.SqlClient;
using System.Data;
using static GroupManagementWithDB.Pages.Groups.ViewDetailsModel;

namespace GroupManagementWithDB.Pages.Groups
{
    public class ViewDetailsModel : PageModel
    {
        private const string cityRegex = @"^[a-zA-Z\s]+$";
        public List<UserInGroupsInfo> listUsersInGroup = new List<UserInGroupsInfo>();

        public GroupsInfo groupInfo = new GroupsInfo();
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
            try
            {
                if (usersInfo != null)
                {
                    PermissionLevel = usersInfo.PermissionLevel;
                    FirstName = usersInfo.firstName;
                }
                if (PermissionLevel < 2)
                {
                    Response.Redirect("/Users/Login");
                    return;
                }

            }
            catch (Exception)
            {
                Response.Redirect("/Users/Login");
            }

            //////////////////////////////

            string id = Request.Query["id"];
            SqlDataReader? reader = null;
            
            try
            {
                connection.Open();
                SqlCommand proc_Users_In_Groups = new SqlCommand("Proc_Users_In_Groups", connection);//object with the SQL query and a database connection
                proc_Users_In_Groups.CommandType = CommandType.StoredProcedure;
                proc_Users_In_Groups.Parameters.AddWithValue("@id_group", id);

                reader = proc_Users_In_Groups.ExecuteReader();// Execute the query and Create a object to read the data



                // loop through each row 
                while (reader.Read())
                {
                    UserInGroupsInfo userInGroupsInfo = new UserInGroupsInfo();
                    userInGroupsInfo.group_id = id;
                    userInGroupsInfo.id = "" + reader.GetInt32(0);
                    userInGroupsInfo.firstName = reader.GetString(1);
                    userInGroupsInfo.lastName = reader.GetString(2);
                    userInGroupsInfo.email = reader.GetString(3);                   
                    listUsersInGroup.Add(userInGroupsInfo);
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

        public class UserInGroupsInfo
        {
            public string group_id = "";
            public string id = "";
            public string firstName = "";
            public string lastName = "";
            public string email = "";
        }
    }
}
