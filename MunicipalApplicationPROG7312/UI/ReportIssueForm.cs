using System;                                    // EventArgs, String, etc.
using System.Collections.Generic;                // LinkedList<T>, IEnumerable<T>
using System.Drawing;                            // Point, Size, Rectangle, Color
using System.IO;                                 // Path, File
using System.Windows.Forms;                      // WinForms controls
using MunicipalApplication;                      // UiKit location (if applicable)
using MunicipalApplicationPROG7312.Persistence;          // SettingsStore (if here)
using MunicipalApplicationPROG7312.Domain;       // Issue, IssueService
using MunicipalApplicationPROG7312.Localization; // L10n


namespace MunicipalApplicationPROG7312.UI
{
    public class ReportIssueForm : Form
    {
        // Inputs
        private readonly TextBox _txtLocation = new TextBox();
        private readonly ComboBox _cmbCategory = new ComboBox();
        private readonly RichTextBox _rtbDescription = new RichTextBox();
        private readonly CheckBox _chkConsent = new CheckBox();

        // Attachments (no List/arrays)
        private readonly Button _btnAttach = new Button();
        private readonly ListBox _lstAttachments = new ListBox();
        private readonly LinkedList<string> _files = new LinkedList<string>();
        private readonly Button _btnRemove = new Button();  // remove selected attachments


        // Engagement
        private readonly ProgressBar _progress = new ProgressBar();
        private readonly Label _lblProgress = new Label();

        // Actions
        private readonly Button _btnSubmit = new Button();
        private readonly Button _btnBack = new Button();

        // Labels
        private readonly Label _lblLocation = new Label();
        private readonly Label _lblCategory = new Label();
        private readonly Label _lblDescription = new Label();
        private readonly Label _lblAttach = new Label();

        // Inline validation
        private readonly ErrorProvider _errors = new ErrorProvider();

        // Application service (store injected; no repository from the form)
        private readonly IssueService _svc = new IssueService(IssueStore.Instance);

