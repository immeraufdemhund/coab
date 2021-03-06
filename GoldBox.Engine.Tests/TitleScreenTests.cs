﻿using NUnit.Framework;
using GoldBox.Classes;
using GoldBox.Classes.DaxFiles;
using GoldBox.Logging;

namespace GoldBox.Engine
{
    [TestFixture]
    public class TitleScreenTests
    {
        private readonly SaveDisplayAsPng _display = new SaveDisplayAsPng(TestContext.CurrentContext.TestDirectory);

        [SetUp]
        public void Setup()
        {
            _display.Reset();
            Config.Setup(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void DisplayTitleScreen()
        {
            Logger.Setup("DisplayTitleScreen");

            gbl.symbol_8x8_set = new DaxBlock[5];
            gbl.dax_8x8d1_201 = new byte[177, 8];
            seg041.Load8x8Tiles();

            var titleScreen = new TitleScreen {KeyboardInput = new FakeKeyboardInput()};

            titleScreen.DisplayTitleScreen();
        }

        private class FakeKeyboardInput : IKeyboardInput
        {
            public void PressAnyKey(int secondsTimeout){}
        }
    }
}
