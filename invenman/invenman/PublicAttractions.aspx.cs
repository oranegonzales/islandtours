using System;
using System.Data;

namespace invenman
{
    public partial class PublicAttractions : System.Web.UI.Page
    {
        private readonly AttractionService svc = new AttractionService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAllAttractions();
            }
        }

        private void LoadAllAttractions()
        {
            try
            {
                DataTable dt = svc.GetAttractions();
                gvAttractions.DataSource = dt;
                gvAttractions.DataBind();
                lblError.Text = "";
            }
            catch
            {
                lblError.Text = "Unable to load attractions at this time. Please try again later.";
            }
        }

        private void LoadAttractionsByParish(string parish)
        {
            try
            {
                DataTable dt;

                if (string.IsNullOrWhiteSpace(parish))
                {
                    dt = svc.GetAttractions();
                }
                else
                {
                    dt = svc.GetAttractionsByParish(parish);
                }

                gvAttractions.DataSource = dt;
                gvAttractions.DataBind();
                lblError.Text = "";
            }
            catch
            {
                lblError.Text = "Unable to filter attractions at this time.";
            }
        }

        protected void ddlParish_SelectedIndexChanged(object sender, EventArgs e)
        {
            string parish = ddlParish.SelectedValue;
            LoadAttractionsByParish(parish);
        }
    }
}
