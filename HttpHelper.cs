using Microsoft.AspNetCore.Authorization;
using SeatPlan.Pages;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace SeatPlan
{
    public class HttpHelper
    {
        private static IHttpContextAccessor _accessor;
        private ISession _session => _accessor.HttpContext.Session;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        //Using Session Data
        public void SetUserAccess(string accessLevel)
        {
            _session.SetString("UserAccess", accessLevel);
        }
        public string GetUserAccess()
        {
            return _session.GetString("UserAccess");
        }
        public void SetUserName(string userName)
        {
            _session.SetString("UserName", userName);
        }
        public string GetUserName()
        {
            return _session.GetString("UserName");
        }

        //Authenticate User
        public static string AuthenticateUser(HttpContext _httpContext)
        {
            string loginName;
            string userNameIdentity = _httpContext.User.Identity.Name;
            if (String.IsNullOrEmpty(userNameIdentity))
            {
                var userName = WindowsIdentity.GetCurrent().Name.Split("\\");
                if ((!userName.Any()) || (userName.First() != "WONGPARTNERSHIP"))
                {
                    return "";
                }
                loginName = userName.Last();
            }
            else
            {
                var userName = userNameIdentity.Split("\\");
                if ((!userName.Any()) || (userName.First() != "WONGPARTNERSHIP"))
                {
                    return "";
                }
                loginName = userName.Last();
            }
            if (String.IsNullOrEmpty(loginName)) return "";
            //Active Directory Lookup for all Staff information
            using (PrincipalContext adAuth = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
            {
                using (UserPrincipal user = new UserPrincipal(adAuth))
                {
                    user.SamAccountName = String.Format("*");
                    using (PrincipalSearcher ps = new PrincipalSearcher(user))
                    {
                        var result = ps.FindAll().Cast<UserPrincipal>();
                        var resultStaff = result.Where(c => c.Enabled == true && c.DistinguishedName.IndexOf("OU=Users") != -1 && !(c.DistinguishedName.IndexOf("Template") != -1 || c.DistinguishedName.IndexOf("Test") != -1 || c.DistinguishedName.IndexOf("LAC") != -1 || c.DistinguishedName.IndexOf("CN=Tech ") != -1 || c.DistinguishedName.IndexOf("=LDR PT") != -1 || c.DistinguishedName.IndexOf("Mailbox") != -1));
                        var resultExist = resultStaff.Where(c => c.SamAccountName == loginName).Select(c => new { c.SamAccountName, c.DistinguishedName });
                        if (!resultExist.Any()) return "";
                    }
                }
            }
            String connectionString = DBConfig.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String sql = "SELECT DeniedAccess, IsAdmin, CanEdit FROM WPSeatPlanUserAccess WHERE SamAccountName=@SamAccountName;";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SamAccountName", loginName);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetBoolean(0)) return "";
                            if (reader.GetBoolean(1)) return "admin";
                            if (reader.GetBoolean(2)) return "edit";
                        }
                    }
                }
            }
            return "default";
        }

        //Find out what is the User's Name
        public static string QueryUserName(HttpContext _httpContext)
        {
            string loginName = "";
            string userNameIdentity = _httpContext.User.Identity.Name;
            if (String.IsNullOrEmpty(userNameIdentity))
            {
                var userName = WindowsIdentity.GetCurrent().Name.Split("\\");
                if ((!userName.Any()) || (userName.First() != "WONGPARTNERSHIP"))
                {
                    return "";
                }
                loginName = userName.Last();
            }
            else
            {
                var userName = userNameIdentity.Split("\\");
                if ((!userName.Any()) || (userName.First() != "WONGPARTNERSHIP"))
                {
                    return "";
                }
                loginName = userName.Last();
            }
            return loginName;
        }
    }
}