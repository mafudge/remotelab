using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RemoteLab.Startup))]
namespace RemoteLab
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
