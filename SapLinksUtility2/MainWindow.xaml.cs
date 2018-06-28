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
            MainWindowFrame.Content = new WelcomePage();
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToolsPanelScrollViewer.Visibility == Visibility.Collapsed)
            {
                GradientPanel.Visibility = Visibility.Visible;
                ToolsPanelScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                ToolsPanelScrollViewer.Visibility = Visibility.Collapsed;
                GradientPanel.Visibility = Visibility.Hidden;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainWindowFrame.Content is SettingsToolPage))
            {
                MainWindowFrame.Content = new SettingsToolPage();
            }
            ToolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            GradientPanel.Visibility = Visibility.Hidden;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainWindowFrame.Content is WelcomePage))
            {
                MainWindowFrame.Content = new WelcomePage();
            }
            ToolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            GradientPanel.Visibility = Visibility.Hidden;
        }

        private void RepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainWindowFrame.Content is RepositoryToolPage))
            {
                MainWindowFrame.Content = new RepositoryToolPage();
            }
            ToolsPanelScrollViewer.Visibility = Visibility.Collapsed;
            GradientPanel.Visibility = Visibility.Hidden;
        }
    }
}
