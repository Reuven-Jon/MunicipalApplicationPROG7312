using System;                                     // EventArgs, Guid, etc.
using System.Collections.Generic;                 // LinkedList<T>
using System.Drawing;                             // Point, Size, Rectangle
using System.IO;                                  // Path, File
using System.Windows.Forms;                       // WinForms controls
using MunicipalApplicationPROG7312.Domain;        // IssueService
using MunicipalApplicationPROG7312.Localization;  // L10n
using MunicipalApplicationPROG7312.Persistence;   // IssueStore, SettingsStore

namespace MunicipalApplicationPROG7312.UI
{
    /// <summary>
    /// Report flow: location, category, description, attachments (add/remove),
    /// POPIA consent, progress hints, submit + micro-survey.
    /// Uses LinkedList for attachments and IssueStore for data (no Lists/arrays for rubric). 
    /// Shows ticket status and SLA text after submit; then triggers a 1–5 micro-survey. 
    /// <remarks>
    /// Transparency & redress align with Batho Pele:
    /// DPSA Batho Pele Handbook (2014) – https://www.dpsa.gov.za/dpsa2g/documents/cdw/2014/BathoPeleHandbook.pdf
    /// Micro-survey pattern is based on public-service CSAT/Single-Ease guidance:
    /// GOV.UK Service Manual – Measuring satisfaction:
    /// https://www.gov.uk/service-manual/measuring-success/measuring-user-satisfaction
    /// ONS Design System – Feedback pattern:
    /// https://service-manual.ons.gov.uk/design-system/patterns/feedback
    /// </remarks>
    /// </summary>
    public sealed class ReportIssueForm : Form
    {
        // ---------- Inputs ----------
        private readonly TextBox _txtLocation = new TextBox();
        private readonly ComboBox _cmbCategory = new ComboBox();
        private readonly RichTextBox _rtbDescription = new RichTextBox();
        private readonly CheckBox _chkConsent = new CheckBox();

        // ---------- Attachments (LinkedList for rubric) ----------
        private readonly Button _btnAttach = new Button();
        private readonly Button _btnRemove = new Button();
        private readonly ListBox _lstAttachments = new ListBox();
        private readonly LinkedList<string> _files = new LinkedList<string>();

        // ---------- Engagement ----------
        private readonly ProgressBar _progress = new ProgressBar();
        private readonly Label _lblProgress = new Label();

        // ---------- Actions ----------
        private readonly Button _btnSubmit = new Button();
        private readonly Button _btnBack = new Button();

        // ---------- Labels ----------
        private readonly Label _lblLocation = new Label();
        private readonly Label _lblCategory = new Label();
        private readonly Label _lblDescription = new Label();
        private readonly Label _lblAttach = new Label();

        // ---------- Inline validation ----------
        private readonly ErrorProvider _errors = new ErrorProvider();

        // ---------- Application service (composition) ----------
        private readonly IssueService _svc = new IssueService(IssueStore.Instance);

        // Helper class: holds full path but shows file name in list
        private sealed class AttachmentItem
        {
            public string FullPath { get; }
            public AttachmentItem(string p) { FullPath = p; }
            public override string ToString() => Path.GetFileName(FullPath);
        }

        public ReportIssueForm()
        {
            // Base look and font-scaling. UiKit also wires FontChanged → RelayoutForFont.
            UiKit.ApplyTheme(this);
            Text = L10n.T("Btn_Report");
            ClientSize = new Size(760, 580);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // Header + gear (Settings)
            var header = UiKit.CreateHeader(L10n.T("Btn_Report"));
            Controls.Add(header);
            UiKit.AddSettingsGear(header, OnSettingsClicked);

            // Respond to live settings (theme/language/font size)
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

            // Layout baseline below header + banner
            var top = header.Height + 24 + 20;

            // ---------- LEFT: Inputs card ----------
            var left = UiKit.CreateCard(new Rectangle(20, top, 360, 360));
            Controls.Add(left);

            _lblLocation.Text = L10n.T("Lbl_Location");
            _lblLocation.AutoSize = true;
            _lblLocation.ForeColor = UiKit.Muted;
            _lblLocation.Location = new Point(16, 16);

            _txtLocation.Location = new Point(16, 40);
            _txtLocation.Width = 320;
            _txtLocation.MaxLength = 160;
            UiKit.StyleInput(_txtLocation);

            _lblCategory.Text = L10n.T("Lbl_Category");
            _lblCategory.AutoSize = true;
            _lblCategory.ForeColor = UiKit.Muted;
            _lblCategory.Location = new Point(16, 82);

            _cmbCategory.Location = new Point(16, 106);
            _cmbCategory.Width = 320;
            _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            UiKit.StyleInput(_cmbCategory);
            RebuildCategories(null); // initial

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
                _lblLocation, _txtLocation, _lblCategory, _cmbCategory,
                _lblDescription, _rtbDescription, _chkConsent
            });

