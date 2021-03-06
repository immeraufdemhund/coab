﻿using System.Collections.Generic;
using System.IO;
using GoldBox.Classes;
using GoldBox.Data;

namespace GoldBox.Engine.ImportCharacters
{
    internal class FindCotABCharactersToAddToParty : IFindCharactersToAddToParty
    {
        public IEnumerable<CharacterToAddToParty> LookIn(string filePath)
        {
            foreach (var fileName in Directory.GetFiles(filePath, "*.guy"))
            {
                var data = File.ReadAllBytes(fileName);
                if (data.Length != Player.StructSize)
                    continue;

                var character = CurseCharacter.Parse(data);
                if (character.NpcByte > 0x7F)
                    continue;

                var isCharacterInTeamList = gbl.TeamList.Find(player => character.NameStructure.Value == player.name.Trim()) != null;
                if (isCharacterInTeamList)
                    continue;

                yield return new CharacterToAddToParty(fileName, character.NameStructure.Value);
            }
        }
    }
}
