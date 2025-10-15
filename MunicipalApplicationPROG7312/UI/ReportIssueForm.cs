using System;                                     // EventArgs, Guid, etc.
using System.Collections.Generic;                 // LinkedList<T>
using System.Drawing;                             // Point, Size, Rectangle
using System.IO;                                  // Path, File
using System.Windows.Forms;                       // WinForms controls
using MunicipalApplicationPROG7312.Domain;        // IssueStatus, IssueService
using MunicipalApplicationPROG7312.Localization;  // L10n
using MunicipalApplicationPROG7312.Persistence;   // IssueStore, SettingsStore, IssueRegistry, EventHub

// --- Type aliases to avoid 'Issue' ambiguity ---
using DomIssue = MunicipalApplicationPROG7312.Domain.Issue;
using DomStatus = MunicipalApplicationPROG7312.Domain.IssueStatus;

namespace MunicipalApplicationPROG7312.UI
{
    /// <summary>
    /// Report flow: location, category, description, attachments (add/remove),
    /// POPIA consent, progress hints, submit + micro-survey.
    /// Uses LinkedList for attachments and IssueStore for data (no Lists/arrays for rubric).
    /// Also updates central Dictionary/Queue/Stack (IssueRegistry) and exposes:
    /// - Undo Last Reported (Stack pop)
    /// - Assign Next Pending (Queue dequeue)
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

        // ---------- Actions (footer) ----------
        private readonly Button _btnSubmit = new Button();
        private readonly Button _btnBack = new Button();
        private readonly Button _btnUndoLast = new Button();       // Stack pop
        private readonly Button _btnAssignNext = new Button();     // Queue dequeue

        // Footer layout container
        private readonly TableLayoutPanel _footer = new TableLayoutPanel();

        // ---------- Labels ----------
        private readonly Label _lblLocation = new Label();
        private readonly Label _lblCategory = new Label();
        private readonly Label _lblDescription = new Label();
        private readonly Label _lblAttach = new Label();
        private readonly PictureBox _logo = new PictureBox();


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
            UiKit.ApplyTheme(this);
            Text = L10n.T("Btn_Report");
            ClientSize = new Size(760, 640);
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

            // ---------- Footer (2x2 grid: Back | Submit  ;  Assign Next | Undo Last) ----------
            _footer.ColumnCount = 2;
            _footer.RowCount = 2;
            _footer.ColumnStyles.Clear();
            _footer.RowStyles.Clear();
            _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            _footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            _footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            _footer.Padding = new Padding(16, 8, 16, 16);
            _footer.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            _footer.Size = new Size(ClientSize.Width - 40, 56 * 2 + _footer.Padding.Vertical + 8);
            _footer.Location = new Point(20, right.Bottom + 20);
            _footer.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Configure buttons and add to grid
            // Row 0
            _btnBack.Text = L10n.T("Btn_Back");
            UiKit.StyleSecondary(_btnBack);
            _btnBack.Dock = DockStyle.Fill;
            _btnBack.Click += (_, __) => Close();

            _btnSubmit.Text = "Submit";
            UiKit.StylePrimary(_btnSubmit);
            _btnSubmit.Dock = DockStyle.Fill;
            _btnSubmit.Click += OnSubmitClick;

            // Row 1
            _btnAssignNext.Text = "Assign Next Pending";
            UiKit.StyleSecondary(_btnAssignNext);
            _btnAssignNext.Dock = DockStyle.Fill;
            _btnAssignNext.Click += OnAssignNextClick;

            _btnUndoLast.Text = "Undo Last Reported";
            UiKit.StyleGhost(_btnUndoLast);
            _btnUndoLast.Dock = DockStyle.Fill;
            _btnUndoLast.Click += OnUndoLastClick;

            // Add to table
            _footer.Controls.Add(_btnBack, 0, 0);
            _footer.Controls.Add(_btnSubmit, 1, 0);
            _footer.Controls.Add(_btnAssignNext, 0, 1);
            _footer.Controls.Add(_btnUndoLast, 1, 1);

            Controls.Add(_footer);

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

            // Enable/disable rubric buttons based on current registry state
            UpdateRubricButtonsEnabled();
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

        // ===== Helper: resolve Issue from store by Id without LINQ =====
        private DomIssue? ResolveIssue(Guid id)
        {
            foreach (var it in IssueStore.Instance.All())
                if (it.Id == id) return it;
            return null;
        }

        // ===== Helper: set status safely without assuming enum names =====
        private void SetStatusSafely(ref DomIssue issue, params string[] desiredNames)
        {
            // Try desired names
            for (int i = 0; i < desiredNames.Length; i++)
            {
                if (Enum.TryParse<DomStatus>(desiredNames[i], ignoreCase: true, out var parsed))
                {
                    issue.Status = parsed;
                    return;
                }
            }
            // Fallbacks
            string[] fallbacks = { "InProgress", "Open", "Pending", "New", "Closed", "Cancelled", "Canceled" };
            for (int i = 0; i < fallbacks.Length; i++)
            {
                if (Enum.TryParse<DomStatus>(fallbacks[i], ignoreCase: true, out var parsed2))
                {
                    issue.Status = parsed2;
                    return;
                }
            }
        }

