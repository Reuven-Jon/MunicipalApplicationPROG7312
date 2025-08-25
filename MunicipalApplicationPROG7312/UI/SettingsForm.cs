using System;                                      // EventArgs
using System.Drawing;                              // Point, Size
using System.Windows.Forms;                        // WinForms
using MunicipalApplicationPROG7312.Persistence;            // SettingsStore
using MunicipalApplicationPROG7312.Localization;
using MunicipalApplicationPROG7312.Models;

namespace MunicipalApplicationPROG7312.UI
{
    // Modal dialog for language/theme/font size
    public class SettingsForm : Form
    {
        private readonly ComboBox _cmbLanguage = new ComboBox();               // Language selection
        private readonly ComboBox _cmbTheme = new ComboBox();                  // Theme selection
        private readonly NumericUpDown _numFont = new NumericUpDown();         // Base font size
        private readonly Button _btnOk = new Button();                         // Save
        private readonly Button _btnCancel = new Button();                     // Cancel

        public SettingsForm()
        {
            UiKit.ApplyTheme(this);                                            // Base aesthetic
            Text = "Settings";                                                 // Window title
            ClientSize = new Size(420, 220);                                   // Dialog size
            FormBorderStyle = FormBorderStyle.FixedDialog;                     // Fixed layout
            MaximizeBox = false; MinimizeBox = false;                          // No resize

            var header = UiKit.CreateHeader("Settings");                       // Header panel
            Controls.Add(header);                                              // Add header

            // Labels
            var lblLang = new Label { Text = "Language", AutoSize = true, ForeColor = UiKit.Muted, Location = new Point(20, 76) }; // Label text
            var lblTheme = new Label { Text = "Theme", AutoSize = true, ForeColor = UiKit.Muted, Location = new Point(20, 116) };  // Label text
            var lblFont = new Label { Text = "Font size", AutoSize = true, ForeColor = UiKit.Muted, Location = new Point(20, 156) }; // Label text

            // Language combobox with 4 codes
            _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;           // Force valid choice
            _cmbLanguage.Location = new Point(120, 72); _cmbLanguage.Width = 260; // Position/size
            _cmbLanguage.Items.AddRange(new object[] { "English (en)", "Afrikaans (af)", "isiXhosa (xh)", "isiZulu (zu)" }); // Options

            // Theme combobox
            _cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;              // Valid choice only
            _cmbTheme.Location = new Point(120, 112); _cmbTheme.Width = 260;   // Position/size
            _cmbTheme.Items.AddRange(new object[] { "Light", "Dark" });        // Themes

            // Font size spinner
            _numFont.Minimum = 8; _numFont.Maximum = 16;                       // Allowed range
            _numFont.Location = new Point(120, 152); _numFont.Width = 80;      // Position/size

            // Buttons
            _btnOk.Text = "Save"; UiKit.StylePrimary(_btnOk);                  // Primary action
            _btnOk.Size = new Size(115, 44); _btnOk.Location = new Point(260, 170); // Size/position
            _btnOk.Click += OnSaveClicked;                                      // Wire save

            _btnCancel.Text = "Cancel"; UiKit.StyleSecondary(_btnCancel);      // Secondary action
            _btnCancel.Size = new Size(115, 46); _btnCancel.Location = new Point(120, 170); // Size/position
            _btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); }; // Close

            Controls.AddRange(new Control[] { lblLang, _cmbLanguage, lblTheme, _cmbTheme, lblFont, _numFont, _btnCancel, _btnOk }); // Add controls

            // Load current values
            var s = SettingsStore.Current;                                      // Read current settings
            _cmbLanguage.SelectedIndex = CodeToIndex(s.LanguageCode);           // Preselect language
            _cmbTheme.SelectedIndex = s.Theme == AppTheme.Dark ? 1 : 0;         // Preselect theme
            _numFont.Value = s.BaseFontSize;                                    // Preselect font size
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            var lang = IndexToCode(_cmbLanguage.SelectedIndex);                 // Read chosen language
            var theme = _cmbTheme.SelectedIndex == 1 ? AppTheme.Dark : AppTheme.Light; // Read theme
            var size = (int)_numFont.Value;                                     // Read font size

            L10n.SetLanguage(lang);                                             // Switch L10n immediately

            var next = new AppSettings                                   // Build new settings
            {
                LanguageCode = lang,
                Theme = theme,
                BaseFontSize = size
            };
            SettingsStore.Update(next);                                         // Save + notify

            DialogResult = DialogResult.OK;                                      // Close as OK
            Close();                                                             // End dialog
        }

        private static int CodeToIndex(string code)                              // Map "en"->0, "af"->1,...
        {
            switch (code)
            {
                case "af": return 1;
                case "xh": return 2;
                case "zu": return 3;
                default: return 0;
            }
        }
        private static string IndexToCode(int i)                                 // Map index->code
        {
            switch (i)
            {
                case 1: return "af";
                case 2: return "xh";
                case 3: return "zu";
                default: return "en";
            }
        }
    }
}
