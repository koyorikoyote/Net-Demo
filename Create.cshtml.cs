using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;

namespace SeatPlan.Pages
{
    [Authorize]
    public class CreateModel : PageModel
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
            if (httpHelper.GetUserAccess() != "admin")
            {
                Response.Redirect("./AccessDenied");
                return;
            }
            if (String.IsNullOrEmpty(httpHelper.GetUserName()))
            {
                httpHelper.SetUserName(HttpHelper.QueryUserName(HttpContext));
            }
            int locationID = 1;
            _ = int.TryParse(Request.Query["id"], out locationID);
            seatInfo.LocationID = locationID == 0 ? 1 : locationID;
            int ddlStaffSelected = 0, ddlSharedStaffSelected = 0, ddlRoomSelected = 0, ddlLocationSelected = 0;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //Fill in textboxes for location
                    using (SqlCommand command = new SqlCommand("SELECT Country, Building, Floor FROM WPSeatPlanLocationRL WHERE LocationID=@LocationID;", connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", seatInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                seatInfo.Country = reader.GetString(0);
                                seatInfo.Building = reader.GetString(1);
                                seatInfo.Floor = reader.GetInt32(2);
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
                                listSharedStaffs.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetString(1) });
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
                errorMessage = "Invalid Input: Missing values required for Space ID and Space Use.";
                return;
            }
            if (seatInfo.LocationID == 0)
            {
                errorMessage = "Invalid Input: Please select a valid location.";
                return;
            }

            //save new seat info into database
            int rowsAffected;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "IF NOT EXISTS (SELECT * FROM WPSeatPlan WHERE SpaceID=@SpaceID) " +
                        "INSERT INTO WPSeatPlan(SpaceID, SpaceUse, LocationID, RoomID, StaffID, Department, Pending, ModifiedBy) " +
                        "VALUES(@SpaceID, @SpaceUse, @LocationID, CASE WHEN @RoomID=0 THEN NULL ELSE @RoomID END, " +
                        "CASE WHEN @StaffID=0 THEN NULL ELSE @StaffID END, @Department, 1, " +
                        "SharedStaffID=CASE WHEN @SharedStaffID=0 THEN NULL ELSE @SharedStaffID END, @ModifiedBy);";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SpaceID", seatInfo.SpaceID.ToUpper());
                        command.Parameters.AddWithValue("@SpaceUse", seatInfo.SpaceUse);
                        command.Parameters.AddWithValue("@LocationID", seatInfo.LocationID);
                        command.Parameters.AddWithValue("@RoomID", seatInfo.RoomID);
                        command.Parameters.AddWithValue("@StaffID", seatInfo.StaffID);
                        command.Parameters.AddWithValue("@Department", seatInfo.Department);
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
                Response.Redirect("./Index?id=" + historyID + "&status=error");
                return;
            }

            if (rowsAffected > 0)
            {
                successMessage = "New Seat Added Successfully";
                Response.Redirect("./Index?id=" + seatInfo.LocationID + "&status=created");
            }
            else
            {
                errorMessage = "Unable to add, entry already exists";
                Response.Redirect("./Index?id=" + historyID + "&status=error");
            }
        }
    }
}