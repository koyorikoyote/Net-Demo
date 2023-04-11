using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.DirectoryServices;

namespace SeatPlan.Pages
{
    [Authorize]
    public class EditStaffModel : PageModel
    {
        public StaffInfo staffInfo = new StaffInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String? UrlBack;
        public List<SelectListItem> listStaffs = new List<SelectListItem>();
        public SelectList? Staff;
        public String? SelectedStaffName;
        public List<SelectListItem> listPhoneExt = new List<SelectListItem>();
        public SelectList? PhoneExt;
        public String? SelectedPhoneExt;
        public List<SelectListItem> listLocation = new List<SelectListItem>();
        public SelectList? Locations;
        public String? SelectedLocation;
        public List<SelectListItem> listDisplayName = new List<SelectListItem>();
        public SelectList? DisplayName;
        public String? SelectedDisplayName;
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
            int StaffID = 0;
            if (int.TryParse(Request.Query["id"], out StaffID) == false) return;
            int ddlLocationSelected = 0;
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT WPS.Name, WPS.PhoneNumber, WPS.FullName, WPL.Floor, WPL.Building, WPL.Country, WPS.LocationID " +
                        "FROM WPSeatPlanStaffRL AS WPS " +
                        "LEFT JOIN WPSeatPlanLocationRL AS WPL ON WPS.LocationID = WPL.LocationID " +
                        "WHERE WPS.StaffID=@StaffID;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@StaffID", StaffID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                staffInfo.StaffID = StaffID;
                                staffInfo.Name = reader.IsDBNull(0) ? default : reader.GetString(0);
                                staffInfo.PhoneNumber = reader.IsDBNull(1) ? default : reader.GetString(1);
                                staffInfo.FullName = reader.IsDBNull(2) ? default : reader.GetString(2);
                                staffInfo.Floor = reader.GetInt32(3);
                                staffInfo.Building = reader.GetString(4);
                                staffInfo.Country = reader.GetString(5);
                                staffInfo.LocationID = reader.GetInt32(6);
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
            SelectedStaffName = String.IsNullOrEmpty(staffInfo.FullName) ? "" : staffInfo.FullName;
            ddlLocationSelected = staffInfo.LocationID;
            try
            {
                //Active Directory Lookup for all Staff information
                listPhoneExt = new List<SelectListItem>();
                listStaffs = new List<SelectListItem>();
                using (PrincipalContext adAuth = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
                {
                    using (UserPrincipal user = new UserPrincipal(adAuth))
                    {
                        user.SamAccountName = String.Format("*");
                        using (PrincipalSearcher ps = new PrincipalSearcher(user))
                        {
                            var result = ps.FindAll().Cast<UserPrincipal>();
                            var resultStaff = result.Where(c => c.Enabled == true && c.DistinguishedName.IndexOf("OU=Users") != -1 && !(c.DistinguishedName.IndexOf("Template") != -1 || c.DistinguishedName.IndexOf("Test") != -1 || c.DistinguishedName.IndexOf("LAC") != -1 || c.DistinguishedName.IndexOf("CN=Tech ") != -1 || c.DistinguishedName.IndexOf("=LDR PT") != -1 || c.DistinguishedName.IndexOf("Mailbox") != -1));
                            string tempDept, tempCo, tempStreetAddress, tempAddress1, tempAddress2;
                            string displayNameRegex;
                            List<Tuple<string, string, string>> listAD = new List<Tuple<string, string, string>>();
                            DirectoryEntry de;
                            foreach (var item in resultStaff)
                            {
                                de = item.GetUnderlyingObject() as DirectoryEntry;
                                tempDept = (de.Properties["department"].Value != null) ? de.Properties["department"].Value.ToString() : "";
                                tempCo = (de.Properties["co"].Value != null) ? de.Properties["co"].Value.ToString() : "";
                                tempStreetAddress = (de.Properties["streetAddress"].Value != null) ? de.Properties["streetAddress"].Value.ToString() : "";
                                if (tempStreetAddress.IndexOf("\r\n") != -1)
                                {
                                    tempAddress1 = tempStreetAddress.Split("\r\n").First();
                                    tempAddress2 = tempStreetAddress.Split("\r\n").Last();
                                    tempAddress1 = Regex.Match(Regex.Replace(Regex.Replace(tempAddress1, "\\B(\\w)", ""), @"\s+", ""), "^.{4}|^.{3}", RegexOptions.None).Value;
                                    tempAddress2 = Regex.Match(Regex.Replace(Regex.Replace(tempAddress2, "\\B(\\w)", ""), @"\s+", ""), "^.{4}|^.{3}", RegexOptions.None).Value;
                                }
                                else
                                {
                                    tempAddress1 = tempStreetAddress;
                                    tempAddress2 = "";
                                    tempAddress1 = Regex.Match(Regex.Match(Regex.Replace(Regex.Replace(tempAddress1, "\\B(\\w)", ""), @"\s+", ""), ".{6}$|.{5}$", RegexOptions.None).Value, "^.{4}|^.{3}", RegexOptions.None).Value;
                                }
                                if ((String.IsNullOrEmpty(tempCo) || tempCo.IndexOf(staffInfo.Country) != -1) && (tempAddress1.IndexOf(staffInfo.Building) != -1 || tempAddress2.IndexOf(staffInfo.Building) != -1 || tempStreetAddress.IndexOf(staffInfo.Building) != -1))
                                {
                                    displayNameRegex = (item.GivenName.Length > 11) ? item.GivenName.Split(" ").First() + " " + Regex.Replace(Regex.Replace(item.GivenName, "\\B(\\w)", ""), @"\s+", "").Substring(1) : item.GivenName;
                                    displayNameRegex = (displayNameRegex.Length > 11) ? displayNameRegex.Substring(0, 11) : displayNameRegex;
                                    listAD.Add(new Tuple<string, string, string>(item.Name, item.VoiceTelephoneNumber, displayNameRegex));
                                }
                            }
                            listAD = listAD.OrderBy(c => c.Item1).ToList();
                            foreach (var item in listAD)
                            {
                                listStaffs.Add(new SelectListItem { Value = item.Item1, Text = item.Item1 });
                                if (String.IsNullOrEmpty(item.Item2) || item.Item2.Length < 4)
                                {
                                    listPhoneExt.Add(new SelectListItem { Value = null, Text = null });
                                }
                                else
                                {
                                    listPhoneExt.Add(new SelectListItem { Value = item.Item2.Substring(item.Item2.Length - 4), Text = item.Item2 });
                                }
                                listDisplayName.Add(new SelectListItem { Value = item.Item3, Text = item.Item3 });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            Staff = new SelectList(listStaffs, "Value", "Text", SelectedStaffName);
            PhoneExt = new SelectList(listPhoneExt, "Value", "Text", "");
            DisplayName = new SelectList(listDisplayName, "Value", "Text", "");
            Locations = new SelectList(listLocation, "Value", "Text", ddlLocationSelected.ToString());
        }
        public void OnPost()
        {
            UrlBack = Request.Form["UrlBack"];
            staffInfo.StaffID = int.Parse(Request.Form["StaffID"]);
            staffInfo.Name = Request.Form["StaffName"];
            staffInfo.FullName = Request.Form["SelectedStaffName"];
            staffInfo.PhoneNumber = Request.Form["PhoneNumber"];
            int locID = 0;
            _ = int.TryParse(Request.Form["SelectedLocation"], out locID);
            staffInfo.LocationID = locID;

            if (String.IsNullOrEmpty(staffInfo.Name))
            {
                errorMessage = "Invalid Input: Missing value required for Staff Display Name.";
                return;
            }
            if (staffInfo.LocationID == 0)
            {
                errorMessage = "Invalid Input: Please select a valid location.";
                return;
            }

            int rowsAffected;
            String sql = "UPDATE WPSeatPlanStaffRL " +
                "SET Name=@Name, " +
                "PhoneNumber=@PhoneNumber, " +
                "FullName=@FullName, " +
                "LocationID=@LocationID, " +
                "ModifiedOn=SYSDATETIME(), " +
                "ModifiedBy=@ModifiedBy " +
                "WHERE StaffID=@StaffID";
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", staffInfo.Name.Trim());
                        command.Parameters.AddWithValue("@PhoneNumber", staffInfo.PhoneNumber.Trim());
                        command.Parameters.AddWithValue("@StaffID", staffInfo.StaffID);
                        command.Parameters.AddWithValue("@FullName", staffInfo.FullName.Trim());
                        command.Parameters.AddWithValue("@LocationID", staffInfo.LocationID);
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
                successMessage = "Staff Edited Successfully";
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