using System;
using System.Collections.Generic;
using System.IO;
using static PartyManager;
using static TurnManager;

public class PartyManager // Honestly many of the things here could had been done within a struct..
{
    public int MonsterPartyTurn { get; set; } = 0;
    public int HeroPartyTurn { get; set; } = 0;
    public Character HeroPlayer { get; set; }
    public List<Character> HeroPartyList { get; set; } = new List<Character>();
    public Character MonsterPlayer { get; set; }
    public List<Character> MonsterPartyList { get; set; } = new List<Character>();
    public List<List<Character>> AdditionalMonsterLists { get; set; } = new List<List<Character>>();

    public List<Gear?> HeroGearInventory = new List<Gear?>();

    public List<Gear?> MonsterGearInventory = new List<Gear?>();
    public List<List<Consumables>?> AdditionalItemLists { get; set; } = new List<List<Consumables>?>();
    public List<List<Gear?>> AdditionalGearLists { get; set; } = new List<List<Gear>>();
    public List<Consumables> HeroItemInventory { get; set; } = new List<Consumables>();
    public List<Consumables> MonsterItemInventory { get; set; } = new List<Consumables>();

    public event Action AdditionalMonsterRound;
    public event Action<TurnManager> ConsumableItemUsed;
    public event Action<PartyManager, TurnManager> AttackSuccesful;
    public event Action<TurnManager> AttackMissed;
    public event Action<PartyManager, TurnManager> AttackInfo;
    public event Action<PartyManager, TurnManager> DefensiveModifierApplied;
    public event Action<PartyManager, TurnManager> OffensiveModifierApplied;
    public event Action<PartyManager, TurnManager> PartyDefeated;
    public event Action<PartyManager, TurnManager> DeathOpponentGearObtained;

    public PartyManager()
    {
        ConsumableItemUsed += ManageCharacterHealth;
    }

    public void ManageCharacterHealth(TurnManager turn) // target character?
    {
        UpdateCharacterHealth(turn);
        RemoveItem(turn);
    }

    private void UpdateCharacterHealth(TurnManager turn)
    {
        turn.SelectedCharacter.CurrentHP += turn.CurrentHealValue;
        turn.SelectedCharacter.CurrentHP = Math.Clamp(turn.SelectedCharacter.CurrentHP, 0, turn.SelectedCharacter.MaxHP);
    }

    private void RemoveItem(TurnManager turn)
    {
        List<Consumables> ItemInventory = turn.GetCurrentItemInventory(this);
        if (ItemInventory.Count > 0) ItemInventory.RemoveAt(turn.ConsumableSelectedNumber);
    }

    public bool CheckForPoisonedCharacter(TurnManager turn) => turn.CurrentPoisonedCharacters.Count > 0;

