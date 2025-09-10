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
        public static void SetHeaderTitle(Control header, string title)
        {
            foreach (Control c in header.Controls)
                if (c is Label && c.Name == "__HeaderTitle") { c.Text = title; break; }
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
    }
}