        public ReportIssueForm()
        {
            // Base look + DPI
            UiKit.ApplyTheme(this);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = L10n.T("Btn_Report");
            ClientSize = new Size(760, 580);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // Header
            var header = UiKit.CreateHeader(L10n.T("Btn_Report"));
            Controls.Add(header);

            // Gear → Settings
            UiKit.AddSettingsGear(header, OnSettingsClicked);

            // Live settings refresh
            SettingsStore.SettingsChanged += OnSettingsChanged;
            UiKit.ApplyRuntimeTheme(this, SettingsStore.Current.Theme, SettingsStore.Current.BaseFontSize);

            // Batho Pele banner
            var bp = new Label
            {
                Text = "Principle: Information • Openness • Redress",
                AutoSize = true,
                ForeColor = UiKit.Muted,
                Location = new Point(20, header.Bottom + 4)
            };
            Controls.Add(bp);
            bp.BringToFront();

            // Layout
            var top = header.Height + 24 + 20;   // header + spacing + banner allowance

            // Left card (inputs)
            var left = UiKit.CreateCard(new Rectangle(20, top, 360, 360));
            Controls.Add(left);

            _lblLocation.Text = L10n.T("Lbl_Location");
            _lblLocation.AutoSize = true;
            _lblLocation.ForeColor = UiKit.Muted;
            _lblLocation.Location = new Point(16, 16);

            _txtLocation.Location = new Point(16, 40);
            _txtLocation.Width = 320;
            _txtLocation.MaxLength = 160; // plain, concrete cap
            UiKit.StyleInput(_txtLocation);

            _lblCategory.Text = L10n.T("Lbl_Category");
            _lblCategory.AutoSize = true;
            _lblCategory.ForeColor = UiKit.Muted;
            _lblCategory.Location = new Point(16, 82);

            _cmbCategory.Location = new Point(16, 106);
            _cmbCategory.Width = 320;
            _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            UiKit.StyleInput(_cmbCategory);
            RebuildCategories(preserveSelectedText: null); // initial fill

            _lblDescription.Text = L10n.T("Lbl_Description");
            _lblDescription.AutoSize = true;
            _lblDescription.ForeColor = UiKit.Muted;
            _lblDescription.Location = new Point(16, 148);

            _rtbDescription.Location = new Point(16, 172);
            _rtbDescription.Size = new Size(320, 140);
            UiKit.StyleInput(_rtbDescription);

            _chkConsent.Text = L10n.T("Consent_Text");
            _chkConsent.AutoSize = true;
            _chkConsent.Location = new Point(16, 320);

            left.Controls.AddRange(new Control[]
            {
                _lblLocation, _txtLocation, _lblCategory, _cmbCategory, _lblDescription, _rtbDescription, _chkConsent
            });

            // Right card (attachments + progress)
            var right = UiKit.CreateCard(new Rectangle(400, top, 340, 360));
            Controls.Add(right);

            // Attach button (left)
            _btnAttach.Text = L10n.T("Btn_AddFile");
            _btnAttach.Location = new Point(16, 44);
            _btnAttach.Width = 180;                       // a bit narrower to fit Remove next to it
            UiKit.StyleGhost(_btnAttach);
            _btnAttach.Click += OnAttachClick;

            // NEW: Remove button (right of Add)
            _btnRemove.Text = L10n.T("Btn_RemoveFile");   // localised
            _btnRemove.Location = new Point(16 + 180 + 8, 44); // to the right of Add
            _btnRemove.Width = 120;
            UiKit.StyleSecondary(_btnRemove);
            _btnRemove.Click += OnRemoveClick;

            // List shows AttachmentItem objects (selection can be multi)
            _lstAttachments.Location = new Point(16, 84);
            _lstAttachments.Size = new Size(300, 170);
            _lstAttachments.SelectionMode = SelectionMode.MultiExtended; // allow multi-select

            // After building _lstAttachments...
            _lstAttachments.SelectionMode = SelectionMode.MultiExtended;                 // allow multi-select
            EventHandler OnAttachmentSelectionChanged = null;
            _lstAttachments.SelectedIndexChanged += OnAttachmentSelectionChanged;        // toggle Remove
            KeyEventHandler OnAttachmentsKeyDown = null;
            _lstAttachments.KeyDown += OnAttachmentsKeyDown;                             // Del key removes

            // Place progress directly under the list so it can’t be “lost” behind anything
            _progress.Location = new Point(16, _lstAttachments.Bottom + 16);             // y follows list
            _progress.Size = new Size(300, 14);
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.BringToFront();                                                    // sit on top

            _lblProgress.Text = L10n.T("Hint_0");
            _lblProgress.AutoSize = true;
            _lblProgress.ForeColor = UiKit.Muted;
            _lblProgress.Location = new Point(16, _progress.Bottom + 6);                 // under bar
            _lblProgress.BringToFront();

            right.Controls.AddRange(new Control[] { _lblAttach, _btnAttach, _btnRemove, _lstAttachments, _progress, _lblProgress });


            // Batho Pele tag near progress/SLA
            var lblBpStd = new Label
            {
                Text = "Principle: Service Standards",
                AutoSize = true,
                ForeColor = UiKit.Muted,
                Location = new Point(right.Left + 16, _progress.Bottom + 20)
            };
            Controls.Add(lblBpStd);
            lblBpStd.BringToFront();

            // Footer buttons (long + centered text)
            _btnBack.Text = L10n.T("Btn_Back");
            UiKit.StyleSecondary(_btnBack);
            UiKit.SizeAndCenter(_btnBack, 280, 52);                        // wider
            _btnBack.Location = new Point(left.Left, left.Bottom + 20);
            _btnBack.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnBack.Click += delegate { Close(); };
             
            _btnSubmit.Text = "Submit"; 
            UiKit.StylePrimary(_btnSubmit); 
            UiKit.SizeAndCenter(_btnSubmit, 280, 56);                      // wider 
            _btnSubmit.Location = new Point(right.Right - _btnSubmit.Width, right.Bottom + 16);  
            _btnSubmit.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _btnSubmit.Click += OnSubmitClick;  

            Controls.Add(_btnBack);
            Controls.Add(_btnSubmit);
            _btnBack.BringToFront();
            _btnSubmit.BringToFront();

            // Keyboard shortcuts
            AcceptButton = _btnSubmit;
            CancelButton = _btnBack;
             
            // Validation triggers (inline; also drives Submit enabled state)
            _txtLocation.TextChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _cmbCategory.SelectedIndexChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _rtbDescription.TextChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _chkConsent.CheckedChanged += (_, __) => { ValidateFields(); UpdateProgress(); };

            // Initial language rebind + validation + progress 
            ApplyLanguage();
            ValidateFields();
            UpdateProgress();
        }

