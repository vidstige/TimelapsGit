using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GitSharp;
using GitSharp.Core.Diff;
using GitSharp.Core.Util;

namespace TimelapsGit
{
    public class MainViewModel: ViewModel, ITimeSpanContainer
    {
        private readonly Repository _repository;
        private ObservableCollection<LineAndCommit> _lines = new ObservableCollection<LineAndCommit>();
        private List<CommitViewModel> _commits = new List<CommitViewModel>();
        private int _selectedCommitNumber = 1;
        private readonly DateTime _now;

        private string _path;
        private readonly AgeGradient _ageGradient;

        public MainViewModel(Repository repository, DateTime now)
        {
            _repository = repository;
            _now = now;
            _ageGradient = new AgeGradient(this);
        }

        public List<CommitViewModel> Commits
        {
            get { return _commits; }
        }

        public int SelectedCommitNumber
        {
            get { return _selectedCommitNumber; }
            set
            {
                _selectedCommitNumber = value;

                Lines = new ObservableCollection<LineAndCommit>(Blame(SelectedCommit, _path));

                RaisePropertyChanged("SelectedCommitNumber");
                RaisePropertyChanged("CurrentMessage");
            }
        }

        public string CurrentMessage
        {
            get { return SelectedCommit.Message; }
        }

        private Commit SelectedCommit
        {
            get { return _commits[_selectedCommitNumber - 1].Commit; }
        }

        public string File
        {
            set
            {
                _path = value.Replace('\\', '/');

                _commits = _repository.CurrentBranch.CurrentCommit.Ancestors.Where(c => c.Changes.Any(change => change.Path == _path)).Select(c => new CommitViewModel(c)).ToList();
                RaisePropertyChanged("Commits");

                SelectedCommitNumber = 1;
            }
            get { return _path; }
        }

        // TODO: This method already exists in the Text class...
        private string GetLine(Encoding encoding, RawText text, int n)
        {
            if (n == text.LineStartIndices.size() - 1) return string.Empty;

            int start = text.LineStartIndices.get(n+1);
            int end = n+2 >= text.LineStartIndices.size() ? text.Content.Count() : text.LineStartIndices.get(n+2);
            return encoding.GetString(text.Content, start, end - start).TrimEnd();
        }

        public LineAndCommit[] Blame(Commit commit, string path)
        {
            var leaf = commit.Tree[path] as Leaf;
            if (leaf == null)
                throw new ArgumentException("The given path does not exist in this commit: " + path);

            byte[] data = leaf.RawData;
            IntList lineMap = RawParseUtils.lineMap(data, 0, data.Length);
            var lines = new LineAndCommit[lineMap.size()];
            var curText = new RawText(data);

            Commit prevAncestor = commit;

            Leaf prevLeaf = null;
            Commit prevCommit = null;
            int emptyLines = lineMap.size();

            foreach (Commit ancestor in commit.Ancestors)
            {
                var cleaf = ancestor.Tree[path] as Leaf;
                if (prevCommit != null && (cleaf == null || cleaf.Hash != prevLeaf.Hash))
                {
                    byte[] prevData = prevLeaf.RawData;
                    if (prevData == null)
                        break;
                    var prevText = new RawText(prevData);
                    var differ = new MyersDiff(prevText, curText);
                    foreach (Edit e in differ.getEdits())
                    {
                        for (int n = e.BeginB; n < e.EndB; n++)
                        {
                            if (lines[n] == null)
                            {
                                lines[n] = CreateViewModel(GetLine(commit.Encoding, curText, n), prevCommit);
                                emptyLines--;
                            }
                        }
                    }
                    if (cleaf == null || emptyLines <= 0)
                        break;
                }
                prevCommit = ancestor;
                prevLeaf = cleaf;
            }
            for (int n = 0; n < lines.Length; n++)
                if (lines[n] == null)
                    lines[n] = CreateViewModel(GetLine(commit.Encoding, curText, n), prevAncestor);
            return lines;
        }

        private LineAndCommit CreateViewModel(string getLine, Commit commit)
        {
            return new LineAndCommit(getLine, commit, _ageGradient);
        }

        public ObservableCollection<LineAndCommit> Lines
        {
            get { return _lines; }
            private set
            {
                _lines = value;
                RaisePropertyChanged("Lines");
            }
        }

        public DateTime Start { get { return _commits.Last().Commit.CommitDate.LocalDateTime; } }
        public DateTime Stop { get { return _commits.First().Commit.CommitDate.LocalDateTime; } }
    }

    public interface ITimeSpanContainer
    {
        DateTime Start { get; }
        DateTime Stop { get; }
    }
}