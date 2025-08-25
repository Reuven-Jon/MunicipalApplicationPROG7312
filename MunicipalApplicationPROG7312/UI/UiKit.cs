using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using MunicipalApplicationPROG7312.Models;

namespace MunicipalApplicationPROG7312.UI
{
    // Tiny theming toolkit for WinForms (.NET Framework, C# 7.3)
    public static class UiKit
    {
        // Fluent-like palette
        public static readonly Color Bg = Color.FromArgb(248, 249, 250); // window background
        public static readonly Color Surface = Color.White;                   // cards/panels
        public static readonly Color Accent = Color.FromArgb(0, 120, 212);   // Windows blue
        public static readonly Color Text = Color.FromArgb(32, 32, 32);
        public static readonly Color Muted = Color.FromArgb(96, 96, 96);
        public static readonly Color Border = Color.FromArgb(220, 223, 226);

        // Call this to change the text in the header's Label created by CreateHeader()
        public static void SetHeaderTitle(Control header, string title)
        {
            foreach (Control c in header.Controls)
            {
                if (c is Label lbl) { lbl.Text = title; break; }
            }
        }

        // Call once in each form ctor to set base look
        public static void ApplyTheme(Form f)
        {
            f.BackColor = Bg;                                   // light window
            f.Font = new Font("Segoe UI", 9F);                  // Windows UI font
            f.StartPosition = FormStartPosition.CenterScreen;   // consistent launch
            EnableDoubleBuffering(f);                           // cut flicker on repaint
        }

        // Header bar with title; add this before other controls and place content below Y=64
        public static Panel CreateHeader(string title)
        {
            var header = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = Surface };
            var line = new Panel { Height = 1, Dock = DockStyle.Bottom, BackColor = Border }; // thin divider
            var lbl = new Label
            {
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

        // Primary action button (rounded, flat)
        public static void StylePrimary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = Accent; b.ForeColor = Color.White;
            b.Height = Math.Max(b.Height, 36);                  // touch-friendly height
            RoundCorners(b, 8);
        }

        // Secondary / outline button
        public static void StyleSecondary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Accent; b.BackColor = Surface; b.ForeColor = Accent;
            b.Height = Math.Max(b.Height, 32);
            RoundCorners(b, 8);
        }

        // Neutral text-button (e.g., attach)
        public static void StyleGhost(Button b)
        {
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = Surface; b.ForeColor = Accent;
            b.Height = Math.Max(b.Height, 32);
        }

        // Inputs look clean on a white "card"
        public static void StyleInput(Control c) { c.BackColor = Surface; c.ForeColor = Text; }

