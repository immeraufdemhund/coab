using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace GoldBox.Engine
{
    [TestFixture]
    public class EclTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void LoadEclFileAndPrintCommandsToConsole(int daxNumber)
        {
            var eclFileData = File.ReadAllBytes(GameFiles.EclDax(daxNumber).FullName);

            ovr003.SetupCommandTable();
            var commandTable = ovr003.CommandTable;
            StringBuilder sb = new StringBuilder(eclFileData.Length);
            for (var i = 0; i < eclFileData.Length; i++)
            {
                var command = eclFileData[i];
                var cmdItem = commandTable[command];
                sb.Append($"Command[{command:X2}]");
                sb.Append($" size:{cmdItem.Size}");
                sb.Append($" {cmdItem.Name}");
                sb.AppendLine();
                i += cmdItem.Size;
            }
            Console.WriteLine(sb.ToString());
        }
    }
}