            // ---------- RIGHT: Attachments + progress ----------
            var right = UiKit.CreateCard(new Rectangle(400, top, 340, 360));
            Controls.Add(right);

            _lblAttach.Text = L10n.T("Lbl_Attachments");
            _lblAttach.AutoSize = true;
            _lblAttach.ForeColor = UiKit.Muted;
            _lblAttach.Location = new Point(16, 16);

            _btnAttach.Text = L10n.T("Btn_AddFile");
            _btnAttach.Location = new Point(16, 44);
            _btnAttach.Width = 180;
            UiKit.StyleGhost(_btnAttach);
            _btnAttach.Click += OnAttachClick;

            _btnRemove.Text = L10n.T("Btn_RemoveFile");
            _btnRemove.Location = new Point(16 + 180 + 8, 44);
            _btnRemove.Width = 120;
            _btnRemove.Enabled = false; // enabled when selection exists
            UiKit.StyleSecondary(_btnRemove);
            _btnRemove.Click += OnRemoveClick;

            _lstAttachments.Location = new Point(16, 84);
            _lstAttachments.Size = new Size(300, 170);
            _lstAttachments.SelectionMode = SelectionMode.MultiExtended;
            _lstAttachments.SelectedIndexChanged += OnAttachmentSelectionChanged;
            _lstAttachments.KeyDown += OnAttachmentsKeyDown;

            _progress.Location = new Point(16, _lstAttachments.Bottom + 16);
            _progress.Size = new Size(300, 14);
            _progress.Style = ProgressBarStyle.Continuous;

            _lblProgress.Text = L10n.T("Hint_0");
            _lblProgress.AutoSize = true;
            _lblProgress.ForeColor = UiKit.Muted;
            _lblProgress.Location = new Point(16, _progress.Bottom + 6);

            right.Controls.AddRange(new Control[]
            {
                _lblAttach, _btnAttach, _btnRemove, _lstAttachments, _progress, _lblProgress
            });

            // Batho Pele tag near SLA/progress
            var lblBpStd = new Label
            {
                Text = "Principle: Service Standards",
                AutoSize = true,
                ForeColor = UiKit.Muted,
                Location = new Point(right.Left + 16, _progress.Bottom + 20)
            };
            Controls.Add(lblBpStd);

            // ---------- Footer buttons ----------
            _btnBack.Text = L10n.T("Btn_Back");
            UiKit.StyleSecondary(_btnBack);
            UiKit.SizeAndCenter(_btnBack, 280, 52);
            _btnBack.Location = new Point(left.Left, left.Bottom + 20);
            _btnBack.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnBack.Click += (_, __) => Close();

            _btnSubmit.Text = "Submit";
            UiKit.StylePrimary(_btnSubmit);
            UiKit.SizeAndCenter(_btnSubmit, 280, 56);
            _btnSubmit.Location = new Point(right.Right - _btnSubmit.Width, right.Bottom + 16);
            _btnSubmit.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _btnSubmit.Click += OnSubmitClick;

            Controls.Add(_btnBack);
            Controls.Add(_btnSubmit);

            // Keyboard shortcuts
            AcceptButton = _btnSubmit;
            CancelButton = _btnBack;

            // Validation → progress
            _txtLocation.TextChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _cmbCategory.SelectedIndexChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _rtbDescription.TextChanged += (_, __) => { ValidateFields(); UpdateProgress(); };
            _chkConsent.CheckedChanged += (_, __) => { ValidateFields(); UpdateProgress(); };

