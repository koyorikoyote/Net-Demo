using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Microsoft.Reporting.NETCore;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Claims;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace SeatPlan.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public HttpHelper httpHelper = new HttpHelper();
        public bool isAdmin = false;
        public List<SeatInfo> listSeats = new List<SeatInfo>();
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public String? selectedLocationID;
        public List<SelectListItem> listLocations = new List<SelectListItem>();
        public SelectList? Locations;
        public List<String> duplicateStaffs = new List<String>();

        public int USE_LOCATION = 1;
        public bool hasPending = false;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public void OnGet()
        {
            httpHelper.SetUserAccess(HttpHelper.AuthenticateUser(HttpContext));
            if (String.IsNullOrEmpty(httpHelper.GetUserAccess()))
            {
                Response.Redirect("./AccessDenied");
                return;
            }
            if (httpHelper.GetUserAccess() == "admin") isAdmin = true;
            String selectedLocationID = Request.Query["id"].ToString();
            String getSeatLayout = "SPGetSeatLayoutMBFC27";
            if (!String.IsNullOrEmpty(selectedLocationID)) int.TryParse(selectedLocationID, out USE_LOCATION);
            switch (USE_LOCATION)
            {
                case 1:
                    getSeatLayout = "SPGetSeatLayoutMBFC27";
                    break;
                case 2:
                    getSeatLayout = "SPGetSeatLayoutMBFC28";
                    break;
                case 3:
                    getSeatLayout = "SPGetSeatLayoutMBFC29";
                    break;
                default:
                    getSeatLayout = "";
                    break;
            }
            try
            {
                //String connectionString = "Data Source=wpsg1devtest31\\instance1;Initial Catalog=WPFacilityMgmt;Persist Security Info=True;User ID=sa;Password=TMS2017!";
                String connectionString = DBConfig.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //Populate a DropDownList with all locations
                    using (SqlCommand command = new SqlCommand("SELECT LocationID, Country, Building, Floor FROM WPSeatPlanLocationRL;", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listLocations.Add(new SelectListItem { Value = reader.GetInt32(0).ToString(), Text = reader.GetInt32(3).ToString() + " " + reader.GetString(2) + " " + reader.GetString(1) });
                            }
                        }
                    }
                    //Get Seat table data
                    using (SqlCommand command = new SqlCommand(getSeatLayout, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SeatInfo seatInfo = new SeatInfo();
                                seatInfo.SpaceID = reader.GetString(0);
                                seatInfo.SpaceUse = reader.GetString(1);
                                seatInfo.Floor = reader.GetInt32(2);
                                seatInfo.Name = reader.IsDBNull(3) ? default : reader.GetString(3).Trim();
                                seatInfo.PhoneNumber = reader.IsDBNull(4) ? default : reader.GetString(4).Trim();
                                seatInfo.Department = reader.IsDBNull(5) ? default : reader.GetString(5);
                                seatInfo.Country = reader.GetString(6);
                                seatInfo.Building = reader.GetString(7);
                                seatInfo.ModifiedOn = (reader.GetBoolean(8)) ? "PENDING" : reader.GetDateTime(9).ToString().Split().First();
                                if (reader.GetBoolean(8) && !hasPending) hasPending = true;
                                listSeats.Add(seatInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            Locations = new SelectList(listLocations, "Value", "Text", selectedLocationID);
            duplicateStaffs = GetDuplicateStaffs(USE_LOCATION);
        }

        public JsonResult OnGetLocation(String selectedLocationID)
        {
            int useLocation;
            if (int.TryParse(selectedLocationID, out useLocation) == false)
            {
                return new JsonResult(listSeats);
            }

            String getSeatLayout = "";
            switch (useLocation)
            {
                case 1:
                    getSeatLayout = "SPGetSeatLayoutMBFC27";
                    break;
                case 2:
                    getSeatLayout = "SPGetSeatLayoutMBFC28";
                    break;
                case 3:
                    getSeatLayout = "SPGetSeatLayoutMBFC29";
                    break;
                default:
                    getSeatLayout = "";
                    break;
            }

            listSeats = new List<SeatInfo>();

            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(getSeatLayout, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SeatInfo seatInfo = new SeatInfo();
                                seatInfo.SpaceID = reader.GetString(0);
                                seatInfo.SpaceUse = reader.GetString(1);
                                seatInfo.Floor = reader.GetInt32(2);
                                seatInfo.Name = reader.IsDBNull(3) ? default : reader.GetString(3).Trim();
                                seatInfo.PhoneNumber = reader.IsDBNull(4) ? default : reader.GetString(4).Trim();
                                seatInfo.Department = reader.IsDBNull(5) ? default : reader.GetString(5);
                                seatInfo.Country = reader.GetString(6);
                                seatInfo.Building = reader.GetString(7);
                                seatInfo.ModifiedOn = (reader.GetBoolean(8)) ? "PENDING" : reader.GetDateTime(9).ToString().Split().First();

                                listSeats.Add(seatInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            duplicateStaffs = GetDuplicateStaffs(useLocation);
            return new JsonResult(listSeats);
        }

        public IActionResult OnPost(ReportService reportService)
        {
            int.TryParse(Request.Form["ddlLocation"].ToString(), out USE_LOCATION);

            //By Default generate MBFC floor 28
            var path = $"{_webHostEnvironment.WebRootPath}\\Reports\\rptMBFC_27.rdlc";
            string dsName = "dsMBFC27";

            if (USE_LOCATION == 2)
            {
                //Change to generate MBFC floor 28
                path = $"{_webHostEnvironment.WebRootPath}\\Reports\\rptMBFC_28.rdlc";
                dsName = "dsMBFC28";
            }
            else if (USE_LOCATION == 3)
            {
                //Change to generate MBFC floor 29
                path = $"{_webHostEnvironment.WebRootPath}\\Reports\\rptMBFC_29.rdlc";
                dsName = "dsMBFC29";
            }

            //Generate the SeatLayout Plan from database as a PDF Report to view
            var datatable = new DataTable();
            datatable = reportService.GetSeatLayout(USE_LOCATION);
            var datatableLegend = new DataTable();
            datatableLegend = reportService.GetReportLegend(USE_LOCATION);

            List<ReportParameter> parameters = new List<ReportParameter>();
            parameters.Add(new ReportParameter("prm1", "Seating Updated: "));
            parameters.Add(new ReportParameter("prm2", DateTime.Now.ToString("dd MMMM yyyy")));
            parameters.Add(new ReportParameter("prm3", "WongPartnership LLP - " + datatable.Rows[0]["Building"].ToString() + " Level " + datatable.Rows[0]["Floor"].ToString()));

            for (int i = 0; i < datatable.Rows.Count; i++)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                parameters.Add(new ReportParameter("dt" + datatable.Rows[i][0].ToString(), datatable.Rows[i][0].ToString()));
#pragma warning restore CS8604 // Possible null reference argument.
            }

            for (int i = 0; i < datatableLegend.Rows.Count; i++)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                parameters.Add(new ReportParameter("legend" + (i + 1).ToString() + "a", datatableLegend.Rows[i][0].ToString()));
                parameters.Add(new ReportParameter("legend" + (i + 1).ToString() + "b", datatableLegend.Rows[i][1].ToString()));
                parameters.Add(new ReportParameter("legend" + (i + 1).ToString() + "c", datatableLegend.Rows[i][2].ToString()));
                parameters.Add(new ReportParameter("legend" + (i + 1).ToString() + "color", datatableLegend.Rows[i][3].ToString()));
#pragma warning restore CS8604 // Possible null reference argument.
            }

            using var fs = new FileStream(path, FileMode.Open);
            LocalReport localReport = new LocalReport();
            localReport.LoadReportDefinition(fs);
            localReport.DataSources.Add(new ReportDataSource(dsName, datatable));
            localReport.SetParameters(parameters);
            byte[] bytes = localReport.Render("PDF");

            return File(bytes, "application/pdf");
        }
        public List<String> GetDuplicateStaffs(int locationID)
        {
            duplicateStaffs = new List<String>();
            try
            {
                String connectionString = DBConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    String sql = "SELECT WP.SpaceID From WPSeatPlan AS WP INNER JOIN " +
                        "(SELECT StaffID " +
                        "FROM WPSeatPlan " +
                        "WHERE LocationID = @LocationID " +
                        "GROUP BY StaffID " +
                        "HAVING COUNT(StaffID)> 1) AS WPS " +
                        "ON WP.StaffID = WPS.StaffID " +
                        "ORDER BY WP.SpaceID";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", locationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            String duplicateID;
                            while (reader.Read())
                            {
                                duplicateID = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                duplicateStaffs.Add(duplicateID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return duplicateStaffs;
        }
    }
    public class SeatInfo
    {
        public String? SpaceID { get; set; }
        public String? SpaceUse { get; set; }
        public int Floor { get; set; }
        public String? Name { get; set; }
        public String? PhoneNumber { get; set; }
        public String? Department { get; set; }
        public String? Country { get; set; }
        public String? Building { get; set; }
        public String? ModifiedOn { get; set; }
        public int LocationID { get; set; }
        public int StaffID { get; set; }
        public int RoomID { get; set; }
        public int SharedStaffID { get; set; }
    }
}