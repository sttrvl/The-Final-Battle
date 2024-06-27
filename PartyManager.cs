﻿using System.Collections.Generic;
using System.IO;
using static TurnManager;

public class PartyManager
{

    // PartyPlayer and List can be a struct, also with both of their inventories
    public Character HeroPlayer { get; set; }
    public List<Character> HeroPartyList { get; set; } = new List<Character>();

    public Character MonsterPlayer { get; set; }
    public List<Character> MonsterPartyList { get; set; } = new List<Character>();
    public List<List<Character>> AdditionalMonsterLists { get; set; } = new List<List<Character>>();

    public List<Gear?> HeroGearInventory = new List<Gear?>()
    {
        new BinaryHelm()
    };

    public List<Gear?> MonsterGearInventory = new List<Gear?>()
    {
        new Dagger()
    };

    public List<List<Gear>> AdditionalGearLists { get; set; } = new List<List<Gear>>(); 
    // not used but has potential to be used

    public List<Consumables> HeroesItemInventory { get; set; } = new List<Consumables>()
    {
        new HealthPotion(), new HealthPotion(), new HealthPotion()
    };

    public List<Consumables> MonstersItemInventory { get; set; } = new List<Consumables>()
    {
        new HealthPotion()
    };

    public event Action AdditionalMonsterRound;
    public event Action<TurnManager> ConsumableItemUsed;
    public event Action<PartyManager, TurnManager> AttackSuccesful;
    public event Action<TurnManager> AttackMissed;
    public event Action<PartyManager, TurnManager> AttackInfo;
    public event Action<PartyManager, TurnManager> DefensiveModifierApplied;
    public event Action<PartyManager, TurnManager> OffensiveModifierApplied;
    public event Action<PartyManager, TurnManager> MonstersDefeated;
    public event Action<PartyManager, TurnManager> DeathOpponentGearObtained;

    public PartyManager()
    {
        ConsumableItemUsed += UpdateCharacterHealth;
    }

