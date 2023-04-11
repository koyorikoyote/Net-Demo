using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace SeatPlan.Pages
{
    [Authorize]
    public class ViewRoomModel : PageModel
    {
        public List<RoomInfo> listRooms = new List<RoomInfo>();
        public RoomInfo roomInfo = new RoomInfo();
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
            int locationID = 1;
            _ = int.TryParse(Request.Query["id"], out locationID);
            roomInfo.LocationID = locationID == 0 ? 1 : locationID;
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
                    String sql = "SELECT WPR.RoomID, WPR.Name, WPR.PhoneNumber, WPL.Floor, WPR.ModifiedOn " +
                        "FROM WPSeatPlanRoomRL AS WPR " +
                        "LEFT JOIN WPSeatPlanLocationRL AS WPL ON WPR.LocationID = WPL.LocationID " +
                        "WHERE WPL.Building=(SELECT Building FROM WPSeatPlanLocationRL WHERE LocationID=@LocationID) AND WPL.Country=(SELECT Country FROM WPSeatPlanLocationRL WHERE LocationID=@LocationID) " +
                        "ORDER BY WPL.Floor, WPR.Name, WPR.ModifiedOn;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", roomInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RoomInfo roomInfo = new RoomInfo();
                                roomInfo.RoomID = reader.GetInt32(0);
                                roomInfo.Name = reader.IsDBNull(1) ? default : reader.GetString(1);
                                roomInfo.PhoneNumber = reader.IsDBNull(2) ? default : reader.GetString(2);
                                roomInfo.Floor = reader.GetInt32(3);
                                roomInfo.ModifiedOn = reader.GetDateTime(4).ToString().Split().First();

                                listRooms.Add(roomInfo);
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
    public class RoomInfo
    {
        public int RoomID { get; set; }
        public String? Name { get; set; }
        public String? PhoneNumber { get; set; }
        public int LocationID { get; set; }
        public String? ModifiedOn { get; set; }
        public int Floor { get; set; }
        public String? Building { get; set; }
        public String? Country { get; set; }
    }
}