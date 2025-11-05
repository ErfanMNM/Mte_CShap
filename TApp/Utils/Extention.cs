using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Utils
{
    public static class FormExtension
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
            {
                try
                {
                    control.Invoke(action);
                }
                catch
                {
                    // Nuốt lỗi khi form đang dispose
                }
            }
            else
            {
                action();
            }
        }
    }
}
