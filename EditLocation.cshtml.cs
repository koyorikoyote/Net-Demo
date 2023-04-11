using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SeatPlan.Pages
{
    [Authorize]
    public class EditLocationModel : PageModel
    {
        public LocationInfo locationInfo = new LocationInfo();
        public String errorMessage = "";
        public String successMessage = "";
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
            int LocationID = 0;
            if (int.TryParse(Request.Query["id"], out LocationID) == false) return;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT Floor, Building, Country " +
                        "FROM WPSeatPlanLocationRL " +
                        "WHERE LocationID=@LocationID;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                locationInfo.LocationID = LocationID;
                                locationInfo.Floor = reader.GetInt32(0);
                                locationInfo.Building = reader.GetString(1);
                                locationInfo.Country = reader.GetString(2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        public void OnPost()
        {
            locationInfo.LocationID = int.Parse(Request.Form["LocationID"]);
            locationInfo.Floor = int.Parse(Request.Form["Floor"]);
            locationInfo.Building = Request.Form["Building"];
            locationInfo.Country = Request.Form["Country"];

            int rowsAffected;
            String sql = "UPDATE WPSeatPlanLocationRL " +
                "SET Floor=@Floor, " +
                "Building=@Building, " +
                "Country=@Country, " +
                "ModifiedOn=SYSDATETIME(), " +
                "ModifiedBy=@ModifiedBy " +
                "WHERE LocationID=@LocationID";
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Floor", locationInfo.Floor);
                        command.Parameters.AddWithValue("@Building", locationInfo.Building.Trim());
                        command.Parameters.AddWithValue("@Country", locationInfo.Country.Trim());
                        command.Parameters.AddWithValue("@LocationID", locationInfo.LocationID);
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Response.Redirect("./ViewLocation?status=error");
                return;
            }
            if (rowsAffected > 0)
            {
                successMessage = "Location Edited Successfully";
                Response.Redirect("./ViewLocation?status=edited");
            }
            else
            {
                errorMessage = "Unable to proceed, entry already exists";
                Response.Redirect("./ViewLocation?status=error");
            }
        }
    }
}