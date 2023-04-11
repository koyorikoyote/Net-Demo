using System.Data;
using System.Data.SqlClient;

namespace SeatPlan.Pages
{
    public class ReportService
    {
        String connectionString = DBConfig.ConnectionString;

        public DataTable GetSeatLayout(int locationID)
        {
            String getSeatReport = "SPGetSeatReportMBFC27";

            switch (locationID)
            {
                case 1:
                    getSeatReport = "SPGetSeatReportMBFC27";
                    break;
                case 2:
                    getSeatReport = "SPGetSeatReportMBFC28";
                    break;
                case 3:
                    getSeatReport = "SPGetSeatReportMBFC29";
                    break;
                default:
                    getSeatReport = "SPGetSeatReportMBFC27";
                    break;
            }

            var dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(getSeatReport, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }

        public DataTable GetReportLegend(int locationID)
        {
            locationID = locationID == 0 ? 1 : locationID;
            var dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String sql = "SELECT Column1, Column2, Column3, RowColor " +
                        "FROM WPSeatPlanLegendRL " +
                        "WHERE LocationID=@LocationID " +
                        "ORDER BY RowNumber";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationID);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }
    }
}