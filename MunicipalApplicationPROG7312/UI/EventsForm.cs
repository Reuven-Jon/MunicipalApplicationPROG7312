using Microsoft.VisualBasic;
using MunicipalApplicationPROG7312.Domain;
using MunicipalApplicationPROG7312.Persistance;
using System;
using System.ComponentModel;          // LicenseManager for design-mode detection
using System.Linq;
using System.Windows.Forms;

namespace MunicipalApplicationPROG7312.UI
{
    public partial class EventsForm : Form
    {
        // Services (can be populated by DI or design-time stubs)
        private IEventStore _store;
        private EventIndex _index;
        private RecommendationEngine _reco;
        private UrgentAnnouncementQueue<LocalEvent> _urgent;
        private NotificationQueue _tips;
        private NavigationHistory<Form> _nav;

        // Helper: are we inside the Windows Forms Designer?
        private static bool IsDesignMode =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        // ------------- Designer-friendly ctor (no params) -------------
        // Lets Visual Studio spawn the form without your DI graph.
        public EventsForm()
        {
            InitializeComponent();

            if (IsDesignMode)
            {
                // Lightweight stubs so the designer won’t crash when it runs your handlers
                _store = new InMemoryEventStore();
                _index = new EventIndex(() => _store.All());
                _reco = new RecommendationEngine(() => _store.All(), _index);
                _urgent = new UrgentAnnouncementQueue<LocalEvent>();
                _tips = new NotificationQueue();
                _nav = new NavigationHistory<Form>();

                // Don’t execute heavy runtime flows in designer
                return;
            }

            // If someone uses the parameterless ctor at runtime, initialise sensibly
            _store = new InMemoryEventStore();
            _index = new EventIndex(() => _store.All());
            _reco = new RecommendationEngine(() => _store.All(), _index);
            _urgent = new UrgentAnnouncementQueue<LocalEvent>();
            foreach (var e in _store.All()) if (e.IsAnnouncement) _urgent.Enqueue(e, e.Urgency);
            _tips = new NotificationQueue();
            _tips.Enqueue("Welcome to Local Events & Announcements!");
            _tips.Enqueue("Tip: Use keywords + category to narrow results.");
            _nav = new NavigationHistory<Form>();

            LoadCategories();
            RefreshGrid();
            ShowNextUrgent();
            ShowNextTip();
            ShowRecommendations();
        }

        // ------------- Runtime ctor (DI) -------------
        public EventsForm(IEventStore store,
                          EventIndex index,
                          RecommendationEngine reco,
                          UrgentAnnouncementQueue<LocalEvent> urgent,
                          NotificationQueue tips,
                          NavigationHistory<Form> nav)
        {
            _store = store;
            _index = index;
            _reco = reco;
            _urgent = urgent;
            _tips = tips;
            _nav = nav;

            InitializeComponent();

            if (!IsDesignMode)
            {
                LoadCategories();
                RefreshGrid();
                ShowNextUrgent();
                ShowNextTip();
                ShowRecommendations();
            }
        }

        // ---------------- UI Logic ----------------
        private void LoadCategories()
        {
            cmbCategory.DataSource = Enum.GetValues(typeof(EventCategory));
            cmbCategory.SelectedIndex = -1;
        }

        private void RefreshGrid()
        {
            var cat = cmbCategory.SelectedIndex >= 0 ? (EventCategory?)cmbCategory.SelectedItem : null;
            var date = chkDate.Checked ? dtpDate.Value.Date : (DateTime?)null;

            var list = _index != null
                ? _index.Search(txtSearch.Text, cat, date).ToList()
                : Enumerable.Empty<LocalEvent>().ToList();

            grid.DataSource = list;
        }

        private void ShowNextUrgent()
        {
            if (_urgent != null && _urgent.TryDequeue(out var e) && e != null)
                lblUrgent.Text = $"URGENT: {e.Title} ({e.Start:g})";
        }

        private void ShowNextTip()
        {
            if (_tips != null && _tips.TryDequeue(out var m))
                lblTip.Text = m;
        }

        private void ShowRecommendations()
        {
            lstRecommended.Items.Clear();

            if (_reco == null) return;

            var rec = _reco.RecommendTopN(5).ToList();
            foreach (var e in rec)
                lstRecommended.Items.Add($"{e.Title} • {e.Start:d} • {e.Category}");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var cat = cmbCategory.SelectedIndex >= 0 ? (EventCategory?)cmbCategory.SelectedItem : null;

            if (_reco != null)
            {
                _reco.Track(new UserAction { Query = txtSearch.Text, FilterCategory = cat });
                RefreshGrid();
                ShowRecommendations();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // Avoid null-forgiving operator for older language versions
            if (_nav != null && _nav.TryPop(out var previous) && previous != null)
            {
                previous.Show();
                Close();
            }
            else
            {
                Close();
            }
        }

        private void chkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtpDate.Enabled = chkDate.Checked;
        }
    }
}
