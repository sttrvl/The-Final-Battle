using System.IO;

public class DisplayInformation
{
    public DisplayInformation(PartyManager party, TurnManager turn)
    {
        party.ConsumableItemUsed        += OnDisplayConsumableUsed;
        party.AttackSuccesful           += OnDisplayActionInfo;
        party.AttackInfo                += OnDisplayDamageAndRemainingHealth;
        party.DefensiveModifierApplied  += OnDisplayDefensiveModifierEffects;
        party.AttackMissed              += OnDisplayMissedAttack;
        party.DeathOpponentGearObtained += OnDisplayGearObtained;
        party.PartyDefeated             += OnDisplayBattleEnd;
        party.SoulBonus                 += OnDisplaySoulBonus;
        party.CharacterPoisoned         += OnDisplayCharacterPoisoned;
        party.PoisonDamage              += OnDisplayPoisonDamage;
        party.PlagueSickDamage          += OnDisplayPlagueSick;
        party.CharacterPlagueSick       += OnDisplayCharacterPlagueSick;
        turn.TauntMessage               += OnDisplayTaunt;
        party.OffensiveModifierApplied  += OnDisplayOffensiveModifierEffects;
        party.ItemsObtained             += OnDisplayItemsObtained;
        party.GearObtained              += OnDisplayGearObtained;
        TurnSkipped                     += OnDisplayTurnSkipped;
    }

