using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

// TODO: Don't change the opacity of the DrawRectangle selection area or the help text...
// NOTE: Add back help text?

namespace Screenshotter
{
    public partial class Form1 : Form
    {
        private const float SELECT_RECT_THICKNESS = 2;
        private const bool COPY_TO_CLIPBOARD = true;

        // Stores the selection points of a rectangular area for the screenshot...
        private int startX = 0, startY = 0, endX = 0, endY = 0;

        private bool isSelecting = false;

        public Form1()
        {
            // Initialize the component...
            InitializeComponent();

            // Make the form stay on top of all other windows...
            TopMost = true;

            // Set background color to black and half the opacity, so we get a nice darkened overlay...
            BackColor = Color.Black;
            Opacity = 0.5f;

            // We don't want a border,
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
        }

        private void Form1_MouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            // Store the starting points of the screenshot...
            startX = mouseEventArgs.X;
            startY = mouseEventArgs.Y;

            isSelecting = true;
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