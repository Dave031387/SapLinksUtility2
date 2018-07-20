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

namespace SapLinksUtility2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainWindowFrame.Content = new WelcomePage();
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (toolsPanelScrollViewer.Visibility == Visibility.Collapsed)
            {
                gradientPanel.Visibility = Visibility.Visible;
                toolsPanelScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                toolsPanelScrollViewer.Visibility = Visibility.Collapsed;
                gradientPanel.Visibility = Visibility.Hidden;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(mainWindowFrame.Content is SettingsToolPage))
            {
                mainWindowFrame.Content = new SettingsToolPage();
            }
            toolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            gradientPanel.Visibility = Visibility.Hidden;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(mainWindowFrame.Content is WelcomePage))
            {
                mainWindowFrame.Content = new WelcomePage();
            }
            toolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            gradientPanel.Visibility = Visibility.Hidden;
        }

        private void RepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(mainWindowFrame.Content is RepositoryToolPage))
            {
                mainWindowFrame.Content = new RepositoryToolPage();
            }
            toolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            gradientPanel.Visibility = Visibility.Hidden;
        }
    }
}
