using Sunny.UI;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using VNQR.Helpers;
using VNQR.Infrastructure;

namespace VNQR
{
    /// <summary>
    /// Form chọn PO de bat dau san xuat
    /// </summary>
    public class SelectPOForm : UIForm
    {
        #region Controls
        private UIPanel panel1 = null!;
        private UILabel lblTitle = null!;
        private UILabel lblSubtitle = null!;
        private ListView lvPOList = null!;
        private UILabel lblInfo = null!;
        private UIButton btnConfirm = null!;
        private UIButton btnCancel = null!;
        private UILabel lblStatus = null!;
        private UIPanel panelInfo = null!;
        private UILabel lblOrderNo = null!;
        private UILabel lblProductName = null!;
        private UILabel lblOrderQty = null!;
        private UILabel lblGtin = null!;
        private UILabel lblProductionDate = null!;
        #endregion

        private DataTable? _poList;
        private string _selectedOrderNo = "";

        public string SelectedOrderNo => _selectedOrderNo;
        public string SelectedProductName { get; private set; } = "";
        public int SelectedOrderQty { get; private set; } = 0;
        public string SelectedGtin { get; private set; } = "";

        public SelectPOForm()
        {
            InitializeComponent();
            LoadPOList();
        }

        private void InitializeComponent()
        {
            this.Text = "Chọn Đơn Hàng Sản Xuất";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RectColor = Color.FromArgb(32, 33, 96);
            this.BackColor = Color.White;

            // Title
            lblTitle = new UILabel
            {
                Text = "CHỌN ĐƠN HÀNG SẢN XUẤT",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 33, 96),
                Location = new Point(30, 25),
                AutoSize = true
            };

            lblSubtitle = new UILabel
            {
                Text = "Chọn một đơn hàng để bắt đầu quy trình sản xuất",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(30, 55),
                AutoSize = true
            };

            // List View
            lvPOList = new ListView
            {
                Location = new Point(30, 85),
                Size = new Size(640, 220),
                FullRowSelect = true,
                GridLines = true,
                View = View.Details,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lvPOList.Columns.Add("STT", 50);
            lvPOList.Columns.Add("Mã PO", 150);
            lvPOList.Columns.Add("Sản phẩm", 200);
            lvPOList.Columns.Add("Số lượng", 80);
            lvPOList.Columns.Add("Ngày SX", 100);
            lvPOList.SelectedIndexChanged += LvPOList_SelectedIndexChanged;
            lvPOList.DoubleClick += LvPOList_DoubleClick;

            // Info Panel
            panelInfo = new UIPanel
            {
                Location = new Point(30, 315),
                Size = new Size(640, 110),
                FillColor = Color.FromArgb(245, 247, 250),
                Radius = 10
            };

            lblOrderNo = new UILabel
            {
                Text = "Mã PO: -",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblProductName = new UILabel
            {
                Text = "Sản phẩm: -",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 40),
                AutoSize = true
            };

            lblOrderQty = new UILabel
            {
                Text = "Số lượng: -",
                Font = new Font("Segoe UI", 9),
                Location = new Point(350, 15),
                AutoSize = true
            };

            lblGtin = new UILabel
            {
                Text = "GTIN: -",
                Font = new Font("Segoe UI", 9),
                Location = new Point(350, 40),
                AutoSize = true
            };

            lblProductionDate = new UILabel
            {
                Text = "Ngày SX: -",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 65),
                AutoSize = true
            };

            panelInfo.Controls.Add(lblOrderNo);
            panelInfo.Controls.Add(lblProductName);
            panelInfo.Controls.Add(lblOrderQty);
            panelInfo.Controls.Add(lblGtin);
            panelInfo.Controls.Add(lblProductionDate);

            // Status
            lblStatus = new UILabel
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Location = new Point(30, 435),
                AutoSize = true
            };

            // Buttons
            btnConfirm = new UIButton
            {
                Text = "XÁC NHẬN",
                Location = new Point(350, 460),
                Size = new Size(150, 40),
                Enabled = false,
                FillColor = Color.FromArgb(32, 33, 96),
                FillHoverColor = Color.FromArgb(60, 65, 140),
                FillPressColor = Color.FromArgb(20, 25, 80)
            };
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new UIButton
            {
                Text = "HỦY",
                Location = new Point(520, 460),
                Size = new Size(150, 40),
                FillColor = Color.LightGray,
                FillHoverColor = Color.DarkGray
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle, lvPOList, panelInfo,
                lblStatus, btnConfirm, btnCancel
            });
        }

        private void LoadPOList()
        {
            try
            {
                var result = po.POLoader.GetAll();
                if (result.IsSuccess && result.Data != null)
                {
                    _poList = result.Data;
                    BindPOList(_poList);
                }
                else
                {
                    lblStatus.Text = "Không có đơn hàng nào.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Lỗi: {ex.Message}";
            }
        }

        private void BindPOList(DataTable table)
        {
            lvPOList.Items.Clear();
            int stt = 1;

            foreach (DataRow row in table.Rows)
            {
                var item = new ListViewItem(stt.ToString());
                item.SubItems.Add(row["orderNo"]?.ToString() ?? "");
                item.SubItems.Add(row["productName"]?.ToString() ?? "");
                item.SubItems.Add(row["orderQty"]?.ToString() ?? "");
                item.SubItems.Add(row["productionDate"]?.ToString() ?? "");
                item.Tag = row;
                lvPOList.Items.Add(item);
                stt++;
            }
        }

        private void LvPOList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvPOList.SelectedItems.Count > 0)
            {
                var row = lvPOList.SelectedItems[0].Tag as DataRow;
                if (row != null)
                {
                    _selectedOrderNo = row["orderNo"]?.ToString() ?? "";
                    SelectedProductName = row["productName"]?.ToString() ?? "";
                    SelectedOrderQty = Convert.ToInt32(row["orderQty"] ?? 0);
                    SelectedGtin = row["gtin"]?.ToString() ?? "";

                    lblOrderNo.Text = $"Mã PO: {_selectedOrderNo}";
                    lblProductName.Text = $"Sản phẩm: {SelectedProductName}";
                    lblOrderQty.Text = $"Số lượng: {SelectedOrderQty:N0}";
                    lblGtin.Text = $"GTIN: {SelectedGtin}";
                    lblProductionDate.Text = $"Ngày SX: {row["productionDate"]?.ToString() ?? "-"}";

                    btnConfirm.Enabled = true;
                }
            }
            else
            {
                ClearInfo();
            }
        }

        private void LvPOList_DoubleClick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedOrderNo))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void ClearInfo()
        {
            _selectedOrderNo = "";
            SelectedProductName = "";
            SelectedOrderQty = 0;
            SelectedGtin = "";

            lblOrderNo.Text = "Mã PO: -";
            lblProductName.Text = "Sản phẩm: -";
            lblOrderQty.Text = "Số lượng: -";
            lblGtin.Text = "GTIN: -";
            lblProductionDate.Text = "Ngày SX: -";

            btnConfirm.Enabled = false;
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedOrderNo))
            {
                lblStatus.Text = "Vui lòng chọn một đơn hàng.";
                return;
            }

            // Validate orderQty
            if (SelectedOrderQty <= 0)
            {
                lblStatus.Text = "Số lượng đơn hàng không hợp lệ.";
                return;
            }

            if (SelectedOrderQty <= 24)
            {
                lblStatus.Text = "Số lượng đơn hàng phải > 24.";
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
