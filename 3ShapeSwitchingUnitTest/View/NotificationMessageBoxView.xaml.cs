using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using ThreeShapeSwitchingUnitTest.Commands;
using ThreeShapeSwitchingUnitTest.Loggers;
using ThreeShapeSwitchingUnitTest.Tests;
using ThreeShapeSwitchingUnitTest.Controls.MessageBox.Views;

using System.Collections.ObjectModel;

using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;

using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ThreeShapeSwitchingUnitTest.Controls.MessageBox.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {

        public NotificationWindow()
        {
            InitializeComponent();
        }
        public NotificationWindow(string description, string imageSource)
        {
            InitializeComponent();
            DescriptionTextBox.Text = description;
            BitmapImage image = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));
            InstructionImage.Source = image;
        }
        private void yesButton_Click(object sender, RoutedEventArgs e) =>
            DialogResult = true;

        private void noButton_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
        public void Setup(string tittle, string description, string imageSource)
        {
            this.Title = tittle;
            DescriptionTextBox.Text = description;
            BitmapImage image = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));
            InstructionImage.Source = image;
        }
    }
}