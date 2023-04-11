using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SeatPlan.Pages
{
    [Authorize]
    public class LegendModel : PageModel
    {
        public LegendInfo legendInfo = new LegendInfo();
        public List<LegendInfo> listLegend = new List<LegendInfo>();
        public String? SelectedColorID;
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
            if (String.IsNullOrEmpty(httpHelper.GetUserName()))
            {
                httpHelper.SetUserName(HttpHelper.QueryUserName(HttpContext));
            }
            int locationID = 1;
            _ = int.TryParse(Request.Query["id"], out locationID);
            legendInfo.LocationID = locationID == 0 ? 1 : locationID;
            LocationName = "- " + Request.Query["name"].ToString();
            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT LegendID, Column1, Column2, Column3, RowColor, ModifiedOn " +
                        "FROM WPSeatPlanLegendRL " +
                        "WHERE LocationID=@LocationID " +
                        "ORDER BY RowNumber";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", legendInfo.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LegendInfo legendInfo = new LegendInfo();
                                legendInfo.LegendID = reader.GetInt32(0);
                                legendInfo.Column1 = reader.IsDBNull(1) ? default : reader.GetString(1);
                                legendInfo.Column2 = reader.IsDBNull(2) ? default : reader.GetString(2);
                                legendInfo.Column3 = reader.IsDBNull(3) ? default : reader.GetString(3);
                                legendInfo.RowColor = reader.IsDBNull(4) ? default : reader.GetString(4);
                                legendInfo.ModifiedOn = reader.GetDateTime(5).ToString();

                                listLegend.Add(legendInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }
        public void OnPostSave(List<LegendInfo> listLegendPost)
        {
            //ViewData["Message"] = Request.Form["LocationID"];
            int locationID;
            if (!int.TryParse(Request.Form["LocationID"], out locationID))
            {
                errorMessage = "Error: Location not found";
                return;
            };

            String sql = "UPDATE WPSeatPlanLegendRL " +
                "SET Column1 " +
                "= CASE LegendID " +
                "WHEN @r1_LegendID THEN @r1_Column1 " +
                "WHEN @r2_LegendID THEN @r2_Column1 " +
                "WHEN @r3_LegendID THEN @r3_Column1 " +
                "WHEN @r4_LegendID THEN @r4_Column1 " +
                "WHEN @r5_LegendID THEN @r5_Column1 " +
                "WHEN @r6_LegendID THEN @r6_Column1 " +
                "WHEN @r7_LegendID THEN @r7_Column1 " +
                "WHEN @r8_LegendID THEN @r8_Column1 " +
                "WHEN @r9_LegendID THEN @r9_Column1 " +
                "WHEN @r10_LegendID THEN @r10_Column1 " +
                "WHEN @r11_LegendID THEN @r11_Column1 " +
                "WHEN @r12_LegendID THEN @r12_Column1 " +
                "WHEN @r13_LegendID THEN @r13_Column1 " +
                "WHEN @r14_LegendID THEN @r14_Column1 " +
                "WHEN @r15_LegendID THEN @r15_Column1 " +
                "WHEN @r16_LegendID THEN @r16_Column1 " +
                "WHEN @r17_LegendID THEN @r17_Column1 " +
                "WHEN @r18_LegendID THEN @r18_Column1 " +
                "WHEN @r19_LegendID THEN @r19_Column1 " +
                "WHEN @r20_LegendID THEN @r20_Column1 " +
                "WHEN @r21_LegendID THEN @r21_Column1 " +
                "WHEN @r22_LegendID THEN @r22_Column1 " +
                "WHEN @r23_LegendID THEN @r23_Column1 " +
                "WHEN @r24_LegendID THEN @r24_Column1 " +
                "END " +
                ", Column2 " +
                "= CASE LegendID " +
                "WHEN @r1_LegendID THEN @r1_Column2 " +
                "WHEN @r2_LegendID THEN @r2_Column2 " +
                "WHEN @r3_LegendID THEN @r3_Column2 " +
                "WHEN @r4_LegendID THEN @r4_Column2 " +
                "WHEN @r5_LegendID THEN @r5_Column2 " +
                "WHEN @r6_LegendID THEN @r6_Column2 " +
                "WHEN @r7_LegendID THEN @r7_Column2 " +
                "WHEN @r8_LegendID THEN @r8_Column2 " +
                "WHEN @r9_LegendID THEN @r9_Column2 " +
                "WHEN @r10_LegendID THEN @r10_Column2 " +
                "WHEN @r11_LegendID THEN @r11_Column2 " +
                "WHEN @r12_LegendID THEN @r12_Column2 " +
                "WHEN @r13_LegendID THEN @r13_Column2 " +
                "WHEN @r14_LegendID THEN @r14_Column2 " +
                "WHEN @r15_LegendID THEN @r15_Column2 " +
                "WHEN @r16_LegendID THEN @r16_Column2 " +
                "WHEN @r17_LegendID THEN @r17_Column2 " +
                "WHEN @r18_LegendID THEN @r18_Column2 " +
                "WHEN @r19_LegendID THEN @r19_Column2 " +
                "WHEN @r20_LegendID THEN @r20_Column2 " +
                "WHEN @r21_LegendID THEN @r21_Column2 " +
                "WHEN @r22_LegendID THEN @r22_Column2 " +
                "WHEN @r23_LegendID THEN @r23_Column2 " +
                "WHEN @r24_LegendID THEN @r24_Column2 " +
                "END " +
                ", Column3 " +
                "= CASE LegendID " +
                "WHEN @r1_LegendID THEN @r1_Column3 " +
                "WHEN @r2_LegendID THEN @r2_Column3 " +
                "WHEN @r3_LegendID THEN @r3_Column3 " +
                "WHEN @r4_LegendID THEN @r4_Column3 " +
                "WHEN @r5_LegendID THEN @r5_Column3 " +
                "WHEN @r6_LegendID THEN @r6_Column3 " +
                "WHEN @r7_LegendID THEN @r7_Column3 " +
                "WHEN @r8_LegendID THEN @r8_Column3 " +
                "WHEN @r9_LegendID THEN @r9_Column3 " +
                "WHEN @r10_LegendID THEN @r10_Column3 " +
                "WHEN @r11_LegendID THEN @r11_Column3 " +
                "WHEN @r12_LegendID THEN @r12_Column3 " +
                "WHEN @r13_LegendID THEN @r13_Column3 " +
                "WHEN @r14_LegendID THEN @r14_Column3 " +
                "WHEN @r15_LegendID THEN @r15_Column3 " +
                "WHEN @r16_LegendID THEN @r16_Column3 " +
                "WHEN @r17_LegendID THEN @r17_Column3 " +
                "WHEN @r18_LegendID THEN @r18_Column3 " +
                "WHEN @r19_LegendID THEN @r19_Column3 " +
                "WHEN @r20_LegendID THEN @r20_Column3 " +
                "WHEN @r21_LegendID THEN @r21_Column3 " +
                "WHEN @r22_LegendID THEN @r22_Column3 " +
                "WHEN @r23_LegendID THEN @r23_Column3 " +
                "WHEN @r24_LegendID THEN @r24_Column3 " +
                "END " +
                ", RowColor " +
                "= CASE LegendID " +
                "WHEN @r1_LegendID THEN @r1_RowColor " +
                "WHEN @r2_LegendID THEN @r2_RowColor " +
                "WHEN @r3_LegendID THEN @r3_RowColor " +
                "WHEN @r4_LegendID THEN @r4_RowColor " +
                "WHEN @r5_LegendID THEN @r5_RowColor " +
                "WHEN @r6_LegendID THEN @r6_RowColor " +
                "WHEN @r7_LegendID THEN @r7_RowColor " +
                "WHEN @r8_LegendID THEN @r8_RowColor " +
                "WHEN @r9_LegendID THEN @r9_RowColor " +
                "WHEN @r10_LegendID THEN @r10_RowColor " +
                "WHEN @r11_LegendID THEN @r11_RowColor " +
                "WHEN @r12_LegendID THEN @r12_RowColor " +
                "WHEN @r13_LegendID THEN @r13_RowColor " +
                "WHEN @r14_LegendID THEN @r14_RowColor " +
                "WHEN @r15_LegendID THEN @r15_RowColor " +
                "WHEN @r16_LegendID THEN @r16_RowColor " +
                "WHEN @r17_LegendID THEN @r17_RowColor " +
                "WHEN @r18_LegendID THEN @r18_RowColor " +
                "WHEN @r19_LegendID THEN @r19_RowColor " +
                "WHEN @r20_LegendID THEN @r20_RowColor " +
                "WHEN @r21_LegendID THEN @r21_RowColor " +
                "WHEN @r22_LegendID THEN @r22_RowColor " +
                "WHEN @r23_LegendID THEN @r23_RowColor " +
                "WHEN @r24_LegendID THEN @r24_RowColor " +
                "END " +
                ", ModifiedOn=SYSDATETIME()" +
                ", ModifiedBy=@ModifiedBy " +
                "WHERE LocationID = @LocationID;";

            try
            {
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        for (int i = 0; i < listLegendPost.Count; i++)
                        {
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_LegendID", listLegendPost[i].LegendID);
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Column1", String.IsNullOrEmpty(listLegendPost[i].Column1) ? DBNull.Value : listLegendPost[i].Column1.Trim());
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Column2", String.IsNullOrEmpty(listLegendPost[i].Column2) ? DBNull.Value : listLegendPost[i].Column2.Trim());
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Column3", String.IsNullOrEmpty(listLegendPost[i].Column3) ? DBNull.Value : (listLegendPost[i].Column3.Trim() == "0" ? DBNull.Value : listLegendPost[i].Column3.Trim()));
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_RowColor", String.IsNullOrEmpty(listLegendPost[i].RowColor) ? DBNull.Value : listLegendPost[i].RowColor.Trim());
                        }
                        command.Parameters.AddWithValue("@LocationID", locationID);
                        HttpHelper httpHelper = new HttpHelper();
                        command.Parameters.AddWithValue("@ModifiedBy", httpHelper.GetUserName());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("./Index?status=edited" + "&id=" + locationID.ToString());
        }
        public void OnPostAutofill(List<LegendInfo> listLegendPost)
        {
            int locationID;
            if (!int.TryParse(Request.Form["LocationID"], out locationID))
            {
                errorMessage = "Error: Location not found";
                return;
            };
            //Required to reload page data
            OnGet();

            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT SUM(CASE WHEN SpaceID LIKE @r1_Param1 OR SpaceID LIKE @r1_Param2 OR REPLACE(Department, '&', '') LIKE @r1_Param3 OR REPLACE(Department, '&', '') LIKE @r1_Param4 THEN 1 ELSE 0 END) AS Count1, " +
                        "SUM(CASE WHEN SpaceID LIKE @r2_Param1 OR SpaceID LIKE @r2_Param2 OR REPLACE(Department, '&', '') LIKE @r2_Param3 OR REPLACE(Department, '&', '') LIKE @r2_Param4 THEN 1 ELSE 0 END) AS Count2, " +
                        "SUM(CASE WHEN SpaceID LIKE @r24_Param1 OR SpaceID LIKE @r24_Param2 OR REPLACE(Department, '&', '') LIKE @r24_Param3 OR REPLACE(Department, '&', '') LIKE @r24_Param4 THEN 1 ELSE 0 END) AS Count24 " +
                        "FROM WPSeatPlan " +
                        "WHERE LocationID = @LocationID";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        String[] replaceRegex = new String[3];
                        String[] splitRegex;

                        //Iterate through the Legend table rows from web postback
                        for (int i = 0; i < listLegendPost.Count; i++)
                        {
                            replaceRegex = new String[3];

                            if (!String.IsNullOrEmpty(listLegendPost[i].Column1))
                            {
                                //Splits string if there is the character '/'
                                splitRegex = Regex.Split(listLegendPost[i].Column1, @"/");
                                if (splitRegex.Count() > 1)
                                {
                                    int splitIndex = 0;
                                    foreach (String line in splitRegex)
                                    {
                                        //Replaces all special characters with nothing, then replaces one or more 'x' characters with '%'
                                        replaceRegex[splitIndex] = Regex.Replace(Regex.Replace(line, @"[^0-9a-zA-Z]+", ""), @"[x]+", "%");
                                        splitIndex++;
                                    }
                                }
                                else
                                {
                                    replaceRegex[0] = Regex.Replace(Regex.Replace(listLegendPost[i].Column1, @"[^0-9a-zA-Z]+", ""), @"[x]+", "%");
                                    replaceRegex[2] = replaceRegex[0];
                                }
                            }
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Param1", String.IsNullOrEmpty(replaceRegex[0]) ? DBNull.Value : "%" + replaceRegex[0].Trim() + "%");
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Param2", String.IsNullOrEmpty(replaceRegex[1]) ? DBNull.Value : "%" + replaceRegex[1].Trim() + "%");
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Param3", String.IsNullOrEmpty(replaceRegex[2]) ? DBNull.Value : replaceRegex[2].Trim());
                            command.Parameters.AddWithValue("@r" + (i + 1).ToString() + "_Param4", String.IsNullOrEmpty(replaceRegex[2]) ? DBNull.Value : (replaceRegex[2].Trim() == "PM" ? "%Practice%Management%" : "% " + replaceRegex[2].Trim()));
                        }
                        command.Parameters.AddWithValue("@LocationID", locationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int j = 0; j < reader.FieldCount; j++)
                                {
                                    listLegendPost[j].Column3 = reader.IsDBNull(j) ? default : (reader.GetInt32(j).ToString() == "0" ? default : reader.GetInt32(j).ToString());
                                }
                            }
                        }
                        listLegend = listLegendPost;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
            successMessage = "All Column 3 values were Auto calculated and replaced. Please check values before submitting.";
        }
    }
    public class LegendInfo
    {
        public int LegendID { get; set; }
        public String? Column1 { get; set; }
        public String? Column2 { get; set; }
        public String? Column3 { get; set; }
        public int RowNumber { get; set; }
        public String? RowColor { get; set; }
        public int LocationID { get; set; }
        public String? ModifiedOn { get; set; }
    }
}