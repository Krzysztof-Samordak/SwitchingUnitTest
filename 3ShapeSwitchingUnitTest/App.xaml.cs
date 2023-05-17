using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace ThreeShapeSwitchingUnitTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void Application_Startup(object sender, StartupEventArgs args)
        {
            Debug.WriteLine("");
            this.MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel.MainViewModel()
            };
            MainWindow.Show();
        }
    }
}
