using System.Diagnostics.Metrics;

public class TurnManager // Possible structs here
{

    public RoundInfo Round = new RoundInfo();
    public class RoundInfo
    {
        public int CurrentRound { get; private set; }
        public int Counter { get; private set; }
        public int Battles { get; private set; }

        public void AddRound() => CurrentRound++;
        public void RemoveRound() => CurrentRound--;
        public void AddCount() => Counter++;
        public void AddBattle() => Battles++;
    }

    public CharacterInfo Current = new CharacterInfo();
    public class CharacterInfo
    {
        public int CharacterNumber { get; set; } = 0; // d
        public Character PlayerType { get; set; } // d
        public Character Character { get; set; } // d

        public List<Character> CharacterList { get; set; } = new List<Character>(); // d

        public List<Consumables> ItemList { get; set; } = new List<Consumables>(); // d

        public int Target { get; private set; } // d
        public AttackAction Attack { get; private set; } // d
        public int Damage { get; private set; } // d

        public double Probability { get; set; }
        public int Gear { get; set; }
        public Consumables Consumable { get; set; }
        public int ConsumableNumber { get; set; }
        public int HealValue;

        public List<Gear?> GearInventory = new List<Gear?>();
        public DefensiveAttackModifier TargetDefensiveModifier { get; set; }
        public OffensiveAttackModifier OffensiveModifier { get; set; }

        public void RemoveCharacter(Character character) => CharacterList.Remove(character);

        public void AddGear(Gear? gear) => GearInventory.Add(gear);
        public void RemoveGear(Gear? gear) => GearInventory.Remove(gear);

        public void IncreaseCharacterNumber() => CharacterNumber++;
        public void ResetCharacterNumber() => CharacterNumber = 0;

        public void SetTarget(int target) => Target = target;
        public void SetAttack(AttackAction attack) => Attack = attack;
        public void SetDamage(int damage) => Damage = damage;
        public void IncreaseDamage(int increase) => Damage += increase;
        public void SetProbability(double probability) => Probability = probability;
        public void SetGear(int gear) => Gear = gear;
        public void SetConsumable(Consumables consumable) => Consumable = consumable;
        public void SetConsumableNumber(int consumableNumber) => ConsumableNumber = consumableNumber;
        public void SetHealValue(int healValue) => HealValue = healValue;

        public void SetTargetDefensiveModifier(DefensiveAttackModifier modifier) => TargetDefensiveModifier = modifier;
        public void SetOffensiveModifier(OffensiveAttackModifier modifier) => OffensiveModifier = modifier;
    }

    public void ClampCurrentDamage()
    {
        if (Current.Damage < 0) Current.SetDamage(0);
    }

    public List<PoisonedCharacterInfo> CurrentPoisonedCharacters { get; set; } = new List<PoisonedCharacterInfo>();
    public List<SickPlaguedCharacterInfo> CurrentSickPlagueCharacters { get; set; } = new List<SickPlaguedCharacterInfo>();
    public struct PoisonedCharacterInfo // maybe should live in party
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

    public struct SickPlaguedCharacterInfo // maybe should live in party
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

    public void SelectStartingPlayer(Character character) => Current.PlayerType = character;

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
                info.DisplayCurrentGearInventory(Current.GearInventory, info);
                break;
            case 0:
            default:
                Current.SetAttack(new Nothing());
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

    public void ManagePoisoned(TurnManager turn, PartyManager party)
    {
        party.RemoveInvalidPoisonedCharacter(turn);
        if (party.CheckForPoisonedCharacter(turn)) party.PoisonCharacter(turn);
    }

    public void ManagePlagueSick(TurnManager turn, PartyManager party)
    {
        party.RemoveInvalidPlagueSickCharacter(turn);
        if (party.CheckForPlagueSickCharacter(turn)) party.PlagueSickCharacter(turn);
    }

