using System.IO;

public class DisplayInformation
{
    public DisplayInformation(PartyManager party)
    {
        party.ConsumableItemUsed        += OnDisplayConsumableUsed;
        party.AttackSuccesful           += OnDisplayActionInfo;
        party.AttackInfo                += OnDisplayDamageAndRemainingHealth;
        party.ModifierApplied           += OnDisplayModifierEffects;
        party.AttackMissed              += OnDisplayMissedAttack;
        party.DeathOpponentGearObtained += OnDisplayGearObtained;
        party.MonstersDefeated          += OnDisplayBattleEnd;
        party.SoulBonus                 += OnDisplaySoulBonus;
        party.CharacterPoisoned         += OnDisplayCharacterPoisoned;
    }

    public void OnDisplayCharacterPoisoned(TurnManager turn, PartyManager party) => 
        Console.WriteLine($"{turn.CurrentOpponentParty(party)[turn.CurrentTarget]} was poisoned!");

    public void OnDisplayConsumableUsed(TurnManager turn) =>
    Console.WriteLine($"{turn.ConsumableSelected} was consumed by {turn.SelectedCharacter}");

    public void OnDisplaySoulBonus(TurnManager turn)
    {
        Console.WriteLine($"{turn.SelectedCharacter} felt an uncanny energy arise within them, they have been granted +1 damage!");
    }
    public void OnDisplayActionInfo(PartyManager party, TurnManager turn)
    {
        Character currentTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
        Console.WriteLine($"{turn.SelectedCharacter} used {turn.CurrentAttack.Name} on {currentTarget}");
    }

    public void OnDisplayDamageAndRemainingHealth(PartyManager party, TurnManager turn)
    {
        DisplayDamageDealt(turn, party);
        DisplayTargetCurrentHP(turn, party);
    }

    public void DisplayDamageDealt(TurnManager turn, PartyManager party)
    {
        Console.WriteLine($"{turn.CurrentAttack} dealts {turn.CurrentDamage} damage to {turn.CurrentOpponentParty(party)[turn.CurrentTarget]} ");
    }

    public void DisplayTargetCurrentHP(TurnManager turn, PartyManager party)
    {
        int target = turn.CurrentTarget;
        List<Character> opponent = turn.CurrentOpponentParty(party);
        Console.WriteLine($"{opponent[target]} is now at {opponent[target].CurrentHP}/{opponent[target].MaxHP} HP. ");
    }

    public void OnDisplayModifierEffects(PartyManager party, TurnManager turn)
    {
        string modifierProperty = turn.CurrentTargetModifier.Value < 0 ? "reduced" : "increased";

        DefensiveAttackModifier targetModifier = turn.CurrentOpponentParty(party)[turn.CurrentTarget].DefensiveAttackModifier;
        Console.WriteLine($"{targetModifier} {modifierProperty} the attack damage by {Math.Abs(turn.CurrentTargetModifier.Value)} point.");
    }

    public void OnDisplayMissedAttack(TurnManager turn) => Console.WriteLine($"{turn.SelectedCharacter} missed!");

    public void OnDisplayGearObtained(PartyManager party, TurnManager turn)
    {
        string currentPartyName = turn.CurrentPartyName(party);
        Gear gear = turn.CurrentOpponentParty(party)[turn.CurrentTarget].Weapon!;
        Console.Write ($" - {currentPartyName}'s obtained {gear.Name} in their inventory!");
    }

    public void OnDisplayBattleEnd(PartyManager party, TurnManager turn)
    {
        string currentName = turn.CurrentPartyName(party);
        string opponentName = turn.OpponentPartyName(party);
        
        if (turn.NumberBattleRounds > 0 && turn.CurrentOpponentParty(party) == party.MonsterPartyList)
            Console.WriteLine($"{opponentName}'s have been defeated. Next battle starting.");
        else if (turn.CurrentCharacterList == party.HeroPartyList)
            Console.WriteLine($"{currentName}'s won!, {opponentName}'s lost. The Uncoded One was defeated.");
        else
            Console.WriteLine($"{currentName}'s won!, {opponentName}'s lost. Uncoded One’s forces have prevailed.");
    }

    // Fix: rework visuals
    public void DisplayGameStatus(PartyManager party, TurnManager turn)
    {
        Console.WriteLine($"{new string(' ', 56)} BATTLE {new string(' ', 56)}");
        Console.WriteLine($"{new string('═', 120)}");
        DisplayPartyInfo(party.HeroPartyList, party, turn);
        Console.WriteLine($"{new string('═', 120)}");
        Console.WriteLine($"{new string(' ', 58)} VS {new string(' ', 58)}");
        Console.WriteLine($"{new string('═', 120)}");
        DisplayPartyInfo(party.MonsterPartyList, party, turn);
        Console.WriteLine($"{new string('═', 120)} \n");
    }

    public void DisplayCharacterTurnText(PartyManager party, TurnManager turn)
    {
        DisplayGameStatus(party, turn);
        DisplayTurnInfo(turn.SelectedCharacter);
        DisplayOptionsMenu(turn);
    }

