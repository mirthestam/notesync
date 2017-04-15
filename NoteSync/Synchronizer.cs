using NLog;

namespace NoteSync
{
    public class Synchronizer : ISynchronizer
    {
        private readonly ILogger _logger;
        private readonly ISourceRepository _sourceRepository;
        private readonly ITargetRepository _targetRepository;
        private readonly IMergeStrategy _mergeStrategy;

        public Synchronizer(ILogger logger, ISourceRepository sourceRepository, ITargetRepository targetRepository, IMergeStrategy mergeStrategy)
        {
            _logger = logger;
            _sourceRepository = sourceRepository;
            _targetRepository = targetRepository;
            _mergeStrategy = mergeStrategy;
        }

        public void Synchronize()
        {
            _logger.Trace("Getting notes from source");

            foreach (var sourceNote in _sourceRepository.GetNotes())
            {
                _logger.Info($"Synchronizing note: {sourceNote.Created} - {sourceNote.Title} ");

                Note note;
                if (!_targetRepository.TryGetNote(sourceNote.Id, out note))
                {
                    _logger.Debug($"Note {sourceNote.Id} does not exist on target.");
                    // This note does not exist yet.
                    // Create a new note
                    note = new Note
                    {                        
                        Created = sourceNote.Created,
                        Id = sourceNote.Id
                    };
                }

                // Merge the note using the merge strategy
                if (!_mergeStrategy.Merge(sourceNote, note))
                {
                    _logger.Warn($"Merge conflict for note {sourceNote.Id}. Skipping note.");
                    // We have a merge conflict.
                    continue;
                }

                _logger.Debug($"Saving note {sourceNote.Id}");
                _targetRepository.SaveNote(note);
            }
        }
    }
}