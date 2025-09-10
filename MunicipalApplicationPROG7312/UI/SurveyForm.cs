using System;                      // EventHandler
using System.Drawing;              // Size, Point
using System.Windows.Forms;        // WinForms
using MunicipalApplicationPROG7312.Localization;

namespace MunicipalApplicationPROG7312.UI
{
    // Simple dialog to collect a 1–5 rating with text labels.
    public class SurveyForm : Form
    {
        private readonly Label _lblPrompt = new Label();              // Question text
        private readonly RadioButton _rb1 = new RadioButton();        // 1 - Very bad
        private readonly RadioButton _rb2 = new RadioButton();        // 2 - Bad
        private readonly RadioButton _rb3 = new RadioButton();        // 3 - Okay
        private readonly RadioButton _rb4 = new RadioButton();        // 4 - Good
        private readonly RadioButton _rb5 = new RadioButton();        // 5 - Excellent
        private readonly Button _btnSubmit = new Button();            // Submit
        private readonly Button _btnSkip = new Button();              // Skip

        public int SelectedScore { get; private set; }                // 1..5

        public SurveyForm()
        {
            Text = L10n.T("Survey_Title");                            // Window caption
            ClientSize = new Size(420, 220);                          // Fixed size
            StartPosition = FormStartPosition.CenterParent;           // Center on parent
            FormBorderStyle = FormBorderStyle.FixedDialog;            // Stable layout
            MaximizeBox = false; MinimizeBox = false;                 // No resize

            _lblPrompt.AutoSize = true;                               // Fit text
            _lblPrompt.Location = new Point(20, 20);                  // Position
            _lblPrompt.Text = L10n.T("Survey_Prompt");                // Localised prompt

            // Arrange radio buttons in a column.
            _rb1.Location = new Point(40, 55);
            _rb2.Location = new Point(40, 80);
            _rb3.Location = new Point(40, 105);
            _rb4.Location = new Point(40, 130);
            _rb5.Location = new Point(40, 155);

            // Localised labels e.g. "1 - Very bad"
            _rb1.Text = "1 - " + L10n.T("Very Bad");
            _rb2.Text = "2 - " + L10n.T("Bad");
            _rb3.Text = "3 - " + L10n.T("Okay");
            _rb4.Text = "4 - " + L10n.T("Good");
            _rb5.Text = "5 - " + L10n.T("Excellent");

            // Buttons
            _btnSubmit.Text = L10n.T("Survey_Submit");
            _btnSubmit.Location = new Point(280, 170);
            _btnSubmit.Enabled = false;                                // Enable after a choice
            _btnSubmit.Click += OnSubmit;

            _btnSkip.Text = L10n.T("Survey_Skip");
            _btnSkip.Location = new Point(200, 170);
            _btnSkip.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

            // Enable submit when any radio is chosen
            EventHandler onPick = delegate { _btnSubmit.Enabled = true; };
            _rb1.CheckedChanged += onPick; _rb2.CheckedChanged += onPick; _rb3.CheckedChanged += onPick;
            _rb4.CheckedChanged += onPick; _rb5.CheckedChanged += onPick;

            Controls.AddRange(new Control[] { _lblPrompt, _rb1, _rb2, _rb3, _rb4, _rb5, _btnSkip, _btnSubmit });
        }

        private void OnSubmit(object sender, EventArgs e)
        {
            // Read selected score
            if (_rb1.Checked) SelectedScore = 1;
            else if (_rb2.Checked) SelectedScore = 2;
            else if (_rb3.Checked) SelectedScore = 3;
            else if (_rb4.Checked) SelectedScore = 4;
            else if (_rb5.Checked) SelectedScore = 5;

            if (SelectedScore < 1) return;                  // Guard if nothing picked
            DialogResult = DialogResult.OK;                 // Return OK to caller
            Close();                                        // Close dialog
        }
    }
}
