using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GitSharp;
using GitSharp.Core.Diff;
using GitSharp.Core.Util;

namespace TimelapsGit
{
    public class LineAndCommit
    {
        private readonly string _line;
        private readonly Commit _commit;

        public LineAndCommit(string line, Commit commit)
        {
            _line = line;
            _commit = commit;
        }

        public string Line { get { return _line; } }
        public Commit Commit { get { return _commit; } }
    }

    public class MainViewModel: INotifyPropertyChanged
    {
        private readonly Repository _repository;
        private readonly ObservableCollection<string> _lines = new ObservableCollection<string>();
        private int _selectedCommitNumber;

        private List<LineAndCommit> _commits;
        private string _path;

        public MainViewModel(Repository repository)
        {
            _repository = repository;
        }

        public int NumberOfChanges
        {
            get { return _commits.Count-1; }
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

        public LineAndCommit Selected
        {
            get { return _commits[_commits.Count - _selectedCommitNumber - 1]; }
        }

        public string CurrentMessage
        {
            get { return Selected.Commit.Message; }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var tmp = PropertyChanged;
            if (tmp != null) tmp(this, new PropertyChangedEventArgs(propertyName));
        }

        public string File
        {
            set
            {
                _path = value;
                _commits = Blame(_repository.CurrentBranch.CurrentCommit, _path).ToList();

                _lines.Clear();
                foreach (var commit in _commits)
                {
                    _lines.Add(commit.Commit.CommitDate +" "+  commit.Line);
                }
                
                RaisePropertyChanged("NumberOfChanges");
                SelectedCommitNumber = _commits.Count - 1;
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

        public ObservableCollection<string> Lines
        {
            get { return _lines; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}