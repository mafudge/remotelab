using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteLab.ComputerManagement
{
    public class FakeComputerManagement : IComputerManagement
    {
        public async Task<bool> ConnectToTcpPortAsync(string ComputerName, string UserDNSDomain, int TcpPort)
        {
            return await Task.Run(() => true);
        }

        public async Task<bool> RebootComputerAsync(string ComputerName, string AdminUser, string AdminPassword, string UserDomain, string UserDNSDomain)

        {
            return await Task.Run( () => true );
        }
    }
}
