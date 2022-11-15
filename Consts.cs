using System.Collections.Generic;
using DevExpress.Xpf.Core;
using System.Windows.Media;
using DevExpress.LookAndFeel;

namespace Budget
{
    public class Consts
    {
        public enum OperationType
        {
            Credit = 0,
            Debet,
            None
        }

        /// <summary>
        /// Тема для общего внешнего вида
        /// </summary>
        public static Dictionary<int, Theme> DevExTheme = new Dictionary<int, Theme>()
        {
            { 1, Theme.VS2019Dark },
            { 2, Theme.DXStyle },
            { 3, Theme.LightGray },
            { 4, Theme.Office2007Black },
            { 5, Theme.Office2007Blue },
            { 6, Theme.Office2007Silver },
            { 7, Theme.Seven },
            { 8, Theme.Office2019Colorful },
            { 9, Theme.Win10Light }
        };
        public static Dictionary<int, SkinStyle> WinFormsSkin = new Dictionary<int, SkinStyle>()
        {
            { 1, SkinStyle.VisualStudio2013Dark },
            { 2, SkinStyle.DevExpress },
            { 3, SkinStyle.Office2013LightGray },
            { 4, SkinStyle.Office2007Black },
            { 5, SkinStyle.Office2007Blue },
            { 6, SkinStyle.Office2007Silver },
            { 7, SkinStyle.Seven },
            { 8, SkinStyle.Office2019Colorful },
            { 9, SkinStyle.VisualStudio2013Light }
        };

        /// <summary>
        /// Цвет фона баннера в зависимости от выбранной темы
        /// </summary>
        public static Dictionary<Theme, Color> BannerColor = new Dictionary<Theme, Color>()
        {
            { Theme.VS2019Dark, Color.FromRgb(50, 90, 152) },
            { Theme.DXStyle, Color.FromRgb(245, 245, 246) },
            { Theme.LightGray, Color.FromRgb(92, 147, 209) },
            { Theme.Office2007Black, Color.FromRgb(226, 228, 231) },
            { Theme.Office2007Blue, Color.FromRgb(223, 236, 255) },
            { Theme.Office2007Silver, Color.FromRgb(234, 236, 238) },
            { Theme.Seven, Color.FromRgb(248, 248, 248) },
            { Theme.Office2019Colorful, Color.FromRgb(226, 229, 235) },
            { Theme.Win10Light, Color.FromRgb(226, 229, 235) }
        };

        /// <summary>
        /// Цвет шрифта в баннере в зависимости от выбранной темы
        /// </summary>
        public static Dictionary<Theme, Color> BannerTextFontColor = new Dictionary<Theme, Color>()
        {
            { Theme.VS2019Dark, Color.FromRgb(192, 235, 255) },
            { Theme.DXStyle, Colors.Black },
            { Theme.LightGray, Colors.White },
            { Theme.Office2007Black, Colors.Black },
            { Theme.Office2007Blue, Colors.Black },
            { Theme.Office2007Silver, Colors.Black },
            { Theme.Seven, Colors.Black },
            { Theme.Office2019Colorful, Colors.Black },
            { Theme.Win10Light, Colors.Black }
        };
    }
}