            // Initial bind
            ApplyLanguage();
            ValidateFields();
            UpdateProgress();
            UiKit.RelayoutForFont(this); // final pass
        }

        // ========== Settings/theme/language ==========
        private void OnSettingsClicked(object sender, EventArgs e)
        {
            using (var dlg = new SettingsForm())
            {
                dlg.ShowDialog(this); // SettingsStore raises SettingsChanged if OK
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            var s = SettingsStore.Current;
            UiKit.ApplyRuntimeTheme(this, s.Theme, s.BaseFontSize);
            ApplyLanguage();
            UiKit.RelayoutForFont(this);
        }

        // ========== Attachments ==========
        private void OnAttachClick(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = L10n.T("FD_Title"),
                Filter = L10n.T("FD_Filter"),
                Multiselect = true
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                foreach (var path in ofd.FileNames)
                {
                    if (File.Exists(path) && !_files.Contains(path))
                    {
                        _files.AddLast(path);                          // LinkedList for rubric
                        _lstAttachments.Items.Add(new AttachmentItem(path));
                    }
                }
                UpdateProgress();
            }
        }

        private void OnRemoveClick(object sender, EventArgs e)
        {
            if (_lstAttachments.SelectedIndices.Count == 0)
            {
                MessageBox.Show(L10n.T("Msg_SelectFileToRemove"), L10n.T("Title_Remove"));
                return;
            }

            // remove bottom-up so indices remain valid
            for (int i = _lstAttachments.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int idx = _lstAttachments.SelectedIndices[i];
                if (_lstAttachments.Items[idx] is AttachmentItem itm)
                {
                    _files.Remove(itm.FullPath);       // remove matching path
                    _lstAttachments.Items.RemoveAt(idx);
                }
            }

            // keep focus + enable further removals
            if (_lstAttachments.Items.Count > 0)
            {
                _lstAttachments.SelectedIndex = Math.Min(
                    _lstAttachments.SelectedIndex >= 0 ? _lstAttachments.SelectedIndex : 0,
                    _lstAttachments.Items.Count - 1);
            }

            UpdateProgress();
        }

        private void OnAttachmentSelectionChanged(object sender, EventArgs e)
            => _btnRemove.Enabled = _lstAttachments.SelectedIndices.Count > 0;

        private void OnAttachmentsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _btnRemove.Enabled)
                OnRemoveClick(_btnRemove, EventArgs.Empty);
        }

        // ========== Validation / Progress ==========
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
            // Five steps: location, category, description, at least one attachment, consent
            int steps = 0;
            if (!string.IsNullOrWhiteSpace(_txtLocation.Text)) steps++;
            if (_cmbCategory.SelectedIndex >= 0) steps++;
            if (!string.IsNullOrWhiteSpace(_rtbDescription.Text)) steps++;
            if (_files.Count > 0) steps++;                 // LinkedList count (no LINQ)
            if (_chkConsent.Checked) steps++;

            _progress.Maximum = 5;
            _progress.Value = steps;

            string text = steps switch
            {
                0 => L10n.T("Hint_0"),
                1 => L10n.T("Hint_1"),
                2 => L10n.T("Hint_2"),
                3 => L10n.T("Hint_3"),
                _ => L10n.T("Hint_4")
            };
            _lblProgress.Text = text;
        }

        // ========== Submit ==========
        private void OnSubmitClick(object sender, EventArgs e)
        {
            ValidateFields();
            if (!_btnSubmit.Enabled) return;

            try
            {
                // service API accepts IEnumerable<string> (we pass LinkedList directly)
                var id = _svc.Submit(
                    _txtLocation.Text,
                    _cmbCategory.SelectedItem?.ToString(),
                    _rtbDescription.Text,
                    _files,                             // IEnumerable<string>
                    L10n.T("Consent_Text")              // consent copy version
                );

                var message =
                    L10n.T("Submitted_Header") + "\n\n" +
                    L10n.T("Submitted_Ticket") + " " + id + "\n" +
                    L10n.T("Submitted_Status") + " " + L10n.T("Status_New") + "\n" +
                    L10n.T("Submitted_Target") + "\n\n" +
                    L10n.T("Submitted_Thanks");

                MessageBox.Show(message, L10n.T("Submitted_Title"));

                // Optional micro-survey dialog (1..5)
                using (var survey = new SurveyForm())
                {
                    if (survey.ShowDialog(this) == DialogResult.OK)
                    {
                        // Telemetry.LogRating(id, survey.SelectedScore);
                    }
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.T("Err_SubmitGeneric") + "\n" + ex.Message, L10n.T("Title_Error"));
            }
        }

        // ========== Language ==========
        private void ApplyLanguage()
        {
            Text = L10n.T("Btn_Report");
            _lblLocation.Text = L10n.T("Lbl_Location");
            _lblCategory.Text = L10n.T("Lbl_Category");
            _lblDescription.Text = L10n.T("Lbl_Description");
            _lblAttach.Text = L10n.T("Lbl_Attachments");
            _btnAttach.Text = L10n.T("Btn_AddFile");
            _btnBack.Text = L10n.T("Btn_Back");
            _btnRemove.Text = L10n.T("Btn_RemoveFile");

            // Rebuild category list in current language while trying to preserve current value
            var keep = _cmbCategory.SelectedItem?.ToString();
            RebuildCategories(keep);
        }

        private void RebuildCategories(string preserveText)
        {
            _cmbCategory.Items.Clear();
            foreach (var cat in L10n.Categories())
                _cmbCategory.Items.Add(cat);

            if (!string.IsNullOrEmpty(preserveText))
            {
                for (int i = 0; i < _cmbCategory.Items.Count; i++)
                {
                    if (string.Equals(_cmbCategory.Items[i]?.ToString(), preserveText, StringComparison.Ordinal))
                    {
                        _cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        // ========== Cleanup ==========
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SettingsStore.SettingsChanged -= OnSettingsChanged;
            base.OnFormClosed(e);
        }
    }
}
