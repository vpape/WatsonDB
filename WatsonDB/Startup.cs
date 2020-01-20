using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WatsonDB.Startup))]
namespace WatsonDB
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