    public bool CheckForPoisonedCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentPoisonedCharacters.Count; index++)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index];
            if (poisoned.TurnsPoisoned == 0)
                turn.CurrentPoisonedCharacters.Remove(poisoned);
        }
        return turn.CurrentPoisonedCharacters.Count > 0;
    }

    public void PoisonCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentPoisonedCharacters.Count; index++)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index]; // we could also pass the poison damage
            poisoned.Character.CurrentHP -= 1;
            poisoned.Character.CurrentHP = poisoned.Character.HealthClamp();
            poisoned.TurnsPoisoned -= 1;
            turn.CurrentPoisonedCharacters[index] = poisoned;
        }   
    }
    public void UpdateCharacterHealth(TurnManager turn)
    {
        turn.SelectedCharacter.CurrentHP += turn.CurrentHealValue;
        turn.SelectedCharacter.CurrentHP = Math.Clamp(turn.SelectedCharacter.CurrentHP, 0, turn.SelectedCharacter.MaxHP);

        if (turn.CurrentItemInventory(this).Count > 0) turn.CurrentItemInventory(this).RemoveAt(turn.ConsumableSelectedNumber);
    }

    public void SetUpParties(List<MenuOption> menu, DisplayInformation info)
    {
        PartySetUpSettings(menu, info);
        Console.Clear();
    }

    public record Level(Gear? gear, params Character[] characters);

    public void PartySetUpSettings(List<MenuOption> menu, DisplayInformation info)
    {
        InputManager input = new InputManager();
        (HeroPlayer, MonsterPlayer) = input.MenuSetter(input.InputMenuOption(menu, info));

        CreateHeroParty(HeroPlayer, new TrueProgrammer(), new VinFletcher());
        CreateMonsterParty(MonsterPlayer, new Skeleton(), new StoneAmarok());

        List<Level> levels = LoadLevelsFromFile("Levels.txt");
        Dictionary<Character, Gear> noGear = new Dictionary<Character, Gear>();
        //AddMonsterRound(noGear, new StoneAmarok(), new StoneAmarok());
        // AddMonsterRound(noGear, new UncodedOne());

        foreach (Level level in levels)
        {
            List<Dictionary<Character, Gear?>> currentGearChoices = new List<Dictionary<Character, Gear?>>();
            if (level.gear != null)
                currentGearChoices.Add(SetUpCharacterWithGear(level.gear, level.characters));
            else
                currentGearChoices.Add(SetUpCharacterWithGear(null, level.characters));

            for (int index = 0; index < currentGearChoices.Count; index++)
            {
                AddMonsterRound(currentGearChoices[0], RetriveCharactersWithGear(currentGearChoices[0]));
            }
        }
    }

    public List<Level> LoadLevelsFromFile(string filePath)
    {
        List<Level> levels = new List<Level>();

        string[] levelStrings = File.ReadAllLines(filePath);
        foreach (string levelString in levelStrings)
        {
            List<Character> characters = new List<Character>();
            string[] tokens = levelString.Split(',');
            for (int index = 1; index < tokens.Length; index++)
                characters.Add(GetCharacter(tokens[index]));

            Level level = new Level(GetGear(tokens[0]), characters.ToArray());
            levels.Add(level);
        }
        return levels;
    }
    public Gear? GetGear(string gear)
    {
        return gear.ToLower() switch
        {
            "sword"  => new Sword(),
            "dagger" => new Dagger(),
            "nogear" => null,
            _        => null
        };
    }

    public Character GetCharacter(string character)
    {
        return character.ToLower() switch
        {
            "trueprogrammer" => new TrueProgrammer(),
            "vinfletcher"    => new VinFletcher(),
            "skeleton"       => new Skeleton(),
            "stoneamarok"    => new StoneAmarok(),
            "uncodedone"     => new UncodedOne(),
            "shadowoctopoid" => new ShadowOctopoid()
        };
    }

    public void CreateHeroParty(Character playerType, params Character[] heroes)
    {
        (HeroPartyList, HeroPlayer) = CreatePlayer(new List<Character>(), playerType);

        foreach (Character character in heroes)
            HeroPartyList.Add(character);
    }

    public void CreateMonsterParty(Character playerType, params Character[] monsters)
    {
        (MonsterPartyList, MonsterPlayer) = CreatePlayer(new List<Character>(), playerType);

        foreach (Character character in monsters)
            MonsterPartyList.Add(character);
    }

    public (List<Character>, Character) CreatePlayer(List<Character> newParty, Character partyType)
    {
        return (newParty, partyType);
    }

    public void AddMonsterRound(Dictionary<Character, Gear?> gearChoices, List<Character> characterList)
    {
        foreach (Character character in characterList)
            if (gearChoices.ContainsKey(character))
                character.Weapon = gearChoices[character];

        AdditionalMonsterLists.Add(characterList);
        AdditionalMonsterRound?.Invoke();
    }

    public Dictionary<Character, Gear?> SetUpCharacterWithGear(Gear? gearType, params Character[] characterType)
    {
        Dictionary<Character, Gear?> gearChoice = new Dictionary<Character, Gear?>();

        foreach (Character character in characterType)
                gearChoice.Add(character, gearType);

        return gearChoice;
    }

    private List<Character> RetriveCharactersWithGear(Dictionary<Character, Gear?> gearChoice)
    {
        List<Character> savedCharacters = new List<Character>();
        foreach (Character character in gearChoice.Keys)
            savedCharacters.Add(character);

        return savedCharacters;
    }

    public bool CheckForEmptyParties() => IsPartyEmpty(HeroPartyList) || IsPartyEmpty(MonsterPartyList);

    public bool IsPartyEmpty(List<Character> party) => party.Count == 0;

    public void DamageTaken(PartyManager party, TurnManager turn)
    {
        if (AttackManager(party, turn))
        {
            CheckModifier(party, turn);
            CheckSideEffect(party, turn);
            CheckTemporaryEffect(party, turn);
            CheckSoulValue(party, turn);

            UpdateHealth(party, turn);
            AttackInfo.Invoke(party, turn);
        }
    }

    private void UpdateHealth(PartyManager party, TurnManager turn)
    {
        turn.CurrentOpponentParty(party)[turn.CurrentTarget].CurrentHP -= turn.CurrentDamage;
        turn.CurrentOpponentParty(party)[turn.CurrentTarget].CurrentHP = turn.CurrentOpponentParty(party)[turn.CurrentTarget].HealthClamp();
    }

    private bool AttackManager(PartyManager party, TurnManager turn)
    {
        if (ManageProbability(turn.CurrentProbability))
        {
            AttackSuccesful.Invoke(party, turn);
            return true;
        }
        else
        {
            AttackMissed.Invoke(turn);
            return false;
        }  
    }

    public event Action<TurnManager> SoulBonus;

    private void CheckSoulValue(PartyManager party, TurnManager turn)
    {
        if (turn.SelectedCharacter.SoulsValue >= 3)
        {
            turn.CurrentDamage += 1;
            turn.SelectedCharacter.SoulsValue = 0;
            SoulBonus?.Invoke(turn);
        }
    }
    private void CheckModifier(PartyManager party, TurnManager turn)
    {
        if (turn.TargetHasDefensiveModifier(party)) ManageDefensiveModifier(party, turn);
        if (turn.TargetHasOffensiveModifier(turn)) ManageOffensiveModifier(party, turn); 
    }

    private void CheckSideEffect(PartyManager party, TurnManager turn)
    {
        if (turn.AttackHasSideEffect()) ManageSideEffect(turn, party);
    }

    private void CheckTemporaryEffect(PartyManager party, TurnManager turn)
    {
        if (turn.AttackHasTemporaryEffect())
        {
            ApplyTemporaryEffect(turn, party);
        }
    }

    private bool ManageProbability(double probability) => new Random().Next(100) < probability * 100;

    public void ManageDefensiveModifier(PartyManager party, TurnManager turn)
    {
        switch (turn.CurrentOpponentParty(party)[turn.CurrentTarget].DefensiveAttackModifier)
        {
            case StoneArmor:
                turn.CurrentTargetDefensiveModifier = new StoneArmor();
                turn.CurrentDamage += turn.CurrentTargetDefensiveModifier.Value;
                DefensiveModifierApplied.Invoke(party, turn);
                break;
            case ObjectSight when turn.CurrentAttack.AttackType == AttackTypes.Decoding:
                turn.CurrentTargetDefensiveModifier = new ObjectSight();
                turn.CurrentDamage += turn.CurrentTargetDefensiveModifier.Value;
                DefensiveModifierApplied.Invoke(party, turn);
                break;
            default:

                break;
        }
    }

    public void ManageOffensiveModifier(PartyManager party, TurnManager turn)
    {
        List<OffensiveAttackModifier?> modifier = new List<OffensiveAttackModifier?>();
        if (turn.SelectedCharacter?.Weapon?.OffensiveAttackModifier != null)
            modifier.Add(turn.SelectedCharacter.Weapon.OffensiveAttackModifier);
        if (turn.SelectedCharacter?.Armor?.OffensiveAttackModifier != null)
            modifier.Add(turn.SelectedCharacter.Armor.OffensiveAttackModifier);

        foreach (OffensiveAttackModifier? offensive in modifier)
        {
            switch (offensive)
            {
                case Binary:
                    turn.CurrentOffensiveModifier = new Binary();
                    turn.CurrentDamage += turn.CurrentOffensiveModifier.Value;
                    OffensiveModifierApplied.Invoke(party, turn);
                    break;
                default:

                    break;
            }
        }
    }

    public event Action<TurnManager> gearStolen;

    public void ManageSideEffect(TurnManager turn, PartyManager party)
    {
        switch (turn.CurrentAttack.AttackSideEffect)
        {
            case AttackSideEffects.Steal:
                Gear? opponentWeapon = turn.CurrentOpponentParty(party)[turn.CurrentTarget].Weapon;
                Gear? opponentArmor  = turn.CurrentOpponentParty(party)[turn.CurrentTarget].Armor;
                if (opponentArmor != null || opponentWeapon != null)
                    if (ManageProbability(new Random().Next(0, 100)))
                    {
                        int choice = new Random().Next(2) == 0 ? 0 : 1;
                        Gear? gearToBeStolen = choice == 0 && opponentArmor != null ? opponentArmor : opponentWeapon;
                        StealGear(turn, party, gearToBeStolen);
                        RemoveGearFromTarget(turn, party);
                        gearStolen?.Invoke(turn);
                    }
                break;
        }
    }

    public event Action<TurnManager, PartyManager> CharacterPoisoned;
    public void ApplyTemporaryEffect(TurnManager turn, PartyManager party)
    {
        switch (turn.CurrentAttack.AttackTemporaryEffect)
        {
            case AttackTemporaryEffects.Poison:
                Character poisonedTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
                turn.CurrentPoisonedCharacters.Add(new PoisonedCharacterInfo(poisonedTarget, turn.CurrentOpponentParty(party), 3));
                CharacterPoisoned?.Invoke(turn, party);
                break;
        }
    }

    private void StealGear(TurnManager turn, PartyManager party, Gear? gearToBeStolen)
    {
        if (turn.CurrentParty(party) == party.HeroPartyList)
            party.HeroGearInventory.Add(gearToBeStolen);
        else
            party.MonsterGearInventory.Add(gearToBeStolen);
    }

    private void RemoveGearFromTarget(TurnManager turn, PartyManager party) =>
        turn.CurrentOpponentParty(party)[turn.CurrentTarget].Weapon = null;

    public void UseConsumableItem(TurnManager turn) => ConsumableItemUsed.Invoke(turn);

    public void DeathManager(PartyManager party, DisplayInformation info, TurnManager turn)
    {
        if (CheckDeath(turn, party))
        {
            info.DisplayCharacterDeath(turn.CurrentOpponentParty(party), turn.CurrentTarget);
            ManageDeath(party, turn);
        }
        ManageMonsterDefeated(party, turn);
    }

    public bool CheckDeath(TurnManager turn, PartyManager party)
    {
        if (turn.CurrentTarget >= 0 && turn.CurrentTarget < turn.CurrentOpponentParty(party).Count)
            return turn.CurrentOpponentParty(party)[turn.CurrentTarget].IsDeath();

        return false;
    }

    public void ManageMonsterDefeated(PartyManager party, TurnManager turn)
    {
        if (CheckMonsterDefeat(party))
        {
            TransferDeathMonsterPartyGear(party, turn);
            TransferDeathMonsterPartyItems(party, turn);
            NextMonsterParty(turn, party);
        } 
    }

    private void TransferDeathMonsterPartyGear(PartyManager party, TurnManager turn)
    {
        string message = "";
        foreach (Gear? gear in MonsterGearInventory)
        {
            HeroGearInventory.Add(gear);
            message += $"{gear} ";
        }
        Console.WriteLine($"{turn.CurrentPartyName(party)}'s obtained: {message}");
    }

    private void TransferDeathMonsterPartyItems(PartyManager party, TurnManager turn)
    {
        string message = "";
        foreach (Consumables item in MonstersItemInventory)
        {
            HeroesItemInventory.Add(item);
            message += $"{item} ";
        }
        Console.WriteLine($"{turn.CurrentPartyName(party)}'s obtained: {message}");
    }

    public void NextMonsterParty(TurnManager turn, PartyManager party)
    {
        if (turn.NumberBattleRounds > 0 && AdditionalMonsterLists.Count > 0)
        {
            foreach (Character character in AdditionalMonsterLists[0])
                MonsterPartyList.Add(character);
                
            AdditionalMonsterLists.RemoveAt(0); // removes "used" monster list
        }
        MonstersDefeated(party, turn);
        turn.AdditionalBattleRoundUsed();
    }

    public void ManageDeath(PartyManager party, TurnManager turn)
    {
        ManageDeathCharacterGear(party, turn);
        ManageDeathCharacterSoul(party, turn);
        RemoveCharacter(turn, party);
    }

    private void RemoveCharacter(TurnManager turn, PartyManager party)
    {
        turn.CurrentOpponentParty(party).Remove(turn.CurrentOpponentParty(party)[turn.CurrentTarget]);
    }

    public void ManageDeathCharacterGear(PartyManager party ,TurnManager turn)
    {
        if (turn.CurrentTargetHasGear(turn.CurrentOpponentParty(this)))
            AddDeathGearToOpponentInventory(party, turn);
    }

    public void ManageDeathCharacterSoul(PartyManager party, TurnManager turn)
    {
        if (turn.CurrentOpponentParty(party)[turn.CurrentTarget].SoulsXP >= 1)
            turn.SelectedCharacter.SoulsValue += turn.CurrentOpponentParty(party)[turn.CurrentTarget].SoulsXP;
            // Add: invoke something? DeathOpponentGearObtained.Invoke(party, turn);
    }

    public void AddDeathGearToOpponentInventory(PartyManager party, TurnManager turn)
    {
        turn.CurrentGearInventory.Add(turn.CurrentOpponentParty(this)[turn.CurrentTarget].Weapon!);
        DeathOpponentGearObtained.Invoke(party, turn);
    }

    public bool CheckMonsterDefeat(PartyManager party) => party.IsPartyEmpty(party.MonsterPartyList);

    public bool OptionAvailable(int choice, TurnManager turn) => !OptionNotAvailable(choice, turn);

    public bool OptionNotAvailable(int? choice, TurnManager turn) => 
        choice == 3 && turn.CurrentGearInventory.Count == 0;

    public bool ActionGearAvailable(AttackActions action, TurnManager turn)
    {
        if (action != AttackActions.Nothing)
            if (action == turn.SelectedCharacter.Weapon?.Execute()) return true; 
        // assuming there is no same attack for gears we are okay
        // else: add && for extra condition

        return false;
    }

    public bool ActionAvailable(AttackActions action, TurnManager turn)
    {
        Character character = turn.SelectedCharacter;
        
        if (action != AttackActions.Nothing)
        {
            if (action == turn.SelectedCharacter.StandardAttack) return true;
            if (action == turn.SelectedCharacter.AdditionalStandardAttack) return true;
        }
        return false;
    }

    public void EquipGear(TurnManager turn, DisplayInformation info)
    {
        if (turn.CurrentGearInventory[turn.SelectedGear] is Armor)
            EquipArmor(turn, info);
        else
            EquipWeapon(turn, info);

        info.DisplayGearEquipped(turn); // could be event
        turn.CurrentGearInventory.RemoveAt(turn.SelectedGear);
    }

    private void EquipWeapon(TurnManager turn, DisplayInformation info) // pontential reuse?
    {
        turn.SelectedCharacter.Weapon = turn.CurrentGearInventory[turn.SelectedGear];
    }

    private void EquipArmor(TurnManager turn, DisplayInformation info)
    {
        turn.SelectedCharacter.Armor = turn.CurrentGearInventory[turn.SelectedGear];
    }
}