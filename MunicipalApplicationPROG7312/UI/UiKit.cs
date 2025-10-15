using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using MunicipalApplicationPROG7312.Models;

namespace MunicipalApplicationPROG7312.UI
{
    // Lightweight UI helpers for WinForms (.NET 8)
    public static class UiKit
    {
        // Palette
        public static readonly Color Bg = Color.FromArgb(248, 249, 250);
        public static readonly Color Surface = Color.White;
        public static readonly Color Accent = Color.FromArgb(0, 120, 212);
        public static readonly Color Text = Color.FromArgb(32, 32, 32);
        public static readonly Color Muted = Color.FromArgb(96, 96, 96);
        public static readonly Color Border = Color.FromArgb(220, 223, 226);

        // ---------- App-level setup ----------

        // Call once in each form ctor
        public static void ApplyTheme(Form f)
        {
            f.AutoScaleMode = AutoScaleMode.Font;          // scale by font
            f.AutoScaleDimensions = new SizeF(96f, 96f);    // baseline
            f.BackColor = Bg;
            f.Font = new Font("Segoe UI", 9F);
            f.StartPosition = FormStartPosition.CenterScreen;
            EnableDoubleBuffering(f);

            // Reflow when font changes at runtime (Settings dialog)
            f.FontChanged += (_, __) => RelayoutForFont(f);
        }

        // Theme helpers
        public static Color ThemeBg(AppTheme t) => t == AppTheme.Dark ? Color.FromArgb(32, 34, 37) : Bg;
        public static Color ThemeSurface(AppTheme t) => t == AppTheme.Dark ? Color.FromArgb(43, 45, 48) : Surface;
        public static Color ThemeText(AppTheme t) => t == AppTheme.Dark ? Color.WhiteSmoke : Text;
        public static Color ThemeMuted(AppTheme t) => t == AppTheme.Dark ? Color.FromArgb(180, 180, 180) : Muted;
        public static Color ThemeBorder(AppTheme t) => t == AppTheme.Dark ? Color.FromArgb(70, 70, 72) : Border;

        // Apply theme + base font to the whole form
        public static void ApplyRuntimeTheme(Form f, AppTheme theme, int baseFontSize)
        {
            f.BackColor = ThemeBg(theme);
            f.Font = new Font("Segoe UI", baseFontSize);
            RefreshColorsRecursive(f.Controls, theme);
            f.Invalidate();
            RelayoutForFont(f); // font change implies reflow
        }

        private static void RefreshColorsRecursive(Control.ControlCollection controls, AppTheme theme)
        {
            foreach (Control c in controls)
            {
                switch (c)
                {
                    case Panel:
                        c.BackColor = ThemeSurface(theme);
                        break;
                    case Label lbl:
                        lbl.ForeColor = lbl.Enabled ? ThemeText(theme) : ThemeMuted(theme);
                        break;
                    case TextBox tb:
                        tb.BackColor = ThemeSurface(theme); tb.ForeColor = ThemeText(theme);
                        break;
                    case RichTextBox rtb:
                        rtb.BackColor = ThemeSurface(theme); rtb.ForeColor = ThemeText(theme);
                        break;
                    case ComboBox cb:
                        cb.BackColor = ThemeSurface(theme); cb.ForeColor = ThemeText(theme);
                        break;
                    case ListBox lb:
                        lb.BackColor = ThemeSurface(theme); lb.ForeColor = ThemeText(theme);
                        break;
                    case ProgressBar:
                        c.BackColor = ThemeSurface(theme);
                        break;
                    case Button b:
                        // Respect intent: set Tag = "primary" on primary buttons once, or leave default heuristic
                        bool isPrimary = (b.Tag as string) == "primary" || b.BackColor == Accent || b.ForeColor == Color.White;
                        if (isPrimary) { b.BackColor = Accent; b.ForeColor = Color.White; }
                        else { b.BackColor = ThemeSurface(theme); b.ForeColor = theme == AppTheme.Dark ? Color.WhiteSmoke : Accent; }
                        break;
                }

                if (c.HasChildren) RefreshColorsRecursive(c.Controls, theme);
            }
        }

        // ---------- Header ----------

        public static Panel CreateHeader(string title)
        {
            var header = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = Surface };

            var line = new Panel { Height = 1, Dock = DockStyle.Bottom, BackColor = Border };
            var lbl = new Label
            {
                Name = "__HeaderTitle",
                Text = title,
                AutoSize = true,
                ForeColor = Text,
                Font = new Font("Segoe UI Semibold", 14F),
                Location = new Point(20, 14)
            };

