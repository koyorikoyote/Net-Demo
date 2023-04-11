using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Globalization;

namespace SeatPlan.Pages
{
    [Authorize]
    public class ViewLocationModel : PageModel
    {
        public List<LocationInfo> listLocations = new List<LocationInfo>();
        public LocationInfo locationInfo = new LocationInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String LocationName = "";
        public int tempLocationID;
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
            int locationID = 1;
            _ = int.TryParse(Request.Query["id"], out locationID);
            tempLocationID = locationID == 0 ? 1 : locationID;
            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT LocationID, Floor, Building, Country, ModifiedOn " +
                        "FROM WPSeatPlanLocationRL;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LocationInfo locationInfo = new LocationInfo();
                                locationInfo.LocationID = reader.GetInt32(0);
                                locationInfo.Floor = reader.GetInt32(1);
                                locationInfo.Building = reader.IsDBNull(2) ? default : reader.GetString(2);
                                locationInfo.Country = reader.IsDBNull(3) ? default : reader.GetString(3);
                                locationInfo.ModifiedOn = reader.GetDateTime(4).ToString().Split().First();

                                listLocations.Add(locationInfo);
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
    }
    public class LocationInfo
    {
        public int LocationID { get; set; }
        public int Floor { get; set; }
        public String? Building { get; set; }
        public String? Country { get; set; }
        public String? ModifiedOn { get; set; }
    }
}