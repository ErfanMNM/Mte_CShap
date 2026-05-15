namespace MHG_Printer.Utils
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
