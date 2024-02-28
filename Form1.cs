// AI chatbot demo, name to be
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ML;

using Microsoft.ML.Data;



namespace windowsformsapp3
{


    public partial class Form1 : Form
    {
        private TaskCompletionSource<bool> animationCompletionSource;
        private string CallPythonScript(string scriptPath, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"{scriptPath} {arguments}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd(); // Read the standard output
                process.WaitForExit();
                return output;
            }
        }

        // Method to perform inference using Hugging Face Transformers
        private void PerformInference()
        {
            // Prepare the input text (you may use user input or any other source)

            if (replyTextBox.Text != "") {
                string inputText = replyTextBox.Text;
                // Update the Python script path and arguments
                string scriptPath = "C:/Users/Wafel/source/repos/AICHATBOT/inference.py";
                string arguments = $"\"{inputText}\"";

                // Call Python script for Hugging Face Transformers inference
                string output = CallPythonScript(scriptPath, arguments);

                // Process the output as needed
                Console.WriteLine("Model Output: " + output);
            } else
            {

            }
        }

       
        void main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public class CollapseMenu
        {
            public class rectanglemenu
            {
                public enum Recsize { x = 400, y = 600 }
                public static bool collapsed = false;
            }
            public static double InterpolationFactor = 0.0; // For interpolation between circle and rectangle
            public static int Size = 80;
            public static int ExpandedSize = 200; // New size for the expanded menu
            public enum Padding { Left = 0, Right = 30, Up = 0, Down = 30 };
        }

        // P/Invoke declarations to enable transparency
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_COLORKEY = 0x1;
        private const int LWA_ALPHA = 0x2;

        private Timer animationTimer;
        private int animationDuration = 500; // Animation duration in milliseconds

        public Form1()
        {

            InitializeComponent();

            // Set up the form properties
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;

            // Make the form transparent
            TransparencyKey = Color.Magenta; // Set the color key to magenta



            // Enable transparency
            IntPtr hWnd = Handle;
            SetWindowLong(hWnd, GWL_EXSTYLE, GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_LAYERED);
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;

            // Attach the Paint event to draw the circle/rectangle on the form
            Paint += Form1_Paint;
            // Subscribe to the MouseDown event
            this.MouseDown += Form1_MouseClick;

            // Set up the timer for animation
            animationTimer = new Timer();
            animationTimer.Interval = 20; // Timer interval in milliseconds
            animationTimer.Tick += AnimationTimer_Tick;
    }
        private async Task AnimateMenu()
        {
            double startInterpolation = CollapseMenu.rectanglemenu.collapsed ? 1.0 : 0.0;
            double targetInterpolation = CollapseMenu.rectanglemenu.collapsed ? 0.0 : 1.0;

            double startWidth = CollapseMenu.Size;
            double targetWidth = CollapseMenu.rectanglemenu.collapsed ? CollapseMenu.ExpandedSize : CollapseMenu.Size;

            int ymenu = Screen.PrimaryScreen.WorkingArea.Height - (int)CollapseMenu.rectanglemenu.Recsize.y;

            // Phase 1: Circle disappearing
            for (int elapsed = 0; elapsed <= animationDuration; elapsed += animationTimer.Interval)
            {
                double progress = (double)elapsed / animationDuration;
                CollapseMenu.InterpolationFactor = startInterpolation - startInterpolation * progress;

                // Calculate the current width and height during the animation
                double currentWidth = startWidth - startWidth * progress;
                double currentHeight = CollapseMenu.Size;

                // Redraw the form
                Invalidate();

                await Task.Delay(animationTimer.Interval);
            }

            // Phase 2: Rectangular menu appearing with the same width and height as the circle
            int xcircle = ClientSize.Width - CollapseMenu.Size - (int)CollapseMenu.Padding.Right;
            int ycircle = Screen.PrimaryScreen.WorkingArea.Height - CollapseMenu.Size - (int)CollapseMenu.Padding.Down;

            for (int elapsed = 0; elapsed <= animationDuration; elapsed += animationTimer.Interval)
            {
                double progress = (double)elapsed / animationDuration;
                CollapseMenu.InterpolationFactor = startInterpolation + (1.0 - startInterpolation) * progress;

                // Calculate the current width and height during the animation
                double currentWidth = targetWidth * progress;
                double currentHeight = CollapseMenu.Size;

                DrawNeumorphicRectangle(CreateGraphics(), xcircle, ycircle, currentWidth, currentHeight);

                // Redraw the form
                Invalidate();

                await Task.Delay(animationTimer.Interval);
            }

            // Ensure the final interpolation factor is set correctly
            CollapseMenu.InterpolationFactor = targetInterpolation;

            // Redraw the form
            Invalidate();

            // Set the result of the TaskCompletionSource to indicate that the animation is complete
            animationCompletionSource.TrySetResult(true);
        }


        private async void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            Screen primaryScreen = Screen.PrimaryScreen;
            Rectangle workArea = primaryScreen.WorkingArea;