        // Simple card container
        public static Panel CreateCard(Rectangle bounds)
        {
            var p = new Panel { Bounds = bounds, BackColor = Surface };
            p.Paint += delegate (object s, PaintEventArgs e)
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); // thin border
            };
            return p;
        }

        // ---------- helpers ----------
        // ---------- helpers ----------
        private static void EnableDoubleBuffering(Control control)
        {
            // Set protected Control.DoubleBuffered = true via reflection
            var prop = typeof(Control).GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (prop != null) prop.SetValue(control, true, null);
        }

        private static void RoundCorners(Control c, int radius)
        {
            // Clip control region to a rounded rectangle
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

        public static void SizeAndCenter(Button b, int width, int height, int cornerRadius = 8)
        {
            b.AutoSize = false;                                      // we control size manually
            b.Size = new Size(width, height);                        // longer + taller
            b.TextAlign = ContentAlignment.MiddleCenter;             // center the caption
            ApplyRounded(b, cornerRadius);                           // re-apply rounded region after resize
        }

        // public wrapper so you can re-round after size changes
        public static void ApplyRounded(Control c, int radius)
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


        public static void FitButtonWidth(Button b, int minWidth, int height, int sidePadding)
        {
            // Measure the rendered text using the button's font
            var sz = TextRenderer.MeasureText(b.Text, b.Font);          // width of label
            var w = Math.Max(minWidth, sz.Width + sidePadding);         // add padding, respect min
            b.Size = new Size(w, height);                               // set final size
        }
        // Pick colors based on theme
        public static Color ThemeBg(AppTheme t) { return t == AppTheme.Dark ? Color.FromArgb(32, 34, 37) : Bg; }
        public static Color ThemeSurface(AppTheme t) { return t == AppTheme.Dark ? Color.FromArgb(43, 45, 48) : Surface; }
        public static Color ThemeText(AppTheme t) { return t == AppTheme.Dark ? Color.WhiteSmoke : Text; }
        public static Color ThemeMuted(AppTheme t) { return t == AppTheme.Dark ? Color.FromArgb(180, 180, 180) : Muted; }
        public static Color ThemeBorder(AppTheme t) { return t == AppTheme.Dark ? Color.FromArgb(70, 70, 72) : Border; }

        // Apply theme + font size to an existing form and its children
        public static void ApplyRuntimeTheme(Form f, AppTheme theme, int baseFontSize)
        {
            f.BackColor = ThemeBg(theme);                                              // Window background
            f.Font = new Font("Segoe UI", baseFontSize);                               // Global font

            RefreshColorsRecursive(f.Controls, theme);                                  // Recolor children
            f.Invalidate();                                                             // Redraw
        }

        // Recursively recolor typical controls for the chosen theme
        private static void RefreshColorsRecursive(Control.ControlCollection controls, AppTheme theme)
        {
            foreach (Control c in controls)
            {
                if (c is Panel) { c.BackColor = ThemeSurface(theme); }
                else if (c is Label) { c.ForeColor = c.Enabled ? ThemeText(theme) : ThemeMuted(theme); }
                else if (c is TextBox) { c.BackColor = ThemeSurface(theme); c.ForeColor = ThemeText(theme); }
                else if (c is RichTextBox) { c.BackColor = ThemeSurface(theme); c.ForeColor = ThemeText(theme); }
                else if (c is ComboBox) { c.BackColor = ThemeSurface(theme); c.ForeColor = ThemeText(theme); }
                else if (c is ListBox) { c.BackColor = ThemeSurface(theme); c.ForeColor = ThemeText(theme); }
                else if (c is ProgressBar) { c.BackColor = ThemeSurface(theme); }
                else if (c is Button)
                {
                    var b = (Button)c;
                    // Respect primary/secondary intent using existing ForeColor/BackColor hues
                    bool isPrimary = b.BackColor == Accent || b.ForeColor == Color.White; // crude heuristic
                    if (isPrimary)
                    {
                        b.BackColor = Accent; b.ForeColor = Color.White;                  // Keep accent for primary
                    }
                    else
                    {
                        b.BackColor = ThemeSurface(theme);                                // Secondary on surface
                        b.ForeColor = theme == AppTheme.Dark ? Color.WhiteSmoke : Accent;
                    }
                }
                // Recurse into children
                if (c.HasChildren) RefreshColorsRecursive(c.Controls, theme);
            }
        }

        public static Color AutoTextOn(Color bg)
        {
            // Pick near-black or near-white text against background for contrast
            var luminance = (0.2126 * bg.R + 0.7152 * bg.G + 0.0722 * bg.B) / 255.0; // Perceptual brightness
            return luminance > 0.6 ? Color.FromArgb(32, 32, 32) : Color.White;         // Dark text on light, light on dark
        }


        // Add a top-right gear button into a header panel
        public static Button AddSettingsGear(Panel header, EventHandler onClick)
        {
            var btn = new Button();                                                    // Create button
            btn.Text = "⚙";                                                            // Unicode gear
            btn.Width = 36; btn.Height = 36;                                           // Square button
            btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0;         // Flat look
            btn.BackColor = Surface; btn.ForeColor = Accent;                           // Match header
            btn.Location = new Point(header.Width - btn.Width - 12, 10);               // Right align
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;                        // Stick to top-right
            btn.Cursor = Cursors.Hand;                                                 // Hand cursor
            btn.TabStop = false;                                                       // Skip tab focus
            btn.Click += onClick;                                                      // Wire handler
            header.Controls.Add(btn);                                                  // Add into header
            return btn;                                                                // Return for further wiring if needed
        }
    }
}
