using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static GroupManagementWithDB.Pages.Groups.MainModel;
using System.Data.SqlClient;
using System.Data;

namespace GroupManagementWithDB.Pages.Groups
{
    public class DisconnectModel : PageModel
    {
        public int PermissionLevel = 0;
        public string FirstName = "";
        public string email = "";
        public void OnGet()
        {
            string id = Request.Query["id"];

            string serverName = System.Environment.MachineName;//gets the pc name

            string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//קוד לכניסה למדע נתונים
            SqlConnection connection = new SqlConnection(connectionString);
            UsersInformation usersInfo = GroupManagementWithDB.Pages.Groups.MainModel.GetUserPermission(connection);
            try
            {
                if (usersInfo != null)
                {
                    PermissionLevel = usersInfo.PermissionLevel;
                    FirstName = usersInfo.firstName;
                    email = usersInfo.email;
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

            try
            {
                
             
                connection.Open();
                SqlCommand Proc_Disconnect_Users_From_Group = new SqlCommand("Proc_Disconnect_Users_From_Group", connection);
                Proc_Disconnect_Users_From_Group.CommandType = CommandType.StoredProcedure;
                Proc_Disconnect_Users_From_Group.Parameters.AddWithValue("@id", int.Parse(id.Split(',')[0]));
                Proc_Disconnect_Users_From_Group.Parameters.AddWithValue("@id_group", int.Parse(id.Split(',')[1]));
                Proc_Disconnect_Users_From_Group.ExecuteNonQuery();

            }
            catch (Exception)
            {


            }
            finally
            {
                if (connection != null) connection.Close();
            }


            Response.Redirect($"/Groups/ViewDetails?id={id.Split(',')[1]}");
        }
    }
}
