using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using static GroupManagementWithDB.Pages.Groups.MainModel;

namespace GroupManagementWithDB.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public int PermissionLevel = 0;
        public string FirstName = "";
        public static string serverName = System.Environment.MachineName;//gets the pc name
        public string connectionString = $@"Server={serverName}\SQLEXPRESS;Database=GroupManagementDB;Trusted_Connection=True";//קוד לכניסה למדע נתונים       
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

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
            }
            catch (Exception)
            {
            }
        }
        public void OnPost()
        {
            try
            {
                string serverName = System.Environment.MachineName;//gets the pc name
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand proc_userloggedin = new SqlCommand("Proc_Del_Tbl_userloggedin", connection);
                proc_userloggedin.CommandType = CommandType.StoredProcedure;
                proc_userloggedin.ExecuteNonQuery();
            }
            catch
            {
                
            }
        }
    }
}