    public event Action<TurnManager> TurnSkipped;

    
    public void OnDisplayPoisonDamage(Character character)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"*Poison*", ConsoleColor.DarkGreen),
            new ColoredText($"deals", ConsoleColor.White),
            new ColoredText($" 1 ", ConsoleColor.Red),
            new ColoredText($"damage to ", ConsoleColor.White),
            new ColoredText($"{character}", CharacterColor(character)),
            new ColoredText($".", ConsoleColor.White),
        };

        LogMessages.Add(colorText);
    }

    public void OnDisplayPlagueSick(Character character)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{character}", CharacterColor(character)),
            new ColoredText($" can't move ", ConsoleColor.DarkYellow),
            new ColoredText($"they have: ", ConsoleColor.DarkYellow),
            new ColoredText($"*Plague Sickness*", ConsoleColor.DarkYellow),            
            new ColoredText($".", ConsoleColor.White),
        };

        LogMessages.Add(colorText);
    }

    public void OnDisplayItemsObtained(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained: ", ConsoleColor.Green)
        };
        foreach (Consumables item in party.MonsterItemInventory)
            colorText.Add(new ColoredText($"{item} ", ItemColor(item)));

        LogMessages.Add(colorText);
    }

    public void OnDisplayGearObtained(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained: ", ConsoleColor.Green)
        };
        foreach (Gear gear in party.MonsterGearInventory)
            colorText.Add(new ColoredText($"{gear}", GearColor(gear)));

        LogMessages.Add(colorText);
    }

    public int DisplayCorrectMenu(int? choice, PartyManager party, TurnManager turn, DisplayInformation info)
    {
        switch (choice)
        {
            case 1:
                info.DisplayActionList(party, turn);
                break;
            case 2:
                info.DisplayCurrentInventoryItems(turn.GetCurrentItemInventory(party), info);
                break;
            case 3:
                turn.GetCurrentItemInventory(party);
                info.DisplayCurrentGearInventory(turn.CurrentGearInventory, info);
                break;
            case 0:
            default:
                turn.CurrentAttack = new Nothing();
                TurnSkipped?.Invoke(turn);
                break;
        };
        return (int)choice;
    }

    public void OnDisplayTurnSkipped(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
            new ColoredText($" did ", ConsoleColor.White),
            new ColoredText($"{turn.CurrentAttack}", CurrentAttackColor(turn)),
            new ColoredText($". Turn was skipped!", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayTaunt(TurnManager turn)
    {
        if (turn.SelectedCharacter.TauntText != null)
        {
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{turn.SelectedCharacter.TauntText}", ConsoleColor.Cyan));
            LogMessages.Add(colorText);
        }
    }

    public void OnDisplayCharacterPoisoned(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        colorText.Add(new ColoredText($"{currentTarget}", CharacterColor(currentTarget)));
        colorText.Add(new ColoredText($" was poisoned!", ConsoleColor.DarkGreen));
        LogMessages.Add(colorText);
    }

    public void OnDisplayCharacterPlagueSick(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        colorText.Add(new ColoredText($"{currentTarget}", CharacterColor(currentTarget)));
        colorText.Add(new ColoredText($" has the rot plague, ", ConsoleColor.DarkGreen));
        colorText.Add(new ColoredText($"they will be unable to have their turn", ConsoleColor.DarkGreen));
        colorText.Add(new ColoredText($".", ConsoleColor.White));
        LogMessages.Add(colorText);
    }

    public void OnDisplayConsumableUsed(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.ConsumableSelected}", ItemColor(turn.ConsumableSelected)),
            new ColoredText($" was consumed", ConsoleColor.White),
            new ColoredText($" by ", ConsoleColor.White),
            new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter))
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplaySoulBonus(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
            new ColoredText($"felt an uncanny", ConsoleColor.White),
            new ColoredText($"energy ", ConsoleColor.Cyan),
            new ColoredText($"arise within them,", ConsoleColor.White),
            new ColoredText($"they have been", ConsoleColor.White),
            new ColoredText($" granted ", ConsoleColor.Cyan),
            new ColoredText($"+1 damage!", ConsoleColor.Cyan)
        };
        
        LogMessages.Add(colorText);
    }

    public void OnDisplayActionInfo(PartyManager party, TurnManager turn)
    {
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
            new ColoredText($" used ", ConsoleColor.White),
            new ColoredText($"{turn.CurrentAttack}", CurrentAttackColor(turn)),
            new ColoredText($" on ", ConsoleColor.White),
            new ColoredText($"{currentTarget}", CharacterColor(currentTarget)),
            new ColoredText($".", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayDamageAndRemainingHealth(PartyManager party, TurnManager turn)
    {
        DisplayDamageDealt(turn, party);
        DisplayTargetCurrentHP(turn, party);
    }

    public void DisplayDamageDealt(TurnManager turn, PartyManager party)
    {
        if (turn.CurrentAttack is AreaAttack)
        {
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{turn.CurrentAttack}", CurrentAttackColor(turn)),
                new ColoredText($" deals ", ConsoleColor.White),
                new ColoredText($"{turn.CurrentDamage}", ConsoleColor.Red),
                new ColoredText($" area damage to ", ConsoleColor.White),
                new ColoredText($"ALL ", ConsoleColor.DarkCyan),
                new ColoredText($"enemies", ConsoleColor.White),
                new ColoredText($".", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }
        else
        {
            
            Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{turn.CurrentAttack}", CurrentAttackColor(turn)),
                new ColoredText($" deals ", ConsoleColor.White),
                new ColoredText($"{turn.CurrentDamage}", ConsoleColor.Red),
                new ColoredText($" damage to ", ConsoleColor.White),
                new ColoredText($"{currentTarget}", CharacterColor(currentTarget)),
                new ColoredText($".", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }
    }

    public void DisplayTargetCurrentHP(TurnManager turn, PartyManager party)
    {
        if (turn.CurrentAttack is AreaAttack)
        {
            string opponent = turn.OpponentPartyName(party);
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"Everyone in ", ConsoleColor.White),
                new ColoredText($"{opponent}'s", ConsoleColor.Yellow),
                new ColoredText($" party is now at ", ConsoleColor.White),
                new ColoredText($"-{turn.CurrentDamage}", ConsoleColor.Red),
                new ColoredText($" less health.", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }
        else
        {
            int target = turn.CurrentTarget;
            Character opponent = turn.CurrentOpponentParty(party)[target];
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{opponent}", CharacterColor(opponent)),
                new ColoredText($" is now at ", ConsoleColor.White),
                new ColoredText($"{opponent.CurrentHP}/{opponent.MaxHP} HP.", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }
    }

    public void OnDisplayDefensiveModifierEffects(PartyManager party, TurnManager turn)
    {
        string modifierProperty = turn.CurrentTargetDefensiveModifier.Value < 0 ? "reduced" : "increased";

        DefensiveAttackModifier? targetModifier = turn.CurrentOpponentParty(party)[turn.CurrentTarget].DefensiveAttackModifier;

        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{targetModifier}", DefensiveModifierColor(targetModifier)),
            new ColoredText($" {modifierProperty} the attack damage by ", ConsoleColor.White),
            new ColoredText($"{Math.Abs(turn.CurrentTargetDefensiveModifier.Value)} point/s.", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayOffensiveModifierEffects(PartyManager party, TurnManager turn)
    {
        string modifierProperty = turn.CurrentOffensiveModifier.Value < 0 ? "reduced" : "increased";
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.CurrentOffensiveModifier}", OffensiveModifierColor(turn.CurrentOffensiveModifier)),
            new ColoredText($"{modifierProperty}  the attack damage by ", ConsoleColor.White),
            new ColoredText($"{Math.Abs(turn.CurrentOffensiveModifier.Value)} point/s.", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayMissedAttack(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
            new ColoredText($" missed!", ConsoleColor.DarkRed)
        };
        LogMessages.Add(colorText);
    }


    public void OnDisplayGearObtained(PartyManager party, TurnManager turn)
    {
        Gear gear = turn.CurrentOpponentParty(party)[turn.CurrentTarget].Weapon!;
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($" - {turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained: ", ConsoleColor.Green),
            new ColoredText($"{gear.Name} ", GearColor(gear)),
            new ColoredText($"in their inventory!", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayBattleEnd(PartyManager party, TurnManager turn)
    {
        string opponentName = turn.OpponentPartyName(party);
        List<ColoredText> colorText = new List<ColoredText>();
        if (turn.CurrentOpponentParty(party) == party.HeroPartyList)
        {
            colorText.Add(new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))));
            colorText.Add(new ColoredText($" won!, ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"{opponentName}'s ", PartyColor(party, turn.CurrentOpponentParty(party))));
            colorText.Add(new ColoredText($"lost. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"Uncoded One’s forces have prevailed.", ConsoleColor.Cyan));
        }
        else if (turn.CurrentOpponentParty(party) == party.MonsterPartyList && turn.NumberBattleRounds <= 0)
        {
            colorText.Add(new ColoredText($"{turn.CurrentPartyName(party)}", PartyColor(party, turn.CurrentParty(party))));
            colorText.Add(new ColoredText($"'s won!, ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"{opponentName}'s", ConsoleColor.Red));
            colorText.Add(new ColoredText($" lost. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"The Uncoded One was defeated.", ConsoleColor.Cyan));
        }
        else
        {
            colorText.Add(new ColoredText($"{opponentName}'s", PartyColor(party, turn.CurrentOpponentParty(party))));
            colorText.Add(new ColoredText($" have been defeated. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"Next battle starting.", ConsoleColor.Cyan));
        }

        LogMessages.Add(colorText);
    }

    public List<List<ColoredText>> LogMessages = new List<List<ColoredText>>();
    public void DisplayLogMessages()
    {
        PrintColoredTextLines(LogMessages);
    }

    public void PrintColoredTextLines(List<List<ColoredText>> lines)
    {
        int row = 1;
        int column = 61;

        for (int index = 0; index < lines.Count; index++)
        {
            for (int index2 = 0; index2 < lines[index].Count; index2++)
            {
                if (column + lines[index][index2].Text.Length > 117)
                {
                    row++;
                    column = 61;
                }

                Console.ForegroundColor = lines[index][index2].Color;
                Console.SetCursorPosition(column, row);
                Console.Write(lines[index][index2].Text);
                column += lines[index][index2].Text.Length;
            }
            row++;
            column = 61;

            if (lines.Count > 25) lines.RemoveAt(0);
        }
        Console.ResetColor();
    }

    public class ColoredText
    {
        public string Text { get; set; }
        public ConsoleColor Color { get; set; }

        public ColoredText(string text, ConsoleColor color)
        {
            Text = text;
            Color = color;
        }
    }

    public void DisplayGameStatus(PartyManager party, TurnManager turn)
    {
        Console.SetCursorPosition(0, 1);
        DisplayPartyInfo(party.HeroPartyList, party, turn);
        Console.SetCursorPosition(0, 10);
        Console.WriteLine($"{new string('═', 60)}");
        DisplayPartyInfo(party.MonsterPartyList, party, turn);
        Console.SetCursorPosition(0, 20);
        Console.WriteLine($"{new string('═', 60)} \n");
        for (int i = 0; i < 120; i++)
        {
            Console.SetCursorPosition(i, 0);
            Console.Write("-");
        }
        for (int i = 0; i < 120; i++)
        {
            Console.SetCursorPosition(i, 29);
            Console.Write("-");
        }
        for (int i = 0; i <= 29; i++)
        {
            Console.SetCursorPosition(60, i);
            Console.WriteLine("|");
        }
        for (int i = 0; i <= 29; i++)
        {
            Console.SetCursorPosition(119, i);
            Console.WriteLine("|");
        }
        for (int i = 0; i <= 29; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.WriteLine("|");
        }
        Console.SetCursorPosition(0, 0);
        DisplayLogMessages();
    }

    public void DisplayCharacterTurnText(PartyManager party, TurnManager turn)
    {
        Console.Clear();
        InputManager input = new InputManager();
        DisplayGameStatus(party, turn);
        DisplayTurnInfo(turn.SelectedCharacter);
        turn.ManageTaunt(turn);
        DisplayOptionsMenu();
    }

    private void DisplayPartyInfo(List<Character> characters, PartyManager party, TurnManager turn)
    {
        foreach (Character character in characters)
            DisplayCharacter(character, turn, characters, party);
    }

    private string DisplayPadding(List<Character> characters, PartyManager party)
    {
        int padding = characters == party.MonsterPartyList ? 65 : 0;
        return new string(' ', padding);
    }

    private void DisplayCharacter(Character character, TurnManager turn, List<Character> characters, PartyManager party)
    {
        string initialPadding = DisplayPadding(characters, party);
        string characterString = ""; // I do not get why this is empty

        characterString     += DisplaySelector(character, turn);
        string secondPadding = CalculateSecondPadding(characterString);
        string gear          = DisplayCurrentGear(character);
        string armor         = DisplayCurrentAmor(character);

        Console.Write($@"  {characterString}{secondPadding}");
        Console.ResetColor();
        Console.Write(":");
        string healthBar = DisplayCurrentHealthBar(character, turn, party);
        Console.Write($" {healthBar}");
        Console.ResetColor();
        
        string soulsBar = DisplayCurrentSoulBar(character, turn, party);
        Console.WriteLine($" {soulsBar}");
        Console.ResetColor();
        Console.WriteLine($@"                 Weapon : {gear}
                 Armor  : {armor}");
    }

    private string DisplayCurrentSoulBar(Character character, TurnManager turn, PartyManager party)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        string soulBar = "";
        if (character is Hero)
        {
            if (character.SoulsValue >= 3)
                soulBar += "|∙|∙|∙|";
            else if (character.SoulsValue >= 2)
                soulBar += "|∙|∙| |";
            else if (character.SoulsValue >= 1)
                soulBar += "|∙| | |";
            else
                soulBar += "| | | |";
        }
        if (character is Monster)
        {
            if (character.SoulsXP >= 3)
                soulBar = "♦♦♦";
            else if (character.SoulsXP >= 2)
                soulBar = " ♦♦";
            else if (character.SoulsXP >= 1)
                soulBar = "  ♦";
            if (character.SoulsXP == 0)
                soulBar = "  °";
        }

        return soulBar;
    }

    private string CalculateSecondPadding(string characterString)
    {
        int counter = 0;
        foreach (char letter in characterString)
            counter++;

        int padding = 22 - counter; // Fix, this here sometimes returns null with Long character names
        return new string(' ', padding);
    }

    private string DisplaySelector(Character character, TurnManager turn)
    {
        if (character.ID == turn.SelectedCharacter.ID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return $"[ {character.Name, -2} ]";
        }
        else
            return $"{character.Name}";
    }
    private string DisplayCurrentGear(Character character)
    {
        if (character.Weapon != null)
            return $"{character.Weapon.Name}";
        else
            return $"     ";
    }

    private string DisplayCurrentAmor(Character character)
    {
        if (character.Armor != null)
            return $"{character.Armor.Name}";
        else
            return $"     ";
    }

    private string DisplayCurrentHealth(Character character, TurnManager turn, PartyManager party)
    {
        string padding = new string(' ', 1);

        return $"{character.CurrentHP}/{character.MaxHP}";
    }

    private string DisplayCurrentHealthBar(Character character, TurnManager turn, PartyManager party)
    {
        string symbol = "";
        string healthBar = "";
        if (character.CurrentHP <= character.MaxHP / 4)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            healthBar = "=     ";
            symbol = $"[!]";
        }
        else if (character.CurrentHP <= character.MaxHP / 3)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            healthBar = "==    ";
            symbol = "[!]";
        }
        else if (character.CurrentHP <= character.MaxHP / 2)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            healthBar = "===   ";
        }
        else if (character.CurrentHP <= 2 * character.MaxHP / 3)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            healthBar = "====  ";
        }
        else if (character.CurrentHP <= 3 * character.MaxHP / 4)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            healthBar = "===== ";
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            healthBar = "======";
        }
        string healthNumber = DisplayCurrentHealth(character, turn, party);

        string padding = "";
        if (healthNumber.Length < 5)
            padding += " ";
        if (symbol is "")
            padding += "   ";

        return $"{healthBar} {healthNumber}{symbol}{padding}";
    }

    public void DisplayCharacterDeath(List<Character> partyList, int target)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{partyList[target]}", CharacterColor(partyList[target])),
            new ColoredText($" has ", ConsoleColor.White),
            new ColoredText($"died", ConsoleColor.Red)
        };
        LogMessages.Add(colorText);
    }


    public void DisplayMenu(List<MenuOption> menu)
    {
        for (int index = 0; index < menu.Count; index++)
            Console.WriteLine($"{index} - {menu[index].Name}");
    }

    public void DisplayOptionsMenu()
    {
        List<MenuOption> menuList = new List<MenuOption>()
        {
            new SkipTurn(),
            new Attack(),
            new UseItem(),
            new EquipGear()
        };
        
        int row = 23;
        for (int index = 0; index < menuList.Count; index++)
        {
            Console.SetCursorPosition(1, row);
            //if (menuList[index] is not EquipGear)
                Console.WriteLine($"{index} - {menuList[index].Name}");
            //else if (turn.CurrentGearInventory.Count > 0)
               //Console.WriteLine($"{index} - {menuList[index].Name}");
            row++;
        }
    }

    public void DisplayCurrentInventoryItems(List<Consumables> currentItems, DisplayInformation info)
    {
        ClearMenu();
        if (currentItems.Count > 0)
            DisplayConsumables(currentItems);
        else
        {
            info.DisplayOptionsMenu();
            Console.SetCursorPosition(17, 25);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] Your item inventory is empty!");
            Console.ResetColor();
        }   
    }

    private void DisplayConsumables(List<Consumables> currentItems)
    { // This has potential for re-use but I'm not sure how I would go from Consumables to Gear or any other (w/o object)
        int count = 0;
        ClearMenu();
        int column = 1;
        int row = 23;
        
        foreach (Consumables item in currentItems)
        {
            string message = $"*{item.Name} ({count}) ";
            if (column + message.Length > 60)
            {
                row++;
                column = 1;
            }
            Console.SetCursorPosition(column, row);
            Console.Write(message); // DEBUG
            column += message.Length;
            count++;
        }
    }

    // Fix this for bots is not displaying properly
    public void DisplayActionList(PartyManager party, TurnManager turn)
    {
        InputManager input = new InputManager();
        ClearMenu();
        int count = 1;
        Console.SetCursorPosition(1, 23);
        foreach (AttackActions action in Enum.GetValues(typeof(AttackActions)))
        {
            if (party.ActionAvailable(action, turn))
            {
                Console.WriteLine($"{count} - {input.Description(action, turn)}");
                count++;

            }
            if (party.ActionGearAvailable(action, turn))
            {
                Console.WriteLine($"{count} - {input.Description(action, turn)}");
                count++;
            }
            Console.SetCursorPosition(1, Console.CursorTop);
        }
    }

    public void ClearMenu()
    {
        for (int index = 22; index < 28; index++)
        {
            Console.SetCursorPosition(1, index);
            Console.WriteLine(new string(' ', 50));
        }
    }

    public void DisplayTurnInfo(Character? currentCharacter)
    {
        Console.SetCursorPosition(1, 21);
        Console.WriteLine($"It's {currentCharacter}'s turn...");
    }

    public void DisplayCurrentGearInventory(List<Gear?> currentGearInventory, DisplayInformation info)
    {
        ClearMenu();
        if (currentGearInventory.Count > 0)
        {
            Console.SetCursorPosition(1, 23);
            DisplayGearInInventory(currentGearInventory);
        }
        else
        {
            info.DisplayOptionsMenu();
            Console.SetCursorPosition(17, 26);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] Your gear inventory is empty!");
            Console.ResetColor();
        }   
    }

    private void DisplayGearInInventory(List<Gear> currentGearInventory)
    { // This has potential for re-use but I'm not sure how I would go from Consumables to Gear or any other (w/o object), maybe structuring it differently
        int count = 0;
        int column = 1;
        int row = 23;

        foreach (Gear? item in currentGearInventory)
        {
            string message = $"*{item.Name} ({count}) ";
            if (column + message.Length > 60)
            {
                row++;
                column = 1;
            }
            Console.SetCursorPosition(column, row);
            Console.Write(message); // DEBUG
            column += message.Length;
            count++;
        }
    }

    public void DisplayGearEquipped(TurnManager turn)
    {
        if (turn.CurrentGearInventory[turn.SelectedGear] is Armor)
        {
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
                new ColoredText($" equipped ", ConsoleColor.White),
                new ColoredText($"{turn.SelectedCharacter.Armor}", GearColor(turn.SelectedCharacter.Armor)),
                new ColoredText($".", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }

        if (turn.CurrentGearInventory[turn.SelectedGear] is Weapon)
        {
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{turn.SelectedCharacter}", CharacterColor(turn.SelectedCharacter)),
                new ColoredText($" equipped ", ConsoleColor.White),
                new ColoredText($"{turn.SelectedCharacter.Weapon}", GearColor(turn.SelectedCharacter.Weapon)),
                new ColoredText($".", ConsoleColor.White)
            };
            LogMessages.Add(colorText);
        }
    } // reuse potential

    private ConsoleColor CharacterColor(Character character)
    {
        return character switch
        {
            TrueProgrammer  => ConsoleColor.Blue,
            VinFletcher     => ConsoleColor.Green,
            Skeleton        => ConsoleColor.DarkGray,
            StoneAmarok     => ConsoleColor.Gray,
            UncodedOne      => ConsoleColor.DarkGreen,
            ShadowOctopoid  => ConsoleColor.Magenta,
            Amarok          => ConsoleColor.DarkBlue,
            EvilRobot       => ConsoleColor.DarkCyan,
            MylaraAndSkorin => ConsoleColor.Cyan,
            _               => ConsoleColor.Yellow
        };
    }

    private ConsoleColor CurrentAttackColor(TurnManager turn)
    {
        return turn.CurrentAttack.Execute() switch
        {
            AttackActions.Punch          => ConsoleColor.Gray,
            AttackActions.BoneCrunch     => ConsoleColor.DarkGray,
            AttackActions.Unraveling     => ConsoleColor.DarkCyan,
            AttackActions.Grapple        => ConsoleColor.DarkYellow,
            AttackActions.Whip           => ConsoleColor.DarkMagenta,
            AttackActions.Bite           => ConsoleColor.DarkRed,
            AttackActions.Scratch        => ConsoleColor.DarkRed,
            AttackActions.SmartRockets   => ConsoleColor.DarkGreen,
            AttackActions.CannonBall     => ConsoleColor.DarkYellow,
            AttackActions.QuickShot      => ConsoleColor.Green,
            AttackActions.Stab           => ConsoleColor.Gray,
            AttackActions.Slash          => ConsoleColor.Gray,
            AttackActions.Nothing        => ConsoleColor.Gray,
            _                            => ConsoleColor.Gray,
        };
    }

    private ConsoleColor ItemColor(Consumables item)
    {
        return item switch
        {
            HealthPotion => ConsoleColor.Red,
            SimulasSoup  => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Magenta
        };
    }

    private ConsoleColor PartyColor(PartyManager party, List<Character> PartyList)
    {
        if (PartyList == party.HeroPartyList)
            return ConsoleColor.DarkGreen;
        else
            return ConsoleColor.DarkRed;
    }

    private ConsoleColor GearColor(Gear gear)
    {
        return gear switch
        {
            BinaryHelm       => ConsoleColor.DarkGreen,
            Sword            => ConsoleColor.Gray,
            Dagger           => ConsoleColor.Gray,
            VinsBow          => ConsoleColor.Green ,
            CannonOfConsolas => ConsoleColor.DarkYellow,
            _                => ConsoleColor.Magenta,
        };
    }

    private ConsoleColor DefensiveModifierColor(DefensiveAttackModifier defensiveModifier)
    {
        return defensiveModifier switch
        {
            StoneArmor => ConsoleColor.DarkGray,
            ObjectSight => ConsoleColor.Cyan,
            _ => ConsoleColor.Blue
        };
    }

    private ConsoleColor OffensiveModifierColor(OffensiveAttackModifier offensiveModifier)
    {
        return offensiveModifier switch
        {
            Binary => ConsoleColor.Green,
            _ => ConsoleColor.DarkYellow
        };
    }
}