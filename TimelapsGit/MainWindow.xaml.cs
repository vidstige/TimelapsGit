using System;
using System.Windows;
using GitSharp;
using System.IO;
using System.Linq;

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

            //var repo = new Repository(@"F:\src\eliteprospects");
            //DataContext = new MainViewModel(repo, DateTime.Now) { File = @"AndroidManifest.xml" };
        }

        private void DropList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Link;
            }

            e.Effects = DragDropEffects.None;
        }

        private void DropList_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            var path = droppedFilenames.First();
            var repo = GetRepository(new DirectoryInfo(Path.GetDirectoryName(path)));
            DataContext = new MainViewModel(repo, DateTime.Now) { File = Relativize(repo, path) };
        }

        private string Relativize(Repository repo, string path)
        {
            if (path.StartsWith(repo.WorkingDirectory))
            {
                return path.Substring(repo.WorkingDirectory.Length).TrimStart("\\".ToArray());
            }
            return path;
        }

        private Repository GetRepository(DirectoryInfo folder)
        {
            if (Repository.IsValid(folder.FullName)) return new Repository(folder.FullName);
            return GetRepository(folder.Parent);
        }
    }
}
