using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Net.Cache;
using System.Reflection;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Korkboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Static Stuff
        public static DependencyProperty IsBoardEnabledProperty = DependencyProperty.Register("IsBoardEnabled", typeof(bool), typeof(MainWindow));

        public static string AppFolder { get; set; }

        static MainWindow()
        {
            AppFolder = Path.Combine
            (
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "John Sedlak",
                "QwikBoard",
                Assembly.GetExecutingAssembly().GetName().Version.ToString()
            );
        }
        #endregion

        private ClipboardMonitor monitor;
        private Timer timer;
        private ClipItem selectedItem = null;
        private bool isPinningItem = false;

        private bool isMouseDown;
        private bool isMouseDragging;
        private Point draggingStartPoint;
        private UIElement realDragSource;
        private UIElement dummyDragSource = new UIElement();

        public MainWindow()
        {
            InitializeComponent();

            BoardPanel.Children.Clear();

            IsBoardEnabled = true;
            
            Settings.PropertyChanged += OnSettingsChanged;

            LoadSettings();

            timer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            timer.Elapsed += (s, e) => { Dispatcher.BeginInvoke(new VoidHandler(OnTimerElapsed)); };
            timer.Start();
        }

        private void OnTimerElapsed()
        {
            if (Settings.TimeLimitInMinutes <= 0) return;

            //foreach (ClipItem clipItem in BoardPanel.Children)
            for(int i = BoardPanel.Children.Count - 1; i >= 0; i--)
            {
                ClipItem clipItem = BoardPanel.Children[i] as ClipItem;

                if (clipItem == null) continue;

                if (DateTime.Now.Subtract(clipItem.TimeStamp).TotalMinutes > Settings.TimeLimitInMinutes)
                {
                    // If the item is pinned, or active then we want to keep it
                    if (clipItem.IsPinned || clipItem.IsSelected)
                        continue;

                    clipItem.MouseLeftButtonUp -= OnClipItemClicked;
                    clipItem.IsPinnedChanged -= OnIsPinnedChanged;
                    BoardPanel.Children.Remove(clipItem);
                }
            }
        }

        private void PopItemOffStack()
        {
            if (BoardPanel.Children.Count > 0)
            {
                ClipItem clipItem = BoardPanel.Children[0] as ClipItem;

                if (clipItem != null) clipItem.MouseLeftButtonUp -= OnClipItemClicked;

                BoardPanel.Children.RemoveAt(0);
            }
        }

        private void LoadSettings()
        {
            AppSettings settings;

            try
            {
                settings = AppSettings.Load(Path.Combine(AppFolder, "settings.xml"));
            }
            catch (FileNotFoundException)
            {
                settings = new AppSettings { AlwaysOnTop = false, ItemNumberLimit = 0, TimeLimitInMinutes = 0 };
            }

            settings.CopyTo(Settings);
        }

        void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            Topmost = Settings.AlwaysOnTop;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            /*
            if (e.Property == Window.WindowStateProperty)
            {
                switch (WindowState)
                {
                    case WindowState.Maximized:
                        MinMaxIcon.Source = new BitmapImage(new Uri("Icons/application-resize-actual.png", UriKind.RelativeOrAbsolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                        break;
                    default:
                        MinMaxIcon.Source = new BitmapImage(new Uri("Icons/application-resize.png", UriKind.RelativeOrAbsolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                        break;
                }
            }
            */
        }

        protected ClipItem GetItemForData(IDataObject data)
        {
            for (int i = 0; i < BoardPanel.Children.Count; i++)
            {
                ClipItem ci = BoardPanel.Children[i] as ClipItem;

                if (ci != null && ci.ContainsData(data)) return ci;
            }

            return null;
        }

        protected bool AlreadyExists(IDataObject data)
        {
            return GetItemForData(data) != null;
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            monitor = new ClipboardMonitor();
            monitor.Changed += OnClipboardChanged;
            monitor.Initialize(this);

            base.OnSourceInitialized(e);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);

            if(!monitor.IsDisposed)
                monitor.Dispose();

            monitor = null;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != this && e.OriginalSource != LayoutRoot)
            {
                return;
            }

            /*if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                Left = e.MouseDevice.GetPosition(this).X;
                Top = e.MouseDevice.GetPosition(this).Y;
            }*/

            DragMove();

            e.Handled = true;
        }

        /// <summary>
        /// Handles when the clipboard has changed by adding the item to the list if it doesn't already exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            if (!IsBoardEnabled) return;        // Check to see that the board is accepting new items
            if (IsSendingToClipboard) return;   // Check to make sure we aren't setting data on the clipboard from Korkboard
            if (AlreadyExists(e.Data))          // Check to see if the item already exists
            {
                SelectedItem = GetItemForData(e.Data);

                return;
            }

            string format = ClipItem.GetFormat(e.Data);

            if (
                (!Settings.IsTextCopyEnabled && format.CompareEx(DataFormats.Text)) ||
                (!Settings.IsFileCopyEnabled && format.CompareEx(ClipItem.FileList)) ||
                (!Settings.IsImageCopyEnabled && format.CompareEx(ClipItem.BitmapSourceFormat)))
            {
                SelectedItem = null;

                return;
            }

            // Trim the list if necessary
            if (Settings.ItemNumberLimit > 0 && BoardPanel.Children.Count > Settings.ItemNumberLimit)
                PopItemOffStack();

            // Create the clip item
            ClipItem ci = new ClipItem(e.Data);

            ci.MouseLeftButtonUp += OnClipItemClicked;
            ci.IsPinnedChanged += OnIsPinnedChanged;

            int i = 0;
            for (i = 0; i < BoardPanel.Children.Count; i++)
            {
                ClipItem child = BoardPanel.Children[i] as ClipItem;

                if (child != null && !child.IsPinned) break;
            }

            BoardPanel.Children.Insert(i, ci);

            // Make it look like the trashbin is full
            TrashBinIcon.IsSelected = false;

            // Set the current item as "selected"
            SelectedItem = ci;
        }

        private void OnIsPinnedChanged(object sender, EventArgs e)
        {
            isPinningItem = true;

            int index = 0;

            for (index = 0; index < BoardPanel.Children.Count; index++)
            {
                if (BoardPanel.Children[index] == sender)
                    break;
            }

            int firstNonPinned = 0;

            for (firstNonPinned = 0; firstNonPinned < BoardPanel.Children.Count; firstNonPinned++)
            {
                ClipItem clipItem = BoardPanel.Children[firstNonPinned] as ClipItem;

                if ((clipItem != null && !clipItem.IsPinned) || BoardPanel.Children[firstNonPinned] == sender) break;
            }

            if (index == 0 || index == firstNonPinned) return;

            BoardPanel.Children.RemoveAt(index);

            if (firstNonPinned >= BoardPanel.Children.Count) BoardPanel.Children.Add(sender as UIElement);
            else BoardPanel.Children.Insert(firstNonPinned, sender as UIElement);
        }

        private void OnClipItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (isPinningItem)
            {
                isPinningItem = false;
                return;
            }

            ClipItem ci = sender as ClipItem;

            if (ci == null) return;

            IsSendingToClipboard = true;

            SelectedItem = ci;

            for (int i = 0; i < BoardPanel.Children.Count; i++)
            {
                ClipItem clipItem = BoardPanel.Children[i] as ClipItem;

                if (clipItem == null) continue;

                clipItem.IsSelected = false;
            }

            ci.SendToClipboard();

            IsSendingToClipboard = false;
        }

        protected void OnMinimizeClicked(object sender, MouseEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void OnCloseClick(object sender, MouseEventArgs e)
        {
            Close();
        }

        protected void OnMinMaxIconClick(object sender, MouseEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        protected void OnAlwaysOnTopIconClicked(object sender, MouseButtonEventArgs e)
        {
            Settings.AlwaysOnTop = !Settings.AlwaysOnTop;
            
            /*
            Topmost = !Topmost;

            if(Topmost)
                AlwaysOnTopIcon.Source = new BitmapImage(new Uri("Icons/applications.png", UriKind.RelativeOrAbsolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
            else
                AlwaysOnTopIcon.Source = new BitmapImage(new Uri("Icons/applications-blue.png", UriKind.RelativeOrAbsolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
          */
        }

        protected void OnOnOffSwitchIconClicked(object sender, MouseButtonEventArgs e)
        {
            IsBoardEnabled = !IsBoardEnabled;
        }

        private void OptionsIcon_Clicked(object sender, MouseButtonEventArgs e)
        {
            //new OptionsDialog(Settings).ShowDialog();

            OptionsDialog od = new OptionsDialog(Settings.GetCopy());

            bool? result = od.ShowDialog();

            if (result != null && result.HasValue && result.Value)
            {
                od.Settings.CopyTo(Settings);

                Settings.Save(Path.Combine(AppFolder, "settings.xml"));
            }
        }

        private void OnClearKorkboardClicked(object sender, MouseButtonEventArgs e)
        {
            TrashBinIcon.IsSelected = true;

            for (int i = BoardPanel.Children.Count - 1; i >= 0; i--)
            {
                ClipItem clipItem = BoardPanel.Children[i] as ClipItem;

                if (clipItem == null) return;

                if (clipItem.IsSelected || clipItem.IsPinned) continue;

                BoardPanel.Children.RemoveAt(i);
            }
        }

        public bool IsBoardEnabled
        {
            get { return (bool)GetValue(IsBoardEnabledProperty); }
            set { SetValue(IsBoardEnabledProperty, value); }
        }

        public AppSettings Settings
        {
            get { return FindResource("CustomSettings") as AppSettings; }
        }

        public bool IsSendingToClipboard { get; private set; }

        public ClipItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != null) selectedItem.IsSelected = false;
                if (value != null) value.IsSelected = true;

                selectedItem = value;
            }
        }

        #region Stack Panel Drag / Drop
        private void sp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == this.BoardPanel)
            {
            }
            else
            {
                isMouseDown = true;
                draggingStartPoint = e.GetPosition(this.BoardPanel);
            }
        }

        private void sp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            isMouseDragging = false;

            if (realDragSource == null)
                return;

            realDragSource.ReleaseMouseCapture();
        }

        private void sp_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                if ((isMouseDragging == false) && ((Math.Abs(e.GetPosition(this.BoardPanel).X - draggingStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(this.BoardPanel).Y - draggingStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    isMouseDragging = true;
                    realDragSource = e.Source as UIElement;
                    realDragSource.CaptureMouse();
                    DragDrop.DoDragDrop(dummyDragSource, new DataObject("UIElement", e.Source, true), DragDropEffects.Move);
                }
            }
        }

        private void sp_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void sp_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                UIElement droptarget = e.Source as UIElement;
                int droptargetIndex = -1, i = 0;

                int currentIndex = 0;
                for (currentIndex = 0; i < BoardPanel.Children.Count; currentIndex++)
                {
                    if (BoardPanel.Children[currentIndex] == realDragSource)
                        break;
                }

                foreach (UIElement element in this.BoardPanel.Children)
                {
                    if (element.Equals(droptarget))
                    {
                        droptargetIndex = i;
                        break;
                    }
                    i++;
                }

                if (droptargetIndex != -1)
                {
                    if (currentIndex < droptargetIndex) droptargetIndex = Math.Max(droptargetIndex - 1, 0);

                    if (currentIndex != droptargetIndex)
                    {
                        this.BoardPanel.Children.Remove(realDragSource);
                        this.BoardPanel.Children.Insert(droptargetIndex, realDragSource);
                    }
                }

                isMouseDown = false;
                isMouseDragging = false;
                realDragSource.ReleaseMouseCapture();
            }
        }  
        #endregion

        
    }
}
