using System;
using System.Drawing;
using System.Windows.Forms;

using MunicipalApplication;                         // UiKit
using MunicipalApplicationPROG7312.Localization;   // L10n
using MunicipalApplicationPROG7312.Persistance;    // IssueRepository
using MunicipalApplicationPROG7312.Domain;         // LocalEvent, EventIndex, RecommendationEngine, queues

namespace MunicipalApplicationPROG7312.UI
{
    public class MainForm : Form
    {
        // UI controls
        private readonly Button _btnReport = new Button();
        private readonly Button _btnEvents = new Button();
        private readonly Button _btnStatus = new Button();
        private readonly ComboBox _cmbLang = new ComboBox();

        private Panel _hero;   // gradient header
        private Panel _card;   // white content card (inside the shadow holder)
        private Label _pillSoonStatus;   // only Status has a pill now
        private Label _footer;

        // ===== Local Events infrastructure =====
        private readonly InMemoryEventStore _eventStore = new InMemoryEventStore();
        private EventIndex _eventIndex;
        private RecommendationEngine _reco;

        private readonly UrgentAnnouncementQueue<LocalEvent> _urgent = new UrgentAnnouncementQueue<LocalEvent>();
        private readonly NotificationQueue _tips = new NotificationQueue();
        private readonly NavigationHistory<Form> _nav = new NavigationHistory<Form>();
        // =======================================

        public MainForm()
        {
            UiKit.ApplyTheme(this);

            InitializeForm();
            IssueRepository.LoadFromDisk();

            InitializeHero();        // gradient title + language combo
            InitializeMenuCard();    // shadowed card with big buttons
            WireResize();            // keep layout tidy on resize

            InitLocalEvents();       // seed queues + index + recommendations
            UpdateTexts();           // localize labels/buttons at startup
        }

