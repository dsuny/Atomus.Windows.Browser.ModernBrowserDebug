using Atomus.Windows.Browser.Models;
using Atomus.Database;
using Atomus.Service;
using System.Threading.Tasks;

namespace Atomus.Windows.Browser.Controllers
{
    internal static class ModernBrowserDebugController
    {
        internal static async Task<IResponse> SearchOpenControlAsync(this ICore core, ModernBrowserSearchModel search)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };
            serviceDataSet["OpenControl"].ConnectionName = core.GetAttribute("DatabaseName");
            serviceDataSet["OpenControl"].CommandText = core.GetAttribute("ProcedureMenuSelect");
            serviceDataSet["OpenControl"].AddParameter("@MENU_ID", DbType.Decimal, 18);
            serviceDataSet["OpenControl"].AddParameter("@ASSEMBLY_ID", DbType.Decimal, 18);
            serviceDataSet["OpenControl"].AddParameter("@USER_ID", DbType.Decimal, 18);

            serviceDataSet["OpenControl"].NewRow();
            serviceDataSet["OpenControl"].SetValue("@MENU_ID", search.MENU_ID);
            serviceDataSet["OpenControl"].SetValue("@ASSEMBLY_ID", search.ASSEMBLY_ID);
            serviceDataSet["OpenControl"].SetValue("@USER_ID", Config.Client.GetAttribute("Account.USER_ID"));

            return await core.ServiceRequestAsync(serviceDataSet);
        }

        internal static IResponse SearchOpenControl(this ICore core, string DatabaseName, string ProcedureID, decimal MENU_ID, decimal ASSEMBLY_ID)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };
            serviceDataSet["OpenControl"].ConnectionName = DatabaseName;
            serviceDataSet["OpenControl"].CommandText = ProcedureID;
            serviceDataSet["OpenControl"].AddParameter("@MENU_ID", DbType.Decimal, 18);
            serviceDataSet["OpenControl"].AddParameter("@ASSEMBLY_ID", DbType.Decimal, 18);
            serviceDataSet["OpenControl"].AddParameter("@USER_ID", DbType.Decimal, 18);

            serviceDataSet["OpenControl"].NewRow();
            serviceDataSet["OpenControl"].SetValue("@MENU_ID", MENU_ID);
            serviceDataSet["OpenControl"].SetValue("@ASSEMBLY_ID", ASSEMBLY_ID);
            serviceDataSet["OpenControl"].SetValue("@USER_ID", Config.Client.GetAttribute("Account.USER_ID"));

            return core.ServiceRequest(serviceDataSet);
        }

        internal static IResponse Login(this ICore core, string DatabaseName, string ProcedureID, string EMAIL, string ACCESS_NUMBER)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet { ServiceName = core.GetAttribute("ServiceName") };
            serviceDataSet["LOGIN"].ConnectionName = DatabaseName;
            serviceDataSet["LOGIN"].CommandText = ProcedureID;
            serviceDataSet["LOGIN"].AddParameter("@EMAIL", DbType.NVarChar, 100);
            serviceDataSet["LOGIN"].AddParameter("@ACCESS_NUMBER", DbType.NVarChar, 4000);

            serviceDataSet["LOGIN"].NewRow();
            serviceDataSet["LOGIN"].SetValue("@EMAIL", EMAIL);
            serviceDataSet["LOGIN"].SetValue("@ACCESS_NUMBER", ACCESS_NUMBER);

            return core.ServiceRequest(serviceDataSet);
        }

        internal static async Task<IResponse> SearchParentMenuAsync(this ICore core, ModernBrowserSearchParentMenuModel search)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };
            serviceDataSet["LoadMenu"].ConnectionName = core.GetAttribute("DatabaseName");
            serviceDataSet["LoadMenu"].CommandText = search.Procedure;
            serviceDataSet["LoadMenu"].AddParameter("@START_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@ONLY_PARENT_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@USER_ID", DbType.Decimal, 18);

            serviceDataSet["LoadMenu"].NewRow();
            serviceDataSet["LoadMenu"].SetValue("@START_MENU_ID", search.START_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@ONLY_PARENT_MENU_ID", search.ONLY_PARENT_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@USER_ID", Config.Client.GetAttribute("Account.USER_ID"));

            return await core.ServiceRequestAsync(serviceDataSet);
        }
    }
}