        // ===================== Handlers =====================

        // Holds the full path but displays only the file name in the ListBox.
      private sealed class AttachmentItem
{
    public string FullPath { get; }
    public AttachmentItem(string p) { FullPath = p; }                       // store full path
    public override string ToString() => Path.GetFileName(FullPath);        // show file name
}



        private void OnSettingsClicked(object sender, EventArgs e)
        {
            using (var dlg = new SettingsForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // SettingsChanged event will refresh the UI
                }
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            var s = SettingsStore.Current;
            UiKit.ApplyRuntimeTheme(this, s.Theme, s.BaseFontSize);  // Apply theme + base font
            ApplyLanguage();                                         // Refresh visible strings

            UiKit.SizeAndCenter(_btnBack, _btnBack.Width, _btnBack.Height);
            UiKit.SizeAndCenter(_btnSubmit, _btnSubmit.Width, _btnSubmit.Height);

            Invalidate();                                            // Redraw
        }
        private void OnAttachmentSelectionChanged(object sender, EventArgs e)
        {
            // enable Remove only when something is selected
            _btnRemove.Enabled = _lstAttachments.SelectedIndices.Count > 0;
        }

        private void OnAttachmentsKeyDown(object sender, KeyEventArgs e)
        {
            // convenience: Delete key removes selection
            if (e.KeyCode == Keys.Delete && _btnRemove.Enabled)
                OnRemoveClick(_btnRemove, EventArgs.Empty);
        }
        private void OnAttachClick(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = L10n.T("FD_Title"),
                Filter = L10n.T("FD_Filter"),
                Multiselect = true
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;           // exit if cancelled

                foreach (var path in ofd.FileNames)                             // iterate chosen files
                {
                    if (File.Exists(path) && !_files.Contains(path))            // dedup by full path
                    {
                        _files.AddLast(path);                                   // keep in LinkedList
                        _lstAttachments.Items.Add(new AttachmentItem(path));    // show friendly label
                    }
                }
                UpdateProgress();                                               // refresh step hint
            }
        }

        // Removes one or more selected attachments both from the ListBox and the LinkedList
        private void OnRemoveClick(object sender, EventArgs e)
        {
            if (_lstAttachments.SelectedIndices.Count == 0)
            {
                MessageBox.Show(L10n.T("Msg_SelectFileToRemove"), L10n.T("Title_Remove"));
                return;
            }

            // remember the last selected index so we can select the next item afterwards
            int lastIdx = _lstAttachments.SelectedIndices[_lstAttachments.SelectedIndices.Count - 1];

            // remove bottom-up so indices don’t shift
            for (int i = _lstAttachments.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int idx = _lstAttachments.SelectedIndices[i];
                var item = _lstAttachments.Items[idx] as AttachmentItem;
                if (item != null)
                {
                    _files.Remove(item.FullPath);            // remove exact path from LinkedList
                    _lstAttachments.Items.RemoveAt(idx);     // remove from UI
                }
            }

            // try select the next logical item so the button stays usable
            if (_lstAttachments.Items.Count > 0)
            {
                int next = Math.Min(lastIdx, _lstAttachments.Items.Count - 1);
                _lstAttachments.SelectedIndex = next;        // keeps focus and keeps button enabled
            }

            UpdateProgress();                                // engagement step recalculation
        }



        // Inline validation: show errors and enable/disable Submit
        private void ValidateFields()
        {
            _errors.SetError(_txtLocation, string.IsNullOrWhiteSpace(_txtLocation.Text) ? L10n.T("Err_MissingLocation") : "");
            _errors.SetError(_cmbCategory, _cmbCategory.SelectedIndex < 0 ? L10n.T("Err_MissingCategory") : "");
            _errors.SetError(_rtbDescription, string.IsNullOrWhiteSpace(_rtbDescription.Text) ? L10n.T("Err_MissingDescription") : "");
            _errors.SetError(_chkConsent, !_chkConsent.Checked ? L10n.T("Err_ConsentNeeded") : "");

            _btnSubmit.Enabled =
                string.IsNullOrEmpty(_errors.GetError(_txtLocation)) &&
                string.IsNullOrEmpty(_errors.GetError(_cmbCategory)) &&
                string.IsNullOrEmpty(_errors.GetError(_rtbDescription)) &&
                string.IsNullOrEmpty(_errors.GetError(_chkConsent));
        }

