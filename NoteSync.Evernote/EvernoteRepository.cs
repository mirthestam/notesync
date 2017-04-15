using System;
using System.Collections.Generic;
using EvernoteSDK;
using EvernoteSDK.Advanced;
using NLog;

namespace NoteSync.Evernote
{
    public class EvernoteRepository : ITargetRepository
    {
        private readonly ILogger _logger;
        private readonly EvernoteSettings _settings;
        private readonly ENSessionAdvanced _session;

        public EvernoteRepository(ILogger logger, EvernoteSettings settings)
        {
            _settings = settings;
            _logger = logger;

            ConfigureSession();
            _session = ENSessionAdvanced.SharedSession;
        }

        public bool TryGetNote(string id, out Note note)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));

            ENSessionFindNotesResult findNoteResult;
            if (!TryFindNote(id, out findNoteResult))
            {
                note = null;
                return false;
            }

            var externalNote = _session.DownloadNote(findNoteResult.NoteRef);

            // Create a local note model representing this nite
            note = new Note
            {
                Id = id,
                Modified = findNoteResult.Updated,
                Title = findNoteResult.Title,
                Body = externalNote.TextContent
            };

            return true;
        }

        public void SaveNote(Note note)
        {
            // TODO: Note has timestamps. Change the evernote timestamps
            // TODO: note has existing tags. Respect the existing tags.
            var externalNote = new ENNote
            {
                Title = note.Title.Trim(),
                Content = ENNoteContent.NoteContentWithString(note.Body),
                TagNames = new List<string>(new[] {_settings.Tag})
            };

            // Find the note to update
            ENSessionFindNotesResult findNoteResult;
            if (TryFindNote(note.Id, out findNoteResult))
            {
                // Update the existing note
                var existingNoteRef = findNoteResult.NoteRef;
                _session.UploadNote(externalNote, ENSession.UploadPolicy.ReplaceOrCreate, findNoteResult.Notebook, existingNoteRef);
            }
            else
            {
                // Upload to the default notebook
                var noteRef = _session.UploadNote(externalNote, null);

                // Set the note identifier in the application meta data
                _session.PrimaryNoteStore.SetNoteApplicationDataEntry(noteRef.Guid, "NoteSync", note.Id);
            }            
            
        }

        private bool TryFindNote(string id, out ENSessionFindNotesResult result)
        {
            // Find all notes owned by this application and still tagged as NoteSync
            var phrase = ENNoteSearch.NoteSearch($"tag:{_settings.Tag} applicationData:NoteSync");
            var findResults = _session.FindNotes(phrase, null, ENSession.SearchScope.All, ENSession.SortOrder.Normal, 100000);

            // Find a note belonging to this ID
            foreach (var findResult in findResults)
            {
                var resultId = _session.PrimaryNoteStore.GetNoteApplicationDataEntry(findResult.NoteRef.Guid, "NoteSync");
                if (resultId == id)
                {
                    result = findResult;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private void ConfigureSession()
        {
            if (string.IsNullOrWhiteSpace(_settings.SessionNoteStoreUrl)) throw new Exception("Evernote Note Store Url has not been configured.");
            if (string.IsNullOrWhiteSpace(_settings.SessionDeveloperToken)) throw new Exception("Evernote Developer token has not been configured.");

            _logger.Debug("Authenticating to EverNote");

            try
            {
                ENSessionAdvanced.SetSharedSessionDeveloperToken(_settings.SessionDeveloperToken, _settings.SessionNoteStoreUrl);
                ENSessionAdvanced.SharedSession.SourceApplication = "NoteSync";

                if (!ENSessionAdvanced.SharedSession.IsAuthenticated)
                {
                    ENSessionAdvanced.SharedSession.AuthenticateToEvernote();
                }
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }

            _logger.Info("Authenticated to EverNote");
        }
    }
}