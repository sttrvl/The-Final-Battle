using System.Data.Common;
using System;
using System.IO;
using static System.Collections.Specialized.BitVector32;
using System.Security.Cryptography.X509Certificates;

public class DisplayInformation
{
    public DisplayInformation(PartyManager party, TurnManager turn)
    {
        turn.TauntMessage               += OnDisplayTaunt;
        turn.TurnSkipped                += OnDisplayTurnSkipped;
        party.ConsumableItemUsed        += OnDisplayConsumableUsed;
        party.GearEquipped              += OnDisplayGearEquipped;
        party.AttackSuccesful           += OnDisplayActionInfo;
        party.AttackInfo                += OnDisplayDamageAndRemainingHealth;
        party.DefensiveModifierApplied  += OnDisplayDefensiveModifierEffects;
        party.OffensiveModifierApplied  += OnDisplayOffensiveModifierEffects;
        party.CharacterPoisoned         += OnDisplayCharacterPoisoned;
        party.PoisonDamage              += OnDisplayPoisonDamage;
        party.PlagueSickDamage          += OnDisplayPlagueSick;
        party.CharacterPlagueSick       += OnDisplayCharacterPlagueSick;
        party.AttackMissed              += OnDisplayMissedAttack;
        party.GearObtained              += OnDisplayGearObtained;
        party.DeathOpponentGearObtained += OnDisplayGearObtained; // check because It's duplicated
        party.ItemsObtained             += OnDisplayItemsObtained;
        party.SoulObtained              += OnDisplaySoulObtained;
        party.SoulBonus                 += OnDisplaySoulBonus;
        party.CharacterDied             += OnDisplayCharacterDeath;
        party.PartyDefeated             += OnDisplayBattleEnd;
    }

    public void OnDisplayTaunt(TurnManager turn)
    {
        if (turn.SelectedCharacter.TauntText != null)
        {
            List<ColoredText> colorText = new List<ColoredText>
            {
                new ColoredText($"{turn.SelectedCharacter.TauntText}", ConsoleColor.Cyan)
            };
            LogMessages.Add(colorText);
        }
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

    public void OnDisplayGearEquipped(TurnManager turn)
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
        OffensiveAttackModifier offensiveModifier = turn.CurrentOffensiveModifier;
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{offensiveModifier}", OffensiveModifierColor(offensiveModifier)),
            new ColoredText($" {modifierProperty}  the attack damage by ", ConsoleColor.White),
            new ColoredText($"{Math.Abs(turn.CurrentOffensiveModifier.Value)} point/s.", ConsoleColor.White)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayCharacterPoisoned(TurnManager turn, PartyManager party)
    {
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{currentTarget}", CharacterColor(currentTarget)),
            new ColoredText($" was poisoned!", ConsoleColor.DarkGreen)
        };
        LogMessages.Add(colorText);
    }

