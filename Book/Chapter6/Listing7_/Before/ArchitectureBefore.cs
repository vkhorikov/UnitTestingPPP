using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Book.Chapter6.Listing7_.Before
{
    public class AuditManager
    {
        private readonly int _maxEntriesPerFile;
        private readonly string _directoryName;

        public AuditManager(int maxEntriesPerFile, string directoryName)
        {
            _maxEntriesPerFile = maxEntriesPerFile;
            _directoryName = directoryName;
        }

        public void AddRecord(string visitorName, DateTime timeOfVisit)
        {
            string[] filePaths = Directory.GetFiles(_directoryName);
            (int index, string path)[] sorted = SortByIndex(filePaths);

            string newRecord = visitorName + ';' + timeOfVisit.ToString("s");

            if (sorted.Length == 0)
            {
                string newFile = Path.Combine(_directoryName, "audit_1.txt");
                File.WriteAllText(newFile, newRecord);
                return;
            }

            (int currentFileIndex, string currentFilePath) = sorted.Last();
            List<string> lines = File.ReadAllLines(currentFilePath).ToList();

            if (lines.Count < _maxEntriesPerFile)
            {
                lines.Add(newRecord);
                string newContent = string.Join("\r\n", lines);
                File.WriteAllText(currentFilePath, newContent);
            }
            else
            {
                int newIndex = currentFileIndex + 1;
                string newName = $"audit_{newIndex}.txt";
                string newFile = Path.Combine(_directoryName, newName);
                File.WriteAllText(newFile, newRecord);
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
}
