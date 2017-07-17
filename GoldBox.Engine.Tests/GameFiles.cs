using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GoldBox.Engine
{
    public class GameFiles
    {
        private static DirectoryInfo DataDirectory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data"));
        public static DirectoryInfo ImportCharacterDirectory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles\\ImportCharacter"));
        public static IEnumerable<FileInfo> TestCurseFiles => ImportCharacterDirectory.EnumerateFiles("*.GUY");
        public static IEnumerable<FileInfo> TestPoolsFiles => ImportCharacterDirectory.EnumerateFiles("*.SAV").Concat(ImportCharacterDirectory.GetFiles("*.CHA"));
        public static FileInfo EclDax(int number) => DataDirectory.GetFiles($"ECL{number}.DAX")[0];
    }
}