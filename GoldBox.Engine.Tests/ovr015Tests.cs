﻿using System.Linq;
using NUnit.Framework;
using GoldBox.Classes;
using GoldBox.Classes.DaxFiles;

namespace GoldBox.Engine
{
    [TestFixture(Category = "ovr015 Tests")]
    public class ovr015Tests
    {
        private ObjectVerifier _verifier = new ObjectVerifier();
        private MapInfo _mapInfo;
        private StepGameTimeSpy _stepGameTimeSpy;
        [SetUp]
        public void Setup()
        {
            _stepGameTimeSpy = new StepGameTimeSpy();
            ovr015.StepGameTime = _stepGameTimeSpy;
            gbl.area2_ptr = new Area2();
            gbl.byte_1D556 = new DaxArray();
            gbl.game_state = GameState.DungeonMap;
            gbl.geo_ptr = new GeoBlock();
            _mapInfo = new MapInfo();
            var maps = new MapInfo[16, 16];
            var width = maps.GetLength(0);
            var height = maps.GetLength(1);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    maps[x, y] = _mapInfo;
                }

            gbl.geo_ptr.maps = maps;
        }

        [TestCase(GameState.AfterCombat)]
        [TestCase(GameState.Camping)]
        [TestCase(GameState.Combat)]
        [TestCase(GameState.EndGame)]
        [TestCase(GameState.Shop)]
        [TestCase(GameState.StartGameMenu)]
        [TestCase(GameState.WildernessMap)]
        public void LockedDoor_GameStateIsNotDungeonMap_NothingHappensDaxIsReset(GameState state)
        {
            gbl.area2_ptr.field_592 = 1;
            gbl.game_state = state;

            ovr015.locked_door();

            VerifyDaxArray1D556IsReset();
            VerifyHeadAndBodyDaxIsReleased();
            _verifier.CheckThat(() => gbl.can_draw_bigpic == false);
            _verifier.CheckThat(() => gbl.area2_ptr.field_592 == 1);
            _verifier.Verify();
        }

        [TestCase(0xFF)]
        [TestCase(short.MaxValue)]
        public void LockedDoor_AndPointerIsNotLessThan0xFFPointerIsResetAndNOthingHappens(short field592)
        {
            gbl.area2_ptr.field_592 = field592;

            ovr015.locked_door();

            VerifyDaxArray1D556IsReset();
            VerifyHeadAndBodyDaxIsReleased();
            _verifier.CheckThat(() => gbl.area2_ptr.field_592 == 0);
            _verifier.CheckThat(() => gbl.can_draw_bigpic == false);
            _verifier.Verify();
        }

        [Test]
        public void LockedDoor_ALIsOne_PartyMovesForward_TimeIsUpdated()
        {
            gbl.EclBlockId = 1;
            _mapInfo.x3_dir_0 = 1;

            ovr015.locked_door(); //TODO make move_party_forward not throw NRE

            VerifyDaxArray1D556IsReset();
            VerifyHeadAndBodyDaxIsReleased();
            VerifyPartyWasMovedForward();
            _verifier.CheckThat(() => gbl.area2_ptr.field_592 == 0);
            _verifier.CheckThat(() => gbl.can_draw_bigpic == true);
            _verifier.Verify();

        }

        private void VerifyHeadAndBodyDaxIsReleased()
        {
            _verifier.CheckThat(() => gbl.headX_dax == null);
            _verifier.CheckThat(() => gbl.bodyX_dax == null);
            _verifier.CheckThat(() => gbl.current_head_id == 0xFF);
            _verifier.CheckThat(() => gbl.current_body_id == 0xFF);
        }

        private void VerifyDaxArray1D556IsReset()
        {
            var daxArray = gbl.byte_1D556;
            _verifier.CheckThat(() => daxArray.frames.All(x => x.picture == null));
            _verifier.CheckThat(() => daxArray.frames.All(x => x.delay == 0));
            _verifier.CheckThat(() => daxArray.numFrames == 0);
            _verifier.CheckThat(() => daxArray.curFrame == 0);
            _verifier.CheckThat(() => gbl.lastDaxFile == string.Empty);
            _verifier.CheckThat(() => gbl.lastDaxBlockId == 0x0FF);
        }

        private void VerifyPartyWasMovedForward()
        {
            _verifier.CheckThat(() => gbl.mapPosX == 1);
            _verifier.CheckThat(() => gbl.mapPosY == 2);
            _verifier.CheckThat(() => gbl.can_bash_door == true);
            _verifier.CheckThat(() => gbl.can_pick_door == true);
            _verifier.CheckThat(() => gbl.can_knock_door == true);
            _verifier.CheckThat(() => _stepGameTimeSpy.LastTimeSlotCalled == 1);
            _verifier.CheckThat(() => _stepGameTimeSpy.LastAmountCalled == 1);
        }

        private class StepGameTimeSpy : IStepGameTime
        {
            public int LastTimeSlotCalled = -1;
            public int LastAmountCalled = -1;

            public void step_game_time(int time_slot, int amount)
            {
                LastTimeSlotCalled = time_slot;
                LastAmountCalled = amount;
            }
        }
    }
}