    public void OnDisplayPoisonDamage(Character character, int poisonDamage)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"*Poison* ", ConsoleColor.DarkGreen),
            new ColoredText($"deals", ConsoleColor.White),
            new ColoredText($" {poisonDamage} ", ConsoleColor.Red),
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

    public void OnDisplayCharacterPlagueSick(TurnManager turn, PartyManager party)
    {
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{currentTarget}", CharacterColor(currentTarget)),
            new ColoredText($" has the rot plague, ", ConsoleColor.DarkGreen),
            new ColoredText($"they will be unable to have their turn", ConsoleColor.DarkGreen),
            new ColoredText($".", ConsoleColor.White)
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

    public void OnDisplayGearObtained(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained: ", ConsoleColor.Green)
        };
        foreach (Gear gear in party.MonsterParty.GearInventory)
            colorText.Add(new ColoredText($"{gear}", GearColor(gear)));

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

    public void OnDisplayItemsObtained(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained: ", ConsoleColor.Green)
        };
        foreach (Consumables item in party.MonsterParty.ItemInventory)
            colorText.Add(new ColoredText($"{item} ", ItemColor(item)));

        LogMessages.Add(colorText);
    }

    public void OnDisplaySoulObtained(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{turn.SelectedCharacter}", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($" obtained ", PartyColor(party, turn.CurrentParty(party))),
            new ColoredText($"{turn.SelectedCharacter.SoulsValue} ", ConsoleColor.Cyan),
            new ColoredText($"souls, ", ConsoleColor.White),
            new ColoredText($"their ability ", ConsoleColor.White),
            new ColoredText($"gauge was filled", ConsoleColor.White),
            new ColoredText($".", ConsoleColor.White)
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

    public void OnDisplayBattleEnd(PartyManager party, TurnManager turn)
    {
        string opponentName = turn.OpponentPartyName(party);
        List<ColoredText> colorText = new List<ColoredText>();
        if (turn.CurrentOpponentParty(party) == party.HeroParty.PartyList)
        {
            colorText.Add(new ColoredText($"{turn.CurrentPartyName(party)}'s", PartyColor(party, turn.CurrentParty(party))));
            colorText.Add(new ColoredText($" won!, ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"{opponentName}'s ", PartyColor(party, turn.CurrentOpponentParty(party))));
            colorText.Add(new ColoredText($"lost. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"Uncoded One’s forces have prevailed.", ConsoleColor.Cyan));
        }
        else if (turn.CurrentOpponentParty(party) == party.MonsterParty.PartyList && turn.NumberBattleRounds <= 0)
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

    public void OnDisplayCharacterDeath(Character character) // should be an event
    {
        List<ColoredText> colorText = new List<ColoredText>
        {
            new ColoredText($"{character}", CharacterColor(character)),
            new ColoredText($" has ", ConsoleColor.White),
            new ColoredText($"died", ConsoleColor.Red)
        };
        LogMessages.Add(colorText);
    }

    //-----
    // maybe this should go in input

    public void DisplayGameStatus(PartyManager party, TurnManager turn)
    {
        DrawPartiesStatus(party, turn);
        DrawSeparators();
        DrawBorder();
        DisplayLogMessages();
    }

    private void DrawPartiesStatus(PartyManager party, TurnManager turn)
    {
        Console.SetCursorPosition(0, 1);
        DisplayPartyInfo(party.HeroParty.PartyList, party, turn);

        Console.SetCursorPosition(0, 11);
        DisplayPartyInfo(party.MonsterParty.PartyList, party, turn);
    }

    private void DrawSeparators()
    {
        Console.SetCursorPosition(0, 10);
        Console.WriteLine($"{new string('═', 60)}");

        Console.SetCursorPosition(0, 20);
        Console.WriteLine($"{new string('═', 60)}\n");
    }

    private void DrawBorder()
    {
        DrawLoopHorizontal(29, 120, "-");
        DrawLoopHorizontal(0, 120, "-");
        DrawLoopVertical(0, 30, "|");
        DrawLoopVertical(60, 30, "|");
        DrawLoopVertical(119, 30, "|");
    }

    public List<List<ColoredText>> LogMessages = new List<List<ColoredText>>();
    public void DisplayLogMessages() => PrintColoredTextLines(LogMessages);

    public void PrintColoredTextLines(List<List<ColoredText>> lines)
    {
        int row = 1;
        int columnStartValue = 61;
        int column = columnStartValue;

        for (int index = 0; index < lines.Count; index++)
        {
            for (int index2 = 0; index2 < lines[index].Count; index2++)
            {
                ColoredText textSegment = lines[index][index2];
                if (TextOverflowing(textSegment.Text.Length, column, 117)) 
                    (column, row) = WrapLines(columnStartValue, row);

                Console.SetCursorPosition(column, row);
                WriteTextSegment(textSegment);
                column += textSegment.Text.Length;
            }
            (column, row) = WrapLines(columnStartValue, row);
            MaxLinesDrawn(lines, 25);
        }
        Console.ResetColor();
    }

    private void WriteTextSegment(ColoredText segment)
    {
        Console.ForegroundColor = segment.Color;
        Console.Write(segment.Text);
    }

    private void MaxLinesDrawn(List<List<ColoredText>> lines, int value)
    {
        if (lines.Count > value) lines.RemoveAt(0);
    }

    private bool TextOverflowing(int textLength, int columnPosition, int maxValue)
    {
        return columnPosition + textLength > maxValue;
    }

    private (int, int) WrapLines(int fixedValue, int row) => (fixedValue, row + 1);

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

    private void DrawLoopHorizontal(int position, int amount, string symbol)
    {
        for (int i = 0; i < amount; i++)
        {
            Console.SetCursorPosition(i, position);
            Console.WriteLine($"{symbol}");
        }
        Console.SetCursorPosition(0, 0);
    }

    private void DrawLoopVertical(int position, int amount, string symbol)
    {
        for (int i = 0; i < amount; i++)
        {
            Console.SetCursorPosition(position, i);
            Console.WriteLine($"{symbol}");
        }
        Console.SetCursorPosition(0, 0);
    }

    public void UpdateTurnDisplay(PartyManager party, TurnManager turn)
    {
        Console.Clear();
        DisplayGameStatus(party, turn);
        DisplayTurnInfo(turn.SelectedCharacter);
        turn.ManageTaunt(turn);
        DisplayOptionsMenu();
    }

    private void DisplayPartyInfo(List<Character> characters, PartyManager party, TurnManager turn)
    {
        foreach (Character character in characters)
            DisplayCharacter(character, turn, party);
    }

    private void DisplayCharacter(Character character, TurnManager turn, PartyManager party)
    {
        DrawCharacterStatus(character, turn, party);
        DrawGear(character);
    }

    private void DrawCharacterStatus(Character character, TurnManager turn, PartyManager party)
    {
        string characterCurrentString = DrawCharacter(character, turn);
        DrawPadding(characterCurrentString);
        DrawSeparator(":");
        DrawHealthBar(character, turn, party);
        DrawSoulBar(character, turn, party);

        Console.WriteLine();
    }

    private string DrawCharacter(Character character, TurnManager turn)
    {
        string characterString = DisplaySelector(character, turn);
        Console.Write($"  {characterString}");
        Console.ResetColor();

        return characterString;
    }

    private void DrawPadding(string characterString) =>Console.Write($"{CalculateSecondPadding(characterString)}");

    private void DrawSeparator(string separator) => Console.Write($"{separator}");

    private void DrawHealthBar(Character character, TurnManager turn, PartyManager party)
    {
        string healthBar = DisplayCurrentHealthBar(character, turn, party);
        Console.Write($" {healthBar}");
        Console.ResetColor();
    }

    private void DrawSoulBar(Character character, TurnManager turn, PartyManager party)
    {
        string soulsBar = DisplayCurrentSoulBar(character, turn, party);
        Console.Write($" {soulsBar}");
        Console.ResetColor();
    }

    private void DrawGear(Character character)
    {
        string weapon = DisplayCurrentGear(character);
        string armor = DisplayCurrentAmor(character);
        Console.WriteLine($"                Weapon  : {weapon}");
        Console.WriteLine($"                Armor   : {armor}");
    }

    private string DisplayCurrentSoulBar(Character character, TurnManager turn, PartyManager party)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        string soulBar = "";
        if (character is Hero)    soulBar = HeroSoulGauge(character);
        if (character is Monster) soulBar = MonsterSoulValue(character);

        return soulBar;
    }

    private string MonsterSoulValue(Character character)
    {
        if (character.SoulsXP >= 3)      return "♦♦♦";
        else if (character.SoulsXP >= 2) return " ♦♦";
        else if (character.SoulsXP >= 1) return "  ♦";

        return "  °";
    }

    private string HeroSoulGauge(Character character)
    {
        if (character.SoulsValue >= 3)      return "|∙|∙|∙|";
        else if (character.SoulsValue >= 2) return "|∙|∙| |";
        else if (character.SoulsValue >= 1) return "|∙| | |";

        return "| | | |";
    }

    private string CalculateSecondPadding(string characterString)
    {
        int counter = 0;
        foreach (char letter in characterString)
            counter++;

        int padding = 22 - counter; // There is a limit to how big a name can be, due to space limitations
        return new string(' ', padding);
    }

    private string DisplaySelector(Character character, TurnManager turn)
    {
        if (character.ID == turn.SelectedCharacter.ID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return $"[ {character.Name, -2} ]";
        }

        return $"{character.Name}";
    }

    private string DisplayCurrentGear(Character character)
    {
        if (character.Weapon != null) return $"{character.Weapon.Name}";

        return $"     ";
    }

    private string DisplayCurrentAmor(Character character)
    {
        if (character.Armor != null) return $"{character.Armor.Name}";

        return $"     ";
    }

    private string DisplayCurrentHealth(Character character) => $"{character.CurrentHP}/{character.MaxHP}";

    private string DisplayCurrentHealthBar(Character character, TurnManager turn, PartyManager party)
    {
        HealthColor(character);
        string healthBar = HealthBar(character);
        string symbol = DetermineSymbolDraw();
        string healthNumber = DisplayCurrentHealth(character);
        string padding = DetermineHealthPadding(healthNumber, symbol);

        return $"{healthBar} {healthNumber}{symbol}{padding}";
    }

    private string DetermineHealthPadding(string healthNumber, string symbol)
    {
        string padding = "";
        if (symbol is "")            padding += "   ";
        if (healthNumber.Length < 5) padding += " ";
        
        return padding;
    }

    private string HealthBar(Character character)
    {
        if (character.CurrentHP <= character.MaxHP / 4)          return "=     ";
        else if (character.CurrentHP <= character.MaxHP / 3)     return "==    ";
        else if (character.CurrentHP <= character.MaxHP / 2)     return "===   ";
        else if (character.CurrentHP <= 2 * character.MaxHP / 3) return "====  ";
        else if (character.CurrentHP <= 3 * character.MaxHP / 4) return "===== ";

        return "======";
    }

    private string DetermineSymbolDraw()
    {
        if (Console.ForegroundColor == ConsoleColor.Red) return $"[!]";

        return "";
    }

    private void HealthColor(Character character)
    {
        if (character.CurrentHP <= character.MaxHP / 3)          Console.ForegroundColor = ConsoleColor.Red;
        else if (character.CurrentHP <= 2 * character.MaxHP / 3) Console.ForegroundColor = ConsoleColor.Yellow;
        else                                                     Console.ForegroundColor = ConsoleColor.Green;
    }

    public void DisplayMenu(List<MenuOption> menu, int row)
    {
        for (int index = 0; index < menu.Count; index++)
        {
            Console.SetCursorPosition(1, row);
            Console.WriteLine($"{index} - {menu[index].Name}");
            row++;
        }
    }

    public void DisplayOptionsMenu()
    {
        List<MenuOption> menuList = DefineMenuToDisplay(new SkipTurn(), new Attack(), new UseItem(), new EquipGear());
        DisplayMenu(menuList, 23);
    }
    
    public List<MenuOption> DefineMenuToDisplay(params MenuOption[] menuOptions)
    {
        List<MenuOption> menuList = new List<MenuOption>();
        // it would be more flexible to pass the enums and foreach enum I add it, maybe
        foreach (MenuOption option in menuOptions)
            menuList.Add(option);

        return menuList;
    }

    public void DisplayCurrentInventoryItems(List<Consumables> currentItems, DisplayInformation info)
    {
        ClearMenu();
        if (currentItems.Count > 0) 
            DisplayConsumables(currentItems);
        else
        {
            info.DisplayOptionsMenu();
            InventoryEmptyMessage(25);
        }   
    }

    private void DisplayConsumables(List<Consumables> currentItems)
    {
        int count = 0;
        ClearMenu();
        int column = 1;
        int row = 23;
        
        foreach (Consumables item in currentItems)
        {
            if (item is not null)
            {
                string message = $"*{item.Name} ({count}) ";
                if (TextOverflowing(message.Length, column, 60)) (column, row) = WrapLines(1, row);
                WriteStringInPosition(column, row, message);
                column = UpdateValueWith(column, message.Length); // this is repeated 
                count++;
            }
        }
    }

    public void OptionDisplayPosition() => Console.SetCursorPosition(1, 23);

    public void DisplayActionList(PartyManager party, TurnManager turn)
    {
        ClearMenu();
        int count = 0;
        OptionDisplayPosition();
        foreach (AttackActions action in Enum.GetValues(typeof(AttackActions)))
        {
            if (party.ActionAvailable(action, turn))     count = DisplayActionCount(count, action, turn);
            if (party.ActionGearAvailable(action, turn)) count = DisplayActionCount(count, action, turn);
            Console.SetCursorPosition(1, Console.CursorTop); // reuse
        }
    }

    public int DisplayActionCount(int count, AttackActions action, TurnManager turn)
    {
        Console.WriteLine($"{count} - {new InputManager().Description(action, turn)}");
        return count += 1;
    }

    public void ClearMenu() // can be made more generalized, but this is also easier to manage
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

    public void DisplayCurrentGearInventory(List<Gear> currentGearInventory, DisplayInformation info)
    {
        ClearMenu();
        // currentGearInventory.Count > 0 is equivalent to .Any
        if (currentGearInventory.Any()) DisplayGearInInventory(currentGearInventory);
        else
        {
            DisplayOptionsMenu();
            InventoryEmptyMessage(26);
        }   
    }

    private void InventoryEmptyMessage(int row)
    {
        Console.SetCursorPosition(17, row);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[!] Your gear inventory is empty!");
        Console.ResetColor();
    }

    private void DisplayGearInInventory(List<Gear> currentGearInventory)
    {
        
        int count = 0;
        int column = 1;
        int row = 23;
        OptionDisplayPosition();
        foreach (Gear? gear in currentGearInventory)
        {
            if (gear is not null) // DEBUG, returns null sometimes
            {
                string message = $"*{gear.Name} ({count}) "; 
                if (TextOverflowing(message.Length, column, 60)) (column, row) = WrapLines(1, row);
                WriteStringInPosition(column, row, message);
                column = UpdateValueWith(column, message.Length); // this is repeated 
                count++;
            }
        }
    }

    private int UpdateValueWith(int value, int update) => value + update;
    private void WriteStringInPosition(int column, int row, string message)
    {
        Console.SetCursorPosition(column, row);
        Console.Write(message);
    }

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

    private ConsoleColor PartyColor(PartyManager party, List<Character> PartyList)
    {
        if (PartyList == party.HeroParty.PartyList) return ConsoleColor.DarkGreen;

        return ConsoleColor.DarkRed;
    }

    private ConsoleColor ItemColor(Consumables item)
    {
        return item switch
        {
            HealthPotion => ConsoleColor.Red,
            SimulasSoup => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Magenta
        };
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