            int x = ClientSize.Width - CollapseMenu.Size - (int)CollapseMenu.Padding.Right;
            int y = workArea.Height - CollapseMenu.Size - (int)CollapseMenu.Padding.Down;

            Rectangle circleBounds = new Rectangle(x, y, CollapseMenu.Size, CollapseMenu.Size);

            // Check if the mouse click is within the bounds of the circle
            if (circleBounds.Contains(e.Location))
            {
                // Disable form and unsubscribe from MouseDown event
                Enabled = false;
                MouseDown -= Form1_MouseClick;

                // Toggle the shape
                CollapseMenu.rectanglemenu.collapsed = !CollapseMenu.rectanglemenu.collapsed;
                animationCompletionSource = new TaskCompletionSource<bool>();
                await AnimateMenu();

                // Wait for the animation to complete
                await animationCompletionSource.Task;

                // Re-enable form and subscribe to MouseDown event
                Enabled = true;
                MouseDown += Form1_MouseClick;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Handle Enter key press
                PerformInference();
            }
        }

        // leave region below untouched for now
        #region animation 


        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!animationCompletionSource.Task.IsCompleted)
                return;

            AnimateMenu();
        }


        private TextBox replyTextBox;

        private async void Form1_Paint(object sender, PaintEventArgs e)
{
    Screen primaryScreen = Screen.PrimaryScreen;
    Rectangle workArea = primaryScreen.WorkingArea;

    int xcircle = ClientSize.Width - CollapseMenu.Size - (int)CollapseMenu.Padding.Right;
    int ycircle = workArea.Height - CollapseMenu.Size - (int)CollapseMenu.Padding.Down;

    if (CollapseMenu.rectanglemenu.collapsed == false)
    {
        double size = CollapseMenu.Size * (1 - CollapseMenu.InterpolationFactor);
        double width = size;
        double height = CollapseMenu.Size;

        // Draw a dark neumorphic rectangle
        DrawNeumorphicRectangle(e.Graphics, xcircle, ycircle, width, height);

        // Draw a blue logo in the middle
        DrawBlueLogo(e.Graphics, xcircle, ycircle, width, height);
    }
    else
    {
                animationCompletionSource = new TaskCompletionSource<bool>();
                
                double size = CollapseMenu.Size * (1 - CollapseMenu.InterpolationFactor);
        double width = size;
        double height = CollapseMenu.Size;

        int xmenu = (int)ClientSize.Width;
        int ymenu = workArea.Height;
        int recx = (int)CollapseMenu.rectanglemenu.Recsize.x;
        int recy = (int)CollapseMenu.rectanglemenu.Recsize.y;

        double recWidth = recx * CollapseMenu.InterpolationFactor + width * (1 - CollapseMenu.InterpolationFactor);
        double recHeight = recy;

        int recPosX = xmenu - recx;

        // Draw a dark neumorphic rectangle
        DrawNeumorphicRectangle(e.Graphics, recPosX, ymenu - recy, recWidth, recHeight);

        // Display chatbox elements
        int chatboxX = recPosX + 10;
        int chatboxY = ymenu - recy + 10;
        int textBoxWidth = recx - 80;
        int textBoxHeight = recy - 60;

        // Draw chatbox
        DrawNeumorphicRectangle(e.Graphics, chatboxX, chatboxY, textBoxWidth, textBoxHeight);

        // Draw text bubbles
        string[] conversation = { "Hello!", "HIHI", "Feel free to ask any questions." };
        int bubbleX = chatboxX + 10;
        int bubbleY = chatboxY + 10;

        foreach (string message in conversation)
        {
            DrawNeumorphicRectangle(e.Graphics, bubbleX, bubbleY, textBoxWidth - 20, 30, Color.LightGray);
            e.Graphics.DrawString(message, Font, Brushes.Black, bubbleX + 5, bubbleY + 5);

            bubbleY += 40;
        }

        // Draw reply box
        int replyBoxY = chatboxY + textBoxHeight + 10;
        DrawNeumorphicRectangle(e.Graphics, chatboxX, replyBoxY, textBoxWidth, 40, Color.White);

        // Create a TextBox for user input
        if (replyTextBox == null)
        {
            replyTextBox = new TextBox();
            replyTextBox.Size = new Size(textBoxWidth - 20, 30);
            replyTextBox.Location = new Point(chatboxX + 10, replyBoxY + 5);
            Controls.Add(replyTextBox);
        }

        // Create the button for sending replies
        createButton(chatboxX + textBoxWidth, replyBoxY, 70, 30, ButtonStyle.Neumorphic);
    }
}




        private void DrawNeumorphicRectangle(Graphics g, int x, int y, double width, double height, Color color)
        {
            // Customize the neumorphic style
            Color darkShadow = ControlPaint.Dark(color);
            Color lightShadow = ControlPaint.Light(color);

            // Draw neumorphic rectangle
            RectangleF rect = new RectangleF((float)x, (float)y, (float)width, (float)height);
            using (GraphicsPath path = RoundedRectangle(rect, 10)) // Use RoundedRectangle method to create a rounded rectangle path
            {
                using (Brush brush = new SolidBrush(color))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            float diameter = radius * 2;
            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // Top left corner
            path.AddArc(arc, 180, 90);

            // Top right corner
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right corner
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left corner
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            return path;
        }

        public enum ButtonStyle
        {
            Default,
            Neumorphic,
            // Add other styles as needed
        }

        void createButton(int X, int Y, int width, int height, ButtonStyle style = ButtonStyle.Default)
        {
            Button button = new Button();
            button.Text = "Click me";
            button.Size = new System.Drawing.Size(width, height);
            button.Location = new System.Drawing.Point(X, Y);

            // Call SetNeumorphicStyle to set the neumorphic style
            SetNeumorphicStyle(button);

            // Switch statement to set button style based on the chosen enum value
            switch (style)
            {
                case ButtonStyle.Neumorphic:
                    // Set neumorphic style (you need to implement this)
                    SetNeumorphicStyle(button);
                    break;
                // Add other cases for different styles if needed
                case ButtonStyle.Default:
                default:
                    // Use default style or any other default behavior
                    break;
            }

            Controls.Add(button);
            button.Click += Button_Click;
        }

        // You need to implement the SetNeumorphicStyle method based on your neumorphic styling logic
        private void SetNeumorphicStyle(Button button)
        {
            // Set initial background color with a subtle gradient
            button.BackColor = Color.FromArgb(240, 240, 240);

            // Set initial border color
            button.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);

            // Set initial box shadow
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 220, 220);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 255, 255);

            // Set initial text color
            button.ForeColor = Color.FromArgb(60, 60, 60);

            // Set rounded corners
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;

            // Add a subtle inner shadow effect for text
            button.Paint += (sender, e) =>
            {
                SizeF fontSize = e.Graphics.MeasureString(button.Text, button.Font);
                PointF textLocation = new PointF
                {
                    X = (button.Width - fontSize.Width) / 2,
                    Y = (button.Height - fontSize.Height) / 2
                };

                
                ControlPaint.DrawBorder(e.Graphics, button.ClientRectangle,
                    Color.Transparent, 20, ButtonBorderStyle.Solid,
                    Color.Transparent, 20, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 20, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 20, ButtonBorderStyle.Solid);
                using (Brush brush = new SolidBrush(button.ForeColor))
                {
                    e.Graphics.DrawString(button.Text, button.Font, brush, textLocation);
                }

            };

            // Handle MouseEnter event for hover animation
            button.MouseEnter += (sender, e) =>
            {
                AnimateHover(button, true);
            };

            // Handle MouseLeave event for hover animation
            button.MouseLeave += (sender, e) =>
            {
                AnimateHover(button, false);
            };
        }

        // Helper method to animate hover glow
        private void AnimateHover(Button button, bool isHovered)
        {
            Color startColor = button.BackColor;
            Color endColor = isHovered ? Color.FromArgb(255, 255, 255) : Color.FromArgb(240, 240, 240);

            Timer colorTimer = new Timer();
            colorTimer.Interval = 20; // Adjust the interval as needed for smoother or faster animation
            colorTimer.Tick += (sender, e) =>
            {
                int steps = 10; // Adjust the number of steps for the animation
                int step = colorTimer.Tag == null ? 0 : (int)colorTimer.Tag;
                float factor = (float)step / steps;

                int R = (int)(startColor.R + factor * (endColor.R - startColor.R));
                int G = (int)(startColor.G + factor * (endColor.G - startColor.G));
                int B = (int)(startColor.B + factor * (endColor.B - startColor.B));

                button.BackColor = Color.FromArgb(R, G, B);

                if (step < steps)
                {
                    step++;
                    colorTimer.Tag = step;
                }
                else
                {
                    colorTimer.Stop();
                    colorTimer.Dispose();
                }
            };

            colorTimer.Start();
        }

        void Button_Click(object sender, EventArgs e)
        {
            PerformInference();
        }
        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 10;

            switch (m.Msg)
            {
                // ... (your existing code for resizing)
            }

            base.WndProc(ref m);
        }
        private void DrawNeumorphicRectangle(Graphics g, int x, int y, double width, double height)
        {
            // Use a default color for the neumorphic rectangle
            Color defaultColor = Color.FromArgb(50, 50, 50);
            DrawNeumorphicRectangle(g, x, y, width, height, defaultColor);
        }

        private void DrawBlueLogo(Graphics g, int x, int y, double width, double height)
        {
            // Draw a blue logo in the middle of the circle
            int logoSize = 40;
            int logoX = (int)(x + (width - logoSize) / 2);
            int logoY = (int)(y + (height - logoSize) / 2);

            using (Brush brush = new SolidBrush(Color.Blue))
            {
                g.FillEllipse(brush, logoX, logoY, logoSize, logoSize);
                // You can customize the logo as needed
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000; // <--- use 0x20000
                return cp;
            }
        }
    }
}
#endregion
