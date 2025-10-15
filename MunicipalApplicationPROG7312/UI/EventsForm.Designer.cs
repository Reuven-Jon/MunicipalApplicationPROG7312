using System;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalApplicationPROG7312.UI
{
    // NOTE: This must share the same namespace + class name as your EventsForm.cs
    partial class EventsForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controls used in EventsForm.cs
        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private Button btnSearch;
        private Label lblUrgent;
        private Label lblTip;
        private ListBox lstRecommended;
        private CheckBox chkDate;
        private DateTimePicker dtpDate;
        private Button btnBack;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates all controls referenced by EventsForm.cs and wires events.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // --- form ---
            this.Text = "Local Events & Announcements";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(780, 630);

            // --- grid ---
            this.grid = new DataGridView();
            this.grid.Location = new Point(24, 120);
            this.grid.Size = new Size(720, 280);
            this.grid.ReadOnly = true;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // --- txtSearch ---
            this.txtSearch = new TextBox();
            this.txtSearch.Location = new Point(24, 24);
            this.txtSearch.Width = 200;
            this.txtSearch.PlaceholderText = "Search keywords...";

            // --- cmbCategory ---
            this.cmbCategory = new ComboBox();
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategory.Location = new Point(240, 24);
            this.cmbCategory.Width = 150;

            // --- chkDate + dtpDate ---
            this.chkDate = new CheckBox();
            this.chkDate.Text = "On date";
            this.chkDate.Location = new Point(410, 24);
            this.chkDate.CheckedChanged += new EventHandler(this.chkDate_CheckedChanged);

            this.dtpDate = new DateTimePicker();
            this.dtpDate.Location = new Point(480, 24);
            this.dtpDate.Width = 200;
            this.dtpDate.Enabled = false;

            // --- btnSearch ---
            this.btnSearch = new Button();
            this.btnSearch.Text = "Search";
            this.btnSearch.Location = new Point(690, 24);
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);

            // --- lblUrgent ---
            this.lblUrgent = new Label();
            this.lblUrgent.AutoSize = true;
            this.lblUrgent.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblUrgent.ForeColor = Color.Maroon;
            this.lblUrgent.Location = new Point(24, 80);
            this.lblUrgent.Text = string.Empty;

            // --- lblTip ---
            this.lblTip = new Label();
            this.lblTip.AutoSize = true;
            this.lblTip.Location = new Point(24, 410);
            this.lblTip.Text = string.Empty;

            // --- lstRecommended ---
            this.lstRecommended = new ListBox();
            this.lstRecommended.Location = new Point(24, 440);
            this.lstRecommended.Size = new Size(720, 120);

            // --- btnBack ---
            this.btnBack = new Button();
            this.btnBack.Text = "Back";
            this.btnBack.Location = new Point(24, 580);
            this.btnBack.Click += new EventHandler(this.btnBack_Click);

            // --- add controls ---
            this.Controls.Add(this.grid);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.chkDate);
            this.Controls.Add(this.dtpDate);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lblUrgent);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.lstRecommended);
            this.Controls.Add(this.btnBack);
        }
    }
}
