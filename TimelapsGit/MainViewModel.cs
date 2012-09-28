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
    public class MainViewModel: ViewModel
    {
        private readonly Repository _repository;
        private ObservableCollection<LineAndCommit> _lines = new ObservableCollection<LineAndCommit>();
        private List<CommitViewModel> _commits = new List<CommitViewModel>();
        private int _selectedCommitNumber = 1;

        private string _path;

        public MainViewModel(Repository repository)
        {
            _repository = repository;
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
                RaisePropertyChanged("SelectedCommitNumber");
                RaisePropertyChanged("CurrentMessage");
            }
        }

        public string CurrentMessage
        {
            get { return _commits[_selectedCommitNumber-1].Commit.Message; }
        }
        
        public string File
        {
            set
            {
                _path = value;

                _commits = _repository.CurrentBranch.CurrentCommit.Ancestors.Select(c => new CommitViewModel(c)).ToList();

                Lines = new ObservableCollection<LineAndCommit>(Blame(_repository.CurrentBranch.CurrentCommit, _path));

                RaisePropertyChanged("Commits");
            }
            get { return _path; }
        }

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
                                lines[n] = new LineAndCommit(GetLine(commit.Encoding, curText, n), prevCommit);
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
                    lines[n] = new LineAndCommit(GetLine(commit.Encoding, curText, n), prevAncestor);
            return lines;
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
    }

    public class CommitViewModel: ViewModel
    {
        private readonly Commit _commit;

        public CommitViewModel(Commit commit)
        {
            _commit = commit;
        }

        public Commit Commit { get { return _commit; } }
    }
}