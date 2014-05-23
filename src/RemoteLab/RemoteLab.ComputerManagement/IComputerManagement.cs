using System;
using System.Threading.Tasks;

namespace RemoteLab.ComputerManagement
{
    public interface IComputerManagement
    {
        Task<bool> ConnectToTcpPortAsync(string ComputerName, string UserDNSDomain, int TcpPort);
        Task<bool> RebootComputerAsync(string ComputerName, string AdminUser, string AdminPassword, string UserDomain, string UserDNSDomain);
    }
}