    private void DisplayPartyInfo(List<Character> characters, PartyManager party, TurnManager turn)
    {
        foreach (Character character in characters)
            DisplayCharacter(character, turn, characters, party);

        Console.WriteLine();
    }

    private string DisplayPadding(List<Character> characters, PartyManager party)
    {
        int padding = characters == party.MonsterPartyList ? 65 : 0;
        return new string(' ', padding);
    }

    private void DisplayCharacter(Character character, TurnManager turn, List<Character> characters, PartyManager party)
    {
        string initialPadding = DisplayPadding(characters, party);
        string characterString = "";

        characterString     += DisplaySelector(character, turn);
        string secondPadding = CalculateSecondPadding(characterString);
        string gear          = DisplayCurrentGear(character);
        string armor         = DisplayCurrentAmor(character);
        string health        = DisplayCurrentHealth(character, turn, party);

        Console.Write($@" {initialPadding}{characterString}{secondPadding}");
        Console.ResetColor();
        Console.Write(":");
        string healthBar = DisplayCurrentHealthBar(character, turn, party);
        Console.Write($" {healthBar}{health}");
        Console.ResetColor();
        string soulsBar = DisplayCurrentSoulBar(character, turn, party);
        Console.Write($" {soulsBar}");
        Console.ResetColor();
        Console.WriteLine($@"
        {initialPadding} Weapon           : {gear}
        {initialPadding} Armor            : {armor}                   ");
    }

    private string DisplayCurrentSoulBar(Character character, TurnManager turn, PartyManager party)
    {
        string soulBar = "Souls : ";
        if (character is Hero)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (character.SoulsValue >= 3)
                soulBar += "|x|x|x| *Bonus Ready*";
            else if (character.SoulsValue >= 2)
                soulBar += "|x|x| |";
            else if (character.SoulsValue >= 1)
                soulBar += "|x| | |";
            else
                soulBar += "| | | |";
        }
        else
            return soulBar = "";

        return soulBar;
    }

    private string CalculateSecondPadding(string characterString)
    {
        int counter = 0;
        foreach (char letter in characterString)
            counter++;

        int padding = 25 - counter;
        return new string(' ', padding);
    }

    private string DisplaySelector(Character character, TurnManager turn)
    {
        if (character.ID == turn.SelectedCharacter.ID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return $"[ {character.Name,-2} ]";
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

        return $"( {character.CurrentHP}/{character.MaxHP} )";
    }

    private string DisplayCurrentHealthBar(Character character, TurnManager turn, PartyManager party)
    {
        string symbol = "";
        string healthBar = "";
        if (character.CurrentHP <= character.MaxHP / 4)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            healthBar = "=     ";
            symbol = "[!]";
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

        return $"{healthBar} {symbol}";
    }
    public void DisplayCharacterDeath(List<Character> partyList, int target) => Console.WriteLine($"{partyList[target]} has died.");

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

        for (int index = 0; index < menuList.Count; index++)
        {
            if (menuList[index] is not EquipGear)
                Console.WriteLine($"{index} - {menuList[index].Name}");
            else if (turn.CurrentGearInventory.Count > 0)
                Console.WriteLine($"{index} - {menuList[index].Name}");
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
        foreach (Consumables item in currentItems)
        {
            Console.WriteLine($"{count} - {item}");
            count++;
        }
    }

    public void DisplayActionList(PartyManager party, TurnManager turn)
    {
        InputManager input = new InputManager();
        int count = 1;
        foreach (AttackActions action in Enum.GetValues(typeof(AttackActions)))
        {
            if (party.ActionAvailable(action, turn))
            {
                Console.WriteLine($"{count} - {input.Description(action)}");
                count++;
            }
            if (party.ActionGearAvailable(action, turn))
            {
                Console.WriteLine($"{count} - {input.Description(action)}");
                count++;
            }
        }
    }

    public void DisplayTurnInfo(Character? currentCharacter) => Console.WriteLine($"It's {currentCharacter}'s turn...");

    public void DisplayCurrentGearInventory(List<Gear> currentGearInventory)
    {
        if (currentGearInventory.Count > 0)
            DisplayGearInInventory(currentGearInventory);
        else
            Console.WriteLine("No items!");
    }

    private void DisplayGearInInventory(List<Gear> currentGearInventory)
    { // This has potential for re-use but I'm not sure how I would go from Consumables to Gear or any other (w/o object)
        int count = 0;
        foreach (Gear gear in currentGearInventory)
        {
            Console.WriteLine($"{count} - {gear}");
            count++;
        }
    }

    public void DisplayGearEquipped(TurnManager turn)
    {
        if (turn.CurrentGearInventory[turn.SelectedGear] is Armor)
            Console.WriteLine($"{turn.SelectedCharacter} equipped {turn.SelectedCharacter.Armor} ");
        if (turn.CurrentGearInventory[turn.SelectedGear] is Weapon)
            Console.WriteLine($"{turn.SelectedCharacter} equipped {turn.SelectedCharacter.Weapon} ");
    }
}