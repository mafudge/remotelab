using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteLab.ComputerManagement
{
    public class FakeComputerManagement : IComputerManagement
    {
        public async Task<bool> ConnectToTcpPortAsync(string ComputerName, string ComputerDomain, int TcpPort)
        {
            return await Task.Run(() => true);
        }

        public async Task<bool> RebootComputerAsync(string ComputerName, string ComputerDomain, string AdminUser, string AdminPassword, string AdminDomain)
        {
            return await Task.Run( () => true );
        }
    }
}
