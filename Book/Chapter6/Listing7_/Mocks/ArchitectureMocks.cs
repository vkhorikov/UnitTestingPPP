using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Xunit;

namespace Book.Chapter6.Listing7_.Mocks
{
    public class AuditManager
    {
        private readonly int _maxEntriesPerFile;
        private readonly string _directoryName;
        private readonly IFileSystem _fileSystem;

        public AuditManager(
            int maxEntriesPerFile,
            string directoryName,
            IFileSystem fileSystem)
        {
            _maxEntriesPerFile = maxEntriesPerFile;
            _directoryName = directoryName;
            _fileSystem = fileSystem;
        }

        public void AddRecord(string visitorName, DateTime timeOfVisit)
        {
            string[] filePaths = _fileSystem.GetFiles(_directoryName);
            (int index, string path)[] sorted = SortByIndex(filePaths);

            string newRecord = visitorName + ';' + timeOfVisit.ToString("s");

            if (sorted.Length == 0)
            {
                string newFile = Path.Combine(_directoryName, "audit_1.txt");
                _fileSystem.WriteAllText(newFile, newRecord);
                return;
            }

            (int currentFileIndex, string currentFilePath) = sorted.Last();
            List<string> lines = _fileSystem.ReadAllLines(currentFilePath);

            if (lines.Count < _maxEntriesPerFile)
            {
                lines.Add(newRecord);
                string newContent = string.Join("\r\n", lines);
                _fileSystem.WriteAllText(currentFilePath, newContent);
            }
            else
            {
                int newIndex = currentFileIndex + 1;
                string newName = $"audit_{newIndex}.txt";
                string newFile = Path.Combine(_directoryName, newName);
                _fileSystem.WriteAllText(newFile, newRecord);
            }
        }

        private (int index, string path)[] SortByIndex(string[] files)
        {
            return files
                .Select(path => (index: GetIndex(path), path))
                .OrderBy(x => x.index)
                .ToArray();
        }

        private int GetIndex(string filePath)
        {
            // File name example: audit_1.txt
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return int.Parse(fileName.Split('_')[1]);
        }
    }

    public interface IFileSystem
    {
        string[] GetFiles(string directoryName);
        void WriteAllText(string filePath, string content);
        List<string> ReadAllLines(string filePath);
    }

    public class Tests
    {
        [Fact]
        public void A_new_file_is_created_for_the_first_entry()
        {
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock
                .Setup(x => x.GetFiles("audits"))
                .Returns(new string[0]);
            var sut = new AuditManager(3, "audits", fileSystemMock.Object);

            sut.AddRecord("Peter", DateTime.Parse("2019-04-09T13:00:00"));

            fileSystemMock.Verify(x => x.WriteAllText(
                @"audits\audit_1.txt",
                "Peter;2019-04-09T13:00:00"));
        }

        [Fact]
        public void A_new_file_is_created_when_the_current_file_overflows()
        {
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock
                .Setup(x => x.GetFiles("audits"))
                .Returns(new string[]
                {
                    @"audits\audit_1.txt",
                    @"audits\audit_2.txt"
                });
            fileSystemMock
                .Setup(x => x.ReadAllLines(@"audits\audit_2.txt"))
                .Returns(new List<string>
                {
                    "Peter; 2019-04-06T16:30:00",
                    "Jane; 2019-04-06T16:40:00",
                    "Jack; 2019-04-06T17:00:00"
                });
            var sut = new AuditManager(3, "audits", fileSystemMock.Object);

            sut.AddRecord("Alice", DateTime.Parse("2019-04-06T18:00:00"));

            fileSystemMock.Verify(x => x.WriteAllText(
                @"audits\audit_3.txt",
                "Alice;2019-04-06T18:00:00"));
        }
    }
}
