using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using static TurnManager;

public class InputManager
{
    public string AskUser(string text)
    {
        Console.Write(text + " ");
        return Console.ReadLine() ?? "";
    }

    public void ManageInputItem(TurnManager turn, PartyManager party)
    {
        RetriveItemProperties(turn, turn.ConsumableSelected.Execute());
        party.UseConsumableItem(turn);
    }

    public void InputAction(PartyManager party, TurnManager turn, AttackActions attack) 
    {
        AttackAction? resultAction = GetAttackAction(attack);
        Gear? resultGear = GetGear(turn.SelectedCharacter.Weapon);

        if (resultGear != null && party.ActionGearAvailable(attack, turn))
            RetriveAttackProperties(turn, resultGear);
        if (resultAction != null && party.ActionAvailable(attack, turn))
            RetriveAttackProperties(turn, resultAction);
    }

    AttackAction? GetAttackAction(AttackActions attack)
    {
        return attack switch
        {
            AttackActions.Punch      => new Punch(),
            AttackActions.BoneCrunch => new BoneCrunch(),
            AttackActions.Unraveling => new Unraveling(),
            AttackActions.Grapple    => new Grapple(),
            AttackActions.Whip       => new Whip(),
            AttackActions.Bite       => new Bite(),
            AttackActions.Scratch    => new Scratch(),
            _                        => new Nothing()
        };
    }

    Gear? GetGear(Gear? weapon)
    {
        return weapon switch
        {
            Sword => new Sword(),
            Dagger => new Dagger(),
            VinsBow => new VinsBow(),
            _ => null
        };
    }

    private void RetriveAttackProperties(TurnManager turn, AttackAction attack)
    {
        turn.CurrentAttack = attack;
        turn.CurrentDamage = attack.AttackDamage;
        turn.CurrentProbability = attack.AttackProbability;

        if (attack is Gear) turn.SelectedCharacter.Weapon = (Gear)attack;
    }

    private void RetriveItemProperties(TurnManager turn, ConsumableItem attack)
    {
        if (turn.ConsumableSelected.Heal != null) turn.CurrentHealValue = (int)turn.ConsumableSelected.Heal;
    }

    public void AskInputAction(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        List<AttackActions> availableActions = ActionAvailableCheck(party, turn);
        
        int inputAction = ChooseAction("Choose an action:", availableActions.Count + 1);
        int opponentPartyCount = turn.CurrentOpponentParty(party).Count;
        // Fix: separate a bit

        info.ClearMenu();
        Console.SetCursorPosition(1, 23);
        if (turn.CurrentOpponentParty(party).Count > 1)
            for (int index = 0; index < turn.CurrentOpponentParty(party).Count; index++)
            {
                Console.WriteLine($"{turn.CurrentOpponentParty(party)[index]}({index})");
                Console.SetCursorPosition(1, Console.CursorTop);
            }

        int? inputTarget = opponentPartyCount == 1 ? 0 : ChooseOption("Choose a target:", opponentPartyCount);


        turn.CurrentTarget = (int)inputTarget;
        InputAction(party, turn, availableActions[inputAction - 1]);
    }

    public List<AttackActions> ActionAvailableCheck(PartyManager party, TurnManager turn)
    {
        List<AttackActions> AvailableActions = new List<AttackActions>();
        foreach (AttackActions action in Enum.GetValues(typeof(AttackActions)))
        {
            if (party.ActionAvailable(action, turn))
                AvailableActions.Add(action);

            if (party.ActionGearAvailable(action, turn))
                AvailableActions.Add(action);
        }

        return AvailableActions;
    }

    public string Description(AttackActions? action) 
    {
        return action switch
        {
            AttackActions.BoneCrunch       => new BoneCrunch().Name,
            AttackActions.Punch            => new Punch().Name,
            AttackActions.Unraveling       => new Unraveling().Name,
            AttackActions.Slash            => new Sword().Name,
            AttackActions.Stab             => new Dagger().Name,
            AttackActions.QuickShot        => new VinsBow().Name,
            AttackActions.Bite             => new Bite().Name,
            AttackActions.Grapple          => new Grapple().Name,
            AttackActions.Whip             => new Whip().Name,
            AttackActions.Nothing          => new Nothing().Name,
            AttackActions.Scratch          => new Scratch().Name,
            _                              => "Unknown"
        };
    }

    public MenuOptions InputMenuOption(List<MenuOption> menu, DisplayInformation info)
    {
        int? choice = null;
        DrawMenu(menu, info);
        while (choice == null || CheckListBounds(choice, menu))
        {
            try
            {
                choice = Convert.ToInt32(AskUser("Please choose a Gamemode:"));
                if (CheckListBounds(choice, menu))
                {
                    DrawMenu(menu, info);
                    Console.Write("Doesn't exist. ");
                }
            }
            catch (FormatException)
            {
                DrawMenu(menu, info);
                Console.Write("Not a number. ");
            }
        }
        return menu[(int)choice].Execute();
    }

    private void DrawMenu(List<MenuOption> menu, DisplayInformation info)
    {
        Console.Clear();
        info.DisplayMenu(menu);
    }

    private bool CheckListBounds(int? choice, List<MenuOption> menu) => choice < 0 || choice >= menu.Count;

    public (Character, Character) MenuSetter(MenuOptions option)
    {
        return option switch
        {
            MenuOptions.ComputerVsComputer => (new Computer(),    new Computer()),
            MenuOptions.PlayerVsComputer   => (new HumanPlayer(), new Computer()),
            MenuOptions.PlayerVsPlayer     => (new HumanPlayer(), new HumanPlayer())
        };
    }

