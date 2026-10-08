using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Helpers
{
    public class NetworkHelper
    {
        public static void SetNetworkAdapter (string AdapterName, bool enable)
        {
            string action = enable ? "Enable-NetAdapter" : "Disable-NetAdapter";
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{action} -Name '{AdapterName}' -Confirm:$false\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using Process process = Process.Start(psi);process.WaitForExit();
        }
    }
}
