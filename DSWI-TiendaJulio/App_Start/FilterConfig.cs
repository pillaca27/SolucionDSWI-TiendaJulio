using System.Web;
using System.Web.Mvc;

namespace DSWI_TiendaJulio
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
