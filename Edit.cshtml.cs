using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace SeatPlan.Pages
{
    [Authorize]
    public class EditModel : PageModel
    {
        public SeatInfo seatInfo = new SeatInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public List<SelectListItem> listStaffs = new List<SelectListItem>();
        public SelectList? Staff;
        public List<SelectListItem> listRooms = new List<SelectListItem>();
        public SelectList? Room;
        public String? SelectedStaffID;
        public String? SelectedRoomID;
        public List<SelectListItem> listLocation = new List<SelectListItem>();
        public SelectList? Locations;
        public String? SelectedLocation;
        public List<SelectListItem> listSharedStaffs = new List<SelectListItem>();
        public SelectList? SharedStaff;
        public String? SelectedSharedStaffID;
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
            String SpaceID = Request.Query["id"].ToString();
            if (String.IsNullOrEmpty(SpaceID)) return;
            int ddlStaffSelected = 0, ddlSharedStaffSelected = 0, ddlRoomSelected = 0, ddlLocationSelected = 0;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT WP.SpaceID, WP.SpaceUse, WPL.Floor, ISNULL(WPR.Name,'') + ISNULL(WPS.Name,'') AS Name, ISNULL(WPR.PhoneNumber,'') + ISNULL(WPS.PhoneNumber,'') AS PhoneNumber, WP.Department, WPL.Country, WPL.Building, WP.LocationID, WP.ModifiedOn, WP.SharedStaffID " +
                        "FROM WPSeatPlan AS WP " +
                        "INNER JOIN WPSeatPlanLocationRL AS WPL ON WP.LocationID = WPL.LocationID " +
                        "LEFT JOIN WPSeatPlanRoomRL AS WPR ON WP.RoomID = WPR.RoomID " +
                        "LEFT JOIN WPSeatPlanStaffRL AS WPS ON WP.StaffID = WPS.StaffID " +
                        "WHERE WP.SpaceID=@SpaceID;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SpaceID", SpaceID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                seatInfo.SpaceID = reader.GetString(0);
                                seatInfo.SpaceUse = reader.GetString(1);
                                seatInfo.Floor = reader.GetInt32(2);
                                seatInfo.Name = reader.IsDBNull(3) ? default : reader.GetString(3);
                                seatInfo.PhoneNumber = reader.IsDBNull(4) ? default : reader.GetString(4);
                                seatInfo.Department = reader.IsDBNull(5) ? default : reader.GetString(5);
                                seatInfo.Country = reader.GetString(6);
                                seatInfo.Building = reader.GetString(7);
                                seatInfo.LocationID = reader.GetInt32(8);
                                seatInfo.ModifiedOn = reader.GetDateTime(9).ToString();
                                seatInfo.SharedStaffID = reader.IsDBNull(10) ? default : reader.GetInt32(10);
                            }
                        }
                    }
                    //Populate a DropDownList with all Staff Names at this location
                    using (SqlCommand command = new SqlCommand("SELECT StaffID, Name FROM WPSeatPlanStaffRL WHERE LocationID=@LocationID ORDER BY Name;", connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", seatInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listStaffs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
                                ddlStaffSelected = (seatInfo.Name == reader.GetString(1)) ? reader.GetInt32(0) : ddlStaffSelected;
                                listSharedStaffs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
                                ddlSharedStaffSelected = (seatInfo.SharedStaffID == reader.GetInt32(0)) ? reader.GetInt32(0) : ddlSharedStaffSelected;
                            }
                        }
                    }
                    //Populate a DropDownList with all Room Names at this location
                    using (SqlCommand command = new SqlCommand("SELECT RoomID, Name FROM WPSeatPlanRoomRL WHERE LocationID=@LocationID ORDER BY Name;", connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", seatInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listRooms.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
                                ddlRoomSelected = (seatInfo.Name == reader.GetString(1)) ? reader.GetInt32(0) : ddlRoomSelected;
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
                                if (seatInfo.LocationID == reader.GetInt32(0))
                                {
                                    ddlLocationSelected = reader.GetInt32(0);
                                    seatInfo.Floor = reader.GetInt32(1);
                                    seatInfo.Building = reader.GetString(2);
                                    seatInfo.Country = reader.GetString(3);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            Staff = new SelectList(listStaffs, "Value", "Text", ddlStaffSelected.ToString());
            Room = new SelectList(listRooms, "Value", "Text", ddlRoomSelected.ToString());
            Locations = new SelectList(listLocation, "Value", "Text", ddlLocationSelected.ToString());
            SharedStaff = new SelectList(listSharedStaffs, "Value", "Text", ddlSharedStaffSelected.ToString());
        }

        public JsonResult OnGetLocation(String selectedLocationID)
        {
            int useLocation;
            listStaffs = new List<SelectListItem>();
            listRooms = new List<SelectListItem>();
            if (int.TryParse(selectedLocationID, out useLocation) == false)
            {
                return new JsonResult(listStaffs);
            }
            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT StaffID, Name FROM WPSeatPlanStaffRL WHERE LocationID=@LocationID ORDER BY Name;", connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", useLocation);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listStaffs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("SELECT RoomID, Name FROM WPSeatPlanRoomRL WHERE LocationID=@LocationID ORDER BY Name;", connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", useLocation);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listRooms.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            if (listStaffs.Count > 0) listStaffs.Add(new SelectListItem { Value = "**", Text = "**" });
            var listStaffRoom = listStaffs.Concat(listRooms);
            return new JsonResult(listStaffRoom);
        }
        public void OnPost()
        {
            int historyID = int.Parse(Request.Form["LocationID"]);
            seatInfo.SpaceID = Request.Form["SpaceID"];
            seatInfo.SpaceUse = Request.Form["SpaceUse"];
            seatInfo.Department = Request.Form["Department"];
            int staffID = 0, roomID = 0, locID = 0, sharedStaffID = 0;
            _ = int.TryParse(Request.Form["SelectedStaffID"], out staffID);
            _ = int.TryParse(Request.Form["SelectedRoomID"], out roomID);
            _ = int.TryParse(Request.Form["SelectedLocation"], out locID);
            _ = int.TryParse(Request.Form["SelectedSharedStaffID"], out sharedStaffID);
            seatInfo.StaffID = staffID;
            seatInfo.RoomID = roomID;
            seatInfo.LocationID = locID;
            seatInfo.SharedStaffID = sharedStaffID;

            if (String.IsNullOrEmpty(seatInfo.SpaceID) || String.IsNullOrEmpty(seatInfo.SpaceUse))
            {
                errorMessage = "Invalid Input: Missing value required for Space Use Type.";
                OnGet();
                return;
            }
            if (seatInfo.LocationID == 0)
            {
                errorMessage = "Invalid Input: Please select a valid location.";
                return;
            }

            int rowsAffected;
            String sql = "UPDATE WPSeatPlan " +
                "SET RoomID=CASE WHEN @RoomID=0 THEN NULL ELSE @RoomID END, " +
                "StaffID=CASE WHEN @StaffID=0 THEN NULL ELSE @StaffID END, SpaceUse=@SpaceUse, " +
                "LocationID=@LocationID, Department=@Department, ModifiedOn=SYSDATETIME(), " +
                "SharedStaffID=CASE WHEN @SharedStaffID=0 THEN NULL ELSE @SharedStaffID END, ModifiedBy=@ModifiedBy " +
                "WHERE SpaceID=@SpaceID";
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SpaceID", seatInfo.SpaceID.Trim().ToUpper());
                        command.Parameters.AddWithValue("@SpaceUse", seatInfo.SpaceUse.Trim());
                        command.Parameters.AddWithValue("@Department", seatInfo.Department.Trim());
                        command.Parameters.AddWithValue("@LocationID", seatInfo.LocationID);
                        command.Parameters.AddWithValue("@RoomID", seatInfo.RoomID);
                        command.Parameters.AddWithValue("@StaffID", seatInfo.StaffID);
                        command.Parameters.AddWithValue("@SharedStaffID", seatInfo.SharedStaffID);
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Response.Redirect("./Index?status=error" + "&id=" + historyID);
                return;
            }
            if (rowsAffected > 0)
            {
                successMessage = "New Staff Added Successfully";
                Response.Redirect("./Index?status=edited" + "&id=" + historyID);
            }
            else
            {
                errorMessage = "Unable to add, entry already exists";
                Response.Redirect("./Index?status=error" + "&id=" + historyID);
            }

        }
    }
}