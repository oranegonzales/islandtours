using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using invenman.Security;

namespace invenman
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
            {
                return;
            }

            AuthSecurity.TryRestoreSession(context);
            if (AuthorizationRules.CanAccess(context))
            {
                return;
            }

            bool loggedIn =
                !string.IsNullOrWhiteSpace(context.Session["Username"] as string);
            context.Response.Redirect(
                loggedIn ? "~/Home.aspx" : "~/Login.aspx",
                false);
            context.ApplicationInstance.CompleteRequest();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception error = Server.GetLastError();
            if (error != null)
            {
                System.Diagnostics.Trace.TraceError(error.ToString());
            }
        }
    }
}
