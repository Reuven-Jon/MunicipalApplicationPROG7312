using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MunicipalApplication;                          // UiKit (your theming helpers)
using MunicipalApplicationPROG7312.Domain;           // LocalEvent, EventIndex, RecommendationEngine
using MunicipalApplicationPROG7312.Persistance;      // (queues you already pass in)
using MunicipalApplicationPROG7312.Localization;     // L10n

namespace MunicipalApplicationPROG7312.UI
{
    public sealed partial class EventsForm : Form

    {
        // ====== Inputs / Filters ======
        private readonly TextBox _txtSearch = new TextBox();
        private readonly CheckBox _chkDate = new CheckBox();
        private readonly DateTimePicker _dtp = new DateTimePicker();
        private readonly Button _btnSearch = new Button();
        private readonly FlowLayoutPanel _flpFilters = new FlowLayoutPanel();  // category chips

        // ====== Urgent banner ======
        private readonly Panel _banner = new Panel();
        private readonly Label _lblUrgent = new Label();

        // ====== Grid ======
        private readonly DataGridView _grid = new DataGridView();

        // ====== Recommendations ======
        private readonly Panel _recoCard = new Panel();
        private readonly Label _recoTitle = new Label();
        private readonly ListBox _lstReco = new ListBox();

        // ====== Footer feed (LinkedList) ======
        private readonly Label _lblFeed = new Label();
        private readonly ListBox _lstFeed = new ListBox();

        // ====== Data / Services provided by MainForm ======
        private readonly InMemoryEventStore _store;
        private readonly EventIndex _index;                    // you already have this
        private readonly RecommendationEngine _engine;         // you already have this
        private readonly UrgentAnnouncementQueue<LocalEvent> _urgent;  // priority queue (lower int => higher)
        private readonly NotificationQueue _tips;              // queue for messages
        private readonly NavigationHistory<Form> _nav;

        // ====== Rubric Data Structures (explicit) ======
        private readonly Dictionary<int, LocalEvent> _byId = new Dictionary<int, LocalEvent>(); // Dictionary
        private readonly HashSet<string> _allCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Set
        private readonly HashSet<string> _selectedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Set
        private readonly Stack<int> _recentViewed = new Stack<int>();      // Stack of event Ids
        private readonly LinkedList<string> _feed = new LinkedList<string>(); // LinkedList rolling feed
        private readonly Stack<string> _recentSearches = new Stack<string>(); // Stack of last searches
        private readonly Dictionary<string, int> _termFreq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // Dictionary for recommendation

