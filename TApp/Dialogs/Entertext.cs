using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace MASAN_SERIALIZATION.Diaglogs
{
    public partial class Entertext : Form
    {
        public event EventHandler EnterClicked;
        public string TextValue { get; set; }
        private bool isShiftEnabled = true; // Trạng thái Shift
        private bool isSymbolEnabled = false; // Trạng thái Symbol
        public string TileText { get; set; } = "Nhập văn bản";

        public bool IsPassword { get; set; } = false; // Biến để xác định có phải nhập mật khẩu hay không
        public Entertext()
        {
            InitializeComponent();
            InitializeKeyboardStyle();
        }
        
        private void InitializeKeyboardStyle()
        {
            // Cài đặt style cho toàn bộ form
            this.BackColor = Color.FromArgb(248, 249, 250);
            
            // Style cho title panel
            uiTitlePanel1.FillColor = Color.FromArgb(63, 81, 181);
            uiTitlePanel1.ForeColor = Color.White;
            uiTitlePanel1.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            
            // Style cho text box
            //StyleTextBox();
            
            // Style cho các phím
            //StyleKeyboardButtons();
            
            // Style cho các nút chức năng
            //StyleFunctionButtons();
        }
        
        private void StyleTextBox()
        {
            textPadTextBox.Font = new Font("Segoe UI", 14F);
            textPadTextBox.FillColor = Color.White;
            textPadTextBox.RectColor = Color.FromArgb(189, 195, 199);
            textPadTextBox.Radius = 10;
            textPadTextBox.RectSize = 2;
            textPadTextBox.Padding = new Padding(10, 8, 10, 8);
        }
        
        private void StyleKeyboardButtons()
        {
            // Style cho các phím chữ
            var letterButtons = new[] { Q, W, E, R, T, Y, U, I, O, P, A, S, D, F, G, H, J, K, L, Z, X, C, V, B, N, M };
            foreach (var button in letterButtons)
            {
                StyleLetterButton(button);
            }
            
            // Style cho các phím đặc biệt
            StyleSpecialButton(Space, "🔲 Dấu Cách");
            StyleSpecialButton(Dot, ".");
            StyleSpecialButton(Dot2, ",");
        }
        
        private void StyleLetterButton(UIButton button)
        {
            button.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            button.FillColor = Color.White;
            button.FillHoverColor = Color.FromArgb(240, 244, 248);
            button.FillPressColor = Color.FromArgb(224, 232, 240);
            button.RectColor = Color.FromArgb(189, 195, 199);
            button.ForeColor = Color.FromArgb(52, 73, 94);
            button.Radius = 8;
            button.RectSize = 1;
        }
        
        private void StyleSpecialButton(UIButton button, string text)
        {
            button.Text = text;
            button.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            button.FillColor = Color.FromArgb(236, 240, 241);
            button.FillHoverColor = Color.FromArgb(220, 226, 227);
            button.FillPressColor = Color.FromArgb(189, 195, 199);
            button.RectColor = Color.FromArgb(149, 165, 166);
            button.ForeColor = Color.FromArgb(52, 73, 94);
            button.Radius = 8;
            button.RectSize = 1;
        }
        
        private void StyleFunctionButtons()
        {
            // Nút Shift
            Shift.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Shift.FillColor = Color.FromArgb(69, 90, 100);
            Shift.FillHoverColor = Color.FromArgb(96, 125, 139);
            Shift.FillPressColor = Color.FromArgb(55, 71, 79);
            Shift.RectColor = Color.FromArgb(69, 90, 100);
            Shift.ForeColor = Color.White;
            Shift.Radius = 10;
            Shift.RectSize = 0;
            Shift.Text = " Shift";
            
            // Nút 123
            S123.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            S123.FillColor = Color.FromArgb(69, 90, 100);
            S123.FillHoverColor = Color.FromArgb(96, 125, 139);
            S123.FillPressColor = Color.FromArgb(55, 71, 79);
            S123.RectColor = Color.FromArgb(69, 90, 100);
            S123.ForeColor = Color.White;
            S123.Radius = 10;
            S123.RectSize = 0;
            S123.Text = "🔢 123";
            
            // Nút Backspace
            uiSymbolButton2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            uiSymbolButton2.FillColor = Color.FromArgb(231, 76, 60);
            uiSymbolButton2.FillHoverColor = Color.FromArgb(241, 148, 138);
            uiSymbolButton2.FillPressColor = Color.FromArgb(192, 57, 43);
            uiSymbolButton2.RectColor = Color.FromArgb(231, 76, 60);
            uiSymbolButton2.ForeColor = Color.White;
            uiSymbolButton2.Radius = 10;
            uiSymbolButton2.RectSize = 0;
            uiSymbolButton2.Symbol = 61703; // Backspace icon
            
            // Nút Enter
            uiSymbolButton1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            uiSymbolButton1.FillColor = Color.FromArgb(46, 125, 50);
            uiSymbolButton1.FillHoverColor = Color.FromArgb(67, 160, 71);
            uiSymbolButton1.FillPressColor = Color.FromArgb(27, 94, 32);
            uiSymbolButton1.RectColor = Color.FromArgb(46, 125, 50);
            uiSymbolButton1.ForeColor = Color.White;
            uiSymbolButton1.Radius = 12;
            uiSymbolButton1.RectSize = 0;
            uiSymbolButton1.Text = " Xác nhận";
            
            // Nút Clear
            btnClear.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClear.FillColor = Color.FromArgb(245, 124, 0);
            btnClear.FillHoverColor = Color.FromArgb(255, 149, 0);
            btnClear.FillPressColor = Color.FromArgb(230, 116, 0);
            btnClear.RectColor = Color.FromArgb(245, 124, 0);
            btnClear.ForeColor = Color.White;
            btnClear.Radius = 10;
            btnClear.RectSize = 0;
            btnClear.Symbol = 61460; // Clear icon
            
            // Nút Close
            Close.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Close.FillColor = Color.FromArgb(158, 158, 158);
            Close.FillHoverColor = Color.FromArgb(183, 28, 28);
            Close.FillPressColor = Color.FromArgb(136, 14, 79);
            Close.RectColor = Color.FromArgb(158, 158, 158);
            Close.ForeColor = Color.White;
            Close.Radius = 10;
            Close.RectSize = 0;
            Close.Symbol = 61453; // Close icon
            
            // Nút Eye Password
            btnEyePass.Font = new Font("Segoe UI", 10F);
            btnEyePass.FillColor = Color.FromArgb(158, 158, 158);
            btnEyePass.FillHoverColor = Color.FromArgb(126, 126, 126);
            btnEyePass.FillPressColor = Color.FromArgb(97, 97, 97);
            btnEyePass.RectColor = Color.FromArgb(158, 158, 158);
            btnEyePass.ForeColor = Color.White;
            btnEyePass.Radius = 8;
            btnEyePass.RectSize = 0;
            btnEyePass.Symbol = 361552; // Eye closed icon
        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            //// Gửi phím về MainForm
            TextValue = textPadTextBox.Text;
            EnterClicked?.Invoke(this, EventArgs.Empty);
            // Đóng form với kết quả OK
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Shift_Click(object sender, EventArgs e)
        {
            // Đổi trạng thái Shift
            isShiftEnabled = !isShiftEnabled;
            Shift.FillColor = isShiftEnabled ? Color.FromArgb(255, 152, 0) : Color.FromArgb(69, 90, 100);
            Shift.FillHoverColor = isShiftEnabled ? Color.FromArgb(255, 183, 77) : Color.FromArgb(96, 125, 139);
            UpdateKeyboardKeys();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            int cursorPosition = textPadTextBox.SelectionStart; // Vị trí con trỏ

            if (sender is UIButton button)
            {
                string key = button.Text;
                
                // Xử lý đặc biệt cho space button
                if (key.Contains("Space"))
                {
                    key = " ";
                }
                
                // Thêm ký tự tại vị trí con trỏ
                textPadTextBox.Text = textPadTextBox.Text.Insert(cursorPosition, key);
                textPadTextBox.Focus(); // Giữ focus cho textbox
                textPadTextBox.SelectionStart = cursorPosition + 1; // Di chuyển con trỏ
            }
        }

        private void UpdateKeyboardKeys()
        {
            A.Text = isShiftEnabled ? "A" : "a";
            B.Text = isShiftEnabled ? "B" : "b";
            C.Text = isShiftEnabled ? "C" : "c";
            D.Text = isShiftEnabled ? "D" : "d";
            E.Text = isShiftEnabled ? "E" : "e";
            F.Text = isShiftEnabled ? "F" : "f";
            G.Text = isShiftEnabled ? "G" : "g";
            H.Text = isShiftEnabled ? "H" : "h";
            I.Text = isShiftEnabled ? "I" : "i";
            J.Text = isShiftEnabled ? "J" : "j";
            K.Text = isShiftEnabled ? "K" : "k";
            L.Text = isShiftEnabled ? "L" : "l";
            M.Text = isShiftEnabled ? "M" : "m";
            N.Text = isShiftEnabled ? "N" : "n";
            O.Text = isShiftEnabled ? "O" : "o";
            P.Text = isShiftEnabled ? "P" : "p";
            Q.Text = isShiftEnabled ? "Q" : "q";
            R.Text = isShiftEnabled ? "R" : "r";
            S.Text = isShiftEnabled ? "S" : "s";
            T.Text = isShiftEnabled ? "T" : "t";
            U.Text = isShiftEnabled ? "U" : "u";
            V.Text = isShiftEnabled ? "V" : "v";
            W.Text = isShiftEnabled ? "W" : "w";
            X.Text = isShiftEnabled ? "X" : "x";
            Y.Text = isShiftEnabled ? "Y" : "y";
            Z.Text = isShiftEnabled ? "Z" : "z";

        }

        private void S123_Click(object sender, EventArgs e)
        {
            isSymbolEnabled = !isSymbolEnabled;
            S123.FillColor = isSymbolEnabled ? Color.FromArgb(255, 152, 0) : Color.FromArgb(69, 90, 100);
            S123.FillHoverColor = isSymbolEnabled ? Color.FromArgb(255, 183, 77) : Color.FromArgb(96, 125, 139);

            Q.Text = isSymbolEnabled ? "1" : "Q";
            W.Text = isSymbolEnabled ? "2" : "W";
            E.Text = isSymbolEnabled ? "3" : "E";
            R.Text = isSymbolEnabled ? "4" : "R";
            T.Text = isSymbolEnabled ? "5" : "T";
            Y.Text = isSymbolEnabled ? "6" : "Y";
            U.Text = isSymbolEnabled ? "7" : "U";
            I.Text = isSymbolEnabled ? "8" : "I";
            O.Text = isSymbolEnabled ? "9" : "O";
            P.Text = isSymbolEnabled ? "0" : "P";
            A.Text = isSymbolEnabled ? "@" : "A";
            S.Text = isSymbolEnabled ? "#" : "S";
            D.Text = isSymbolEnabled ? "$" : "D";
            F.Text = isSymbolEnabled ? "%" : "F";
            G.Text = isSymbolEnabled ? "&" : "G";
            H.Text = isSymbolEnabled ? "*" : "H";
            J.Text = isSymbolEnabled ? "(" : "J";
            K.Text = isSymbolEnabled ? ")" : "K";
            L.Text = isSymbolEnabled ? "-" : "L";
            Z.Text = isSymbolEnabled ? "+" : "Z";
            X.Text = isSymbolEnabled ? "=" : "X";
            C.Text = isSymbolEnabled ? "/" : "C";
            V.Text = isSymbolEnabled ? "\\" : "V";
            B.Text = isSymbolEnabled ? "|" : "B";
            N.Text = isSymbolEnabled ? "~" : "N";
            M.Text = isSymbolEnabled ? "<" : "M";


            Shift.Enabled = !isSymbolEnabled;
            if (!isSymbolEnabled)
            {
                isShiftEnabled = true;
                Shift.FillColor = Color.FromArgb(255, 152, 0);
            }
            else
            {
                Shift.FillColor = Color.FromArgb(158, 158, 158);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textPadTextBox.Text = "";
            textPadTextBox.Focus(); // Giữ focus cho textbox
        }

        private void Space_Click(object sender, EventArgs e)
        {
            int cursorPosition = textPadTextBox.SelectionStart; // Vị trí con trỏ
            // Thêm khoảng trắng tại vị trí con trỏ
            textPadTextBox.Text = textPadTextBox.Text.Insert(cursorPosition, " ");
            
            textPadTextBox.Focus(); // Giữ focus cho textbox
            textPadTextBox.SelectionStart = cursorPosition + 1; // Di chuyển con trỏ
        }

        private void uiSymbolButton2_Click(object sender, EventArgs e)
        {
            int cursorPosition = textPadTextBox.SelectionStart; // Vị trí con trỏ
            if (cursorPosition > 0)
            {
                // Xóa ký tự trước con trỏ
                textPadTextBox.Text = textPadTextBox.Text.Remove(cursorPosition - 1, 1);
                
                textPadTextBox.Focus(); // Giữ focus cho textbox
                textPadTextBox.SelectionStart = cursorPosition - 1; // Cập nhật lại vị trí con trỏ
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Entertext_Load(object sender, EventArgs e)
        {
            textPadTextBox.Text = TextValue;

            // Thiết lập tiêu đề của form
            uiTitlePanel1.Text = TileText;

            // Thiết lập chế độ nhập liệu
            if (IsPassword)
            {
                textPadTextBox.PasswordChar = '●'; // Hiển thị mật khẩu với ký tự đẹp hơn
                btnEyePass.Visible = true; // Hiển thị nút mắt để hiển thị/ẩn mật khẩu
            }
            else
            {
                textPadTextBox.PasswordChar = '\0'; // Hiển thị ký tự thực
                btnEyePass.Visible = false; // Ẩn nút mắt
            }
            
            // Focus vào textbox
            textPadTextBox.Focus();
        }

        private void btnEyePass_Click(object sender, EventArgs e)
        {
            if(textPadTextBox.PasswordChar == '●')
            {
                // Hiển thị mật khẩu
                textPadTextBox.PasswordChar = '\0'; // Hiển thị ký tự thực
                // Cập nhật biểu tượng nút
                btnEyePass.Symbol = 361550; // Biểu tượng mắt mở
                btnEyePass.FillColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                // Ẩn mật khẩu
                textPadTextBox.PasswordChar = '●'; // Hiển thị mật khẩu
                // Cập nhật biểu tượng nút
                btnEyePass.Symbol = 361552; // Biểu tượng mắt đóng
                btnEyePass.FillColor = Color.FromArgb(158, 158, 158);
            }
        }
    }
}
