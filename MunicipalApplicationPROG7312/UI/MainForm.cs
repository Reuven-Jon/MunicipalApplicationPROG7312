using System;
using System.Drawing;
using System.Windows.Forms;
using MunicipalApplication;
using MunicipalApplicationPROG7312.Localization;
using MunicipalApplicationPROG7312.Persistance;

namespace MunicipalApplicationPROG7312.UI
{
    public class MainForm : Form
    {
        private readonly Button _btnReport = new Button();
        private readonly Button _btnEvents = new Button();
        private readonly Button _btnStatus = new Button();
        private readonly ComboBox _cmbLang = new ComboBox();
        private Control _header;

        public MainForm()
        {
            UiKit.ApplyTheme(this);

            InitializeForm();
            IssueRepository.LoadFromDisk();
            InitializeHeader();
            InitializeLanguageComboBox();
            InitializeMenuCard();
        }

        private void InitializeForm()
        {
            Text = L10n.T("Main_Title");
            ClientSize = new Size(560, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        private void InitializeHeader()
        {
            _header = UiKit.CreateHeader(L10n.T("Main_Title"));
            Controls.Add(_header);
        }

        private void InitializeLanguageComboBox()
        {
            _cmbLang.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLang.Width = 180;
            _cmbLang.Location = new Point(ClientSize.Width - _cmbLang.Width - 20, 14);
            _cmbLang.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmbLang.SelectedIndexChanged += OnLanguageChanged;
            PopulateLanguageOptions();
            _header.Controls.Add(_cmbLang);
        }

        private void PopulateLanguageOptions()
        {
            var currentCode = (_cmbLang.SelectedItem as LangOption)?.Code ?? L10n.CurrentLanguageCode;

            _cmbLang.SelectedIndexChanged -= OnLanguageChanged; // Unsubscribe

            _cmbLang.Items.Clear();
            int i = 0, selected = -1;

            foreach (var opt in L10n.LanguageOptions())
            {
                _cmbLang.Items.Add(opt);
                if (opt.Code == currentCode) selected = i;
                i++;
            }

                _cmbLang.SelectedIndex = selected >= 0 ? selected : 0;

            _cmbLang.SelectedIndexChanged += OnLanguageChanged; // Re-subscribe
        }

        private void InitializeMenuCard()
        {
            var card = UiKit.CreateCard(new Rectangle(20, 80, 520, 180));
            Controls.Add(card);

            _btnReport.Size = new Size(320, 40);
            _btnReport.Location = new Point(20, 24);
            _btnReport.Click += delegate { new ReportIssueForm().ShowDialog(this); };
            UiKit.StylePrimary(_btnReport);

            _btnEvents.Enabled = false;
            _btnEvents.Size = new Size(320, 36);
            _btnEvents.Location = new Point(20, 74);
            UiKit.StyleSecondary(_btnEvents);

            _btnStatus.Enabled = false;
            _btnStatus.Size = new Size(320, 36);
            _btnStatus.Location = new Point(20, 118);
            UiKit.StyleSecondary(_btnStatus);

            card.Controls.Add(_btnReport);
            card.Controls.Add(_btnEvents);
            card.Controls.Add(_btnStatus);

            UpdateButtonTexts();
        }

        private void UpdateButtonTexts()
        {
            _btnReport.Text = L10n.T("Btn_Report");
            _btnEvents.Text = L10n.T("Btn_Events");
            _btnStatus.Text = L10n.T("Btn_Status");
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            var opt = _cmbLang.SelectedItem as LangOption;
            if (opt != null) L10n.SetLanguage(opt.Code);

            // Refresh UI texts
            Text = L10n.T("Main_Title");
            UiKit.SetHeaderTitle(_header, L10n.T("Main_Title"));  // update the label inside header
            UpdateButtonTexts();
            PopulateLanguageOptions();                            // keep selection in sync
        }
    }
}