    public void ManagePartyTurns(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroParty.PartyList)
            party.HeroParty.AddTurns();
        else
            party.MonsterParty.AddTurns();
    }

    public void UpdateCharacterNumber()
    {
        if (Current.CharacterNumber < Current.CharacterList.Count)
            Current.IncreaseCharacterNumber();
        else
            Current.ResetCharacterNumber();
    }


    public void NextBattle() => Round.AddBattle();

    public void CheckForNextRound(TurnManager turn, PartyManager party)
    {
        if (party.HeroParty.TurnsPlayed == party.MonsterParty.TurnsPlayed)
        {
            ManageTurnEnd(turn, party);
            Round.AddRound();
        }
    }

    public void AdvanceToNextParty() => Round.AddCount();

    public void AdditionalBattleRoundUsed() => Round.RemoveRound();

    public void CurrentSelectedCharacter()
    {
        if (Current.CharacterNumber >= Current.CharacterList.Count) Current.ResetCharacterNumber();

        Current.Character = Current.CharacterList[Current.CharacterNumber];
    }

    public List<Consumables> GetCurrentItemInventory(PartyManager party)
    {
        return Current.ItemList =
            Current.CharacterList == party.HeroParty.PartyList ? party.HeroParty.ItemInventory : party.MonsterParty.ItemInventory;
    }

    public List<Character> CurrentOpponentParty(PartyManager party) =>
        Current.CharacterList == party.HeroParty.PartyList ? party.MonsterParty.PartyList : party.HeroParty.PartyList;

    public string CurrentPartyName(PartyManager party) => Current.CharacterList == party.HeroParty.PartyList ? "Hero" : "Monster";
    public string OpponentPartyName(PartyManager party) => CurrentPartyName(party) == "Hero" ? "Monster" : "Hero";

    public bool TargetHasDefensiveModifier(PartyManager party) =>
        CurrentOpponentParty(party)[Current.Target].DefensiveAttackModifier != null;

    public bool TargetHasOffensiveModifier() => Current.Character?.Armor?.OffensiveAttackModifier  != null ||                                                Current.Character?.Weapon?.OffensiveAttackModifier != null;

    public bool AttackHasSideEffect() => Current.Attack.AttackSideEffect != null;

    public bool AttackHasTemporaryEffect() => Current.Attack.AttackTemporaryEffect != null;

    public void CurrentPartyTurnData(PartyManager party)
    {
        Current.CharacterList = CurrentParty(party);
        Current.PlayerType = CurrentPlayerType(party);
    }

    public List<Character> CurrentParty(PartyManager party) => 
        Round.Counter % 2 == 0 ? party.HeroParty.PartyList : party.MonsterParty.PartyList;

    public Character CurrentPlayerType(PartyManager party) => Round.Counter % 2 == 0 ? party.HeroParty.Player : party.MonsterParty.Player;

    public void CurrentPartyTurnSetUp(PartyManager party)
    {
        CurrentPartyTurnData(party);
        CurrentSelectedCharacter();
        UpdateCurrentGearInventory(party);
    }

    public event Action CharacterTurnEnd;
    public event Action<PartyManager> PartyTurnEnd;
    public void RunCurrentParty(TurnManager turn, DisplayInformation info, PartyManager party)
    {
        for (int index = 0; index < Current.CharacterList.Count; index++)
        {
            Current.Character = Current.CharacterList[index];
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
        if (Current.PlayerType is Computer) Thread.Sleep(500);
    }

    public List<Character> TauntedCharacters = new List<Character>();
    public event Action<TurnManager> TauntMessage;
    public void ManageTaunt(TurnManager turn)
    {
        if (CheckTaunt())
        {                                               
            TauntedCharacters.Add(Current.Character);
            TauntMessage?.Invoke(turn);
        }      
    }

    public bool CheckTaunt() => !TauntedCharacters.Contains(Current.Character);

    public bool CurrentTargetHasGear(List<Character> opponentParty) => opponentParty[Current.Target].Weapon is not null;

    public void CheckSelectedCharacterGear(PartyManager party)
    {
        if (SelectedCharacterHasEquippedWeapon()) TransferSelectedCharacterWeapon(party);
        if (SelectedCharacterHasEquippedArmor()) TransferSelectedCharacterArmor(party);
    }

    public void TransferSelectedCharacterWeapon(PartyManager party)
    {
        if (Current.GearInventory == party.HeroParty.GearInventory)
            party.HeroParty.AddGear(Current.Character.Weapon!);
        else
            party.MonsterParty.AddGear(Current.Character.Weapon!);
    }

    public void TransferSelectedCharacterArmor(PartyManager party)
    {
        if (Current.GearInventory == party.HeroParty.GearInventory)
            party.HeroParty.AddGear(Current.Character.Armor!);
        else
            party.MonsterParty.AddGear(Current.Character.Armor!);
    }

    public bool SelectedCharacterHasEquippedWeapon() => Current.Character.Weapon is not null;
    public bool SelectedCharacterHasEquippedArmor() => Current.Character.Armor is not null;

    public void UpdateCurrentGearInventory(PartyManager party) => Current.GearInventory = Current.CharacterList == 
            party.HeroParty.PartyList ? party.HeroParty.GearInventory : party.MonsterParty.GearInventory;

    public bool CurrentPlayerIsComputer() => Current.PlayerType is Computer;

    public Character OpponentPlayer(PartyManager party)
    {
        if (CurrentParty(party) == party.HeroParty.PartyList)
            return party.MonsterParty.Player;
        else
            return party.HeroParty.Player;
    }
}