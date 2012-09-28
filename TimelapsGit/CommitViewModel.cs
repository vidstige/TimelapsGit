using GitSharp;

namespace TimelapsGit
{
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