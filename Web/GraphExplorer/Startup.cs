using Owin;
using System.Web.Configuration;

namespace GraphExplorer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string config = WebConfigurationManager.AppSettings["AuthenticationEnabled"];
            if (config.Equals("true")) {
                ConfigureAuth(app);
            }

        }
    }
}
