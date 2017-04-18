using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace NoteSync.Sony
{
    public class SonyRepository : ISourceRepository
    {
        private static readonly Regex TagRegex = new Regex(@"\[.*?\]", RegexOptions.Compiled);

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

        private bool TryReadNoteFromFile(string fileName, out Note result)
        {
            var note = new Note
            {
                Id = Path.GetFileNameWithoutExtension(fileName)
            };

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
                        note.Created = dateTime;
                        note.Modified = dateTime; // Note the sony reader has no modified timestamp

                        var lines = textNode.Value.Split(Environment.NewLine.ToCharArray()).ToList();

                        // Tags on first line are notebook tags. This should be one or none.
                        // Tags on second line are note tags                    
                        var titleTags = ExtractTags(lines[0]).ToList();
                        if (titleTags.Count == 1) note.Folder = titleTags[0].Trim();
                        
                        note.Title = RemoveTags(lines[0]);
                        lines.RemoveAt(0); // Remove the title line from the note body                        

                        // Check whether we have any lines left (could be, a note only has a titel)
                        if (lines.Count >= 1)
                        {
                            // Try to fetch tags from the second line. This is an optional tag line
                            note.Tags = ExtractTags(lines[0]);
                            lines[0] = RemoveTags(lines[0]);
                        }
                                                
                        // Build the body. Skip trailing empty lines
                        note.Body = string.Join(Environment.NewLine, lines.SkipWhile(string.IsNullOrWhiteSpace));
                        result = note;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }            

        public string[] ExtractTags(string line)
        {
            var matches = TagRegex.Matches(line);

            return matches
                .Cast<Match>()
                .Select(m => m.Value.Substring(1, m.Value.Length - 2))
                .ToArray();
        }

        public string RemoveTags(string line)
        {
            return TagRegex.Replace(line, "").Trim();
        }
    }
}