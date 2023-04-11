using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SeatPlan.Pages
{
    [Authorize]
    public class AccessControlModel : PageModel
    {
        public List<AccessControlInfo> listAccessControl = new List<AccessControlInfo>();
        public AccessControlInfo accessControlInfo = new AccessControlInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String LocationName = "";
        public void OnGet()
        {
            HttpHelper httpHelper = new HttpHelper();
            if (String.IsNullOrEmpty(httpHelper.GetUserAccess()))
            {
                httpHelper.SetUserAccess(HttpHelper.AuthenticateUser(HttpContext));
                if (String.IsNullOrEmpty(httpHelper.GetUserAccess()))
                {
                    Response.Redirect("./AccessDenied");
                    return;
                }
            }
            if (httpHelper.GetUserAccess() != "admin")
            {
                Response.Redirect("./AccessDenied");
                return;
            }
            if (String.IsNullOrEmpty(httpHelper.GetUserName()))
            {
                httpHelper.SetUserName(HttpHelper.QueryUserName(HttpContext));
            }

            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT SamAccountName, FullName, Department, Country, Building, CanEdit, IsAdmin, DeniedAccess, ModifiedOn " +
                        "FROM WPSeatPlanUserAccess " +
                        "ORDER BY ModifiedOn DESC, Country DESC, Building, Department, FullName;";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AccessControlInfo accessControlInfo = new AccessControlInfo();
                                accessControlInfo.SamAccountName = reader.GetString(0);
                                accessControlInfo.FullName = reader.IsDBNull(1) ? default : reader.GetString(1);
                                accessControlInfo.Department = reader.IsDBNull(2) ? default : reader.GetString(2);
                                accessControlInfo.Country = reader.GetString(3);
                                accessControlInfo.Building = reader.GetString(4);
                                accessControlInfo.CanEdit = reader.GetBoolean(5);
                                accessControlInfo.IsAdmin = reader.GetBoolean(6);
                                accessControlInfo.DeniedAccess = reader.GetBoolean(7);
                                accessControlInfo.ModifiedOn = reader.GetDateTime(8).ToString().Split().First();

                                listAccessControl.Add(accessControlInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error: " + ex.Message;
                return;
            }
        }
        public void OnPostSave(List<AccessControlInfo> listAccessControlPost)
        {
            int rowsAffected = 0;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE WPSeatPlanUserAccess SET ";
                    String sqlCanEdit = "CanEdit= CASE SamAccountName";
                    String sqlIsAdmin = "IsAdmin= CASE SamAccountName";
                    String sqlDeniedAccess = "DeniedAccess= CASE SamAccountName";
                    for (int i = 0; i < listAccessControlPost.Count; i++)
                    {
                        sqlCanEdit += " WHEN @SamAccountName_" + i + " THEN @CanEdit_" + i;
                        sqlIsAdmin += " WHEN @SamAccountName_" + i + " THEN @IsAdmin_" + i;
                        sqlDeniedAccess += " WHEN @SamAccountName_" + i + " THEN @DeniedAccess_" + i;
                    }
                    sqlCanEdit += " END, ";
                    sqlIsAdmin += " END, ";
                    sqlDeniedAccess += " END, ";
                    sql += sqlCanEdit + sqlIsAdmin + sqlDeniedAccess + "ModifiedOn=SYSDATETIME(), ModifiedBy=@ModifiedBy;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        for (int i = 0; i < listAccessControlPost.Count; i++)
                        {
                            command.Parameters.AddWithValue("@SamAccountName_" + i, listAccessControlPost[i].SamAccountName);
                            command.Parameters.AddWithValue("@CanEdit_" + i, listAccessControlPost[i].CanEdit);
                            command.Parameters.AddWithValue("@IsAdmin_" + i, listAccessControlPost[i].IsAdmin);
                            command.Parameters.AddWithValue("@DeniedAccess_" + i, listAccessControlPost[i].DeniedAccess);
                        }
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            if (rowsAffected > 0)
            {
                successMessage = "User Access Edited Successfully";
                Response.Redirect("./Index?status=edited");
            }
            else
            {
                errorMessage = "Unable to proceed, entry already exists";
                Response.Redirect("./Index?status=error");
            }
        }
    }
    public class AccessControlInfo
    {
        public String? SamAccountName { get; set; }
        public String? FullName { get; set; }
        public String? Department { get; set; }
        public String? Country { get; set; }
        public String? Building { get; set; }
        public bool CanEdit { get; set; }
        public bool IsAdmin { get; set; }
        public bool DeniedAccess { get; set; }
        public String? ModifiedOn { get; set; }
    }
}