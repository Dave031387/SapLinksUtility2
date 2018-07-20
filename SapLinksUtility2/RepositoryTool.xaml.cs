﻿using SapLinksUtility2.Windows;
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
    /// Interaction logic for RepositoryToolPage.xaml
    /// </summary>
    public partial class RepositoryToolPage : Page
    {
        public RepositoryToolPage()
        {
            InitializeComponent();
        }

        private void DataElementButton_Click(object sender, RoutedEventArgs e)
        {
            DataFileWindow dataFileWindow = new DataFileWindow();
            dataFileWindow.ShowDialog();
        }
    }
}
