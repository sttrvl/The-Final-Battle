using System.IO;
using System.Security.Cryptography.X509Certificates;
using static DisplayInformation;
using static System.Net.Mime.MediaTypeNames;

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
        party.CharacterPlagueSick       += OnDisplayCharacterPlagueSick;
        turn.TauntMessage               += OnDisplayTaunt;
        party.OffensiveModifierApplied  += OnDisplayOffensiveModifierEffects;
        party.ItemsObtained             += OnDisplayItemsObtained;
        party.GearObtained              += OnDisplayGearObtained;
        TurnSkipped                     += OnDisplayTurnSkipped;
    }

    public event Action<TurnManager> TurnSkipped;

    public void OnDisplayItemsObtained(TurnManager turn, PartyManager party)
    {
        string message = "";
        foreach (Consumables item in party.MonstersItemInventory)
            message += $"{item} ";

        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.CurrentPartyName(party)}'s", ConsoleColor.Blue));
        colorText.Add(new ColoredText($" obtained: ", ConsoleColor.Green));
        colorText.Add(new ColoredText($"{message}", ConsoleColor.Magenta));
        LogMessages.Add(colorText);
    }

    public void OnDisplayGearObtained(TurnManager turn, PartyManager party)
    {
        string message = "";
        foreach (Gear gear in party.MonsterGearInventory)
            message += $"{gear} ";

        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.CurrentPartyName(party)}'s", ConsoleColor.Blue));
        colorText.Add(new ColoredText($" obtained: ", ConsoleColor.Green));
        colorText.Add(new ColoredText($"{message}", ConsoleColor.Magenta));
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
                info.DisplayCurrentInventoryItems(turn.CurrentItemInventory(party));
                break;
            case 3:
                if (party.OptionNotAvailable(choice, turn) == false)
                    info.DisplayCurrentGearInventory(turn.CurrentGearInventory);
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
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.SelectedCharacter}", ConsoleColor.Yellow));
        colorText.Add(new ColoredText($" did ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{turn.CurrentAttack}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($". Turn was skipped!", ConsoleColor.White));
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
        colorText.Add(new ColoredText($"{turn.CurrentOpponentParty(party)[turn.CurrentTarget]}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($"was poisoned!", ConsoleColor.DarkGreen));
        LogMessages.Add(colorText);
    }

    public void OnDisplayCharacterPlagueSick(TurnManager turn, PartyManager party)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.CurrentOpponentParty(party)[turn.CurrentTarget]}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($" has the rot plague, ", ConsoleColor.DarkGreen));
        colorText.Add(new ColoredText($"they will be unable to have their turn", ConsoleColor.DarkGreen));
        colorText.Add(new ColoredText($".", ConsoleColor.White));
        LogMessages.Add(colorText);
    }

    public void OnDisplayConsumableUsed(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.ConsumableSelected}", ConsoleColor.Magenta));
        colorText.Add(new ColoredText($" was consumed", ConsoleColor.White));
        colorText.Add(new ColoredText($" by ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{turn.SelectedCharacter}", ConsoleColor.Yellow));
        LogMessages.Add(colorText);
    }

    public void OnDisplaySoulBonus(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.SelectedCharacter}", ConsoleColor.Yellow));
        colorText.Add(new ColoredText($"felt an uncanny energy arise within them, they have been granted +1 damage!", ConsoleColor.White));
        LogMessages.Add(colorText);
    }

    public void OnDisplayActionInfo(PartyManager party, TurnManager turn)
    {
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.SelectedCharacter}", ConsoleColor.Yellow));
        colorText.Add(new ColoredText($" used ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{turn.CurrentAttack.Name}", ConsoleColor.White));
        colorText.Add(new ColoredText($" on ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{currentTarget}", ConsoleColor.White));
        colorText.Add(new ColoredText($".", ConsoleColor.White));
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
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{turn.CurrentAttack}", ConsoleColor.DarkRed));
            colorText.Add(new ColoredText($" deals ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{turn.CurrentDamage}", ConsoleColor.Red));
            colorText.Add(new ColoredText($" area damage to ", ConsoleColor.White));
            colorText.Add(new ColoredText($"ALL ", ConsoleColor.DarkCyan));
            colorText.Add(new ColoredText($"enemies", ConsoleColor.White));
            colorText.Add(new ColoredText($".", ConsoleColor.White));
            LogMessages.Add(colorText);
        }
        else
        {
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{turn.CurrentAttack}", ConsoleColor.DarkRed));
            colorText.Add(new ColoredText($" deals ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{turn.CurrentDamage}", ConsoleColor.Red));
            colorText.Add(new ColoredText($" damage to ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{turn.CurrentOpponentParty(party)[turn.CurrentTarget]}", ConsoleColor.DarkRed));
            colorText.Add(new ColoredText($".", ConsoleColor.White));
            LogMessages.Add(colorText);
        }
    }

    public void DisplayTargetCurrentHP(TurnManager turn, PartyManager party)
    {
        if (turn.CurrentAttack is AreaAttack)
        {
            string opponent = turn.OpponentPartyName(party);
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"Everyone in ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{opponent}'s", ConsoleColor.Yellow));
            colorText.Add(new ColoredText($" party is now at ", ConsoleColor.White));
            colorText.Add(new ColoredText($"-{turn.CurrentDamage}", ConsoleColor.Red));
            colorText.Add(new ColoredText($" less health.", ConsoleColor.White));
            LogMessages.Add(colorText);
        }
        else
        {
            int target = turn.CurrentTarget;
            List<Character> opponent = turn.CurrentOpponentParty(party);
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{opponent[target]}", ConsoleColor.DarkRed));
            colorText.Add(new ColoredText($" is now at ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{opponent[target].CurrentHP}/{opponent[target].MaxHP} HP.", ConsoleColor.White));
            LogMessages.Add(colorText);
        }

    }

    public void OnDisplayDefensiveModifierEffects(PartyManager party, TurnManager turn)
    {
        string modifierProperty = turn.CurrentTargetDefensiveModifier.Value < 0 ? "reduced" : "increased";

        DefensiveAttackModifier? targetModifier = turn.CurrentOpponentParty(party)[turn.CurrentTarget].DefensiveAttackModifier;

        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{targetModifier}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($" {modifierProperty} the attack damage by ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{Math.Abs(turn.CurrentTargetDefensiveModifier.Value)} point/s.", ConsoleColor.White));
        LogMessages.Add(colorText);
    }

    public void OnDisplayOffensiveModifierEffects(PartyManager party, TurnManager turn)
    {
        string modifierProperty = turn.CurrentOffensiveModifier.Value < 0 ? "reduced" : "increased";
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.CurrentOffensiveModifier}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($"{modifierProperty}  the attack damage by ", ConsoleColor.White));
        colorText.Add(new ColoredText($"{Math.Abs(turn.CurrentOffensiveModifier.Value)} point/s.", ConsoleColor.White));
        LogMessages.Add(colorText);
    }

    public void OnDisplayMissedAttack(TurnManager turn)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{turn.SelectedCharacter}", ConsoleColor.Yellow));
        colorText.Add(new ColoredText($" missed!", ConsoleColor.DarkRed));
        LogMessages.Add(colorText);
    }


    public void OnDisplayGearObtained(PartyManager party, TurnManager turn)
    {
        string currentPartyName = turn.CurrentPartyName(party);
        Gear gear = turn.CurrentOpponentParty(party)[turn.CurrentTarget].Weapon!;
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($" - {currentPartyName}'s ", ConsoleColor.White));
        colorText.Add(new ColoredText($" obtained: ", ConsoleColor.Green));
        colorText.Add(new ColoredText($" {gear.Name} ", ConsoleColor.Cyan));
        colorText.Add(new ColoredText($"in their inventory!", ConsoleColor.DarkRed));
        LogMessages.Add(colorText);
    }

    public void OnDisplayBattleEnd(PartyManager party, TurnManager turn)
    {
        string currentName = turn.CurrentPartyName(party);
        string opponentName = turn.OpponentPartyName(party);

        List<ColoredText> colorText = new List<ColoredText>();
        if (turn.CurrentOpponentParty(party) == party.HeroPartyList)
        {
            colorText.Add(new ColoredText($"{currentName}'s", ConsoleColor.Yellow));
            colorText.Add(new ColoredText($" won!, ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"{opponentName}'s ", ConsoleColor.Red));
            colorText.Add(new ColoredText($"lost. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"Uncoded One’s forces have prevailed.", ConsoleColor.Cyan));
        }
        else if (turn.CurrentOpponentParty(party) == party.MonsterPartyList && turn.NumberBattleRounds <= 0)
        {
            colorText.Add(new ColoredText($"{currentName}", ConsoleColor.Yellow));
            colorText.Add(new ColoredText($"'s won!, ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"{opponentName}'s", ConsoleColor.Red));
            colorText.Add(new ColoredText($" lost. ", ConsoleColor.Cyan));
            colorText.Add(new ColoredText($"The Uncoded One was defeated.", ConsoleColor.Cyan));
        }
        else
        {
            colorText.Add(new ColoredText($"{opponentName}'s", ConsoleColor.DarkRed));
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
        DisplayOptionsMenu(turn);
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
                soulBar = "♦♦";
            else if (character.SoulsXP >= 1)
                soulBar = "♦";
            if (character.SoulsXP == 0)
                soulBar = "°";
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
        if (symbol != "[!]")
            padding += "   ";

        return $"{healthBar} {healthNumber}{symbol}{padding}";
    }

    public void DisplayCharacterDeath(List<Character> partyList, int target)
    {
        List<ColoredText> colorText = new List<ColoredText>();
        colorText.Add(new ColoredText($"{partyList[target]}", ConsoleColor.DarkRed));
        colorText.Add(new ColoredText($" has ", ConsoleColor.White));
        colorText.Add(new ColoredText($"died", ConsoleColor.Red));
        LogMessages.Add(colorText);
    }


    public void DisplayMenu(List<MenuOption> menu)
    {
        for (int index = 0; index < menu.Count; index++)
            Console.WriteLine($"{index} - {menu[index].Name}");
    }

    public void DisplayOptionsMenu(TurnManager turn)
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
            if (menuList[index] is not EquipGear)
                Console.WriteLine($"{index} - {menuList[index].Name}");
            else if (turn.CurrentGearInventory.Count > 0)
                Console.WriteLine($"{index} - {menuList[index].Name}");
            row++;
        }
    }

    public void DisplayCurrentInventoryItems(List<Consumables> currentItems)
    {
        if (currentItems.Count > 0)
            DisplayConsumables(currentItems);
        else
            Console.WriteLine("No items!");
    }

    private void DisplayConsumables(List<Consumables> currentItems)
    { // This has potential for re-use but I'm not sure how I would go from Consumables to Gear or any other (w/o object)
        int count = 0;
        ClearMenu();
        Console.SetCursorPosition(1, 23);
        foreach (Consumables item in currentItems)
        {
            Console.WriteLine($" -{item}({count}) ");
            Console.SetCursorPosition(1, Console.CursorTop);
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


    public void DisplayCurrentGearInventory(List<Gear?> currentGearInventory)
    {
        ClearMenu();
        Console.SetCursorPosition(1, 23);
        if (currentGearInventory.Count > 0)
            DisplayGearInInventory(currentGearInventory);
        else
            Console.WriteLine("No items!");
    }

    private void DisplayGearInInventory(List<Gear> currentGearInventory)
    { // This has potential for re-use but I'm not sure how I would go from Consumables to Gear or any other (w/o object), maybe structuring it differently
        int count = 0;
        Console.SetCursorPosition(1, 23);
        foreach (Gear gear in currentGearInventory)
        {
            Console.WriteLine($"{count} - {gear}");
            Console.SetCursorPosition(1, Console.CursorTop);
            count++;
        }
    }

    public void DisplayGearEquipped(TurnManager turn)
    {
        if (turn.CurrentGearInventory[turn.SelectedGear] is Armor)
        {
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{ turn.SelectedCharacter } ", ConsoleColor.Yellow));
            colorText.Add(new ColoredText($" equipped ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{turn.SelectedCharacter.Armor}", ConsoleColor.Magenta));
            colorText.Add(new ColoredText($".", ConsoleColor.White));
            LogMessages.Add(colorText);
        }

        if (turn.CurrentGearInventory[turn.SelectedGear] is Weapon)
        {
            List<ColoredText> colorText = new List<ColoredText>();
            colorText.Add(new ColoredText($"{turn.SelectedCharacter} ", ConsoleColor.Yellow));
            colorText.Add(new ColoredText($" equipped ", ConsoleColor.White));
            colorText.Add(new ColoredText($"{turn.SelectedCharacter.Weapon}", ConsoleColor.Magenta));
            colorText.Add(new ColoredText($".", ConsoleColor.White));
            LogMessages.Add(colorText);
        }
    } // reuse potential
}