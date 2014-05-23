using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Management;
using System.Net.Sockets;
using Elmah;


namespace RemoteLab.ComputerManagement
{
    public class WindowsComputerManagement : IComputerManagement 
    {

        private ErrorLog Logger;
        public WindowsComputerManagement(ErrorLog Logger) 
        { 
            this.Logger = Logger;
        }

        public async Task<bool> RebootComputerAsync(string ComputerName, string AdminUser, string AdminPassword, string UserDomain, string UserDNSDomain)

        {
            var ManagementPath = new ManagementPath(String.Format(@"\\{0}.{1}\root\cimv2", ComputerName, UserDNSDomain));
            var Options = new ConnectionOptions()
            {
                EnablePrivileges = true,
                Username = AdminUser,
                Password = AdminPassword,
                Authority = "ntlmdomain:" + UserDomain,
                Timeout = new TimeSpan(0, 0, 5),
                Authentication = AuthenticationLevel.Default
            };

            try
            {
                var Scope = new ManagementScope(ManagementPath, Options);
                Scope.Connect();
                var Query = new SelectQuery("Win32_OperatingSystem");
                using (var Searcher = new ManagementObjectSearcher(Scope, Query))
                foreach( ManagementObject OS in Searcher.Get())
                {
                    using (ManagementBaseObject InParams = OS.GetMethodParameters("Win32Shutdown"))
                    {
                        InParams["Flags"] = 6;
                        ManagementBaseObject OutParams = await Task.Run(() => OS.InvokeMethod("Win32Shutdown", InParams, null));
                        OutParams.Dispose();
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Log(new Elmah.Error(e));
                return false;
            }

            return true;
        }

        public async Task<bool> ConnectToTcpPortAsync(string ComputerName, string UserDNSDomain, int TcpPort)
        {

            var Hostname = String.Format(@"{0}.{1}", ComputerName, UserDNSDomain);
            var TcpClient = new TcpClient();
            try
            {
                TcpClient.ReceiveTimeout = 10;
                TcpClient.SendTimeout = 10;
                await TcpClient.ConnectAsync(Hostname, TcpPort);
            }
            catch (SocketException e)
            {
                Logger.Log(new Elmah.Error(e));
                return false;
            }

            return true;
        }

    }
}