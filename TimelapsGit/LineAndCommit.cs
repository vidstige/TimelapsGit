using System.Windows.Media;
using GitSharp;

namespace TimelapsGit
{
    public class LineAndCommit: ViewModel
    {
        private readonly string _line;
        private readonly Commit _commit;
        private readonly AgeGradient _ageGradient;

        public LineAndCommit(string line, Commit commit, AgeGradient ageGradient)
        {
            _line = line;
            _commit = commit;
            _ageGradient = ageGradient;
        }

        public string Line { get { return _line; } }
        public Commit Commit { get { return _commit; } }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                return new SolidColorBrush(_ageGradient.Lookup(_commit.CommitDate.LocalDateTime));
            }
        }

    }
}