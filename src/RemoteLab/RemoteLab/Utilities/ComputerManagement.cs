using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Management;
using System.Net.Sockets;

namespace RemoteLab.Utilities
{
    public class ComputerManagement
    {
        public ComputerManagement() { }

        public async Task<bool> RebootComputerAsync( String ComputerName, String ComputerDomain, String AdminUser, String AdminPassword, String AdminDomain)
        {
            //TODO: Mock this. For now, return true
            return true;

            var ManagementPath = new ManagementPath(String.Format(@"\\{0}.{1}\root\cimv2",ComputerName,ComputerDomain));
            var Options = new ConnectionOptions() {
                EnablePrivileges = true,
                Username = AdminUser,
                Password = AdminPassword,
                Authority = "ntlmdomain:" + AdminDomain,
                Timeout = new TimeSpan(0,0,5),
                Authentication = AuthenticationLevel.Default
            };

            try 
	        {	        
		        var Scope = new ManagementScope(ManagementPath, Options);
                Scope.Connect();
                var Query = new SelectQuery("Win32_OperatingSystem");
                var Searcher = new ManagementObjectSearcher(Scope, Query);
                ManagementObject OS = (ManagementObject) Searcher.Get().GetEnumerator().Current;
                ManagementBaseObject InParams = OS.GetMethodParameters("Win32Shutdown");
                InParams["Flags"] = 6;
                ManagementBaseObject OutParams = await Task.Run( () => OS.InvokeMethod("Win32Shutdown", InParams, null));

	        }
	        catch (Exception e)
	        {
		        Elmah.ErrorLog.GetDefault(HttpContext.Current).Log( new Elmah.Error(e));
                return false;		     
	        }
                
            return true;
        }

        public async Task<bool> ConnectToTcpPortAsync(String ComputerName, String ComputerDomain, int TcpPort)
        {
            //TODO: Mock this. For now, return true
            return true;

            var Hostname = String.Format(@"{0}.{1}",ComputerName, ComputerDomain);
            var TcpClient = new TcpClient();
            try 
	        {	        
		        TcpClient.ReceiveTimeout = 10;
                TcpClient.SendTimeout = 10;
                await TcpClient.ConnectAsync(Hostname, TcpPort);
	        }
	        catch (SocketException e)
	        {
		        Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(e));
                return false;		        
	        }

            return true;
        }

    }
}