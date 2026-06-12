using System;
using System.Collections.Generic;
using System.Text;

namespace VNQR.Helpers
{
    public class po
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
