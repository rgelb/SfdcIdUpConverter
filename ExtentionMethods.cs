using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SfdcIdUpConverter
{
    public static class ExtentionMethods
    {
        static public void RaiseEvent(this EventHandler @event, object sender, EventArgs e)
        {
            if (@event != null)
                @event(sender, e);
        }

        static public void RaiseEvent<T>(this EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            if (@event != null)
                @event(sender, e);
        }

        public static void AdjustUrl(this PartnerSoap.SforceService svc)
        {
            if (AppSettings.IsSandbox)
                svc.Url = svc.Url.Replace("login", "test");
            else
                svc.Url = svc.Url.Replace("test", "login");
        }

        public static void AdjustUrl(this ApexSvcSoap.ApexService svc, string partnerSvcUrl)
        {
            // basically we need to grab the domain part of the partnerSvcUrl 
            // and use it to replace the domain part of the svc service

            Uri uriPartnerSvc = new Uri(partnerSvcUrl);
            Uri uriApexSvc = new Uri(svc.Url);

            

            svc.Url = svc.Url.Replace(uriApexSvc.Host, uriPartnerSvc.Host);

        }

        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(str.Length, length));
        }

    }
}
