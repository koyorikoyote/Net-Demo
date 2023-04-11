using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SeatPlan.Pages
{
    [Authorize]
    public class EditRoomModel : PageModel
    {
        public RoomInfo roomInfo = new RoomInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public List<SelectListItem> listLocation = new List<SelectListItem>();
        public SelectList? Locations;
        public String? SelectedLocation;
        public String? UrlBack;
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
            if (String.IsNullOrEmpty(httpHelper.GetUserName()))
            {
                httpHelper.SetUserName(HttpHelper.QueryUserName(HttpContext));
            }
            UrlBack = Regex.Replace((Request.GetTypedHeaders().Referer.ToString()), @"&status=.*", "");
            int RoomID = 0;
            if (int.TryParse(Request.Query["id"], out RoomID) == false) return;
            int ddlLocationSelected = 0;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT WPR.Name, WPR.PhoneNumber, WPL.Floor, WPL.Building, WPL.Country, WPR.LocationID " +
                        "FROM WPSeatPlanRoomRL AS WPR " +
                        "LEFT JOIN WPSeatPlanLocationRL AS WPL ON WPR.LocationID = WPL.LocationID " +
                        "WHERE WPR.RoomID=@RoomID";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RoomID", RoomID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                roomInfo.RoomID = RoomID;
                                roomInfo.Name = reader.IsDBNull(0) ? default : reader.GetString(0);
                                roomInfo.PhoneNumber = reader.IsDBNull(1) ? default : reader.GetString(1);
                                roomInfo.Floor = reader.GetInt32(2);
                                roomInfo.Building = reader.GetString(3);
                                roomInfo.Country = reader.GetString(4);
                                roomInfo.LocationID = reader.GetInt32(5);
                            }
                        }
                    }
                    //Fill dropdownlist for location
                    using (SqlCommand command = new SqlCommand("SELECT LocationID, Floor, Building, Country FROM WPSeatPlanLocationRL;", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listLocation.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetInt32(1).ToString() + " " + reader.GetString(2) + " " + reader.GetString(3) });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            ddlLocationSelected = roomInfo.LocationID;
            Locations = new SelectList(listLocation, "Value", "Text", ddlLocationSelected.ToString());
        }
        public void OnPost()
        {
            UrlBack = Request.Form["UrlBack"];
            roomInfo.RoomID = int.Parse(Request.Form["RoomID"]);
            roomInfo.Name = Request.Form["Name"];
            roomInfo.PhoneNumber = Request.Form["PhoneNumber"];
            int locID = 0;
            _ = int.TryParse(Request.Form["SelectedLocation"], out locID);
            roomInfo.LocationID = locID;

            if (String.IsNullOrEmpty(roomInfo.Name))
            {
                errorMessage = "Invalid Input: Missing value required for Room Name.";
                return;
            }
            if (roomInfo.LocationID == 0)
            {
                errorMessage = "Invalid Input: Please select a valid location.";
                return;
            }

            int rowsAffected;
            String sql = "UPDATE WPSeatPlanRoomRL " +
                "SET Name=@Name, " +
                "PhoneNumber=@PhoneNumber, " +
                "LocationID=@LocationID, " +
                "ModifiedOn=SYSDATETIME(), " +
                "ModifiedBy=@ModifiedBy " +
                "WHERE RoomID=@RoomID";
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", roomInfo.Name.Trim());
                        command.Parameters.AddWithValue("@PhoneNumber", roomInfo.PhoneNumber.Trim());
                        command.Parameters.AddWithValue("@RoomID", roomInfo.RoomID);
                        command.Parameters.AddWithValue("@LocationID", roomInfo.LocationID);
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Response.Redirect(UrlBack + "&status=error");
                return;
            }

            if (rowsAffected > 0)
            {
                successMessage = "Room Edited Successfully";
                Response.Redirect(UrlBack + "&status=edited");
            }
            else
            {
                errorMessage = "Unable to proceed, entry already exists";
                Response.Redirect(UrlBack + "&status=error");
            }
        }
    }
}