    public void RemoveInvalidSickCharacter(TurnManager turn)
    {
        for (int index = turn.CurrentPoisonedCharacters.Count - 1; index >= 0; index--)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index];
            if (poisoned.TurnsPoisoned == 0 || poisoned.Character.IsDeath()) turn.CurrentPoisonedCharacters.RemoveAt(index);
        }
    }

    public void RemoveInvalidPlagueCharacter(TurnManager turn)
    {
        for (int index = turn.CurrentSickPlagueCharacters.Count - 1; index >= 0; index--)
        {
            SickPlaguedCharacterInfo sick = turn.CurrentSickPlagueCharacters[index];
            if (sick.TurnsSick == 0 || sick.Character.IsDeath()) turn.CurrentSickPlagueCharacters.RemoveAt(index);
        }
    }

    public bool CheckForPlagueSickCharacter(TurnManager turn) => turn.CurrentSickPlagueCharacters.Count > 0;


    public event Action<Character, int> PoisonDamage;

    public void PoisonCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentPoisonedCharacters.Count; index++)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index];
            UpdateHealthReduce(poisoned.Character, poisoned.PoisonDamage);
            poisoned.TurnsPoisoned -= 1;
            turn.CurrentPoisonedCharacters[index] = poisoned;
            PoisonDamage.Invoke(poisoned.Character, poisoned.PoisonDamage);
        }   
    }

    public event Action<Character> PlagueSickDamage;
    public void PlagueSickCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        {
            SickPlaguedCharacterInfo sick = turn.CurrentSickPlagueCharacters[index];
            sick.Character.ForcedChoice = 0;
            sick.TurnsSick -= 1;
            turn.CurrentSickPlagueCharacters[index] = sick;
            PlagueSickDamage.Invoke(sick.Character);
        }
    }

    public void SetUpParties(List<MenuOption> menu, DisplayInformation info, TurnManager turn, PartyManager party)
    {
        ManagePlayers(turn, menu, info);
        PartySetUpSettings(menu, info, turn, party);
        Console.Clear();
    }

    private void ManagePlayers(TurnManager turn, List<MenuOption> menu, DisplayInformation info)
    {
        (HeroPlayer, MonsterPlayer) = new InputManager().MenuSetter(new InputManager().InputMenuOption(menu, info));
        SelectStartingPlayer(HeroPlayer, turn);
    }

    // this should maybe go in Turn
    private void SelectStartingPlayer(Character character, TurnManager turn) => turn.SelectedPlayerType = HeroPlayer;

    public record Level(Consumables? ExtraItemType, int itemAmount, 
                        Gear? ExtraGearType       , int gearAmount, Gear? equippedGearChoice, params Character[] characters);


    public void PartySetUpSettings(List<MenuOption> menu, DisplayInformation info, TurnManager turn, PartyManager party)
    {
        List<Level> levels = LoadLevelsFromFile("Levels.txt", turn);

        foreach (Level level in levels)
        {
            List<Dictionary<Character, Gear?>> currentGearChoices = FileGearChoices(level);
            ManageFileRounds(currentGearChoices, level, party);
        }
    }

    private void ManageFileRounds(List<Dictionary<Character, Gear?>> currentGearChoices, Level level, PartyManager party)
    {
        for (int index = 0; index < currentGearChoices.Count; index++)
            AddRound(currentGearChoices[0], RetriveCharactersWithGear(currentGearChoices[0]), party,
                     level.ExtraItemType, level.itemAmount, level.ExtraGearType, level.gearAmount);
    }

    private List<Dictionary<Character, Gear?>> FileGearChoices(Level level)
    {
        List<Dictionary<Character, Gear?>> choices = new List<Dictionary<Character, Gear?>>();
        if (level.equippedGearChoice != null)
            choices.Add(SetUpCharacterWithGear(level.equippedGearChoice, level.characters));
        else
            choices.Add(SetUpCharacterWithGear(null, level.characters));

        return choices;
    }

    // fix encapsulate better
    public List<Level> LoadLevelsFromFile(string filePath, TurnManager turn)
    {
        string[] levelStrings = File.ReadAllLines(filePath);
        List<string> levelStringList = levelStrings.ToList();
        IgnoreListFirstLines(levelStringList, 2);

        List<Level> levels = new List<Level>();
        foreach (string levelString in levelStringList)
        {
            string[] tokens = levelString.Split(',');

            List<Character> characters = ManageFileCharacters(turn, tokens);
            int extraItemAmount = ManageFileItemAmount(tokens);
            int extraGearAmount = ManageFileGearAmount(tokens);

            levels.Add(new Level(GetItem(tokens[0].Trim(), turn), extraItemAmount, GetGear(tokens[2].Trim(), turn),
                       extraGearAmount, GetGear(tokens[4].Trim(), turn), characters.ToArray()));
        }
        return levels;
    }

    private int ManageFileItemAmount(string[] tokens) => Convert.ToInt32(tokens[1].Trim());
    private int ManageFileGearAmount(string[] tokens) => Convert.ToInt32(tokens[3].Trim());

    private List<Character> ManageFileCharacters(TurnManager turn, string[] tokens)
    {
        List<Character> characters = new List<Character>();
        for (int index = 5; index < tokens.Length; index++)
            characters.Add(GetCharacter(tokens[index].Trim(), turn));

        return characters;
    }

    private void IgnoreListFirstLines(List<string> levelStringList, int amount)
    {
        levelStringList.RemoveRange(0, amount);
    }

    public Consumables? GetItem(string consumableType, TurnManager turn)
    {
        return consumableType.ToLower() switch
        {
            "healthpotion" => new HealthPotion(),
            "simulassoup"  => new SimulasSoup(turn),
            "empty" => null,
            _       => null
        };
    }

    public Gear? GetGear(string gear, TurnManager turn)
    {
        return gear.ToLower() switch
        {
            "sword"            => new Sword(),
            "dagger"           => new Dagger(),
            "vinsbow"          => new VinsBow(),
            "cannonofconsolas" => new CannonOfConsolas(turn),
            "binaryhelm"       => new BinaryHelm(),
            "nogear"           => null,
            "default"          => default,
            _                  => null
        };
    }

    public Character GetCharacter(string character, TurnManager turn)
    {
        return character.ToLower() switch
        {
            "trueprogrammer"  => new TrueProgrammer(turn.SelectedPlayerType), 
            "vinfletcher"     => new VinFletcher(),
            "skeleton"        => new Skeleton(),
            "stoneamarok"     => new StoneAmarok(),
            "uncodedone"      => new UncodedOne(),
            "shadowoctopoid"  => new ShadowOctopoid(),
            "amarok"          => new Amarok(),
            "evilrobot"       => new EvilRobot(),
            "mylaraandskorin" => new MylaraAndSkorin(turn),
        };
    }

    public void AddRound(Dictionary<Character, Gear?> gearChoices, List<Character> characterList, PartyManager party, 
                         Consumables itemType, int itemAmount, Gear? gearType, int gearAmount)
    {
        AddGearChoices(gearChoices, characterList);

        if (party.IsPartyEmpty(party.HeroPartyList))
            CreateHeroParty(gearType, gearAmount, itemType, itemAmount, characterList);
        else if (party.IsPartyEmpty(party.MonsterPartyList))
            CreateMonsterParty(gearType, gearAmount, itemType, itemAmount, characterList);
        else
            ManageAdditionalMonsterLists(gearType, gearAmount, itemType, itemAmount, characterList);
    }

    
    private void CreateHeroParty(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                    List<Character> characterList)
    {
        foreach (Character character in characterList)
            HeroPartyList.Add(character);

        if (IsValid(gearType, gearAmount))
            for (int index = 0; index < gearAmount; index++)
                HeroGearInventory.Add(gearType);

        if (IsValid(itemType, itemAmount))
            for (int index = 0; index < itemAmount; index++)
                HeroItemInventory.Add(itemType);
    }
    // I think it was more complicated to try to reuse code here, or at least that I can think of
    private void CreateMonsterParty(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                    List<Character> characterList)
    {
        foreach (Character character in characterList)
            MonsterPartyList.Add(character);

        if (IsValid(gearType, gearAmount))
            for (int index = 0; index < gearAmount; index++)
                MonsterGearInventory.Add(gearType);

        if (IsValid(itemType, itemAmount))
            for (int index = 0; index < itemAmount; index++)
                MonsterItemInventory.Add(itemType);
    }

    private void ManageAdditionalMonsterLists(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                              List<Character> characterList)
    {
        if (IsValid(gearType, gearAmount))
            AdditionalGearLists.Add(CreateGearList(gearType, gearAmount));
        else
            AdditionalGearLists.Add(new List<Gear?>());

        if (IsValid(itemType, itemAmount))
            AdditionalItemLists.Add(CreateItemList(itemType, itemAmount));
        else
            AdditionalItemLists.Add(new List<Consumables>());

        AdditionalMonsterLists.Add(characterList);
        AdditionalMonsterRound?.Invoke();
    }

    private List<Gear?> CreateGearList(Gear? inventoryItem, int amount)
    {
        List<Gear?> itemList = new List<Gear?>();
        for (int index = 0; index < amount; index++)
            itemList.Add(inventoryItem);

        return itemList;
    }

    private List<Consumables> CreateItemList(Consumables inventoryItem, int amount)
    {
        List<Consumables> itemList = new List<Consumables>();
        for (int index = 0; index < amount; index++)
            itemList.Add(inventoryItem);

        return itemList;
    }

    private bool IsValid(InventoryItem? inventoryItem, int amount) => inventoryItem != null && amount > 0;
    private void AddGearChoices(Dictionary<Character, Gear?> gearChoices, List<Character> characterList)
    {
        foreach (Character character in characterList)
            if (gearChoices.ContainsKey(character) && gearChoices[character] != default)
                character.Weapon = gearChoices[character];
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

            ManageHealth(party, turn);
            AttackInfo.Invoke(party, turn);
        }
    }

    private void ManageHealth(PartyManager party, TurnManager turn)
    {
        if (turn.CurrentAttack is AreaAttack)
            foreach (Character character in turn.CurrentOpponentParty(party))
                UpdateHealthReduce(character, turn.CurrentDamage);
        else
            UpdateHealthReduce(turn.CurrentOpponentParty(party)[turn.CurrentTarget], turn.CurrentDamage);
    }

    private void UpdateHealthReduce(Character character, int damage)
    {
        character.CurrentHP -= damage;
        character.CurrentHP = character.HealthClamp();
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
        if (turn.AttackHasTemporaryEffect()) ApplyTemporaryEffect(turn, party);
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
                turn.ClampDamage();
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
        List<Character> opponentParty = turn.CurrentOpponentParty(party);
        int target = turn.CurrentTarget;
        switch (turn.CurrentAttack.AttackTemporaryEffect)
        {
            case AttackTemporaryEffects.Poison:
                Character poisonedTarget = opponentParty[target];
                turn.CurrentPoisonedCharacters.Add(new PoisonedCharacterInfo(poisonedTarget, opponentParty, 3, 1));
                CharacterPoisoned?.Invoke(turn, party);
                break;
            case AttackTemporaryEffects.RotPlague:
                Character sickPlagueTarget = opponentParty[target];

                if (IsNotSick(turn, sickPlagueTarget))
                    turn.CurrentSickPlagueCharacters.Add(new SickPlaguedCharacterInfo(sickPlagueTarget, opponentParty, 1));
                CharacterPlagueSick?.Invoke(turn, party);
                break;
        }
    }

    private bool IsNotSick(TurnManager turn, Character sickPlagueTarget)
    {
        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        {
            SickPlaguedCharacterInfo sick = turn.CurrentSickPlagueCharacters[index];
            if (sick.Character.ID == sickPlagueTarget.ID) return false;
        }

        return true;
    }
    public event Action<TurnManager, PartyManager> CharacterPlagueSick;

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

    public event Action<Character> CharacterDied;
    public void DeathManager(PartyManager party, DisplayInformation info, TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentOpponentParty(party).Count; index++)
        {
            if (CheckDeath(turn.CurrentOpponentParty(party)[index]))
            {
                CharacterDied.Invoke(turn.CurrentOpponentParty(party)[index]);
                ManageDeath(party, turn);
            }
        }
    }

    public bool CheckDeath(Character character)
    {
        return character.IsDeath();
    }

    public void ManagePartyDefeated(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        if (CheckPartyDefeat(party, party.MonsterPartyList)) ManageMonsterDefeated(party, turn);
        if (CheckPartyDefeat(party, party.HeroPartyList)) ManageHeroesDefeated(party, turn, info);
    }

    public void ManageMonsterDefeated(PartyManager party, TurnManager turn)
    {
        TransferDeathMonsterPartyGear(party, turn);
        TransferDeathMonsterPartyItems(party, turn);
        NextMonsterParty(turn, party);
    }

    public void ManageHeroesDefeated(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        if (party.IsPartyEmpty(party.HeroPartyList)) PartyDefeated.Invoke(party, turn);
    }

    private void TransferDeathMonsterPartyGear(PartyManager party, TurnManager turn)
    {
        for (int index = 0; index < MonsterGearInventory.Count; index++)
        {
            HeroGearInventory.Add(MonsterGearInventory[index]);
            MonsterGearInventory.Remove(MonsterGearInventory[index]);
        }

        GearObtained?.Invoke(turn, party);
    }

    public event Action<TurnManager, PartyManager> GearObtained;

    private void TransferDeathMonsterPartyItems(PartyManager party, TurnManager turn)
    {
        string message = "";
        for (int index = 0; index < MonsterItemInventory.Count; index++)
        {
            HeroItemInventory.Add(MonsterItemInventory[index]);
            MonsterItemInventory.Remove(MonsterItemInventory[index]);
        }
            
        ItemsObtained?.Invoke(turn, party);
    }

    public event Action<TurnManager, PartyManager> ItemsObtained;

    public void NextMonsterParty(TurnManager turn, PartyManager party)
    {
        if (HasAdditionalRounds(turn) && HasAdditionalMonsters())
        {
            AddAdditionalMonsterList();
            AddAdditionalItemList();
            AddAdditionalGearList();
        }
        PartyDefeated?.Invoke(party, turn);
        turn.AdditionalBattleRoundUsed();
    }

    private bool HasAdditionalRounds(TurnManager turn) => turn.NumberBattleRounds > 0;
    private bool HasAdditionalMonsters() => AdditionalMonsterLists.Count > 0; // can be reused

    private void AddAdditionalItemList()
    {
        if (AdditionalItemLists.Count > 0)
            foreach (Consumables items in AdditionalItemLists[0])
                MonsterItemInventory.Add(items);

        AdditionalItemLists.RemoveAt(0);
    }
    private void AddAdditionalGearList()
    {
        if (AdditionalGearLists.Count > 0)
            foreach (Gear? gear in AdditionalGearLists[0])
                MonsterGearInventory.Add(gear);

        AdditionalGearLists.RemoveAt(0);
    }
    private void AddAdditionalMonsterList()
    {
        foreach (Character character in AdditionalMonsterLists[0])
            MonsterPartyList.Add(character);

        AdditionalMonsterLists.RemoveAt(0);
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
        if (turn.CurrentTargetHasGear(turn.CurrentOpponentParty(this))) AddDeathGearToOpponentInventory(party, turn);
    }

    public event Action<TurnManager, PartyManager> SoulObtained;
    public void ManageDeathCharacterSoul(PartyManager party, TurnManager turn)
    {
        if (turn.SelectedCharacter is Hero) // maybe in a method
        {
            UpdateSoulValue(turn);
            SoulObtained?.Invoke(turn, this);
        }
    }

    public void UpdateSoulValue(TurnManager turn)
    {
        if (turn.CurrentOpponentParty(this)[turn.CurrentTarget].SoulsXP >= 1)
            turn.SelectedCharacter.SoulsValue += turn.CurrentOpponentParty(this)[turn.CurrentTarget].SoulsXP;
    }
    public void AddDeathGearToOpponentInventory(PartyManager party, TurnManager turn)
    {
        turn.CurrentGearInventory.Add(turn.CurrentOpponentParty(this)[turn.CurrentTarget].Weapon!);
        DeathOpponentGearObtained.Invoke(party, turn);
    }

    public bool CheckPartyDefeat(PartyManager party, List<Character> partyList) => party.IsPartyEmpty(partyList);

    public bool OptionAvailable(int? choice, TurnManager turn) => !OptionNotAvailable(choice, turn);

    public bool OptionNotAvailable(int? choice, TurnManager turn) => 
        choice == 3 && turn.CurrentGearInventory.Count == 0;

    public bool ActionGearAvailable(AttackActions action, TurnManager turn)
    {
        if (action != AttackActions.Nothing)
            if (action == turn.SelectedCharacter.Weapon?.Execute()) return true; 
        // assuming there is no same attack for gears we are okay, else: add && for extra condition
        return false;
    }

    public bool ActionAvailable(AttackActions action, TurnManager turn)
    {        
        if (action != AttackActions.Nothing)
        {
            if (action == turn.SelectedCharacter.StandardAttack) return true;
            if (action == turn.SelectedCharacter.AdditionalStandardAttack) return true;
        }
        return false;
    }

    public event Action<TurnManager> GearEquipped;

    public void EquipGear(TurnManager turn)
    {
        EquipGear(turn.CurrentGearInventory[turn.SelectedGear], turn);
        GearEquipped.Invoke(turn);
        RemoveGearFromInventory(turn);
    }

    private void RemoveGearFromInventory(TurnManager turn) => turn.CurrentGearInventory.RemoveAt(turn.SelectedGear);

    private void EquipGear(Gear gear, TurnManager turn)
    {
        if (gear is Armor) turn.SelectedCharacter.Armor = turn.CurrentGearInventory[turn.SelectedGear];
        else turn.SelectedCharacter.Weapon = turn.CurrentGearInventory[turn.SelectedGear];
    }
}