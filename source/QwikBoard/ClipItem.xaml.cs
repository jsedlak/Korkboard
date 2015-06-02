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
using System.Collections.Specialized;

namespace Korkboard
{
    /// <summary>
    /// Interaction logic for ClipItem.xaml
    /// </summary>
    public partial class ClipItem : UserControl
    {
        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ClipItem));
        public static DependencyProperty IsPinnedProperty = DependencyProperty.Register("IsPinned", typeof(bool), typeof(ClipItem));
        public static DependencyProperty TimeStampProperty = DependencyProperty.Register("TimeStamp", typeof(DateTime), typeof(ClipItem));

        public static Brush UnselectedForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
        public static Brush SelectedForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        public static Brush UnselectedBrush = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        public static Brush SelectedBrush = new SolidColorBrush(Color.FromArgb(255, 46, 9, 39));
        public static Brush HoverBrush = new SolidColorBrush(Color.FromArgb(255, 5, 143, 135));

        public const string BitmapSourceFormat = "System.Windows.Media.Imaging.BitmapSource";
        public const string FileList = "Shell IDList Array";

        public event EventHandler IsPinnedChanged;

        private Brush oldBrush;
        private readonly List<TextBlock> _blocks = new List<TextBlock>();

        public ClipItem()
        {
            InitializeComponent();

            TimeStamp = DateTime.Now;

            Background = UnselectedBrush;
        }

        public ClipItem(IDataObject data)
            : this()
        {
            Data = data;

            Initialize();
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            Background = oldBrush;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            oldBrush = Background;
            Background = HoverBrush;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

            Background = oldBrush;
        }

        public static string GetFormat(IDataObject data)
        {
            string[] formats = data.GetFormats(true);

            if (formats.Contains(DataFormats.Text))
            {
                return DataFormats.Text;
            }
            else if (formats.Contains(BitmapSourceFormat))
            {
                return BitmapSourceFormat;
            }
            else if (formats.Contains(FileList))
            {
                return FileList;
            }

            return null;
        }

        public void Initialize()
        {
            string format = GetFormat(Data);
            switch (format)
            {
                case "Text":
                    string textData = (string)Data.GetData(DataFormats.Text);

                    TextBlock tb = new TextBlock();
                    tb.Margin = new Thickness(10);
                    tb.Text = textData;
                    tb.Foreground = UnselectedForegroundBrush;

                    _blocks.Add(tb);

                    RootBorder.Child = tb;
                    //AddVisualChild(tb);

                    Format = DataFormats.Text;
                    FormattedData = textData;
                    break;
                case BitmapSourceFormat:
                    BitmapSource source = (BitmapSource)Data.GetData(BitmapSourceFormat);

                    Image img = new Image();
                    img.Margin = new Thickness(10);
                    img.Source = source;

                    RootBorder.Child = img;
                    //AddVisualChild(img);

                    Format = BitmapSourceFormat;
                    FormattedData = source;
                    break;
                case FileList:
                    StringCollection files = Clipboard.GetFileDropList();

                    string output = "";

                    foreach (string s in files) output += s + "\r\n";

                    TextBlock txb = new TextBlock();
                    txb.Margin = new Thickness(10);
                    txb.Text = output;
                    txb.Foreground = UnselectedForegroundBrush;

                    _blocks.Add(txb);

                    RootBorder.Child = txb;
                    //AddVisualChild(tb);

                    Format = FileList;
                    FormattedData = files;
                    break;
            }
        }

        public bool ContainsData(IDataObject data)
        {
            string format = GetFormat(data);

            object val = data.GetData(format);

            /*if (format == Format && val == FormattedData) return true;

            return false;*/
            return format.CompareEx(Format) && val.Equals(FormattedData);
        }

        public void SendToClipboard()
        {
            if (Format.CompareEx(BitmapSourceFormat))
                Clipboard.SetImage(FormattedData as BitmapSource);
            else if (Format.CompareEx(FileList))
                Clipboard.SetFileDropList((StringCollection)FormattedData);
            else
                Clipboard.SetData(Format, FormattedData);

            IsSelected = true;
        }

        private void PinButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            IsPinned = PinButton.IsSelected;
        }

        public DateTime TimeStamp
        {
            get { return (DateTime)GetValue(TimeStampProperty); }
            set { SetValue(TimeStampProperty, value); }
        }

        public string Format { get; private set; }
        public object FormattedData { get; private set; }

        public IDataObject Data { get; private set; }

        public bool IsPinned
        {
            get { return (bool)GetValue(IsPinnedProperty); }
            set
            {
                SetValue(IsPinnedProperty, value);

                if (IsPinnedChanged != null) IsPinnedChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set 
            {
                SetValue(IsSelectedProperty, value);

                if (value)
                {
                    Background = SelectedBrush;
                    _blocks.ForEach(m => m.Foreground = SelectedForegroundBrush);

                }
                else
                {
                    Background = UnselectedBrush;
                    _blocks.ForEach(m => m.Foreground = UnselectedForegroundBrush);
                }
            }
        }
    }
}
