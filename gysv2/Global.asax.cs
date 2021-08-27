using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace gysv2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //log4net ≈‰÷√
            FileInfo configFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config");
            XmlConfigurator.ConfigureAndWatch(configFile);// π”√ConfigureAndWatch]

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
