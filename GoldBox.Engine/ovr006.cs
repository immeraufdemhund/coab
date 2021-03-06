using GoldBox.Classes;
using System.Collections.Generic;

namespace GoldBox.Engine
{
    class ovr006
    {
        private static readonly Affects[] affects_array;

        static ovr006()
        {
            affects_array = new []
            {
                Affects.sticks_to_snakes,
                Affects.charm_person,
                Affects.reduce,
                Affects.silence_15_radius,
                Affects.spiritual_hammer,
                Affects.fumbling,
                Affects.confuse,
                Affects.affect_28,
                Affects.snake_charm,
                Affects.paralyze,
                Affects.sleep,
                Affects.clear_movement,
                Affects.affect_5b,
                Affects.entangle,
                Affects.affect_89,
                Affects.affect_8b,
                Affects.fear,
                Affects.owlbear_hug_round_attack,
                Affects.helpless
            };

        }

        internal static void AfterCombatExpAndTreasure()
        {
            gbl.area2_ptr.field_58E = 0;
            gbl.byte_1AB14 = false;

            if (gbl.inDemo == false)
            {
                CleanupPlayersStateAfterCombat();
            }

            gbl.game_state = GameState.AfterCombat;

            DeallocateNonTeamMemebers();

            if (gbl.inDemo == false)
            {
                foreach (Player player in gbl.TeamList)
                {
                    ovr025.reclac_player_values(player);
                }

                if (gbl.party_killed == false || gbl.combat_type == CombatType.duel)
                {
                    if (gbl.party_fled)
                    {
                        gbl.items_pointer.Clear();
                    }

                    if (gbl.inDemo == false)
                    {
                        distributeNpcTreasure();
                        displayCombatResults(gbl.exp_to_add);
                        distributeCombatTreasure();
                    }

                    gbl.items_pointer.Clear();
                }
                else
                {
                    gbl.area2_ptr.field_58E = 0x80;
                    seg037.DrawFrame_Outer();
                    gbl.textXCol = 2;
                    gbl.textYCol = 6;
                    seg041.press_any_key("The monsters rejoice for the party has been destroyed", true, 10, 0x16, 0x25, 5, 2);
                    seg041.DisplayAndPause("Press any key to continue", 13);
                }

                gbl.DelayBetweenCharacters = true;
                gbl.area2_ptr.field_6E0 = 0;
                gbl.area2_ptr.field_6E2 = 0;
                gbl.area2_ptr.field_6E4 = 0;
                gbl.area2_ptr.field_5C6 = 0;
                gbl.area2_ptr.isDuel = false;
            }
        }