        private void UpdateProgress()
        {
            int steps = 0;
            if (!string.IsNullOrWhiteSpace(_txtLocation.Text)) steps++;
            if (_cmbCategory.SelectedIndex >= 0) steps++;
            if (!string.IsNullOrWhiteSpace(_rtbDescription.Text)) steps++;
            if (_files.Count > 0) steps++;                           // LinkedList count (no LINQ)
            if (_chkConsent.Checked) steps++;
            if (_files.First != null) steps++;

            _progress.Maximum = 6;
            _progress.Value = steps;

            string text;
            switch (steps)
            {
                case 0: text = L10n.T("Hint_0"); break;
                case 1: text = L10n.T("Hint_1"); break;
                case 2: text = L10n.T("Hint_2"); break;
                case 3: text = L10n.T("Hint_3"); break;
                default: text = L10n.T("Hint_4"); break;
            }
            _lblProgress.Text = text;
        }

        private void OnSubmitClick(object sender, EventArgs e)
        {
            // Use inline validation state; if not valid, do nothing
            ValidateFields();
            if (!_btnSubmit.Enabled) return;

            try
            {
                // Call the service; record consent text version for audit
                var id = _svc.Submit(
                    _txtLocation.Text,
                    _cmbCategory.SelectedItem?.ToString(),
                    _rtbDescription.Text,
                    _files,                                      // IEnumerable<string> from LinkedList
                    L10n.T("Consent_Text")
                );

                // Feedback (transparency)
                var message =
                    L10n.T("Submitted_Header") + "\n\n" +
                    L10n.T("Submitted_Ticket") + " " + id + "\n" +
                    L10n.T("Submitted_Status") + " " + L10n.T("Status_New") + "\n" +
                    L10n.T("Submitted_Target") + "\n\n" +
                    L10n.T("Submitted_Thanks");
                    L10n.T("Submitted Title"); // Ensure in .resx

                MessageBox.Show(message, L10n.T("Submitted_Title"));

                // Micro-survey (1–5)
                using (var survey = new SurveyForm())
                {
                    if (survey.ShowDialog(this) == DialogResult.OK)
                    {
                        // Example: Telemetry.LogRating(id, survey.SelectedScore);
                    }
                }

                Close(); // back to main
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.T("Err_SubmitGeneric") + "\n" + ex.Message, L10n.T("Title_Error"));
            }
        }

        // Centralised language rebind (called on start and when settings change)
        private void ApplyLanguage()
        {
            Text = L10n.T("Btn_Report");

            _lblLocation.Text = L10n.T("Lbl_Location");
            _lblCategory.Text = L10n.T("Lbl_Category");
            _lblDescription.Text = L10n.T("Lbl_Description");
            _lblAttach.Text = L10n.T("Lbl_Attachments");

            _btnAttach.Text = L10n.T("Btn_AddFile");
            _btnBack.Text = L10n.T("Btn_Back");

            // Rebuild categories in selected language; preserve selection by value
            var selectedText = _cmbCategory.SelectedItem?.ToString();
            RebuildCategories(selectedText);
        }

        private void RebuildCategories(string preserveSelectedText)
        {
            // Clear and add items from an IEnumerable (no AddRange/arrays)
            _cmbCategory.Items.Clear();
            foreach (var cat in L10n.Categories()) _cmbCategory.Items.Add(cat);

            if (!string.IsNullOrEmpty(preserveSelectedText))
            {
                // Try to re-select the same text if it exists
                for (int i = 0; i < _cmbCategory.Items.Count; i++)
                {
                    if (string.Equals(_cmbCategory.Items[i]?.ToString(), preserveSelectedText, StringComparison.Ordinal))
                    {
                        _cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        // Clean up event subscription
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SettingsStore.SettingsChanged -= OnSettingsChanged;
            base.OnFormClosed(e);
        }
    }
}
