using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Reporting.Map.WebForms.BingMaps;

namespace SeatPlan.Pages
{
    [Authorize]
    public class CreateAccessModel : PageModel
    {
        public AccessControlInfo accessControlInfo = new AccessControlInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public String? UrlBack;
        public List<SelectListItem> listFullName = new List<SelectListItem>();
        public SelectList? FullName;
        public String? SelectedFullName;
        public List<SelectListItem> listSamDepartment = new List<SelectListItem>();
        public SelectList? SamDepartment;
        public String? SelectedSamDepartment;
        public List<SelectListItem> listCountryBuilding = new List<SelectListItem>();
        public SelectList? CountryBuilding;
        public String? SelectedCountryBuilding;

        public void OnGet()
        {
            HttpHelper httpHelper = new HttpHelper();
            if (String.IsNullOrEmpty(httpHelper.GetUserAccess()))
            {
                Response.Redirect("./AccessDenied");
                return;
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
            listFullName = new List<SelectListItem>();
            listSamDepartment = new List<SelectListItem>();
            listCountryBuilding = new List<SelectListItem>();
            try
            {
                //Active Directory Lookup for all Staff information
                using (PrincipalContext adAuth = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
                {
                    using (UserPrincipal user = new UserPrincipal(adAuth))
                    {
                        user.SamAccountName = String.Format("*");
                        using (PrincipalSearcher ps = new PrincipalSearcher(user))
                        {
                            var result = ps.FindAll().Cast<UserPrincipal>();
                            var resultStaff = result.Where(c => c.Enabled == true && c.DistinguishedName.IndexOf("OU=Users") != -1 && !(c.DistinguishedName.IndexOf("Template") != -1 || c.DistinguishedName.IndexOf("Test") != -1 || c.DistinguishedName.IndexOf("LAC") != -1 || c.DistinguishedName.IndexOf("Tech ") != -1 || c.DistinguishedName.IndexOf("=LDR") != -1 || c.DistinguishedName.IndexOf("=Tea") != -1 || c.DistinguishedName.IndexOf("Mailbox") != -1));
                            string tempDept, tempCo, tempStreetAddress, tempAddress1, tempAddress2;
                            string displayNameRegex;
                            List<Tuple<string, string, string, string, string>> listAD = new List<Tuple<string, string, string, string, string>>();
                            DirectoryEntry de;
                            foreach (var item in resultStaff)
                            {
                                de = item.GetUnderlyingObject() as DirectoryEntry;
                                tempDept = (de.Properties["department"].Value != null) ? de.Properties["department"].Value.ToString() : "";
                                tempCo = (de.Properties["co"].Value != null) ? de.Properties["co"].Value.ToString() : ((de.Properties["c"].Value != null) ? de.Properties["c"].Value.ToString() : "");
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
                                listAD.Add(new Tuple<string, string, string, string, string>(item.Name, item.SamAccountName, tempDept, tempCo, (Regex.Match(tempAddress1, @"[^a-zA-Z]+").Success ? (Regex.Replace(tempAddress1, @"[^a-zA-Z]+", "").Length >= Regex.Replace(tempAddress2, @"[^a-zA-Z]+", "").Length ? Regex.Replace(tempAddress1, @"[^a-zA-Z]+", "") : Regex.Replace(tempAddress2, @"[^a-zA-Z]+", "")) : Regex.Replace(tempAddress1, @"[^a-zA-Z]+", ""))));
                            }
                            listAD = listAD.OrderByDescending(c => c.Item4).ThenBy(c => c.Item1).ToList();
                            foreach (var item in listAD)
                            {
                                listFullName.Add(new SelectListItem { Value = item.Item1, Text = item.Item1 });
                                listSamDepartment.Add(new SelectListItem { Value = item.Item2 + "\\" + item.Item3, Text = item.Item2 + "\\" + item.Item3 });
                                listCountryBuilding.Add(new SelectListItem { Value = item.Item4 + "\\" + item.Item5, Text = item.Item4 + "\\" + item.Item5 });
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            FullName = new SelectList(listFullName, "Value", "Text", "");
            SamDepartment = new SelectList(listSamDepartment, "Value", "Text", "");
            CountryBuilding = new SelectList(listCountryBuilding, "Value", "Text", "");
        }
        public void OnPost()
        {
            accessControlInfo.SamAccountName = Request.Form["SamAccountName"];
            accessControlInfo.FullName = Request.Form["SelectedFullName"];
            accessControlInfo.Department = Request.Form["Department"];
            accessControlInfo.Country = Request.Form["Country"];
            accessControlInfo.Building = Request.Form["Building"];
            accessControlInfo.CanEdit = (Request.Form["CanEdit"] == "on") ? true : false;
            accessControlInfo.IsAdmin = (Request.Form["IsAdmin"] == "on") ? true : false;
            accessControlInfo.DeniedAccess = (Request.Form["DeniedAccess"] == "on") ? true : false;

            if (String.IsNullOrEmpty(accessControlInfo.SamAccountName))
            {
                errorMessage = "Invalid Input: Please select the Name of User.";
                return;
            }
            int rowsAffected;
            //save new user info access into database
            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "UPDATE WPSeatPlanUserAccess SET FullName=@FullName, Department=@Department, Country=@Country, Building=@Building, CanEdit=@CanEdit, IsAdmin=@IsAdmin, DeniedAccess=@DeniedAccess, ModifiedOn=SYSDATETIME(), ModifiedBy=@ModifiedBy " +
                        "WHERE SamAccountName=@SamAccountName " +
                        "IF @@ROWCOUNT=0 " +
                        "INSERT INTO WPSeatPlanUserAccess(SamAccountName, FullName, Department, Country, Building, CanEdit, IsAdmin, DeniedAccess, ModifiedBy) " +
                        "VALUES(@SamAccountName, @FullName, @Department, @Country, @Building, @CanEdit, @IsAdmin, @DeniedAccess, @ModifiedBy);";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SamAccountName", accessControlInfo.SamAccountName.Trim());
                        command.Parameters.AddWithValue("@FullName", accessControlInfo.FullName.Trim());
                        command.Parameters.AddWithValue("@Department", accessControlInfo.Department.Trim());
                        command.Parameters.AddWithValue("@Country", accessControlInfo.Country.Trim());
                        command.Parameters.AddWithValue("@Building", accessControlInfo.Building.Trim());
                        command.Parameters.AddWithValue("@CanEdit", accessControlInfo.CanEdit);
                        command.Parameters.AddWithValue("@IsAdmin", accessControlInfo.IsAdmin);
                        command.Parameters.AddWithValue("@DeniedAccess", accessControlInfo.DeniedAccess);
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Response.Redirect("./AccessControl?status=error");
                return;
            }

            if (rowsAffected > 0)
            {
                successMessage = "New Staff Added Successfully";
                Response.Redirect("./AccessControl?status=created");
            }
            else
            {
                errorMessage = "Unable to add, entry already exists";
                Response.Redirect("./AccessControl?status=error");
            }
        }
    }
}