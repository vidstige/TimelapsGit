using GitSharp;

namespace TimelapsGit
{
    public class LineAndCommit: ViewModel
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
}