        private static void CleanupPlayersStateAfterCombat()
        {
            gbl.partyAnimatedCount = 0;
            gbl.party_killed = true;
            gbl.party_fled = false;

            foreach (Player player in gbl.TeamList)
            {
                if (player.actions != null &&
                    player.actions.nonTeamMember)
                {
                    break;
                }

                if (player.health_status == Status.running)
                {
                    gbl.party_fled = true;
                }
            }

            bool no_exp = false;

            foreach (Player player in gbl.TeamList)
            {
                if (player.in_combat ||
                    player.health_status == Status.unconscious ||
                    player.health_status == Status.running ||
                    player.health_status == Status.dying)
                {
                    no_exp = true;
                    break;
                }
            }

            if (gbl.combat_type == CombatType.duel ||
                (gbl.area2_ptr.isDuel && no_exp))
            {
                gbl.party_killed = false;
            }

            gbl.battleWon = false;

            if (gbl.combat_type == CombatType.normal ||
                gbl.inDemo == false)
            {
                foreach (Player player in gbl.TeamList)
                {
                    if (player.actions != null && player.actions.nonTeamMember)
                    {
                        // have gotten past first 6 characters (the party)
                        break;
                    }

                    if (player.health_status == Status.running ||
                        player.health_status == Status.animated ||
                        player.health_status == Status.okey)
                    {
                        if (player.combat_team == CombatTeam.Ours &&
                            player.control_morale < Control.NPC_Base)
                        {
                            gbl.party_killed = false;
                        }
                    }

                    if (player.health_status == Status.animated ||
                        player.health_status == Status.okey)
                    {
                        gbl.battleWon = true;
                        gbl.party_fled = false;
                    }

                    if (player.in_combat == false ||
                        player.health_status == Status.animated)
                    {
                        gbl.partyAnimatedCount++;
                    }

                    System.Array.ForEach(affects_array, affect => ovr024.remove_affect(null, affect, player));
                }

                if (gbl.battleWon)
                {
                    gbl.exp_to_add = calc_battle_exp();
                    addExp(gbl.exp_to_add);
                }


                if (gbl.party_killed == false)
                {
                    var to_remove = new List<Player>();

                    foreach (Player player in gbl.TeamList)
                    {
                        if (player.actions != null && player.actions.nonTeamMember)
                        {
                            break;
                        }

                        if (gbl.party_fled == false)
                        {
                            switch (player.health_status)
                            {
                                case Status.running:
                                    player.health_status = Status.okey;
                                    player.in_combat = true;
                                    break;

                                case Status.dying:
                                    if (gbl.area2_ptr.isDuel)
                                    {
                                        player.health_status = Status.okey;
                                        player.in_combat = true;
                                        player.hit_point_current = 1;
                                    }
                                    else
                                    {
                                        player.health_status = Status.unconscious;
                                    }
                                    break;

                                case Status.unconscious:
                                    if (player.hit_point_current > 0)
                                    {
                                        player.health_status = Status.okey;
                                        player.in_combat = true;
                                    }
                                    else if (gbl.area2_ptr.isDuel)
                                    {
                                        player.health_status = Status.okey;
                                        player.in_combat = true;
                                        player.hit_point_current = 1;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            gbl.area2_ptr.field_58E = 0x81;

                            if (player.health_status == Status.running)
                            {
                                player.health_status = Status.okey;
                                player.in_combat = true;

                            }
                            else
                            {
                                to_remove.Add(player);
                            }
                        }
                    }

                    foreach (Player player in to_remove)
                    {
                        gbl.SelectedPlayer = ovr018.FreeCurrentPlayer(player, true, false);
                    }
                }
                else
                {
                    var to_remove = new List<Player>();
                    foreach (Player player in gbl.TeamList)
                    {
                        if (player.actions != null &&
                            player.actions.nonTeamMember == false)
                        {
                            to_remove.Add(player);
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (Player player in to_remove)
                    {
                        gbl.SelectedPlayer = ovr018.FreeCurrentPlayer(player, true, false);
                    }

                    gbl.area2_ptr.party_size = 0;
                }
            }
            else
            {
                foreach (Player player in gbl.TeamList)
                {
                    if (player.in_combat &&
                        player.health_status == Status.okey &&
                        player.combat_team == CombatTeam.Ours)
                    {
                        gbl.battleWon = true;
                        gbl.exp_to_add = calc_battle_exp();
                        addExp(gbl.exp_to_add);
                    }
                }

                foreach (Player player in gbl.TeamList)
                {
                    if (player.health_status == Status.okey ||
                        player.health_status == Status.animated)
                    {
                        player.in_combat = true;
                    }

                    if (player.health_status == Status.dying)
                    {
                        player.health_status = Status.unconscious;
                    }
                }
            }
        }

        private static void DeallocateNonTeamMemebers()
        {
            gbl.area2_ptr.field_590 = 0;

            var to_remove = new Dictionary<Player, bool>();
            foreach (Player player in gbl.TeamList)
            {
                bool check = (player.actions != null && player.actions.nonTeamMember);

                if (check || player.combat_team == CombatTeam.Enemy)
                {
                    gbl.byte_1AB14 = true;
                    if (player.in_combat == false)
                    {
                        gbl.area2_ptr.field_590++;
                    }

                    to_remove.Add(player, check);
                }
                else
                {
                    if (player.actions != null)
                    {
                        player.actions = null;
                    }
                }
            }

            foreach (KeyValuePair<Player, bool> kvp in to_remove)
            {
                ovr018.FreeCurrentPlayer(kvp.Key, true, kvp.Value);
            }

            gbl.SelectedPlayer = gbl.TeamList.Count > 0 ? gbl.TeamList[0] : null;
        }

        private static void distributeNpcTreasure()
        {
            bool treasureTaken = false;

            int npcParts = 0;
            int totalParts = 0;

            foreach (Player player in gbl.TeamList)
            {
                if (player.control_morale >= Control.NPC_Base &&
                    player.health_status == Status.okey)
                {
                    npcParts += player.npcTreasureShareCount & 7;
                    totalParts += player.npcTreasureShareCount & 7;
                }
                else
                {
                    totalParts++;
                }
            }

            if (npcParts > 0)
            {
                treasureTaken = gbl.pooled_money.ScaleAll(npcParts / totalParts);
            }

            if (treasureTaken)
            {
                seg037.DrawFrame_Outer();
                int yCol = 0;

                foreach (Player player in gbl.TeamList)
                {
                    if (player.control_morale >= Control.NPC_Base &&
                        player.health_status == Status.okey &&
                        player.npcTreasureShareCount > 0)
                    {
                        string output = player.name + " takes and hides " + ((player.sex == 0) ? "his" : "her") + " share.";

                        seg041.press_any_key(output, true, 10, 0x16, 0x22, yCol + 5, 5);

                        yCol += 2;
                    }
                }

                ovr027.displayInput(false, 1, new MenuColorSet(15, 15, 15), "press <enter>/<return> to continue", string.Empty);
            }
        }

        private static void displayCombatResults(int exp)
        {
            seg037.DrawFrame_Outer();

            if (gbl.byte_1AB14 ||
                gbl.combat_type == CombatType.duel)
            {
                if (gbl.party_fled)
                {
                    seg041.displayString("The party has fled.", 0, 10, 3, 1);

                    exp = 0;

                    gbl.items_pointer.Clear();

                    gbl.pooled_money.ClearAll();
                }
                else
                {
                    if ((gbl.combat_type == CombatType.duel && gbl.battleWon == false) ||
                        (gbl.battleWon == false && gbl.area2_ptr.isDuel))
                    {
                        gbl.area2_ptr.field_58E = 0x80;
                        seg041.displayString("You have lost the fight.", 0, 10, 3, 1);

                        exp = 0;
                    }
                    else
                    {
                        if (gbl.combat_type == CombatType.duel)
                        {
                            seg041.displayString("You have won the duel.", 0, 10, 3, 1);
                        }
                        else
                        {
                            seg041.displayString("The party has won.", 0, 10, 3, 1);
                        }
                    }
                }
            }
            else
            {
                seg041.displayString("The party has found Treasure!", 0, 10, 3, 1);
            }

            string text;
            if (gbl.combat_type == CombatType.duel)
            {
                text = "The duelist receives " + exp;
            }
            else
            {
                text = "Each character receives " + exp;
            }

            seg041.displayString(text, 0, 10, 5, 1);
            seg041.displayString("experience points.", 0, 10, 7, 1);

            ovr027.displayInput(false, 1, new MenuColorSet(15, 15, 15), "press <enter>/<return> to continue", string.Empty);
        }

        private static void distributeCombatTreasure()
        {
            byte spellId = 0; /* Simeon */

            ovr025.LoadPic();

            bool done = false;
            do
            {
                bool items_present;
                bool money_present;
                ovr022.treasureOnGround(out items_present, out money_present);

                string text = "View Pool Exit";
                string suffix = " Exit";
                bool can_detect_magic = false;

                if (items_present)
                {
                    foreach (int id in gbl.SelectedPlayer.spellList.IdList())
                    {
                        if ((id == 5 || id == 11 || id == 0x4d) &&
                            gbl.SelectedPlayer.in_combat)
                        {
                            can_detect_magic = true;
                            spellId = (byte)id;
                            break;
                        }
                    }
                }

                if (can_detect_magic)
                {
                    suffix = " Detect Exit";
                }

                if (money_present)
                {
                    text = "View Take Pool Share" + suffix;
                }
                else if (items_present)
                {
                    text = "View Take Pool" + suffix;
                }

                bool ctrl_key;
                char input_key = ovr027.displayInput(out ctrl_key, true, 1, gbl.defaultMenuColors, text, "");

                switch (input_key)
                {
                    case 'V':
                        ovr020.viewPlayer();
                        break;

                    case 'T':
                        take_treasure(ref items_present, ref money_present);
                        break;

                    case 'P':
                        if (ctrl_key == false)
                        {
                            ovr022.poolMoney();
                        }
                        break;

                    case 'S':
                        ovr022.share_pooled();
                        break;

                    case 'D':
                        ovr023.sub_5D2E1(false, QuickFight.False, spellId);
                        break;

                    case 'E':
                    case '\0':
                        ovr022.treasureOnGround(out items_present, out money_present);

                        if (money_present || items_present)
                        {
                            seg041.press_any_key("There is still treasure left.  ", true, 10, TextRegion.NormalBottom);
                            seg041.press_any_key("Do you want to go back and claim your treasure?", false, 15, TextRegion.NormalBottom);
                            int menu_selected = ovr008.sub_317AA(false, false, gbl.defaultMenuColors, "~Yes ~No", "");

                            if (menu_selected == 1)
                            {
                                done = true;
                            }
                            else
                            {
                                seg037.draw8x8_clear_area(0x16, 0x26, 17, 1);
                            }
                        }
                        else
                        {
                            done = true;
                        }
                        break;

                    case 'G':
                        ovr020.scroll_team_list(input_key);
                        ovr025.PartySummary(gbl.SelectedPlayer);
                        break;

                    case 'O':
                        ovr020.scroll_team_list(input_key);
                        ovr025.PartySummary(gbl.SelectedPlayer);
                        break;
                }
            } while (done == false);
        }

        private static int calc_battle_exp()
        {
            if (gbl.combat_type == CombatType.duel)
            {
                return gbl.SelectedPlayer.HitDice * 100;
            }
            else
            {
                /* Go through all players in battle
                 * Add the money from each monster
                 */
                int total = 0;

                foreach (Player player in gbl.TeamList)
                {
                    if (player.combat_team == CombatTeam.Enemy &&
                        player.health_status != Status.okey &&
                        player.health_status != Status.running)
                    {
                        gbl.byte_1AB14 = true;

                        gbl.pooled_money += player.Money;

                        total += player.field_13E * player.hit_point_rolled;
                        total += player.field_13C;

                        if (gbl.area2_ptr.field_5C6 != 1)
                        {
                            foreach (Item item in player.items)
                            {
                                ovr025.ItemDisplayNameBuild(false, false, 0, 0, item);

                                Item newItem = item.ShallowClone();
                                newItem.readied = false;
                                gbl.items_pointer.Add(newItem);
                            }
                        }
                    }
                }

                total += gbl.pooled_money.GetExpWorth();



                foreach (Item item_ptr in gbl.items_pointer)
                {
                    if (item_ptr == gbl.item_ptr) break;

                    if (item_ptr.plus > 0)
                    {
                        total += item_ptr.plus * 400;
                    }
                }

                return total / (gbl.area2_ptr.party_size - gbl.partyAnimatedCount);
            }
        }

        private static void addExp(int exp_to_add)
        {
            foreach (Player player in gbl.TeamList)
            {
                if (player.in_combat &&
                    player.health_status != Status.animated)
                {
                    int new_exp = exp_to_add;

                    switch (player._class)
                    {
                        case ClassId.cleric:
                            if (player.stats2.Wis.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;

                        case ClassId.fighter:
                            if (player.stats2.Str.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;

                        case ClassId.paladin:
                            if (player.stats2.Str.full > 15 &&
                                player.stats2.Wis.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;

                        case ClassId.ranger:
                            if (player.stats2.Str.full > 15 &&
                                player.stats2.Int.full > 15 &&
                                player.stats2.Wis.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;

                        case ClassId.magic_user:
                            if (player.stats2.Int.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;

                        case ClassId.thief:
                            if (player.stats2.Dex.full > 15)
                            {
                                new_exp = exp_to_add + (exp_to_add / 10);
                            }
                            break;


                        default:
                            if (player._class == ClassId.mc_c_f ||
                                (player._class >= ClassId.mc_c_r && player._class <= ClassId.mc_f_t) ||
                                player._class == ClassId.mc_f_t)
                            {
                               // duel class
                                new_exp = exp_to_add / 2;
                            }
                            else if (player._class == ClassId.mc_c_f_m ||
                                player._class == ClassId.mc_f_mu_t)
                            {
                                // triple class
                                new_exp = exp_to_add / 3;
                            }
                            break;

                    }

                    player.exp += new_exp;
                }
            }
        }

        private static void select_treasure(ref int index, out Item selectedItem, out char key)
        {
            seg037.DrawFrame_Outer();

            var list = new List<MenuItem>();

            if (Cheats.sort_treasure)
            {
                gbl.items_pointer.Sort((a, b) => a._value.CompareTo(b._value));
            }

            gbl.items_pointer.ForEach(item =>
                {
                    ovr025.ItemDisplayNameBuild(false, false, 0, 0, item);
                    list.Insert(0, new MenuItem(item.name, item));
                });

            bool redrawMenuItems = true;
            MenuItem selected;
            key = ovr027.sl_select_item(out selected, ref index, ref redrawMenuItems, true, list,
                 0x16, 0x26, 1, 1, gbl.defaultMenuColors, "Take", "Items: ");

            selectedItem = selected != null ? selected.Item : null;
        }

        private static void take_items_treasure()
        {
            bool stop;
            int index = 0;

            do
            {
                Item item;
                char key;

                select_treasure(ref index, out item, out key);

                if (key != 'T' &&
                    key != '\r')
                {
                    stop = true;
                }
                else
                {
                    stop = false;

                    bool willOverload = ovr007.PlayerAddItem(item);

                    if (willOverload == false)
                    {
                        gbl.items_pointer.Remove(item);

                        stop = gbl.items_pointer.Count == 0;
                    }
                }
            } while (stop == false);

            ovr025.LoadPic();
        }

        private static void take_treasure(ref bool items_present, ref bool money_present)
        {
            if (money_present)
            {
                if (items_present)
                {
                    bool done = false;
                    do
                    {
                        char key = ovr027.displayInput(true, 1, gbl.defaultMenuColors, "Money Items Exit", "Take: ");

                        switch (key)
                        {
                            case 'M':
                                ovr022.TakePoolMoney();
                                ovr025.LoadPic();
                                break;

                            case 'I':
                                take_items_treasure();
                                break;

                            case 'E':
                            case '\0':
                                done = true;
                                break;

                            case 'G':
                                ovr020.scroll_team_list(key);
                                break;

                            case 'O':
                                ovr020.scroll_team_list(key);
                                break;
                        }

                        ovr025.PartySummary(gbl.SelectedPlayer);
                        ovr022.treasureOnGround(out items_present, out money_present);

                        if (money_present == false ||
                            items_present == false)
                        {
                            done = true;
                        }
                    } while (done == false);
                }
                else
                {
                    ovr022.TakePoolMoney();
                    ovr025.LoadPic();
                }
            }
            else
            {
                take_items_treasure();
            }
        }
    }
}
