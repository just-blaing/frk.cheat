using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

public class window : Form
{
    private static readonly Color colback = Color.FromArgb(11, 11, 18);
    private static readonly Color coltop = Color.FromArgb(17, 17, 28);
    private static readonly Color colfield = Color.FromArgb(26, 26, 40);
    private static readonly Color colfieldhover = Color.FromArgb(38, 38, 58);
    private static readonly Color colaccent = Color.White;
    private static readonly Color colaccenthover = Color.FromArgb(214, 214, 224);
    private static readonly Color colmuted = Color.FromArgb(122, 122, 150);
    private static readonly Color colgood = Color.FromArgb(94, 214, 140);
    private static readonly Color colbad = Color.FromArgb(255, 99, 99);

    private readonly TextBox pathbox = new TextBox();
    private readonly Button browse = new Button();
    private readonly Button install = new Button();
    private readonly Button remove = new Button();
    private readonly Label status = new Label();
    private readonly ProgressBar bar = new ProgressBar();

    public window()
    {
        Text = "frk.cheat installer";
        ClientSize = new Size(520, 300);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = colback;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var top = new Panel();
        top.SetBounds(0, 0, 520, 70);
        top.BackColor = coltop;
        top.Controls.Add(label("frk.cheat", 20, 10, 300, 32, 18f, FontStyle.Bold, Color.White));
        top.Controls.Add(label("the escapists 2 installer", 22, 42, 300, 20, 9.5f, FontStyle.Regular, colmuted));

        var line = new Panel();
        line.SetBounds(0, 70, 520, 2);
        line.BackColor = colaccent;

        var caption = label("game folder", 20, 92, 200, 18, 9f, FontStyle.Regular, colmuted);

        pathbox.Font = new Font("Segoe UI", 10f);
        pathbox.SetBounds(20, 114, 386, 26);
        pathbox.BackColor = colfield;
        pathbox.ForeColor = Color.White;
        pathbox.BorderStyle = BorderStyle.FixedSingle;
        pathbox.Text = finder.find();

        style(browse, "browse", 414, 112, 86, 30, colfield, colfieldhover);
        style(install, "install", 20, 160, 235, 46, colaccent, colaccenthover);
        style(remove, "remove", 265, 160, 235, 46, colfield, colfieldhover);
        install.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        install.ForeColor = colback;
        remove.Font = new Font("Segoe UI", 10.5f);

        bar.SetBounds(20, 226, 480, 6);
        bar.Style = ProgressBarStyle.Marquee;
        bar.MarqueeAnimationSpeed = 25;
        bar.Visible = false;

        status.AutoSize = false;
        status.SetBounds(20, 244, 480, 44);
        bool found = File.Exists(Path.Combine(pathbox.Text, finder.exename));
        status.ForeColor = found ? colgood : colmuted;
        status.Text = found ? "game found" : "game not found, pick the folder";

        browse.Click += pick;
        install.Click += (s, e) => run(true);
        remove.Click += (s, e) => run(false);

        Controls.AddRange(new Control[] { top, line, caption, pathbox, browse, install, remove, bar, status });
    }

    private static Label label(string text, int x, int y, int w, int h, float size, FontStyle fs, Color color)
    {
        var l = new Label();
        l.Text = text;
        l.SetBounds(x, y, w, h);
        l.Font = new Font("Segoe UI", size, fs);
        l.ForeColor = color;
        l.BackColor = Color.Transparent;
        return l;
    }

    private static void style(Button b, string text, int x, int y, int w, int h, Color normal, Color hover)
    {
        b.Text = text;
        b.SetBounds(x, y, w, h);
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = hover;
        b.FlatAppearance.MouseDownBackColor = normal;
        b.BackColor = normal;
        b.ForeColor = Color.White;
        b.Cursor = Cursors.Hand;
    }

    private void pick(object sender, EventArgs e)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            if (Directory.Exists(pathbox.Text))
            {
                dialog.SelectedPath = pathbox.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathbox.Text = dialog.SelectedPath;
                bool found = File.Exists(Path.Combine(dialog.SelectedPath, finder.exename));
                say(found ? "game found" : "game not found in this folder", found ? colgood : colbad);
            }
        }
    }

    private void say(string text, Color color)
    {
        BeginInvoke(new Action(() =>
        {
            status.ForeColor = color;
            status.Text = text;
        }));
    }

    private void toggle(bool busy)
    {
        BeginInvoke(new Action(() =>
        {
            install.Enabled = !busy;
            remove.Enabled = !busy;
            browse.Enabled = !busy;
            pathbox.Enabled = !busy;
            bar.Visible = busy;
        }));
    }

    private void run(bool add)
    {
        string game = pathbox.Text.Trim();
        toggle(true);

        var worker = new Thread(() =>
        {
            try
            {
                if (add)
                {
                    setup.install(game, text => say(text, colmuted));
                    say("done, start the game and press insert", colgood);
                }
                else
                {
                    setup.uninstall(game);
                    say("removed", colgood);
                }
            }
            catch (Exception e)
            {
                say(e.Message, colbad);
            }

            toggle(false);
        });

        worker.IsBackground = true;
        worker.Start();
    }
}