        public EventsForm(
            InMemoryEventStore store,
            EventIndex index,
            RecommendationEngine engine,
            UrgentAnnouncementQueue<LocalEvent> urgent,
            NotificationQueue tips,
            NavigationHistory<Form> nav)
        {
            _store = store;
            _index = index;
            _engine = engine;
            _urgent = urgent;
            _tips = tips;
            _nav = nav;

            UiKit.ApplyTheme(this);
            Text = L10n.T("Btn_Events");
            ClientSize = new Size(860, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            BuildHeader();
            BuildBanner();
            BuildFilters();
            BuildGrid();
            BuildReco();
            BuildFooter();

            // initial data bind
            BuildIndexesAndSets();
            RenderUrgentBanner();
            RefreshGrid();
            RefreshRecommendations();
            DrainTipsToFeed();
        }

        // =======================
        // UI BUILDERS
        // =======================
        private void BuildHeader()
        {
            var header = UiKit.CreateHeader(L10n.T("Btn_Events"));
            Controls.Add(header);

            // Search bar
            _txtSearch.PlaceholderText = "Search keywords...";
            _txtSearch.Width = 260;
            _txtSearch.Location = new Point(16, header.Bottom + 10);
            UiKit.StyleInput(_txtSearch);
            Controls.Add(_txtSearch);

            _chkDate.Text = "On date";
            _chkDate.AutoSize = true;
            _chkDate.Location = new Point(_txtSearch.Right + 12, _txtSearch.Top + 4);
            Controls.Add(_chkDate);

            _dtp.Format = DateTimePickerFormat.Long;
            _dtp.Width = 220;
            _dtp.Location = new Point(_chkDate.Right + 12, _txtSearch.Top - 2);
            _dtp.Enabled = false;
            Controls.Add(_dtp);

            _chkDate.CheckedChanged += (_, __) => { _dtp.Enabled = _chkDate.Checked; };

            _btnSearch.Text = "Search";
            UiKit.StylePrimary(_btnSearch);
            _btnSearch.Size = new Size(110, 34);
            _btnSearch.Location = new Point(_dtp.Right + 12, _txtSearch.Top - 2);
            _btnSearch.Click += (_, __) => DoSearch();
            Controls.Add(_btnSearch);
        }

        private void BuildBanner()
        {
            _banner.Height = 40;
            _banner.Width = ClientSize.Width - 32;
            _banner.Location = new Point(16, _txtSearch.Bottom + 8);
            _banner.BackColor = Color.FromArgb(255, 245, 227); // soft orange
            _banner.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _banner.Padding = new Padding(10, 10, 10, 6);

            _lblUrgent.AutoSize = true;
            _lblUrgent.Font = new Font("Segoe UI Semibold", 10.5f);
            _lblUrgent.ForeColor = Color.FromArgb(160, 60, 0);

            _banner.Controls.Add(_lblUrgent);
            Controls.Add(_banner);
        }

        private void BuildFilters()
        {
            // Chips-like category selectors
            _flpFilters.Location = new Point(16, _banner.Bottom + 6);
            _flpFilters.Size = new Size(ClientSize.Width - 32, 32);
            _flpFilters.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _flpFilters.AutoSize = false;
            _flpFilters.WrapContents = false;
            _flpFilters.AutoScroll = true;
            Controls.Add(_flpFilters);
        }

        private void BuildGrid()
        {
            _grid.Location = new Point(16, _flpFilters.Bottom + 6);
            _grid.Size = new Size(ClientSize.Width - 32, 300);
            _grid.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _grid.ReadOnly = true;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AllowUserToAddRows = false;
            _grid.RowHeadersVisible = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ShowDetail(e.RowIndex); };
            _grid.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && _grid.CurrentCell != null) { ShowDetail(_grid.CurrentCell.RowIndex); e.Handled = true; } };

