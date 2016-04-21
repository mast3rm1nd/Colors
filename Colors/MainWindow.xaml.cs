using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;



namespace Colors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);



        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);


        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);


        public MainWindow()
        {
            InitializeComponent();
            UpdateImage();
            UpdateAllTextFields();

            checkForTime.Elapsed += new ElapsedEventHandler(checkForTime_Elapsed);

            //this.Cursor = Cursors.Hand;
        }

        

        private void Slider_R_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateAllTextFields();

            UpdateImage();
        }

        private void Slider_G_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateAllTextFields();

            UpdateImage();
        }

        private void Slider_B_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateAllTextFields();

            UpdateImage();
        }


        void UpdateAllTextFields()
        {
            TextBox_R_Dec.Text = Slider_R.Value.ToString();
            TextBox_G_Dec.Text = Slider_G.Value.ToString();
            TextBox_B_Dec.Text = Slider_B.Value.ToString();

            TextBox_R_Hex.Text = String.Format("{0:X2}", (int)Slider_R.Value);
            TextBox_G_Hex.Text = String.Format("{0:X2}", (int)Slider_G.Value);
            TextBox_B_Hex.Text = String.Format("{0:X2}", (int)Slider_B.Value);

            TextBox_HTML_Code.Text = String.Format("#{0:X2}{1:X2}{2:X2}", (int)Slider_R.Value, (int)Slider_G.Value, (int)Slider_B.Value);
        }


        void UpdateImage()
        {
            Bitmap bmp = new Bitmap((int)Image_Color.Width, (int)Image_Color.Height);
            System.Drawing.Color color = System.Drawing.Color.FromArgb((byte)Slider_R.Value, (byte)Slider_G.Value, (byte)Slider_B.Value);

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                    bmp.SetPixel(x, y, color);


            Image_Color.Source = BitmapToImageSource(bmp);
        }


        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }



        System.Timers.Timer checkForTime = new System.Timers.Timer(200);
        private void Button_Get_Color_from_Screen_Click(object sender, RoutedEventArgs e)
        {
            if (checkForTime.Enabled)
            {
                checkForTime.Enabled = false;
                Button_Get_Color_from_Screen.Content = "Get Color from Screen";
            }
            else
            {
                checkForTime.Enabled = true;
                Button_Get_Color_from_Screen.Content = "Stop color picking";
            }
            //var c = GetColorAt(cursor);
        }



        
        private void checkForTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Factory.StartNew(this.GetColorAndUpdateUI);
            //System.Windows.Point cursor = new System.Windows.Point();
            //GetCursorPos(ref cursor);

            //if (cursor == prevPos) //чтобы только при изменении положения работало
            //    return;

            //prevPos = cursor;

            //var color = GetPixelColor((int)cursor.X, (int)cursor.Y);

            //Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            //    Slider_R.Value = color.R;
            //    Slider_G.Value = color.G;
            //    Slider_B.Value = color.B;

            //    UpdateAllTextFields();
            //    UpdateImage();
            //    //InvalidateVisual;
            //    //
            //}));

            //Dispatcher.Invoke(InvalidateVisual);
        }



        System.Drawing.Point prevPos = new System.Drawing.Point();
        void GetColorAndUpdateUI()
        {
            System.Drawing.Point cursor = new System.Drawing.Point();
            //System.Windows.Point cursor = new System.Windows.Point();

            GetCursorPos(ref cursor);

            if (cursor == prevPos) //чтобы только при изменении положения работало
                return;

            prevPos = cursor;

            

            var color = GetColorAt(cursor);

            //Dispatcher.BeginInvoke(new Action(delegate ()
            //{
            Dispatcher.Invoke(new Action(() =>
            {
                //TextBox_HTML_Code.Text = String.Format("{0:F2}, {1:F2}", cursor.X, cursor.Y);
                Slider_R.Value = color.R;
                Slider_G.Value = color.G;
                Slider_B.Value = color.B;

                UpdateAllTextFields();
                UpdateImage();
            }));
            //InvalidateVisual;
            //
            //}));
        }



        Bitmap screenPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        public System.Drawing.Color GetColorAt(System.Drawing.Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))        //если событие вызвано перетаскиванием файлов
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);    //получаем список всех перетащеных файлов

                if (files.Count() != 1)                              //если файл не 1
                {
                    MessageBox.Show("Поддерживается обработка только одного файла."); //то говорим, что можно только 1 (=
                    return;                                                           //уходим
                }

                var image = System.Drawing.Image.FromFile(files[0]);

                var meanColor = GetMeanColorOfImage(image);

                Slider_R.Value = meanColor.R;
                Slider_G.Value = meanColor.G;
                Slider_B.Value = meanColor.B;

                UpdateAllTextFields();
                UpdateImage();
            }
        }

        System.Drawing.Color GetMeanColorOfImage(System.Drawing.Image image)
        {
            uint r = 0;
            uint g = 0;
            uint b = 0;

            uint totalPixels = (uint)image.Height * (uint)image.Width;

            using (Bitmap bmp = new Bitmap(image))
            {
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        var color = bmp.GetPixel(x, y);

                        r += color.R;
                        g += color.G;
                        b += color.B;
                    }
            }

            r /= totalPixels;
            g /= totalPixels;
            b /= totalPixels;
            
            return System.Drawing.Color.FromArgb(r, g, b);
        }


        //static public System.Drawing.Color GetPixelColor(int x, int y)
        //{
        //    IntPtr hdc = GetDC(IntPtr.Zero);
        //    uint pixel = GetPixel(hdc, x, y);
        //    ReleaseDC(IntPtr.Zero, hdc);
        //    System.Drawing.Color color = System.Drawing.Color.FromArgb((int)(pixel & 0x000000FF),
        //                 (int)(pixel & 0x0000FF00) >> 8,
        //                 (int)(pixel & 0x00FF0000) >> 16);
        //    return color;
        //}
    }
}
