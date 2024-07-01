using System.IO;

public class TurnManager
{
    public int Round { get; private set; } = 0;
    public int RoundCounter { get; private set; }
    public int NumberBattleRounds { get; private set; } = 0;
    private int CharacterNumber { get; set; } = 0;
    public Character SelectedPlayerType { get; set; }
    public List<Character> CurrentCharacterList { get; set; } = new List<Character>();

    public List<Consumables> CurrentItemList { get; set; } = new List<Consumables>();
    public Character SelectedCharacter { get; set; }
    public int CurrentTarget { get; set; }
    public AttackActions SelectedAttack { get; set; }
    public AttackAction CurrentAttack { get; set; }
    public int CurrentDamage { get; set; }
    public void ClampDamage()
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
    public struct PoisonedCharacterInfo
    {
        public Character Character { get; set; }
        public List<Character> CharacterParty { get; set; }
        public int TurnsPoisoned { get; set; }

        public PoisonedCharacterInfo(Character character, List<Character> characterParty, int turnsPoisoned)
        {
            Character = character;
            CharacterParty = characterParty;
            TurnsPoisoned = turnsPoisoned;
        }
    };

    public struct SickPlaguedCharacterInfo
    {
        public Character Character { get; set; }
        public List<Character> CharacterParty { get; set; }
        public int TurnsSick { get; set; }
        public int? ForcedChoice { get; set; } = null;

        public SickPlaguedCharacterInfo(Character character, List<Character> characterParty, int turnsSick)
        {
            Character = character;
            CharacterParty = characterParty;
            TurnsSick = turnsSick;
        }
    }

    public event Action CharacterTurnEnd;
    public event Action<TurnManager, PartyManager> PartyTurnEnd;
    public event Action? turnSkipped;
    public TurnManager(PartyManager party)
    {
        CharacterTurnEnd += UpdateCharacterNumber;
        PartyTurnEnd += ManageTurnEnd;
        party.AdditionalMonsterRound += NextBattle;
    }

    public void ManageTurnEnd(TurnManager turn, PartyManager party)
    {
        
        ManagePoisoned(turn, party);
        ManagePlagueSick(turn, party);
        ManagePartyTurns(turn, party);
    }

    public void ManagePartyTurns(TurnManager turn, PartyManager party)
    {
        if (CurrentParty(party) == party.HeroPartyList)
            party.HeroPartyTurn++;
        else
            party.MonsterPartyTurn++;
    }
    
    public void GetCurrentPartyTurn(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroPartyList)
            CurrentPartyTurn = party.HeroPartyTurn;
        else
            CurrentPartyTurn = party.MonsterPartyTurn;
    }
    private int CurrentPartyTurn { get; set; } = 0;
    public void ManagePoisoned(TurnManager turn, PartyManager party)
    {
        GetCurrentPartyTurn(party);
        if (Round % 2 == 0)
            if (party.CheckForPoisonedCharacter(turn))
            {
                party.PoisonCharacter(turn);
            }
    }

    public void ManagePlagueSick(TurnManager turn, PartyManager party)
    {
        GetCurrentPartyTurn(party);
        if (Round % 2 == 0)
            if (party.CheckForPlagueSickCharacter(turn))
            {
                party.PlagueSickCharacter(turn);
            }
    }

    public void UpdateCharacterNumber() =>
        CharacterNumber = CharacterNumber < CurrentCharacterList.Count ? CharacterNumber + CharacterNumber++ : 0;

    public void NextBattle() => NumberBattleRounds++;
    public void CheckForNextRound(PartyManager party)
    {
        if (party.HeroPartyTurn == party.MonsterPartyTurn) Round++;
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
            CurrentCharacterList == party.HeroPartyList ? party.HeroItemInventory : party.MonsterItemInventory;
    }



        

    public List<Character> CurrentOpponentParty(PartyManager party) =>
        CurrentCharacterList == party.HeroPartyList ? party.MonsterPartyList : party.HeroPartyList;

    public string CurrentPartyName(PartyManager party) => CurrentCharacterList == party.HeroPartyList ? "Hero" : "Monster";
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
        RoundCounter % 2 == 0 ? party.HeroPartyList : party.MonsterPartyList;

    public Character CurrentPlayerType(PartyManager party) => RoundCounter % 2 == 0 ? party.HeroPlayer : party.MonsterPlayer;

    public void PartyTurnSetUp(PartyManager party)  
    {
        CurrentPartyTurnData(party);
        CurrentSelectedCharacter(party);
        UpdateCurrentGearInventory(party);
    }

    public event Action<TurnManager> TauntMessage;
    public List<Character> TauntedCharacters = new List<Character>();

    public void RunCurrentParty(TurnManager turn, DisplayInformation info, PartyManager party)
    {
        InputManager input = new InputManager();
        for (int index = 0; index < CurrentCharacterList.Count; index++)
        {
            SelectedCharacter = CurrentCharacterList[index];
            // Fix: separate way more DisplayCharacterTurnNext, might rename
            info.DisplayCharacterTurnText(party, turn);
            input.UserManager(turn, party, info);

            party.DeathManager(party, info, turn);
            party.ManagePartyDefeated(party, turn, info);

            CharacterTurnEnd?.Invoke();
            if (party.CheckForEmptyParties()) break;

            CheckComputerDelay(turn);
        }
        PartyTurnEnd?.Invoke(turn, party);
    }

    private void CheckComputerDelay(TurnManager turn)
    {
        if (turn.SelectedPlayerType is Computer) Thread.Sleep(500);
    }

    public void ManageTaunt(TurnManager turn) // we could make it so if the type of that character is in the list
    {                                         // so ones of the same type do not taunt again, but this makes more sense
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
        if (CurrentGearInventory == party.HeroGearInventory)
            party.HeroGearInventory.Add(SelectedCharacter.Weapon!);
        else
            party.MonsterGearInventory.Add(SelectedCharacter.Weapon!);
    }

    public void TransferSelectedCharacterArmor(PartyManager party)
    {
        if (CurrentGearInventory == party.HeroGearInventory)
            party.HeroGearInventory.Add(SelectedCharacter.Armor!);
        else
            party.MonsterGearInventory.Add(SelectedCharacter.Armor!);
    }

    public bool SelectedCharacterHasEquippedWeapon() => SelectedCharacter.Weapon is not null;
    public bool SelectedCharacterHasEquippedArmor() => SelectedCharacter.Armor is not null;

    public void UpdateCurrentGearInventory(PartyManager party) => CurrentGearInventory =
        CurrentCharacterList == party.HeroPartyList ? party.HeroGearInventory : party.MonsterGearInventory;

    public bool CurrentPlayerIsComputer() => SelectedPlayerType is Computer;

    public Character OpponentPlayer(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroPartyList)
            return party.MonsterPlayer;
        else
            return party.HeroPlayer;
    }
}