            Controls.Add(_grid);
        }

        private void BuildReco()
        {
            _recoCard.Location = new Point(16, _grid.Bottom + 10);
            _recoCard.Size = new Size(ClientSize.Width - 32, 120);
            _recoCard.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _recoCard.BackColor = Color.White;
            _recoCard.Padding = new Padding(12);
            _recoCard.BorderStyle = BorderStyle.FixedSingle;

            _recoTitle.Text = "Recommended for you";
            _recoTitle.AutoSize = true;
            _recoTitle.Font = new Font("Segoe UI Semibold", 10.5f);
            _recoTitle.ForeColor = UiKit.Text;

            _lstReco.Location = new Point(12, 36);
            _lstReco.Size = new Size(_recoCard.Width - 24, 70);
            _lstReco.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _lstReco.DoubleClick += (_, __) => { if (_lstReco.SelectedItem is LocalEvent ev) OpenById(ev.Id); };

            _recoCard.Controls.Add(_recoTitle);
            _recoCard.Controls.Add(_lstReco);
            Controls.Add(_recoCard);
        }

        private void BuildFooter()
        {
            _lblFeed.Text = "Feed";
            _lblFeed.AutoSize = true;
            _lblFeed.ForeColor = UiKit.Muted;
            _lblFeed.Location = new Point(16, _recoCard.Bottom + 6);
            Controls.Add(_lblFeed);

            _lstFeed.Location = new Point(16, _lblFeed.Bottom + 6);
            _lstFeed.Size = new Size(ClientSize.Width - 32, 70);
            _lstFeed.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            Controls.Add(_lstFeed);

            var btnBack = new Button { Text = L10n.T("Btn_Back") };
            UiKit.StyleSecondary(btnBack);
            btnBack.Size = new Size(120, 34);
            btnBack.Location = new Point(16, ClientSize.Height - btnBack.Height - 10);
            btnBack.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnBack.Click += (_, __) => { if (_nav.TryPop(out var parent)) { parent.Show(); Close(); } else Close(); };
            Controls.Add(btnBack);

            Resize += (_, __) =>
            {
                _recoCard.Width = ClientSize.Width - 32;
                _lstReco.Width = _recoCard.Width - 24;
                _banner.Width = ClientSize.Width - 32;
                _grid.Width = ClientSize.Width - 32;
                _lstFeed.Width = ClientSize.Width - 32;
            };
        }

        // =======================
        // DATA / BINDING
        // =======================
        private void BuildIndexesAndSets()
        {
            _byId.Clear();
            _allCategories.Clear();

            foreach (var ev in _store.All())
            {
                _byId[ev.Id] = ev; // Dictionary for O(1)
                if (!string.IsNullOrWhiteSpace(ev.Category.ToString()))
                    _allCategories.Add(ev.Category.ToString());

            }

            BuildCategoryChips();
        }

        private void BuildCategoryChips()
        {
            _flpFilters.Controls.Clear();

            foreach (var cat in _allCategories.OrderBy(c => c))
            {
                var chk = new CheckBox
                {
                    Text = cat,
                    AutoSize = true,
                    Margin = new Padding(6, 6, 6, 6)
                };
                chk.CheckedChanged += (_, __) =>
                {
                    if (chk.Checked) _selectedCategories.Add(cat);
                    else _selectedCategories.Remove(cat);
                    RefreshGrid();        // live filtering
                    RefreshRecommendations();
                };

                // chip-ish feel
                chk.FlatStyle = FlatStyle.Flat;
                chk.Padding = new Padding(6, 2, 6, 2);
                chk.BackColor = Color.White;
                chk.Tag = cat;

                _flpFilters.Controls.Add(chk);
            }
        }

        private void RenderUrgentBanner()
        {
            if (_urgent.Count == 0)
            {
                _lblUrgent.Text = "No urgent announcements.";
                return;
            }

            // Safely get next without removing
            if (_urgent.TryDequeue(out var top))
            {
                _lblUrgent.Text = $"URGENT: {top.Title} ({top.Start:yyyy/MM/dd HH:mm})";
                _urgent.Enqueue(top, top.Urgency); // put it back to preserve queue
            }
            else
            {
                _lblUrgent.Text = "No urgent announcements.";
            }


        }

        private void RefreshGrid()
        {
            var query = _txtSearch.Text?.Trim() ?? string.Empty;
            var onDate = _chkDate.Checked ? _dtp.Value.Date : (DateTime?)null;

            IEnumerable<LocalEvent> data = _store.All();

            // Filter by categories (HashSet)
            if (_selectedCategories.Count > 0)
                data = data.Where(e => e.Category != null && _selectedCategories.Contains(e.Category.ToString()));


            // Filter by date
            if (onDate.HasValue)
                data = data.Where(e => e.Start.Date <= onDate && e.End.Date >= onDate);

            // Filter by keyword(s)
            if (!string.IsNullOrWhiteSpace(query))
            {
                var terms = SplitTerms(query);
                data = data.Where(e =>
                    terms.Any(t =>
                        (e.Title?.IndexOf(t, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                        (e.Description?.IndexOf(t, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                        (e.Location?.IndexOf(t, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                        (e.Category.ToString().IndexOf(t, StringComparison.OrdinalIgnoreCase)) >= 0));
            }

            BindGrid(data.OrderBy(e => e.Start));
        }
        private static string FormatTags(object? tags)
        {
            if (tags is null) return "";
            if (tags is System.Collections.Generic.IEnumerable<string> seq)
                return string.Join(", ", seq);
            return tags.ToString() ?? "";
        }

        private void BindGrid(IEnumerable<LocalEvent> rows)
        {
            // Project to shallow, grid-friendly model (avoids auto-gen oddities)
            var list = rows.Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                e.Category,
                Start = e.Start.ToString("yyyy/MM/dd HH:mm"),
                End = e.End.ToString("yyyy/MM/dd HH:mm"),
                e.Location,
                e.IsAnnouncement,
                e.Urgency,
                Tags = FormatTags(e.Tags)

            }).ToList();

            _grid.DataSource = list;

            // Make “announcement” rows accent
            foreach (DataGridViewRow r in _grid.Rows)
            {
                bool isAnn = Convert.ToBoolean(r.Cells["IsAnnouncement"].Value ?? false);
                if (isAnn)
                {
                    r.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 238);
                    r.DefaultCellStyle.SelectionBackColor = Color.FromArgb(252, 232, 200);
                }
            }
        }

        // =======================
        // SEARCH & RECOMMENDATIONS
        // =======================
        private void DoSearch()
        {
            var q = _txtSearch.Text?.Trim() ?? string.Empty;

            // Stack + Dictionary telemetry
            if (!string.IsNullOrWhiteSpace(q))
            {
                _recentSearches.Push(q);                            // Stack
                foreach (var t in SplitTerms(q))
                {
                    if (string.IsNullOrWhiteSpace(t)) continue;
                    _termFreq[t] = _termFreq.TryGetValue(t, out var n) ? n + 1 : 1;  // Dictionary
                }
            }

            RefreshGrid();
            RefreshRecommendations();
            DrainTipsToFeed();
        }

        private void RefreshRecommendations()
        {
            // Score events: term overlap + category affinity + upcoming boost
            var now = DateTime.Now;
            var terms = _termFreq.OrderByDescending(kv => kv.Value)
                                 .Select(kv => kv.Key)
                                 .Take(8)
                                 .ToArray();

            IEnumerable<LocalEvent> all = _store.All();
            var scored = new List<(LocalEvent ev, double score)>();

            foreach (var e in all)
            {
                double s = 0;

                // term matches in title/desc/location/category
                for (int i = 0; i < terms.Length; i++)
                {
                    var t = terms[i];
                    int w = _termFreq[t];
                    if (!string.IsNullOrEmpty(e.Title) && e.Title.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) s += 2 * w;
                    if (!string.IsNullOrEmpty(e.Description) && e.Description.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) s += 1.5 * w;
                    if (!string.IsNullOrEmpty(e.Category.ToString()) && e.Category.ToString().IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                        s += 1.5 * w;
                    if (!string.IsNullOrEmpty(e.Location) && e.Location.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) s += 1.0 * w;
                }

                // category filters (if any are selected, favor them)
                if (_selectedCategories.Count > 0 && _selectedCategories.Contains(e.Category.ToString()))
                    s += 4.0;

                // upcoming boost (near future)
                var days = (e.Start - now).TotalDays;
                if (days >= 0 && days <= 14) s += 2.0;
                if (e.IsAnnouncement) s += 1.0; // small nudge

                if (s > 0) scored.Add((e, s));
            }

            var top = scored.OrderByDescending(x => x.score)
                            .ThenBy(x => x.ev.Start)
                            .Take(6)
                            .Select(x => x.ev)
                            .ToList();

            _lstReco.Items.Clear();
            foreach (var ev in top)
                _lstReco.Items.Add(ev);
            _lstReco.DisplayMember = "Title";
        }

        // =======================
        // INTERACTION
        // =======================
        private void ShowDetail(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _grid.Rows.Count) return;
            int id = Convert.ToInt32(_grid.Rows[rowIndex].Cells["Id"].Value);
            OpenById(id);
        }

        private void OpenById(int id)
        {
            if (!_byId.TryGetValue(id, out var ev)) return;

            // Track recently viewed (Stack)
            _recentViewed.Push(id);

            // Feed message (LinkedList)
            _feed.AddFirst($"Viewed: {ev.Title} • {ev.Start:yyyy/MM/dd}");
            TrimFeed();

            // Simple detail dialog
            var body =
                $"{ev.Title}\n\n{ev.Description}\n\n" +
                $"Category: {ev.Category}\nLocation: {ev.Location}\n" +
                $"When: {ev.Start:yyyy/MM/dd HH:mm} → {ev.End:yyyy/MM/dd HH:mm}\n\n" +
                (ev.IsAnnouncement ? "Announcement" : "Event");
            MessageBox.Show(body, "Event", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _lstFeed.Items.Clear();
            foreach (var line in _feed) _lstFeed.Items.Add(line);
        }

        // =======================
        // FEED (Queue->LinkedList)
        // =======================
        private void DrainTipsToFeed()
        {
            // Move messages from your NotificationQueue into a rolling LinkedList feed
            while (_tips.TryDequeue(out var msg))
                _feed.AddFirst(msg);

            TrimFeed();
            _lstFeed.Items.Clear();
            foreach (var line in _feed) _lstFeed.Items.Add(line);
        }

        private void TrimFeed()
        {
            // keep last 12
            while (_feed.Count > 12) _feed.RemoveLast();
        }

        // =======================
        // Helpers
        // =======================
        private static string[] SplitTerms(string query)
        {
            return (query ?? "")
                .Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
