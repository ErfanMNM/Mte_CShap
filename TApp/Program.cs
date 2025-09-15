namespace TApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "TApp", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("Phần mềm đang chạy rồi bro!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
        }
    }
}