        private void InitializeForm()
        {
            Text = L10n.T("Main_Title");
            ClientSize = new Size(720, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
        }

        // --- HERO (gradient header) ---
        private void InitializeHero()
        {
            _hero = UiKit.CreateHero(
                L10n.T("Main_Title"),
                "Fast, transparent fault reporting"   // keep literal; swap to L10n if you add a key
            );
            Controls.Add(_hero);

            // Language selector (top-right in hero)
            _cmbLang.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLang.Width = 180;
            _cmbLang.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmbLang.Location = new Point(ClientSize.Width - _cmbLang.Width - 20, 20);
            _cmbLang.SelectedIndexChanged += OnLanguageChanged;

            PopulateLanguageOptions();
            _hero.Controls.Add(_cmbLang);
        }

        // --- CONTENT CARD (shadow + big buttons) ---
        private void InitializeMenuCard()
        {
            var holder = UiKit.CreateShadowCard(
                new Rectangle(24, _hero.Bottom + 16, ClientSize.Width - 48, 230));
            Controls.Add(holder);
            _card = (Panel)holder.Tag;

            // Section title
            var title = new Label
            {
                Text = "Get started",
                AutoSize = true,
                ForeColor = UiKit.Text,
                Font = new Font("Segoe UI Semibold", 12.5f),
                Location = new Point(20, 18)
            };
            _card.Controls.Add(title);

            // Primary action: Report Issue
            _btnReport.Text = L10n.T("Btn_Report");
            _btnReport.Size = new Size(_card.Width - 40, 50);
            _btnReport.Location = new Point(20, 56);
            UiKit.StyleMenuButton(_btnReport, primary: true);
            _btnReport.Click += (s, e) => new ReportIssueForm().ShowDialog(this);

            // Local Events (ENABLED + BLUE primary)
            _btnEvents.Enabled = true;
            _btnEvents.Text = L10n.T("Btn_Events");
            _btnEvents.Size = new Size(_card.Width - 40, 46);
            _btnEvents.Location = new Point(20, 112);
            UiKit.StyleMenuButton(_btnEvents, primary: true);
            // Force blue accent in case theme primary differs
            _btnEvents.BackColor = Color.FromArgb(0, 120, 212);
            _btnEvents.ForeColor = Color.White;
            _btnEvents.Click += OnOpenEvents;

            // Status (still coming soon)
            _btnStatus.Enabled = false;
            _btnStatus.Text = L10n.T("Btn_Status");
            _btnStatus.Size = new Size(_card.Width - 40, 46);
            _btnStatus.Location = new Point(20, 162);
            UiKit.StyleMenuButton(_btnStatus, primary: false);

            // Only Status has a "coming soon" pill
            _pillSoonStatus = UiKit.Pill("coming soon", Color.White, Color.FromArgb(140, 0, 120, 212));

            // Add controls (no pill for Events)
            _card.Controls.AddRange(new Control[]
            {
                _btnReport, _btnEvents, _btnStatus, _pillSoonStatus
            });

            // Footer note
            _footer = new Label
            {
                Text = "Built for coursework • v2.0 (Part 2)",
                AutoSize = true,
                ForeColor = UiKit.Muted,
                Location = new Point(24, _card.Bottom + 12)
            };
            Controls.Add(_footer);

            PositionPills();
        }

        // --- Local Events data wiring ---
        private void InitLocalEvents()
        {
            _eventIndex = new EventIndex(() => _eventStore.All());
            _reco = new RecommendationEngine(() => _eventStore.All(), _eventIndex);

            // Seed queues from current events
            foreach (var e in _eventStore.All())
                if (e.IsAnnouncement) _urgent.Enqueue(e, e.Urgency);

            _tips.Enqueue("Welcome to Local Events & Announcements!");
            _tips.Enqueue("Tip: Use keywords + category to narrow results.");
        }

        private void OnOpenEvents(object sender, EventArgs e)
        {
            var f = new EventsForm(_eventStore, _eventIndex, _reco, _urgent, _tips, _nav);
            _nav.Push(this);
            Hide();
            f.Show();
        }

        // --- Layout maintenance on resize ---
        private void WireResize()
        {
            Resize += (s, e) =>
            {
                _cmbLang.Location = new Point(ClientSize.Width - _cmbLang.Width - 20, 20);

                // Move/size card & shadow
                var holder = _card.Parent; // shadow holder
                holder.Bounds = new Rectangle(24, _hero.Bottom + 16, ClientSize.Width - 48, 230);
                _card.Bounds = holder.Bounds;

                // Buttons track card width
                _btnReport.Width = _card.Width - 40;
                _btnEvents.Width = _card.Width - 40;
                _btnStatus.Width = _card.Width - 40;

                PositionPills();
                _footer.Location = new Point(24, _card.Bottom + 12);
            };
        }

        private void PositionPills()
        {
            if (_pillSoonStatus != null)
            {
                _pillSoonStatus.Location = new Point(
                    _btnStatus.Right - _pillSoonStatus.Width - 12,
                    _btnStatus.Top + (_btnStatus.Height - _pillSoonStatus.Height) / 2);
            }
        }

        // --- Language handling ---
        private void PopulateLanguageOptions()
        {
            var current = (_cmbLang.SelectedItem as LangOption)?.Code ?? L10n.CurrentLanguageCode;

            _cmbLang.SelectedIndexChanged -= OnLanguageChanged;
            _cmbLang.Items.Clear();

            int i = 0, selected = -1;
            foreach (var opt in L10n.LanguageOptions())
            {
                _cmbLang.Items.Add(opt);
                if (opt.Code == current) selected = i;
                i++;
            }

            _cmbLang.SelectedIndex = selected >= 0 ? selected : 0;
            _cmbLang.SelectedIndexChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (_cmbLang.SelectedItem is LangOption opt)
                L10n.SetLanguage(opt.Code);

            UpdateTexts();
            PopulateLanguageOptions(); // keep selection stable
        }

        private void UpdateTexts()
        {
            Text = L10n.T("Main_Title");

            // Update the hero/header text correctly
            UiKit.SetHeaderTitle(_hero, L10n.T("Main_Title"));

            _btnReport.Text = L10n.T("Btn_Report");
            _btnEvents.Text = L10n.T("Btn_Events");
            _btnStatus.Text = L10n.T("Btn_Status");
        }
    }
}
