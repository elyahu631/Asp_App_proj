using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages.UserGroups
{
    public class LeaveGroupModel : PageModel
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
            try
            {
                string id = Request.Query["id"];
                connection.Open();
                SqlCommand Proc_Leave_Group = new SqlCommand("Proc_Leave_Group", connection);
                Proc_Leave_Group.CommandType = CommandType.StoredProcedure;
                Proc_Leave_Group.Parameters.AddWithValue("@id_group", id);
                Proc_Leave_Group.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
            finally
            {
                if (connection != null) connection.Close();
            }

             Response.Redirect("/UserGroups/MyGroups");
        }
    }
}
