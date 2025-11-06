using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace TTManager.Helpers
{
    public class FilePathEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
        {
            return UITypeEditorEditStyle.Modal; // hiển thị hộp thoại
        }

        public override object EditValue(ITypeDescriptorContext ? context, IServiceProvider? provider, object? value)
        {
            if (context == null || provider == null)
                throw new ArgumentNullException("Context hoặc Provider không được null.");

            if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService service)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Chọn file SQLite";
                    ofd.Filter = "SQLite Database (*.db;*.sqlite)|*.db;*.sqlite|Tất cả (*.*)|*.*";
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    if (value is string currentPath && !string.IsNullOrWhiteSpace(currentPath))
                    {
                        try
                        {
                            ofd.InitialDirectory = Path.GetDirectoryName(currentPath);
                        }
                        catch { }
                    }

                    if (ofd.ShowDialog() == DialogResult.OK)
                        return ofd.FileName;
                }
            }

            return value;
        }
    }
}
