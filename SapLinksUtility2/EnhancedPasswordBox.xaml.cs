using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SapLinksUtility2
{
    /// <summary>
    /// Interaction logic for EnhancedPasswordBox.xaml
    /// </summary>
    public partial class EnhancedPasswordBox : UserControl
    {
        public EnhancedPasswordBox()
        {
            InitializeComponent();
            toggleButton.IsChecked = true;
            toggleButton.Content = "Show";
            textBox.Visibility = Visibility.Hidden;
            passwordBox.Visibility = Visibility.Visible;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)toggleButton.IsChecked)
            {
                toggleButton.Content = "Show";
                textBox.Visibility = Visibility.Hidden;
                passwordBox.Visibility = Visibility.Visible;
                passwordBox.Password = textBox.Text;
            }
            else
            {
                toggleButton.Content = "Hide";
                passwordBox.Visibility = Visibility.Hidden;
                textBox.Visibility = Visibility.Visible;
                textBox.Text = passwordBox.Password;
            }
        }
    }
}
