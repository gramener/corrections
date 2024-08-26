using System;
using System.Net.Http;
using System.Windows.Forms;

namespace ClipboardTrayApp
{
    public class Program : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        public Program()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Clipboard Tray App";
            trayIcon.Icon = new System.Drawing.Icon(SystemIcons.Application, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            // Register hotkey (Win + Shift + T)
            RegisterHotKey(this.Handle, 0, MOD_WIN | MOD_SHIFT, Keys.T.GetHashCode());
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                // Handle hotkey event
                HandleHotkey();
            }

            base.WndProc(ref m);
        }

        private async void HandleHotkey()
        {
            // Get clipboard text
            string text = Clipboard.GetText();

            // Send to API
            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(text);
                HttpResponseMessage response = await client.PostAsync("https://example.com/api", content); // Replace with your API
                string result = await response.Content.ReadAsStringAsync();

                // Set response to clipboard
                Clipboard.SetText(result);

                // Show tray notification
                trayIcon.ShowBalloonTip(1000, "Clipboard Tray App", "Response copied to clipboard", ToolTipIcon.Info);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up resources.
                trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

        private const uint MOD_WIN = 0x0008;
        private const uint MOD_SHIFT = 0x0004;
    }
}
