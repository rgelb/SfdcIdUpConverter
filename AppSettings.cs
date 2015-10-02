using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SfdcIdUpConverter
{
    public static class AppSettings
    {
        public static string UserName 
        { 
            get 
            {
                ConfigurationManager.RefreshSection("appSettings");
                return ConfigurationManager.AppSettings["SfdcUid"] ?? ""; 
            }             
        }

        public static string Password
        {
            get
            {
                ConfigurationManager.RefreshSection("appSettings");
                return ConfigurationManager.AppSettings["SfdcPwd"] ?? ""; 
            }      
        }

        public static bool IsSandbox
        {
            get
            {
                ConfigurationManager.RefreshSection("appSettings");
                string text = ConfigurationManager.AppSettings["IsSandbox"] ?? "";

                switch (text.ToUpper())
                {
                    case "Y":
                    case "YES":
                    case "TRUE":
                        return true;
                    case "N":
                    case "NO":
                    case "FALSE":
                    default:
                        return false;
                }
            }
        }

    }

}
