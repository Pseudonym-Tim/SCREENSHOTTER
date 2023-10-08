using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// TODO: Don't change the opacity of the DrawRectangle selection area or the help text...
// NOTE: Add back help text?

namespace Screenshotter
{
    public partial class Form1 : Form
    {
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int MYACTION_HOTKEY_ID = 1;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_SNAPSHOT = 0x7B;
        private const float SELECT_RECT_THICKNESS = 2;
        private const bool COPY_TO_CLIPBOARD = true;

        // Stores the selection points of a rectangular area for the screenshot...
        private int startX = 0, startY = 0, endX = 0, endY = 0;

        private bool isSelecting = false;

        public Form1()
        {
            // Initialize the component...
            InitializeComponent();

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();

            NotifyIcon notifyIcon;
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += (sender, args) => Show(); // Show the form when user double-clicks on the tray icon

            // Optionally: Add context menu to notify icon for user interaction
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Exit", (sender, args) => Close());

            notifyIcon.ContextMenu = contextMenu;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide the form
            ShowInTaskbar = false; // Hide from taskbar
            base.OnLoad(e);
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                TopMost = true; // Make the form stay on top of all other windows...
                ShowInTaskbar = false; // Don't show in taskbar...

                // Set background color to black and half the opacity, so we get a nice darkened overlay...
                BackColor = Color.Black;
                Opacity = 0.5f;

                // We don't want a border...
                FormBorderStyle = FormBorderStyle.None;

                // Set the form's size to the screen size, we want to cover the whole screen...
                Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                // Set the cursor to a crosshair to indicate selection of the screenshot area...
                Cursor = Cursors.Cross;

                // Subscribe to events...
                MouseDown += new MouseEventHandler(Form1_MouseDown);
                MouseMove += new MouseEventHandler(Form1_MouseMove);
                MouseUp += new MouseEventHandler(Form1_MouseUp);
                Paint += new PaintEventHandler(Form1_Paint);

                // Enable double buffering and ignore erasing the background to prevent flicker when painting...
                SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

                int currentExStyle = GetWindowLong(Handle, GWL_EXSTYLE);
                SetWindowLong(Handle, GWL_EXSTYLE, currentExStyle | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);

                Show();
                WindowState = FormWindowState.Maximized;
            }

            base.WndProc(ref m);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            // Store the starting points of the screenshot...
            startX = mouseEventArgs.X;
            startY = mouseEventArgs.Y;

            isSelecting = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey(Handle, MYACTION_HOTKEY_ID, MOD_CONTROL, VK_SNAPSHOT);
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, MYACTION_HOTKEY_ID);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            // Store the ending points of the screenshot...
            endX = mouseEventArgs.X;
            endY = mouseEventArgs.Y;

            Invalidate();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            // Make sure that we actually selected an area instead of just clicking!
            // Otherwise we'd get an error when creating a new Bitmap!
            if(startX == endX && startY == endY) 
            { 
                isSelecting = false;
                return; 
            }

            // Make the form invisible so we can take a nice clean screenshot, without the overlay and selection area...
            Visible = false;

            // Create a new bitmap image with the selected area dimensions...
            Bitmap screenshot = new Bitmap(Math.Abs(startX - endX), Math.Abs(startY - endY));

            // Create a graphics object from the bitmap image...
            using(Graphics graphics = Graphics.FromImage(screenshot))
            {
                // Copy the selected area to the bitmap...
                graphics.CopyFromScreen(Math.Min(startX, endX), Math.Min(startY, endY), 0, 0, screenshot.Size);
            }

            // Get the user's Pictures folder...
            string picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // Create the "screenshots" subdirectory if it doesn't already exist...
            string screenshotsFolder = Path.Combine(picturesFolder, "screenshots");
            Directory.CreateDirectory(screenshotsFolder);

            // Create the full file path with a timestamp...
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
            string filePath = Path.Combine(screenshotsFolder, fileName);

            // Save the screenshot image to the file...
            screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            // Copy the screenshot to the clipboard for convenience...
            if(COPY_TO_CLIPBOARD) { Clipboard.SetImage(screenshot); }

            // Open the screenshot image?
            bool openScreenshotImage = (ModifierKeys & Keys.Control) == Keys.Control;
            if(openScreenshotImage) { System.Diagnostics.Process.Start(filePath); }

            // Close the application!
            Close();
        }

        private void Form1_Paint(object sender, PaintEventArgs paintEventArgs)
        {
            // Get the Graphics object from the PaintEventArgs...
            Graphics graphics = paintEventArgs.Graphics;

            //DrawHelpText(graphics);

            if(!isSelecting) { return; } // Fuck off if we're not selecting...

            Pen pen = new Pen(Color.Red, SELECT_RECT_THICKNESS); // Create a red pen with a specified width...

            // Draw a rectangle on the form with the selected area dimensions...
            graphics.DrawRectangle(pen, Math.Min(startX, endX), Math.Min(startY, endY), Math.Abs(startX - endX), Math.Abs(startY - endY));
        }

        /*private void DrawHelpText(Graphics graphics)
        {
            // Create a new font and brush...
            Font font = new Font("Arial", 16);
            Brush brush = Brushes.Red;

            // Draw the text at the top left corner of the screen...
            string message = "Hold CTRL to open the screenshot image...";
            float cornerOffset = 10;
            graphics.DrawString(message, font, brush, cornerOffset, cornerOffset);

            // Draw a custom underline for the text...
            const float underlineY = 36;
            Pen thickPen = new Pen(Color.Red, 2);
            float underlineX1 = cornerOffset;
            float underlineX2 = underlineX1 + TextRenderer.MeasureText(message, font).Width;
            graphics.DrawLine(thickPen, underlineX1, underlineY, underlineX2, underlineY);
        }*/
    }
}