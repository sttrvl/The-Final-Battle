public class TurnManager // Possible structs here
{
    public int Round { get; private set; } = 0;  // could go with round counter
    public int RoundCounter { get; private set; }
    public int NumberBattleRounds { get; private set; } = 0;
    private int CharacterNumber { get; set; } = 0;
    public Character SelectedPlayerType { get; set; }  // could go with the character list
    public Character SelectedCharacter { get; set; }
    public List<Character> CurrentCharacterList { get; set; } = new List<Character>();

    public List<Consumables> CurrentItemList { get; set; } = new List<Consumables>();

    public int CurrentTarget { get; set; }
    public AttackAction CurrentAttack { get; set; }
    public int CurrentDamage { get; set; }
    public void ClampDamage() // maybe elsewhere
    {
        if (CurrentDamage < 0) CurrentDamage = 0;
    }
    public double CurrentProbability { get; set; }
    public int SelectedGear { get; set; }
    public Consumables ConsumableSelected { get; set; }
    public int ConsumableSelectedNumber { get; set; }
    public int CurrentHealValue;

    public List<Gear?> CurrentGearInventory = new List<Gear?>();
    public DefensiveAttackModifier CurrentTargetDefensiveModifier { get; set; }
    public OffensiveAttackModifier CurrentOffensiveModifier { get; set; }
    public List<PoisonedCharacterInfo> CurrentPoisonedCharacters { get; set; } = new List<PoisonedCharacterInfo>();
    public List<SickPlaguedCharacterInfo> CurrentSickPlagueCharacters { get; set; } = new List<SickPlaguedCharacterInfo>();
    public struct PoisonedCharacterInfo //maybe elsewhere
    {
        public Character Character { get; set; }
        public List<Character> CharacterParty { get; set; }
        public int TurnsPoisoned { get; set; }
        public int PoisonDamage { get; private set; }

        public PoisonedCharacterInfo(Character character, List<Character> characterParty, int turnsPoisoned, int poisonDamage)
        {
            Character = character;
            CharacterParty = characterParty;
            TurnsPoisoned = turnsPoisoned;
            PoisonDamage = poisonDamage;
        }
    };

    public struct SickPlaguedCharacterInfo // maybe elsewhere
    {
        public Character Character { get; set; }
        public List<Character> CharacterParty { get; set; }
        public int TurnsSick { get; set; }

        public SickPlaguedCharacterInfo(Character character, List<Character> characterParty, int turnsSick)
        {
            Character = character;
            CharacterParty = characterParty;
            TurnsSick = turnsSick;
        }
    }

    public event Action? turnSkipped;
    public TurnManager(PartyManager party)
    {
        CharacterTurnEnd             += UpdateCharacterNumber;
        PartyTurnEnd                 += ManagePartyTurns;
        party.AdditionalMonsterRound += NextBattle;
    }

    public event Action<TurnManager> TurnSkipped;

    public void SelectStartingPlayer(Character character, TurnManager turn) => turn.SelectedPlayerType = character;

    public int CurrentMenu(int? choice, PartyManager party, TurnManager turn, DisplayInformation info)
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

    public void ManageTurnEnd(TurnManager turn, PartyManager party)
    {
        ManagePoisoned(turn, party);
        ManagePlagueSick(turn, party);
    }

    public void ManagePartyTurns(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroParty.PartyList)
            party.HeroParty.AddTurns();
        else
            party.MonsterParty.AddTurns();
    }
    
    public void ManagePoisoned(TurnManager turn, PartyManager party)
    {
        party.RemoveInvalidSickCharacter(turn);
        if (party.CheckForPoisonedCharacter(turn)) party.PoisonCharacter(turn);
    }

    public void ManagePlagueSick(TurnManager turn, PartyManager party)
    {
        party.RemoveInvalidPlagueCharacter(turn);
        if (party.CheckForPlagueSickCharacter(turn)) party.PlagueSickCharacter(turn);
    }

    public void UpdateCharacterNumber() =>
        CharacterNumber = CharacterNumber < CurrentCharacterList.Count ? CharacterNumber + CharacterNumber++ : 0;

    public void NextBattle() => NumberBattleRounds++;
    public void CheckForNextRound(TurnManager turn, PartyManager party)
    {
        if (party.HeroParty.TurnsPlayed == party.MonsterParty.TurnsPlayed)
        {
            ManageTurnEnd(turn, party);
            Round++;
        }
    }

    public void AdvanceToNextParty() => RoundCounter++;
    public void AdditionalBattleRoundUsed() => NumberBattleRounds--;

    public void CurrentSelectedCharacter(PartyManager party)
    {
        CharacterNumber = CharacterNumber < CurrentCharacterList.Count ? CharacterNumber : 0;
        SelectedCharacter = CurrentCharacterList[CharacterNumber];
    }

    public List<Consumables> GetCurrentItemInventory(PartyManager party)
    {
        return CurrentItemList = 
            CurrentCharacterList == party.HeroParty.PartyList ? party.HeroParty.ItemInventory : party.MonsterParty.ItemInventory;
    }

    public List<Character> CurrentOpponentParty(PartyManager party) =>
        CurrentCharacterList == party.HeroParty.PartyList ? party.MonsterParty.PartyList : party.HeroParty.PartyList;

    public string CurrentPartyName(PartyManager party) => CurrentCharacterList == party.HeroParty.PartyList ? "Hero" : "Monster";
    public string OpponentPartyName(PartyManager party) => CurrentPartyName(party) == "Hero" ? "Monster" : "Hero";

    public bool TargetHasDefensiveModifier(PartyManager party) =>
        CurrentOpponentParty(party)[CurrentTarget].DefensiveAttackModifier != null;

    public bool TargetHasOffensiveModifier(TurnManager turn) => turn.SelectedCharacter?.Armor?.OffensiveAttackModifier  != null ||turn.SelectedCharacter?.Weapon?.OffensiveAttackModifier != null;

    public bool AttackHasSideEffect() => CurrentAttack.AttackSideEffect != null;

    public bool AttackHasTemporaryEffect() => CurrentAttack.AttackTemporaryEffect != null;

    public void CurrentPartyTurnData(PartyManager party)
    {
        CurrentCharacterList = CurrentParty(party);
        SelectedPlayerType = CurrentPlayerType(party);
    }

    public List<Character> CurrentParty(PartyManager party) => 
        RoundCounter % 2 == 0 ? party.HeroParty.PartyList : party.MonsterParty.PartyList;

    public Character CurrentPlayerType(PartyManager party) => RoundCounter % 2 == 0 ? party.HeroParty.Player : party.MonsterParty.Player;

    public void CurrentPartyTurnSetUp(PartyManager party)
    {
        CurrentPartyTurnData(party);
        CurrentSelectedCharacter(party);
        UpdateCurrentGearInventory(party);
    }

    public event Action CharacterTurnEnd;
    public event Action<PartyManager> PartyTurnEnd;
    public void RunCurrentParty(TurnManager turn, DisplayInformation info, PartyManager party)
    {
        for (int index = 0; index < CurrentCharacterList.Count; index++)
        {
            SelectedCharacter = CurrentCharacterList[index]; // might have a method for this
            info.UpdateTurnDisplay(party, turn);
            new InputManager().UserManager(turn, party, info);
            
            CheckComputerDelay(turn);
            party.DeathManager(turn);
            party.ManagePartyDefeated(turn);
            CharacterTurnEnd?.Invoke();
            if (party.CheckForEmptyParties()) break;
        }   
        PartyTurnEnd?.Invoke(party);
    }

    private void CheckComputerDelay(TurnManager turn)
    {
        if (turn.SelectedPlayerType is Computer) Thread.Sleep(500);
    }

    public List<Character> TauntedCharacters = new List<Character>();
    public event Action<TurnManager> TauntMessage;
    public void ManageTaunt(TurnManager turn)
    {
        if (CheckTaunt())
        {                                               
            TauntedCharacters.Add(SelectedCharacter);
            TauntMessage?.Invoke(turn);
        }      
    }

    public bool CheckTaunt() => !TauntedCharacters.Contains(SelectedCharacter);

    public bool CurrentTargetHasGear(List<Character> opponentParty) => opponentParty[CurrentTarget].Weapon is not null;

    public void CheckSelectedCharacterGear(PartyManager party)
    {
        if (SelectedCharacterHasEquippedWeapon()) TransferSelectedCharacterWeapon(party);
        if (SelectedCharacterHasEquippedArmor()) TransferSelectedCharacterArmor(party);
    }

    public void TransferSelectedCharacterWeapon(PartyManager party)
    {
        if (CurrentGearInventory == party.HeroParty.GearInventory)
            party.HeroParty.AddGear(SelectedCharacter.Weapon!);
        else
            party.MonsterParty.AddGear(SelectedCharacter.Weapon!);
    }

    public void TransferSelectedCharacterArmor(PartyManager party)
    {
        if (CurrentGearInventory == party.HeroParty.GearInventory)
            party.HeroParty.AddGear(SelectedCharacter.Armor!);
        else
            party.MonsterParty.AddGear(SelectedCharacter.Armor!);
    }

    public bool SelectedCharacterHasEquippedWeapon() => SelectedCharacter.Weapon is not null;
    public bool SelectedCharacterHasEquippedArmor() => SelectedCharacter.Armor is not null;

    public void UpdateCurrentGearInventory(PartyManager party) => CurrentGearInventory = CurrentCharacterList == 
            party.HeroParty.PartyList ? party.HeroParty.GearInventory : party.MonsterParty.GearInventory;

    public bool CurrentPlayerIsComputer() => SelectedPlayerType is Computer;

    public Character OpponentPlayer(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroParty.PartyList)
            return party.MonsterParty.Player;
        else
            return party.HeroParty.Player;
    }
}
