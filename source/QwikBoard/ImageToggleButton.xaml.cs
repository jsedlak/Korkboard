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
using System.Windows.Media.Effects;

namespace Korkboard
{
    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageToggleButton : UserControl
    {
        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ImageToggleButton));

        public static DependencyProperty CurrentImageProperty = DependencyProperty.Register("CurrentImage", typeof(ImageSource), typeof(ImageToggleButton));
        public static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageToggleButton));
        public static DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(ImageSource), typeof(ImageToggleButton));
        public static DependencyProperty ImageEffectProperty = DependencyProperty.Register("ImageEffect", typeof(Effect), typeof(ImageToggleButton), new PropertyMetadata(new DropShadowEffect { ShadowDepth = 2 }));

        public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent("Clicked", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(ImageToggleButton));

        public ImageToggleButton()
        {
            InitializeComponent();

            //Icon.Source = Image;
        }

        protected void RaiseClickedEvent(MouseButtonEventArgs e)
        {
            RaiseEvent(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ImageProperty && CurrentImage == null)
                CurrentImage = Image;
            else if (e.Property == IsSelectedProperty)
            {
                if ((bool)e.NewValue) CurrentImage = SelectedImage;
                else CurrentImage = Image;
            }
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSelected = !IsSelected;

            MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice) 
            { 
                RoutedEvent = ClickedEvent, 
                Source = this 
            };

            RaiseClickedEvent(args);
        }

        /// <summary>
        /// Gets or Sets whether or not the button is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the current image of the button.
        /// </summary>
        public ImageSource CurrentImage
        {
            get { return GetValue(CurrentImageProperty) as ImageSource; }
            set { SetValue(CurrentImageProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the non-selected image of the button.
        /// </summary>
        public ImageSource Image
        {
            get { return GetValue(ImageProperty) as ImageSource; }
            set
            { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the selected image of the button.
        /// </summary>
        public ImageSource SelectedImage
        {
            get { return GetValue(SelectedImageProperty) as ImageSource; }
            set { SetValue(SelectedImageProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the effect applied to the image.
        /// </summary>
        public Effect ImageEffect
        {
            get { return GetValue(ImageEffectProperty) as Effect; }
            set { SetValue(ImageEffectProperty, value); }
        }

        public event MouseButtonEventHandler Clicked
        {
            add { AddHandler(ClickedEvent, value); }
            remove { RemoveHandler(ClickedEvent, value); }
        }
    }
}
