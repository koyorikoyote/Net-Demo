using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace SeatPlan.Pages
{
    [Authorize]
    public class ViewStaffModel : PageModel
    {
        public List<StaffInfo> listStaffs = new List<StaffInfo>();
        public StaffInfo staffInfo = new StaffInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String LocationName = "";
        public bool isAdmin = false;
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
            if (httpHelper.GetUserAccess() == "admin") isAdmin = true;
            int locationID = 1;
            _ = int.TryParse(Request.Query["id"], out locationID);
            staffInfo.LocationID = locationID == 0 ? 1 : locationID;
            var tempLocName = Request.Query["name"].ToString().Split(" ");
            int tempLocNumber;
            LocationName = "- ";
            foreach (var temp in tempLocName)
            {
                if (!int.TryParse(temp, out tempLocNumber))
                {
                    LocationName += (temp + " ");
                }
            }

            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT WPS.StaffID, WPS.Name, WPS.PhoneNumber, WPL.Floor, WPS.ModifiedOn " +
                        "FROM WPSeatPlanStaffRL AS WPS " +
                        "LEFT JOIN WPSeatPlanLocationRL AS WPL ON WPS.LocationID = WPL.LocationID " +
                        "WHERE WPL.Building=(SELECT Building FROM WPSeatPlanLocationRL WHERE LocationID=@LocationID) AND WPL.Country=(SELECT Country FROM WPSeatPlanLocationRL WHERE LocationID=@LocationID) " +
                        "ORDER BY WPL.Floor, WPS.Name, WPS.ModifiedOn;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", staffInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                StaffInfo staffInfo = new StaffInfo();
                                staffInfo.StaffID = reader.GetInt32(0);
                                staffInfo.Name = reader.IsDBNull(1) ? default : reader.GetString(1);
                                staffInfo.PhoneNumber = reader.IsDBNull(2) ? default : reader.GetString(2);
                                staffInfo.Floor = reader.GetInt32(3);
                                staffInfo.ModifiedOn = reader.GetDateTime(4).ToString().Split().First();

                                listStaffs.Add(staffInfo);
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
    public class StaffInfo
    {
        public int StaffID { get; set; }
        public String? Name { get; set; }
        public String? PhoneNumber { get; set; }
        public String? FullName { get; set; }
        public int LocationID { get; set; }
        public String? ModifiedOn { get; set; }
        public int Floor { get; set; }
        public String? Building { get; set; }
        public String? Country { get; set; }
        public String? Department { get; set; }
    }
}