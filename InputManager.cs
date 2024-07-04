public class InputManager
{
    public string AskUser(string text)
    {
        Console.Write(text + " ");
        return Console.ReadLine() ?? "";
    }

    public void ManageInputItem(TurnManager turn, PartyManager party)
    {
        RetriveItemProperties(turn);
        party.UseConsumableItem(turn);
    }

    public void InputAction(PartyManager party, TurnManager turn, AttackActions attack) 
    {
        AttackAction? resultAction = GetAttackAction(attack);
        Gear? resultGear = GetGear(turn.Current.Character.Weapon, turn);

        if (resultGear != null && party.ActionGearAvailable(attack, turn))
            RetriveAttackProperties(turn, resultGear);
        if (resultAction != null && party.ActionAvailable(attack, turn))
            RetriveAttackProperties(turn, resultAction);
    }

    AttackAction? GetAttackAction(AttackActions attack)
    {
        return attack switch
        {
            AttackActions.Punch          => new Punch(),
            AttackActions.BoneCrunch     => new BoneCrunch(),
            AttackActions.Unraveling     => new Unraveling(),
            AttackActions.Grapple        => new Grapple(),
            AttackActions.Whip           => new Whip(),
            AttackActions.Bite           => new Bite(),
            AttackActions.Scratch        => new Scratch(),
            AttackActions.SmartRockets   => new SmartRockets(),
            _                            => new Nothing()
        };
    }

    Gear? GetGear(Gear? weapon, TurnManager turn)
    {
        return weapon switch
        {
            Sword            => new Sword(),
            Dagger           => new Dagger(),
            VinsBow          => new VinsBow(),
            CannonOfConsolas => new CannonOfConsolas(turn),
            _ => null
        };
    }

    private void RetriveAttackProperties(TurnManager turn, AttackAction attack)
    {
        turn.Current.SetAttack(attack);
        turn.Current.SetDamage(attack.AttackDamage);
        turn.Current.SetProbability(attack.AttackProbability);

        if (attack is Gear) turn.Current.Character.Weapon = (Gear)attack;
    }

    private void RetriveItemProperties(TurnManager turn)
    {
        if (turn.Current.Consumable.Heal != null) turn.Current.SetHealValue((int)turn.Current.Consumable.Heal);
    }

    public void AskInputAction(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        List<AttackActions> availableActions = ActionAvailableCheck(party, turn);
        
        int inputAction = ChooseOption("Choose an action:", availableActions.Count);
        InputAction(party, turn, availableActions[inputAction]);

        DrawOpponentTargets(turn, party, info);
        turn.Current.SetTarget(ChooseTarget(turn, party));
    }

    private void DrawOpponentTargets(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        info.ClearMenu();
        info.OptionDisplayPosition();
        if (turn.CurrentOpponentParty(party).Count > 1)
        {
            for (int index = 0; index < turn.CurrentOpponentParty(party).Count; index++)
            {
                Console.WriteLine($"{turn.CurrentOpponentParty(party)[index]}({index})");
                Console.SetCursorPosition(1, Console.CursorTop);
            }
        }
    }

    private int ChooseTarget(TurnManager turn, PartyManager party)
    {
        if (turn.Current.Attack is AreaAttack) return 0;

        int opponentPartyCount = turn.CurrentOpponentParty(party).Count;
        return opponentPartyCount == 1 ? 0 : ChooseOption("Choose a target:", opponentPartyCount);
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

    public string Description(AttackActions? action, TurnManager turn) 
    {
        return action switch
        {
            AttackActions.BoneCrunch       => new BoneCrunch().Name,
            AttackActions.Punch            => new Punch().Name,
            AttackActions.Unraveling       => new Unraveling().Name,
            AttackActions.Slash            => new Sword().Name,
            AttackActions.Stab             => new Dagger().Name,
            AttackActions.QuickShot        => new VinsBow().Name,
            AttackActions.CannonBall       => new CannonOfConsolas(turn).Name,
            AttackActions.Bite             => new Bite().Name,
            AttackActions.Grapple          => new Grapple().Name,
            AttackActions.Whip             => new Whip().Name,
            AttackActions.Nothing          => new Nothing().Name,
            AttackActions.Scratch          => new Scratch().Name,
            AttackActions.SmartRockets     => new SmartRockets().Name,
            _                              => "Unknown"
        };
    }

    public MenuOptions InputMenuOption(List<MenuOption> menu, DisplayInformation info)
    {
        int? choice = null;
        while (choice == null || CheckMenuListBounds(choice, menu))
        { 
            info.DrawMenu(menu);
            choice = ChooseOption("Please choose a Gamemode:", menu.Count);
        }
        return menu[(int)choice].Execute();
    }

    private bool CheckMenuListBounds(int? choice, List<MenuOption> menu) => choice < 0 || choice >= menu.Count;

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

        return turn.CurrentMenu((int)choice, party, turn, info);
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
        if (CheckPlagueEffects(turn, party, info)) return;

        int? option = OptionsMenuInput(party, info, turn);

        if (option == 1)
        {
            AskInputAction(turn, party, info);
            party.DamageTaken(party, turn);
        }
        if (option == 2)
        {
            if (IsListEmpty(turn.GetCurrentItemInventory(party).Count))
            {
                UserManager(turn, party, info);
                return;
            }
                
            ChooseInputItem(turn, party);
            ManageInputItem(turn, party);
        }
        if (option == 3)
        {
            if (IsListEmpty(turn.Current.GearInventory.Count))
            {
                UserManager(turn, party, info);
                return;
            }

            ChooseInputGear(turn);
            turn.CheckSelectedCharacterGear(party);
            party.ManageEquipGear(turn);
        }
    }

    private bool IsListEmpty(int count) => count == 0;

    public void ComputerAction(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        if (CheckPlagueEffects(turn, party, info)) return;

        int? computerChoice = new Computer().ComputerMenuOption(party, turn, info); 

        if (computerChoice == 1)
        {
            new Computer().ExecuteAction(party, turn);
            party.DamageTaken(party, turn);
        }
        if (computerChoice == 2)
        {
            new Computer().ComputerSelectItem(turn.GetCurrentItemInventory(party), turn);
            ManageInputItem(turn, party);
        }
        if (computerChoice == 3)
        {
            new Computer().ComputerSelectGear(turn);
            turn.CheckSelectedCharacterGear(party);
            party.ManageEquipGear(turn);
        }
    }

    private bool CheckPlagueEffects(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        {
            int? value = turn.CurrentSickPlagueCharacters[index].Character.ForcedChoice;
            if (CheckForPlague(turn, index) && NumberIsNotNull(value))
            {
                if (turn.CurrentSickPlagueCharacters[index].Character.ForcedChoice != null)
                {
                    ForceChoice(turn, party, info, index);
                    return true;
                }
            }
        }

        return false;
    }

    private void ForceChoice(TurnManager turn, PartyManager party, DisplayInformation info, int index)
    {
        turn.CurrentMenu(turn.CurrentSickPlagueCharacters[index].Character.ForcedChoice, party, turn, info);
    }

    private bool NumberIsNotNull(int? choice) => choice != null;

    private bool CheckForPlague(TurnManager turn, int index) => 
        turn.CurrentSickPlagueCharacters[index].Character.ID == turn.Current.Character.ID;

    public void ChooseInputGear(TurnManager turn)
    {
        int choice = ChooseOption("Choose gear to equip:", turn.Current.GearInventory.Count);
        turn.Current.SetGear(choice);
    }
    
    public void ChooseInputItem(TurnManager turn, PartyManager party)
    {
        turn.Current.SetConsumableNumber(ChooseOption("Choose an item:", turn.GetCurrentItemInventory(party).Count));
        turn.Current.SetConsumable(turn.GetCurrentItemInventory(party)[turn.Current.ConsumableNumber]);
    }

    public int ChooseOption(string prompt, int maxIndex)
    {
        int? choice = null;
        InputPosition();
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
                ClearSingleLine(50);
                string notNumber = "Not a number.";
                Console.Write(notNumber);
                Console.SetCursorPosition(notNumber.Length + 2, 22);
            }
        }
        return (int)choice;
    }

    public void InputPosition() => Console.SetCursorPosition(1, 22);

    public void ClearSingleLine(int value)
    {
        InputPosition();
        Console.Write(new string(' ', value));
        InputPosition();
    }
}