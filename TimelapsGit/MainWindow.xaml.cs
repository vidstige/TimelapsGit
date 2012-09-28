using System;
using System.Windows;
using GitSharp;

namespace TimelapsGit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var repo = new Repository(@"F:\src\eliteprospects");
            DataContext = new MainViewModel(repo) { File = @"AndroidManifest.xml" };
        }
    }
}
