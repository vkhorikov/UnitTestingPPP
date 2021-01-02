using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace Book.Chapter6.Listing7_.Functional
{
    public class AuditManager
    {
        private readonly int _maxEntriesPerFile;

        public AuditManager(int maxEntriesPerFile)
        {
            _maxEntriesPerFile = maxEntriesPerFile;
        }

        public FileUpdate AddRecord(
            FileContent[] files,
            string visitorName,
            DateTime timeOfVisit)
        {
            (int index, FileContent file)[] sorted = SortByIndex(files);

            string newRecord = visitorName + ';' + timeOfVisit.ToString("s");

            if (sorted.Length == 0)
            {
                return new FileUpdate("audit_1.txt", newRecord);
            }

            (int currentFileIndex, FileContent currentFile) = sorted.Last();
            List<string> lines = currentFile.Lines.ToList();

            if (lines.Count < _maxEntriesPerFile)
            {
                lines.Add(newRecord);
                string newContent = string.Join("\r\n", lines);
                return new FileUpdate(currentFile.FileName, newContent);
            }
            else
            {
                int newIndex = currentFileIndex + 1;
                string newName = $"audit_{newIndex}.txt";
                return new FileUpdate(newName, newRecord);
            }
        }

        private (int index, FileContent file)[] SortByIndex(
            FileContent[] files)
        {
            return files
                .Select(file => (index: GetIndex(file.FileName), file))
                .OrderBy(x => x.index)
                .ToArray();
        }

        private int GetIndex(string fileName)
        {
            // File name example: audit_1.txt
            string name = Path.GetFileNameWithoutExtension(fileName);
            return int.Parse(name.Split('_')[1]);
        }
    }

    public struct FileUpdate
    {
        public readonly string FileName;
        public readonly string NewContent;

        public FileUpdate(string fileName, string newContent)
        {
            FileName = fileName;
            NewContent = newContent;
        }
    }

    public class FileContent
    {
        public readonly string FileName;
        public readonly string[] Lines;

        public FileContent(string fileName, string[] lines)
        {
            FileName = fileName;
            Lines = lines;
        }
    }

    public class Persister
    {
        public FileContent[] ReadDirectory(string directoryName)
        {
            return Directory
                .GetFiles(directoryName)
                .Select(x => new FileContent(
                    Path.GetFileName(x),
                    File.ReadAllLines(x)))
                .ToArray();
        }

        public void ApplyUpdate(string directoryName, FileUpdate update)
        {
            string filePath = Path.Combine(directoryName, update.FileName);
            File.WriteAllText(filePath, update.NewContent);
        }
    }

    public class ApplicationService
    {
        private readonly string _directoryName;
        private readonly AuditManager _auditManager;
        private readonly Persister _persister;

        public ApplicationService(string directoryName, int maxEntriesPerFile)
        {
            _directoryName = directoryName;
            _auditManager = new AuditManager(maxEntriesPerFile);
            _persister = new Persister();
        }

        public void AddRecord(string visitorName, DateTime timeOfVisit)
        {
            FileContent[] files = _persister.ReadDirectory(_directoryName);
            FileUpdate update = _auditManager.AddRecord(
                files, visitorName, timeOfVisit);
            _persister.ApplyUpdate(_directoryName, update);
        }
    }

    public class Tests
    {
        [Fact]
        public void A_new_file_is_created_when_the_current_file_overflows()
        {
            var sut = new AuditManager(3);
            var files = new FileContent[]
            {
                new FileContent("audit_1.txt", new string[0]),
                new FileContent("audit_2.txt", new string[]
                {
                    "Peter; 2019-04-06T16:30:00",
                    "Jane; 2019-04-06T16:40:00",
                    "Jack; 2019-04-06T17:00:00"
                })
            };

            FileUpdate update = sut.AddRecord(
                files, "Alice", DateTime.Parse("2019-04-06T18:00:00"));

            Assert.Equal("audit_3.txt", update.FileName);
            Assert.Equal("Alice;2019-04-06T18:00:00", update.NewContent);
            Assert.Equal(
                new FileUpdate("audit_3.txt", "Alice;2019-04-06T18:00:00"),
                update);
            update.Should().Be(
                new FileUpdate("audit_3.txt", "Alice;2019-04-06T18:00:00"));
        }
    }
}
