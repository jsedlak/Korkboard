using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Korkboard
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        private Dictionary<string, UserControl> controls = new Dictionary<string, UserControl>();
        private UserControl currentControl = null;

        public OptionsDialog(AppSettings settings)
        {
            InitializeComponent();

            Settings = settings;

            //System.Diagnostics.Debug.WriteLine("Received settings");
            //System.Diagnostics.Debug.WriteLine("    AlwaysOnTop: " + settings.AlwaysOnTop.ToString());
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem currentItem = OptionsList.SelectedItem as TreeViewItem;

            if (!(currentItem.Tag is string)) return;

            string tag = (string)currentItem.Tag;

            if (string.IsNullOrWhiteSpace(tag)) return;

            SwitchTo(tag);
        }

        private void SwitchTo(string controlType)
        {
            if (currentControl != null && currentControl.GetType().Name == controlType) return;

            if (controls.ContainsKey(controlType))
            {
                SwitchTo(controls[controlType]);
                return;
            }

            UserControl userControl = Activator.CreateInstance(FocusedGames.Reflection.TypeHelper.FindType(controlType)) as UserControl;
            
            //System.Diagnostics.Debug.WriteLine("Binding settings to options control");
            userControl.DataContext = Settings;
            //System.Diagnostics.Debug.WriteLine("    AlwaysOnTop: " + Settings.AlwaysOnTop.ToString());

            if (userControl != null)
            {
                ControlRoot.Children.Add(userControl);

                controls.Add(controlType, userControl);
                SwitchTo(userControl);
            }
        }

        private void SwitchTo(UserControl control)
        {
            if (currentControl != null) currentControl.Visibility = System.Windows.Visibility.Hidden;

            currentControl = control;
            control.Visibility = System.Windows.Visibility.Visible;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public AppSettings Settings { get; set; }
    }
}
