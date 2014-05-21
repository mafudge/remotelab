using System;
using System.Threading.Tasks;

namespace RemoteLab.ComputerManagement
{
    public interface IComputerManagement
    {
        Task<bool> ConnectToTcpPortAsync(string ComputerName, string ComputerDomain, int TcpPort);
        Task<bool> RebootComputerAsync(string ComputerName, string ComputerDomain, string AdminUser, string AdminPassword, string AdminDomain);
    }
}
