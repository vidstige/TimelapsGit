using System;
using System.Windows;
using GitSharp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TimelapsGit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
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
            try
            {
                var droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (droppedFilenames != null)
                {
                    var path = droppedFilenames.First();
                    var repo = GetRepository(new DirectoryInfo(Path.GetDirectoryName(path)));
                    _viewModel = new MainViewModel(repo);
                    DataContext = _viewModel;
                    new Task(() => _viewModel.Load(Relativize(repo, path))).Start();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Could not open the file: " + ex.Message);
            }
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
            if (folder.Root.Name == folder.Name) throw new NotInsideGitRepository("File is not inside a git-repository");
            if (Repository.IsValid(folder.FullName)) return new Repository(folder.FullName);
            return GetRepository(folder.Parent);
        }
    }

    internal class NotInsideGitRepository : Exception
    {
        public NotInsideGitRepository(string message): base(message)
        {
            
        }
    }
}
