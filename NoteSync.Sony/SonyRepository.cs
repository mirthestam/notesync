using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace NoteSync.Sony
{
    public class SonyRepository : ISourceRepository
    {
        private readonly ILogger _logger;
        private readonly SonySettings _settings;

        public SonyRepository(ILogger logger, SonySettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public IEnumerable<Note> GetNotes()
        {
            var notes = new List<Note>();

            var readerDrive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Removable && d.VolumeLabel == _settings.Volume);
            if (readerDrive == null)
            {
                _logger.Info($"No volume with name `{_settings.Volume}` exists.");
                return Enumerable.Empty<Note>();
            }

            var sourceDirectory = $"{readerDrive.Name}{_settings.Database}";
            if (!Directory.Exists(sourceDirectory))
            {
                _logger.Info($"Volume with name `{_settings.Volume}` has no valid database.");
                return Enumerable.Empty<Note>();
            }

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                Note note;
                if (TryReadNoteFromFile(file, out note))
                {
                    notes.Add(note);
                }
            }

            return notes;
        }

        private bool TryReadNoteFromFile(string fileName, out Note note)
        {
            var tempNote = new Note();

            using (var stream = File.OpenRead(fileName))
            {
                using (var xmlReader = new XmlTextReader(stream))
                {
                    var document = new XPathDocument(xmlReader);
                    var navigator = document.CreateNavigator();
                    if (navigator.NameTable == null) throw new Exception("NameTable is null");
                    var manager = new XmlNamespaceManager(navigator.NameTable);
                    manager.AddNamespace("note", "http://www.sony.com/notepad");

                    var createDateAttribute = navigator.SelectSingleNode("note:notepad/@createDate", manager);
                    var textNode = navigator.SelectSingleNode("note:notepad/text", manager);

                    if (createDateAttribute != null && textNode != null)
                    {
                        // Extract the created date
                        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var value = createDateAttribute.ValueAsDouble;
                        dateTime = dateTime.AddMilliseconds(value);
                        dateTime = dateTime.ToLocalTime();

                        tempNote.Created = dateTime;
                        tempNote.Modified = dateTime; // Note the sony reader has no modified timestamp
                        tempNote.Id = Path.GetFileNameWithoutExtension(fileName);
                        tempNote.Title = textNode.Value.Split(Environment.NewLine.ToCharArray())[0];
                        tempNote.Body = textNode.Value;

                        note = tempNote;
                        return true;
                    }
                }
            }

            note = null;
            return false;
        }
    }
}