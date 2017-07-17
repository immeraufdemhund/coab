using System.Collections.Generic;
using GoldBox.Classes;
using GoldBox.Logging;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace GoldBox.Engine
{
    [TestFixture(Category = "Character Import Tests")]
    public class ovr017Tests
    {
        private readonly ObjectVerifier _verifier = new ObjectVerifier();
        private List<MenuItem> displayNames;
        private List<MenuItem> fileNames;

        [OneTimeSetUp]
        public void ClassSetup()
        {
            Config.Setup(TestContext.CurrentContext.TestDirectory);
            Config.SavePath = GameFiles.ImportCharacterDirectory.FullName;
        }

        [SetUp]
        public void Setup()
        {
            Assert.That(GameFiles.TestCurseFiles, Has.Length.EqualTo(6));
        }

        [Test]
        public void TestGettingMenuItemsForCurse()
        {
            var expectedFileNames = GameFiles.TestCurseFiles.Select(x => Path.Combine(Config.SavePath, x.Name));
            var expectedDisplayNames = new string[] { "LEDERA", "MARK", "MATHEW", "PHILIPPE", "SHARA", "TRAVIS" };

            BuildImportPlayerList(ImportSource.Curse);

            _verifier.CheckThat((() => fileNames.Count.Equals(6)));
            _verifier.CheckThat(() => displayNames.Count.Equals(6));
            _verifier.CheckThat(() => !fileNames.Select(x=>x.Text).Except(expectedFileNames).Any());
            _verifier.CheckThat(() => !displayNames.Select(x=>x.Text).Except(expectedDisplayNames).Any());
            _verifier.Verify();
        }

        [Test]
        public void TestGettingMenuItemsForPoolrad()
        {
            var expectedFileNames = GameFiles.TestPoolsFiles.Select(x => Path.Combine(Config.SavePath, x.Name));
            var expectedDisplayNames = new string[] { "NATE","ANDY", "ISHA" };

            BuildImportPlayerList(ImportSource.Pool);

            _verifier.CheckThat(() => fileNames.Count.Equals(3));
            _verifier.CheckThat(() => displayNames.Count.Equals(3));
            _verifier.CheckThat(() => !fileNames.Select(x => x.Text).Except(expectedFileNames).Any());
            _verifier.CheckThat(() => !displayNames.Select(x => x.Text).Except(expectedDisplayNames).Any());
            _verifier.Verify();
        }
        private void BuildImportPlayerList(ImportSource importSource)
        {
            gbl.import_from = importSource;
            ovr017.BuildLoadablePlayersLists(out fileNames, out displayNames);
        }
    }
}