    public int OptionsMenuInput(PartyManager party, DisplayInformation info, TurnManager turn)
    {
        int options = 0;
        foreach (CharacterOptions o in Enum.GetValues(typeof(CharacterOptions)))
            options++;

        
        int? choice = ChooseOption("Choose what to do:", options);

        return info.DisplayCorrectMenu((int)choice, party, turn, info);
    }

    public void UserManager(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        if (turn.CurrentPlayerIsComputer())
            ComputerAction(party, turn, info);
        else
            HumanAction(party, info, turn);
    }

    public void HumanAction(PartyManager party, DisplayInformation info, TurnManager turn)
    {
        InputManager input = new InputManager();
        int? option = null;
        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        {
            if (turn.CurrentSickPlagueCharacters[index].Character.ID == turn.SelectedCharacter.ID)
            {
                if (turn.CurrentSickPlagueCharacters[index].Character.ForcedChoice != null)
                {
                    int? value = turn.CurrentSickPlagueCharacters[index].Character.ForcedChoice;
                    info.DisplayCorrectMenu(value, party, turn, info);
                    return;
                }
            }
        }
        option = input.OptionsMenuInput(party, info, turn);


        if (option == 1)
        {
            input.AskInputAction(turn, party, info);
            party.DamageTaken(party, turn);
        }
        if (option == 2)
        {
            if (turn.CurrentItemInventory(party).Count == 0)
            {
                UserManager(turn, party, info);
                return;
            }
                
            ChooseInputItem(turn, party);
            ManageInputItem(turn, party);
        }
        if (option == 3)
        {
            if (party.OptionAvailable(option, turn))
            {
                ChooseInputGear(party, turn, info);
                turn.CheckSelectedCharacterGear(party);
                party.EquipGear(turn, info);
            }
            else
            {
                UserManager(turn, party, info);
                return;
            }
        }
    }

    public void ComputerAction(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        Computer computer = new Computer();
        int? computerChoice = null;

        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        { 
            if (turn.CurrentSickPlagueCharacters[index].Character.ID == turn.SelectedCharacter.ID)
            {
                if (turn.CurrentSickPlagueCharacters[index].ForcedChoice != null)
                {
                    info.DisplayCorrectMenu(turn.CurrentSickPlagueCharacters[index].ForcedChoice, party, turn, info);
                    return;
                }
            }
        }
        
        computerChoice = computer.MenuOption(party, turn, info);

        if (computerChoice == 1)
        {
            computer.ExecuteAction(party, turn);
            party.DamageTaken(party, turn);
        }
        if (computerChoice == 2)
        {
            computer.SelectItem(turn.CurrentItemInventory(party), turn);
            ManageInputItem(turn, party);
        }
        if (computerChoice == 3)
        {
            if (party.OptionAvailable(computerChoice, turn))
            {
                computer.SelectGear(turn);
                turn.CheckSelectedCharacterGear(party);
                party.EquipGear(turn, info);
            }
        }
    }

    public void ChooseInputGear(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        int choice = ChooseOption("Choose gear to equip:", turn.CurrentGearInventory.Count);
        turn.SelectedGear = choice;
    }
    
    public void ChooseInputItem(TurnManager turn, PartyManager party)
    {
        turn.ConsumableSelectedNumber = ChooseOption("Choose an item:", turn.CurrentItemInventory(party).Count);
        turn.ConsumableSelected = turn.CurrentItemInventory(party)[turn.ConsumableSelectedNumber];
    }

    public int ChooseOption(string prompt, int maxIndex)
    {
        int? choice = null;
        Console.SetCursorPosition(1, 22);
        while (choice == null || choice < 0 || choice >= maxIndex)
        {
            try
            {
                if (choice < 0 || choice >= maxIndex)
                {
                    ClearSingleLine(50);
                    string invalidChoice = "Invalid choice.";
                    Console.Write(invalidChoice);
                    choice = null;
                    Console.SetCursorPosition(invalidChoice.Length + 2, 22);
                }
                choice = Convert.ToInt32(AskUser(prompt));
            }
            catch (FormatException)
            {
                ClearSingleLine(0);
                string notNumber = "Not a number.";
                Console.Write(notNumber);
                Console.SetCursorPosition(notNumber.Length + 2, 22);
            }
        }
        return (int)choice;
    }

    private int ChooseAction(string prompt, int maxIndex)
    {
        int? inputAction = null;
        Console.SetCursorPosition(1, 22);
        while (inputAction == null || inputAction <= 0 || inputAction >= maxIndex)
        {
            try
            {
                if (inputAction <= 0 || inputAction >= maxIndex)
                {
                    ClearSingleLine(50);
                    string invalidChoice = "Invalid choice.";
                    Console.Write(invalidChoice);
                    inputAction = null;
                    Console.SetCursorPosition(invalidChoice.Length + 2, 22);
                }
                inputAction = Convert.ToInt32(AskUser(prompt));
            }
            catch (FormatException)
            {
                ClearSingleLine(50);
                string notNumber = "Not a number.";
                Console.Write(notNumber);
                Console.SetCursorPosition(notNumber.Length + 2, 22);
            }
        }
        return (int)inputAction!;
    }

    public void ClearSingleLine(int value)
    {
        Console.SetCursorPosition(1, 22);
        Console.Write(new string(' ', 50));
        Console.SetCursorPosition(1, 22);
    }
}