            header.Controls.Add(lbl);
            header.Controls.Add(line);
            return header;
        }

        // Update the text of the header label created above
        public static void SetHeaderTitle(Control headerOrHero, string title)
        {
            if (headerOrHero == null) return;

            // Prefer the named Hero label (CreateHero)
            var heroTitle = headerOrHero.Controls["HeroTitle"] as Label;
            if (heroTitle != null)
            {
                heroTitle.Text = title;
                return;
            }

            // Fallback: first label inside a classic header (CreateHeader)
            foreach (Control c in headerOrHero.Controls)
            {
                if (c is Label lbl)
                {
                    lbl.Text = title;
                    return;
                }
            }
        }


        // Add a gear button to the top-right of the given header
        public static Button AddSettingsGear(Panel header, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = "⚙",
                Width = 36,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface,
                ForeColor = Accent,
                TabStop = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(header.Width - 36 - 12, 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            header.Controls.Add(btn);
            return btn;
        }

        // ---------- Cards / grids ----------

        public static Panel CreateCard(Rectangle bounds)
        {
            var p = new Panel { Bounds = bounds, BackColor = Surface };
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        // TableLayoutPanel configured for responsive rows in a card
        public static TableLayoutPanel CreateGrid()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12)
            };
            grid.RowStyles.Clear();
            return grid;
        }

        // ---------- Buttons / inputs ----------

        public static void StylePrimary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Accent;
            b.ForeColor = Color.White;
            b.Tag = "primary";                     // used by theme refresher
            b.Height = Math.Max(b.Height, 36);
            RoundCorners(b, 8);
        }

        public static void StyleSecondary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Accent;
            b.BackColor = Surface;
            b.ForeColor = Accent;
            b.Tag = "secondary";
            b.Height = Math.Max(b.Height, 32);
            RoundCorners(b, 8);
        }

        public static void StyleGhost(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Surface;
            b.ForeColor = Accent;
            b.Tag = "ghost";
            b.Height = Math.Max(b.Height, 32);
        }

        public static void StyleInput(Control c)
        {
            c.BackColor = Surface; c.ForeColor = Text;
        }

        public static void SizeAndCenter(Button b, int width, int height, int cornerRadius = 8)
        {
            b.AutoSize = false;
            b.Size = new Size(width, height);
            b.TextAlign = ContentAlignment.MiddleCenter;
            ApplyRounded(b, cornerRadius);
        }

        public static void FitButtonWidth(Button b, int minWidth, int height, int sidePadding)
        {
            var sz = TextRenderer.MeasureText(b.Text, b.Font);
            var w = Math.Max(minWidth, sz.Width + sidePadding);
            b.Size = new Size(w, height);
        }

        // ---------- Font-responsive relayout ----------

        public static void RelayoutForFont(Form f)
        {
            if (f == null) return;
            f.SuspendLayout();
            AdjustRecursive(f);
            f.ResumeLayout(true);
            f.PerformLayout();
        }

        private static void AdjustRecursive(Control c)
        {
            // Wrap labels inside their parent width
            if (c is Label l)
            {
                l.AutoSize = true;
                if (l.MaximumSize.Width == 0)
                {
                    int pad = 24;
                    int maxW = Math.Max(80, (l.Parent?.ClientSize.Width ?? 400) - l.Left - pad);
                    l.MaximumSize = new Size(maxW, 0);
                }
            }

            // Wrap checkbox text as well
            if (c is CheckBox cb)
            {
                cb.AutoSize = true;
                if (cb.MaximumSize.Width == 0)
                {
                    int pad = 24;
                    int maxW = Math.Max(80, (cb.Parent?.ClientSize.Width ?? 400) - cb.Left - pad);
                    cb.MaximumSize = new Size(maxW, 0);
                }
            }

            // Buttons: size to text and keep caption centered
            if (c is Button b)
            {
                var s = TextRenderer.MeasureText(b.Text, b.Font);
                b.Width = Math.Max(b.Width, s.Width + 28);
                b.Height = Math.Max(b.Height, s.Height + 16);
                b.TextAlign = ContentAlignment.MiddleCenter;
                // keep rounded region after size change
                ApplyRounded(b, 8);
            }

            // TextBox / ComboBox: height tracks font; allow horizontal stretch
            if (c is TextBox || c is ComboBox)
            {
                c.Height = Math.Max(c.Height, c.PreferredSize.Height);
                c.Anchor |= AnchorStyles.Left | AnchorStyles.Right;
            }

            // RichTextBox: give room based on font height
            if (c is RichTextBox rtb)
            {
                rtb.Height = Math.Max(rtb.Height, (int)(rtb.Font.Height * 6.0));
                rtb.Anchor |= AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            foreach (Control child in c.Controls)
                AdjustRecursive(child);
        }

        // ---------- Low-level helpers ----------

        private static void EnableDoubleBuffering(Control control)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(control, true, null);
        }

        private static void RoundCorners(Control c, int radius)
        {
            var r = c.ClientRectangle;
            using (var path = new GraphicsPath())
            {
                int d = radius * 2;
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                c.Region = new Region(path);
            }
        }

        public static void ApplyRounded(Control c, int radius) => RoundCorners(c, radius);

        // Contrast util
        public static Color AutoTextOn(Color bg)
        {
            var luminance = (0.2126 * bg.R + 0.7152 * bg.G + 0.0722 * bg.B) / 255.0;
            return luminance > 0.6 ? Color.FromArgb(32, 32, 32) : Color.White;
        }

        // --- Gradient hero header with title + subtitle ---
        public static Panel CreateHero(string title, string subtitle = null)
        {
            var hero = new Panel { Height = 84, Dock = DockStyle.Top, BackColor = Color.Transparent };

            // keep your blue background paint
            hero.Paint += (s, e) =>
            {
                using (var lg = new LinearGradientBrush(
                    hero.ClientRectangle,
                    Color.FromArgb(0, 132, 255),     // lighter blue top
                    Color.FromArgb(0, 100, 210),     // deeper blue bottom
                    90f))
                {
                    e.Graphics.FillRectangle(lg, hero.ClientRectangle);
                }
                using (var pen = new Pen(Border))
                    e.Graphics.DrawLine(pen, 0, hero.Height - 1, hero.Width, hero.Height - 1);
            };

            var lblTitle = new Label
            {
                Name = "HeroTitle",                   // <-- so we can target it later
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 16f),
                ForeColor = Color.White,              // default; we’ll override in MainForm
                BackColor = Color.Transparent,
                Location = new Point(20, 14)
            };
            hero.Controls.Add(lblTitle);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                var lblSub = new Label
                {
                    Name = "HeroSubtitle",
                    Text = subtitle,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(230, 240, 255),
                    BackColor = Color.Transparent,
                    Location = new Point(22, 44)
                };
                hero.Controls.Add(lblSub);
            }

            return hero;
        }

        public static void SetHeroTitleColor(Control hero, Color color)
        {
            var lbl = hero.Controls["HeroTitle"] as Label;
            if (lbl != null) lbl.ForeColor = color;
        }


        // --- Shadowed card container for content area ---
        public static Panel CreateShadowCard(Rectangle bounds)
        {
            var holder = new Panel { Bounds = new Rectangle(bounds.X + 2, bounds.Y + 6, bounds.Width, bounds.Height), BackColor = Color.Transparent };
            var card = new Panel { Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), BackColor = Surface };
            holder.ParentChanged += (s, e) =>
            {
                if (holder.Parent != null) holder.Parent.Controls.Add(card);
                card.BringToFront();
            };
            holder.Paint += (s, e) =>
            {
                using (var path = new GraphicsPath())
                using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    var r = new Rectangle(card.Left - holder.Left, card.Top - holder.Top, card.Width, card.Height);
                    int rad = 10; int d = rad * 2;
                    path.AddArc(r.X, r.Y, d, d, 180, 90);
                    path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                    path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                    path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.TranslateTransform(0, 0);
                    e.Graphics.FillPath(shadow, path);
                }
            };

            // light border
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            holder.Tag = card; // you can access card via (Panel)holder.Tag
            return holder;
        }

        // --- Big pill buttons with hover effect ---
        public static void StyleMenuButton(Button b, bool primary)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.BorderColor = primary ? Accent : Accent;
            b.Font = new Font("Segoe UI Semibold", 11f);
            b.Height = 48;
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.BackColor = primary ? Accent : Surface;
            b.ForeColor = primary ? Color.White : Accent;
            RoundCorners(b, 12);

            var normal = b.BackColor;
            var hover = primary ? Color.FromArgb(0, 105, 190) : Color.FromArgb(240, 244, 248);
            b.MouseEnter += (_, __) => { b.BackColor = hover; };
            b.MouseLeave += (_, __) => { b.BackColor = normal; };
        }

        // --- Small “pill” tag, e.g., (coming soon) ---
        public static Label Pill(string text, Color fg, Color bg)
        {
            var l = new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = fg,
                BackColor = bg,
                Padding = new Padding(8, 3, 8, 3),
                Font = new Font("Segoe UI", 9f)
            };
            l.Paint += (s, e) => { ApplyRounded(l, 10); };
            return l;
        }

    }
}
