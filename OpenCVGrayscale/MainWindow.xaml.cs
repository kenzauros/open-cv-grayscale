using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OpenCVGrayscale
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Test();
            }
        }

        void Test()
        {
            var blue = 0.114;
            var green = 0.587;
            var red = 0.299;

            Debug.WriteLine(new string('-', 80));

            var mat = Cv2.ImRead("lenna.jpg", ImreadModes.Color);
            mat = mat.Resize(mat.Size(), 2.0, 2.0);
            original.Source = mat.ToBitmapSource();

            var watch = new Stopwatch();
            watch.Start();

            // CvtColor バージョン

            var mat1 = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC1);
            Cv2.CvtColor(mat, mat1, ColorConversionCodes.BGR2GRAY);
            grayscale1.Source = mat1.ToBitmapSource();

            Debug.WriteLine($"CvtColor: {mat1.Sum().Val0}, Type: {mat1.Type()}, takes {watch.ElapsedTicks}"); watch.Restart();

            // Mat.Transform バージョン

            var coefficients = new[] { blue, green, red };

            var tr = new Mat(1, 3, MatType.CV_64F, coefficients);
            var transformed = mat.Transform(tr);
            var mat2 = transformed;
            grayscale2.Source = mat2.ToBitmapSource();

            Debug.WriteLine($"Transform: {mat2.Sum().Val0}, Type: {mat2.Type()}, takes {watch.ElapsedTicks}"); watch.Restart();

            // 配列 (浮動小数) バージョン

            var a1 = new byte[mat.Total() * mat.Channels()];
            Marshal.Copy(mat.Data, a1, 0, a1.Length);

            var res = new byte[mat.Width * mat.Height];
            var n = 0;
            for (int i = 0; i < res.Length; i++)
            {
                var c = (a1[n] * blue + a1[n + 1] * green + a1[n + 2] * red);
                res[i] = (byte)Math.Round(c);
                n += 3;
            }
            var mat3 = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC1, res);
            grayscale3.Source = mat3.ToBitmapSource();
            Debug.WriteLine($"Array1: {mat3.Sum().Val0}, Type: { mat3.Type() }, takes {watch.ElapsedTicks}"); watch.Restart();
            
            // 配列 (ビットシフト) バージョン

            var coefficientsN = new[] { (int)(blue * 65536), (int)(green * 65536), (int)(red * 65536) };

            var res2 = new byte[mat.Width * mat.Height];
            var m = 0;
            for (int i = 0; i < res.Length; i++)
            {
                var c = ((
                    a1[m] * coefficientsN[0] +
                    a1[m + 1] * coefficientsN[1] +
                    a1[m + 2] * coefficientsN[2]
                    ) >> 16);
                res[i] = (byte)c;
                m += 3;
            }
            var mat4 = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC1, res);
            grayscale3.Source = mat4.ToBitmapSource();
            Debug.WriteLine($"Array2: {mat4.Sum().Val0}, Type: { mat4.Type() }, takes {watch.ElapsedTicks}"); watch.Restart();
        }
    }
}
