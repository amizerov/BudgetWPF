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
using System.Windows.Shapes;
using Microsoft.Win32;
using DevExpress.Xpf.Core;
using System.IO;

namespace Budget
{
    public partial class AddImage : DXWindow
    {
        private string _initialImg;
        private string _imgPath;

        public delegate void ImageChooseHandler(string path);
        public event ImageChooseHandler OnImageChosen;

        public AddImage(string imgPath)
        {
            InitializeComponent();

            _initialImg = _imgPath = !String.IsNullOrEmpty(imgPath) ? imgPath : String.Empty;
            if (!String.IsNullOrEmpty(imgPath))
                imgAttach.Source = new BitmapImage(new Uri(imgPath));
        }

        private void imgAttach_Validate(object sender, DevExpress.Xpf.Editors.ValidationEventArgs e)
        {
            if (!(e.Value is BitmapImage))
            {
                _imgPath = String.Empty;
                return;
            }

            var img = (BitmapImage)e.Value;
            var str = (FileStream)img.StreamSource;

            if (str != null)
                _imgPath = str.Name;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            OnImageChosen(_imgPath);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            OnImageChosen(_initialImg);
            this.Close();
        }
    }
}
