using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;

namespace invenman
{
    [WebService(Namespace = "http://invenman.local/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AttractionService : WebService
    {
        private readonly string connStr;

        public AttractionService()
        {
            connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;
        }

        [WebMethod]
        public DataTable GetAttractions()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT AttractionID, Name, Parish, Category, BasePrice, ChildPrice, OpenDays " +
                "FROM Attractions WHERE IsActive = 1 ORDER BY Parish, Name", conn))
            {
                DataTable dt = new DataTable("Attractions");
                da.Fill(dt);
                return dt;
            }
        }

        [WebMethod]
        public DataTable GetAttractionByID(int attractionID)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT AttractionID, Name, Description, Parish, Category, BasePrice, ChildPrice, OpenDays " +
                "FROM Attractions WHERE IsActive = 1 AND AttractionID = @AttractionID", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionID;

                DataTable dt = new DataTable("Attraction");
                da.Fill(dt);
                return dt;
            }
        }

        [WebMethod]
        public DataTable GetAttractionsByParish(string parish)
        {
            if (string.IsNullOrWhiteSpace(parish))
            {
                return GetAttractions();
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT AttractionID, Name, Parish, Category, BasePrice, ChildPrice, OpenDays " +
                "FROM Attractions WHERE IsActive = 1 AND Parish = @Parish ORDER BY Name", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Parish", SqlDbType.VarChar, 100).Value = parish;

                DataTable dt = new DataTable("Attractions");
                da.Fill(dt);
                return dt;
            }
        }
    }
}