        // ===== Helper: build a caption without assuming a Title property =====
        private string GetIssueCaption(DomIssue issue)
        {
            var t = issue.GetType();

            // Try common names
            string[] names = { "Title", "Summary", "Subject", "Name" };
            for (int i = 0; i < names.Length; i++)
            {
                var p = t.GetProperty(names[i]);
                if (p != null && p.PropertyType == typeof(string))
                {
                    var val = p.GetValue(issue) as string;
                    if (!string.IsNullOrWhiteSpace(val)) return val!;
                }
            }

            // Fallback: Description
            var descProp = t.GetProperty("Description");
            if (descProp != null && descProp.PropertyType == typeof(string))
            {
                var desc = descProp.GetValue(issue) as string;
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    if (desc!.Length > 60) desc = desc.Substring(0, 60) + "…";
                    return desc!;
                }
            }

            // Fallback: Category
            var catProp = t.GetProperty("Category");
            if (catProp != null && catProp.PropertyType == typeof(string))
            {
                var cat = catProp.GetValue(issue) as string;
                if (!string.IsNullOrWhiteSpace(cat)) return $"({cat})";
            }

            // Final fallback: ID
            return issue.Id.ToString();
        }

        // ========== Submit ==========
        private void OnSubmitClick(object sender, EventArgs e)
        {
            // Validate first; bail if not valid
            ValidateFields();
            if (!_btnSubmit.Enabled) return;

            try
            {
                // Submit once
                var id = _svc.Submit(
                    _txtLocation.Text,
                    _cmbCategory.SelectedItem?.ToString(),
                    _rtbDescription.Text,
                    _files,                             // LinkedList<string> OK (IEnumerable<string>)
                    L10n.T("Consent_Text")
                );

                // ---- Update Dictionary, Queue, Stack (rubric) ----
                var created = ResolveIssue(id);               // no LINQ; scans IssueStore
                if (created != null)
                {
                    var reg = IssueRegistry.Instance;
                    reg.Index[created.Id] = created;          // Dictionary: O(1)
                    reg.Pending.Enqueue(created.Id);          // Queue: pending FIFO
                    reg.RecentlyReported.Push(created.Id);    // Stack: LIFO for “recently reported”
                    EventHub.PublishIssueReported(created);   // optional notify other forms
                }

                // ---- Existing success UI remains unchanged ----
                var message =
                    L10n.T("Submitted_Header") + "\n\n" +
                    L10n.T("Submitted_Ticket") + " " + id + "\n" +
                    L10n.T("Submitted_Status") + " " + L10n.T("Status_New") + "\n" +
                    L10n.T("Submitted_Target") + "\n\n" +
                    L10n.T("Submitted_Thanks");

                MessageBox.Show(message, L10n.T("Submitted_Title"));

                using (var survey = new SurveyForm())
                {
                    if (survey.ShowDialog(this) == DialogResult.OK)
                    {
                        // Telemetry.LogRating(id, survey.SelectedScore);
                    }
                }

                // After a successful submit, buttons may change state for the session
                UpdateRubricButtonsEnabled();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.T("Err_SubmitGeneric") + "\n" + ex.Message, L10n.T("Title_Error"));
            }
        }

        // ========== NEW: Assign Next Pending (Queue) ==========
        private void OnAssignNextClick(object sender, EventArgs e)
        {
            var reg = IssueRegistry.Instance;
            if (reg.Pending.Count == 0)
            {
                MessageBox.Show("No pending issues in the queue.", "Assign", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateRubricButtonsEnabled();
                return;
            }

            var nextId = reg.Pending.Dequeue();
            var issue = ResolveIssue(nextId);
            if (issue == null)
            {
                reg.Index.Remove(nextId);
                MessageBox.Show("The next pending issue could not be found.", "Assign", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateRubricButtonsEnabled();
                return;
            }

            SetStatusSafely(ref issue, "Assigned");

            MessageBox.Show($"Assigned next pending issue:\n\n{GetIssueCaption(issue)}\n(ID: {issue.Id})",
                "Assigned", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateRubricButtonsEnabled();
        }

        // ========== NEW: Undo Last Reported (Stack) ==========
        private void OnUndoLastClick(object sender, EventArgs e)
        {
            var reg = IssueRegistry.Instance;
            if (reg.RecentlyReported.Count == 0)
            {
                MessageBox.Show("No recently reported issues to undo.", "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateRubricButtonsEnabled();
                return;
            }

            var lastId = reg.RecentlyReported.Pop();

            // Remove this ID from the pending queue if still present (Queue<T> has no direct remove)
            var temp = new Queue<Guid>();
            while (reg.Pending.Count > 0)
            {
                var qid = reg.Pending.Dequeue();
                if (qid != lastId) temp.Enqueue(qid);
            }
            while (temp.Count > 0) reg.Pending.Enqueue(temp.Dequeue());

            // Remove from dictionary
            reg.Index.Remove(lastId);

            // Also mark the in-store issue with a withdrawn-like status
            var issue = ResolveIssue(lastId);
            if (issue != null)
            {
                SetStatusSafely(ref issue, "Withdrawn", "Cancelled", "Canceled", "Closed");
                MessageBox.Show($"Undid last reported issue:\n\n{GetIssueCaption(issue)}\n(ID: {issue.Id})",
                    "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The last reported issue was not found in the store. Registry entries have been cleaned.",
                    "Undo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateRubricButtonsEnabled();
        }

        // Enable/disable the two rubric buttons according to registry state
        private void UpdateRubricButtonsEnabled()
        {
            var reg = IssueRegistry.Instance;
            _btnAssignNext.Enabled = reg.Pending.Count > 0;
            _btnUndoLast.Enabled = reg.RecentlyReported.Count > 0;
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

            // Rebuild categories (preserve current selection text if any)
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
