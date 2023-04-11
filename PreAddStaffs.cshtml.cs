using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using Microsoft.Reporting.Map.WebForms.BingMaps;

namespace SeatPlan.Pages
{
    [Authorize]
    [RequestFormLimits(ValueCountLimit = int.MaxValue)]
    [IgnoreAntiforgeryToken]
    public class PreAddStaffsModel : PageModel
    {
        public List<StaffInfo> listStaffs = new List<StaffInfo>();
        public List<StaffInfo> listStaffsRemove = new List<StaffInfo>();
        public StaffInfo staffInfo = new StaffInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String LocationName = "";
        public String preAddCount = "";
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
                //Active Directory Lookup for all Staff information
                listStaffs = new List<StaffInfo>();
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
                            List<Tuple<string, string, string, string, string, string, string>> listAD = new List<Tuple<string, string, string, string, string, string, string>>();
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
                                tempAddress1 = Regex.Replace(tempAddress1, @"[^0-9a-zA-Z]+", "");
                                tempAddress2 = Regex.Replace(tempAddress2, @"[^0-9a-zA-Z]+", "");
                                displayNameRegex = (item.GivenName.Length > 11) ? item.GivenName.Split(" ").First() + " " + Regex.Replace(Regex.Replace(item.GivenName, "\\B(\\w)", ""), @"\s+", "").Substring(1) : item.GivenName;
                                displayNameRegex = (displayNameRegex.Length > 11) ? displayNameRegex.Substring(0, 11) : displayNameRegex;
                                tempDept = Regex.Replace(Regex.Replace(tempDept, "\\B(\\w)", ""), @"\s+", "");
                                listAD.Add(new Tuple<string, string, string, string, string, string, string>(item.Name, ((String.IsNullOrEmpty(item.VoiceTelephoneNumber) || item.VoiceTelephoneNumber.Length < 4) ? item.VoiceTelephoneNumber : item.VoiceTelephoneNumber.Substring(item.VoiceTelephoneNumber.Length - 4)), displayNameRegex, tempDept, tempCo, tempAddress1, tempAddress2));
                            }
                            listAD = listAD.OrderBy(c => c.Item1).ToList();
                            foreach (var item in listAD)
                            {
                                listStaffs.Add(new StaffInfo { FullName = item.Item1, PhoneNumber = item.Item2, Name = item.Item3, Department = item.Item4, Country = item.Item5, Building = item.Item6 + ((item.Item7.Length > 0) ? "/" : "") + item.Item7 });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT FullName FROM WPSeatPlanStaffRL WHERE FullName IS NOT NULL;";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                StaffInfo staffInfo = new StaffInfo();
                                staffInfo.FullName = reader.IsDBNull(0) ? default : reader.GetString(0);

                                listStaffsRemove.Add(staffInfo);
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
            HashSet<string> removeFullNames = new HashSet<string>(listStaffsRemove.Select(x => x.FullName));
            listStaffs.RemoveAll(x => removeFullNames.Contains(x.FullName));

            preAddCount = listStaffs.Count.ToString();
        }

        public void OnPostSave(List<StaffInfo> listStaffsPost)
        {
            int rowsAffected = 0;
            listStaffsPost.RemoveAll(x => String.IsNullOrEmpty(x.FullName));
            List<StaffInfo> dupListStaffsPost = listStaffsPost.ToList();
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    for (int j = 0; j <= dupListStaffsPost.Count / 100; j++)
                    {
                        if (j == 0)
                        {
                            listStaffsPost = dupListStaffsPost.Take(100).ToList();
                        }
                        else if (j > (dupListStaffsPost.Count / 100) - 1)
                        {
                            listStaffsPost = dupListStaffsPost.Skip(100 * j).Take(dupListStaffsPost.Count % 100).ToList();
                        }
                        else
                        {
                            listStaffsPost = dupListStaffsPost.Skip(100 * j).Take(100).ToList();
                        }

                        String sql = "INSERT INTO WPSeatPlanStaffRL (FullName, Name, PhoneNumber, Department, LocationID, ModifiedBy) VALUES ";
                        for (int i = 0; i < listStaffsPost.Count; i++)
                        {
                            if (i != 0) sql += ",";
                            sql += "(@FullName_" + i + ", @Name_" + i + ", @PhoneNumber_" + i + ", @Department_" + i;
                            sql += ", ISNULL((SELECT TOP 1 LocationID FROM WPSeatPlanLocationRL WHERE Building=@BuildingA_" + i +
                                " OR Building=@BuildingB_" + i + " OR Country=@Country_" + i + "),1), @ModifiedBy)";

                        }
                        sql += ";";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            for (int i = 0; i < listStaffsPost.Count; i++)
                            {
                                command.Parameters.AddWithValue("@FullName_" + i, listStaffsPost[i].FullName);
                                command.Parameters.AddWithValue("@Name_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].Name)) ? listStaffsPost[i].Name : "");
                                command.Parameters.AddWithValue("@PhoneNumber_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].PhoneNumber)) ? listStaffsPost[i].PhoneNumber : "");
                                command.Parameters.AddWithValue("@Department_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].Department)) ? listStaffsPost[i].Department : "");
                                command.Parameters.AddWithValue("@BuildingA_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].Building)) ? listStaffsPost[i].Building.Split("/").First() : "");
                                command.Parameters.AddWithValue("@BuildingB_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].Building)) ? listStaffsPost[i].Building.Split("/").Last() : "");
                                command.Parameters.AddWithValue("@Country_" + i, (!String.IsNullOrEmpty(listStaffsPost[i].Country)) ? listStaffsPost[i].Country : "");
                            }
                            HttpHelper httpHelper = new HttpHelper();
                            command.Parameters.AddWithValue("@ModifiedBy", "PREADD//" + httpHelper.GetUserName());

                            rowsAffected += command.ExecuteNonQuery();
                        }
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
                successMessage = "New Staffs Pre-Added Successfully";
                Response.Redirect("./ViewStaff?status=PreAdded");
            }
            else
            {
                errorMessage = "Unable to proceed, duplicate entry already exists";
                Response.Redirect("./ViewStaff?status=error");
            }
        }
    }
}