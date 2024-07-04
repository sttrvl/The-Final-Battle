using static TurnManager;

public class PartyManager
{
    public PartyInfo HeroParty = new PartyInfo();
    public PartyInfo MonsterParty = new PartyInfo();
    public class PartyInfo // this is wrong as a record, so now is a class
    {
        public int TurnsPlayed { get; private set; } = 0;
        public Character Player { get; private set; }
        public List<Character> PartyList { get; private set; } = new List<Character>();
        public List<Consumables> ItemInventory { get; private set; } = new List<Consumables>();
        public List<Gear?> GearInventory { get; private set; } = new List<Gear?>();

        public void UpdatePlayer(Character character) => Player = character;
        public void AddCharacter(Character character) => PartyList.Add(character);
        public void AddItem(Consumables item) => ItemInventory.Add(item);
        public void RemoveItem(Consumables item) => ItemInventory.Remove(item);
        public void AddGear(Gear? gear) => GearInventory.Add(gear);
        public void RemoveGear(Gear? gear) => GearInventory.Remove(gear);
        public void AddTurns() => TurnsPlayed++;
    }

    public AdditionalList AdditionalMonsters = new AdditionalList();

    public class AdditionalList // same here
    {
        public List<List<Character>> CharacterLists { get; private set; } = new List<List<Character>>();
        public List<List<Consumables>?> ItemLists { get; private set; } = new List<List<Consumables>?>();
        public List<List<Gear?>> GearLists { get; private set; } = new List<List<Gear?>>();

        public void AddCharacterList(List<Character> characters) => CharacterLists.Add(characters);
        public void AddGearList(List<Gear?> gear) => GearLists.Add(gear);
        public void AddItemList(List<Consumables>? item) => ItemLists.Add(item);
        public void RemoveCharacterListAt(int position) => CharacterLists.RemoveAt(position);
        public void RemoveGearListAt(int position) => GearLists.RemoveAt(position);
        public void RemoveItemListAt(int position) => ItemLists.RemoveAt(position);
    }

    public PartyManager()
    {
        ConsumableItemUsed += ManageCharacterHealth;
    }

    public void ManageCharacterHealth(TurnManager turn) // target character?
    {
        UpdateCharacterHealth(turn);
        RemoveItem(turn);
    }

    private void UpdateCharacterHealth(TurnManager turn) => 
        UpdateHealthSum(turn.Current.Character, turn.Current.HealValue);

    private void RemoveItem(TurnManager turn)
    {
        List<Consumables> ItemInventory = turn.GetCurrentItemInventory(this);
        if (ItemInventory.Count > 0) ItemInventory.RemoveAt(turn.Current.ConsumableNumber);
    }

    public bool CheckForPoisonedCharacter(TurnManager turn) => turn.CurrentPoisonedCharacters.Count > 0;

    public void RemoveInvalidPoisonedCharacter(TurnManager turn)
    {
        for (int index = turn.CurrentPoisonedCharacters.Count - 1; index >= 0; index--)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index];
            if (poisoned.TurnsPoisoned == 0 || poisoned.Character.IsDeath()) turn.CurrentPoisonedCharacters.RemoveAt(index);
        }
    }

    public void RemoveInvalidPlagueSickCharacter(TurnManager turn)
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
        (Character player1, Character player2) = 
            new InputManager().MenuSetter(new InputManager().InputMenuOption(menu, info));
        HeroParty.UpdatePlayer(player1);
        MonsterParty.UpdatePlayer(player2);

        turn.SelectStartingPlayer(HeroParty.Player);
    }

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

    private void IgnoreListFirstLines(List<string> levelStringList, int amount) => 
        levelStringList.RemoveRange(0, amount);

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
            "trueprogrammer"  => new TrueProgrammer(turn.Current.PlayerType), 
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

        if (party.IsPartyEmpty(HeroParty.PartyList))
            CreateHeroParty(gearType, gearAmount, itemType, itemAmount, characterList);
        else if (party.IsPartyEmpty(party.MonsterParty.PartyList))
            CreateMonsterParty(gearType, gearAmount, itemType, itemAmount, characterList);
        else
            ManageAdditionalMonsterLists(gearType, gearAmount, itemType, itemAmount, characterList);
    }

    private void CreateHeroParty(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                    List<Character> characterList)
    {
        foreach (Character character in characterList)
            HeroParty.AddCharacter(character);

        if (IsValid(gearType, gearAmount))
            for (int index = 0; index < gearAmount; index++)
                HeroParty.AddGear(gearType);

        if (IsValid(itemType, itemAmount))
            for (int index = 0; index < itemAmount; index++)
                HeroParty.AddItem(itemType);
    }
    // I think it was more complicated to try to reuse code here, or at least that I can think of
    private void CreateMonsterParty(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                    List<Character> characterList)
    {
        foreach (Character character in characterList)
            MonsterParty.AddCharacter(character);

        if (IsValid(gearType, gearAmount))
            for (int index = 0; index < gearAmount; index++)
                MonsterParty.AddGear(gearType);

        if (IsValid(itemType, itemAmount))
            for (int index = 0; index < itemAmount; index++)
                MonsterParty.AddItem(itemType);
    }

    public event Action AdditionalMonsterRound;
    private void ManageAdditionalMonsterLists(Gear? gearType, int gearAmount, Consumables itemType, int itemAmount,
                                              List<Character> characterList)
    {
        if (IsValid(gearType, gearAmount))
            AdditionalMonsters.AddGearList(CreateGearList(gearType, gearAmount));
        else
            AdditionalMonsters.AddGearList(new List<Gear?>());

        if (IsValid(itemType, itemAmount))
            AdditionalMonsters.AddItemList(CreateItemList(itemType, itemAmount));
        else
            AdditionalMonsters.AddItemList(new List<Consumables>());

        AdditionalMonsters.AddCharacterList(characterList);
        AdditionalMonsterRound?.Invoke();
    }

    private bool IsValid(InventoryItem? inventoryItem, int amount) => inventoryItem != null && amount > 0;

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

    public bool CheckForEmptyParties() => IsPartyEmpty(HeroParty.PartyList) ||
                                          IsPartyEmpty(MonsterParty.PartyList);

    public bool IsPartyEmpty(List<Character> party) => party.Count == 0;


    public event Action<PartyManager, TurnManager> AttackInfo;
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
        if (turn.Current.Attack is AreaAttack)
            foreach (Character character in turn.CurrentOpponentParty(party))
                UpdateHealthReduce(character, turn.Current.Damage);
        else
            UpdateHealthReduce(turn.CurrentOpponentParty(party)[turn.Current.Target], turn.Current.Damage);
    }

    private void UpdateHealthReduce(Character character, int damage)
    {
        character.CurrentHP -= damage;
        character.CurrentHP = character.HealthClamp();
    }

    private void UpdateHealthSum(Character character, int damage) // maybe if the value + sum if the value - reduce
    {
        character.CurrentHP += damage;
        character.CurrentHP = character.HealthClamp();
    }

    public event Action<PartyManager, TurnManager> AttackSuccesful;
    public event Action<TurnManager> AttackMissed;
    private bool AttackManager(PartyManager party, TurnManager turn)
    {
        if (ManageProbability(turn.Current.Probability))
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
        if (turn.Current.Character.SoulsValue >= 3)
        {
            turn.Current.IncreaseDamage(1);
            turn.Current.Character.SoulsValue = 0;
            SoulBonus?.Invoke(turn);
        }
    }

    private void CheckModifier(PartyManager party, TurnManager turn)
    {
        if (turn.TargetHasDefensiveModifier(party)) ManageDefensiveModifier(party, turn);
        if (turn.TargetHasOffensiveModifier()) ManageOffensiveModifier(party, turn); 
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

    public event Action<PartyManager, TurnManager> DefensiveModifierApplied;
    public void ManageDefensiveModifier(PartyManager party, TurnManager turn)
    {
        switch (turn.CurrentOpponentParty(party)[turn.Current.Target].DefensiveAttackModifier)
        {
            case StoneArmor:
                turn.Current.SetTargetDefensiveModifier(new StoneArmor());
                turn.Current.IncreaseDamage(turn.Current.TargetDefensiveModifier.Value);
                DefensiveModifierApplied.Invoke(party, turn);
                break;
            case ObjectSight when turn.Current.Attack.AttackType == AttackTypes.Decoding:
                turn.Current.SetTargetDefensiveModifier(new ObjectSight());
                turn.Current.IncreaseDamage(turn.Current.TargetDefensiveModifier.Value);
                turn.ClampCurrentDamage();
                DefensiveModifierApplied.Invoke(party, turn);
                break;
            default:

                break;
        }
    }

    public event Action<PartyManager, TurnManager> OffensiveModifierApplied;
    public void ManageOffensiveModifier(PartyManager party, TurnManager turn)
    {
        List<OffensiveAttackModifier?> modifier = new List<OffensiveAttackModifier?>();
        if (turn.Current.Character?.Weapon?.OffensiveAttackModifier != null)
            modifier.Add(turn.Current.Character.Weapon.OffensiveAttackModifier);
        if (turn.Current.Character?.Armor?.OffensiveAttackModifier != null)
            modifier.Add(turn.Current.Character.Armor.OffensiveAttackModifier);

        foreach (OffensiveAttackModifier? offensive in modifier)
        {
            switch (offensive)
            {
                case Binary:
                    turn.Current.SetOffensiveModifier(new Binary());
                    turn.Current.IncreaseDamage(turn.Current.OffensiveModifier.Value);
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
        switch (turn.Current.Attack.AttackSideEffect)
        {
            case AttackSideEffects.Steal:
                Gear? opponentWeapon = turn.CurrentOpponentParty(party)[turn.Current.Target].Weapon;
                Gear? opponentArmor  = turn.CurrentOpponentParty(party)[turn.Current.Target].Armor;
                if (opponentArmor != null || opponentWeapon != null)
                    if (ManageProbability(new Random().Next(0, 100)))
                    {
                        int choice = new Random().Next(2) == 0 ? 0 : 1;
                        Gear? gearToBeStolen = choice == 0 && opponentArmor != null ? opponentArmor : opponentWeapon;
                        StealGear(turn, gearToBeStolen);
                        RemoveGearFromTarget(turn, party);
                        gearStolen?.Invoke(turn);
                    }
                break;
        }
    }

    public event Action<TurnManager, PartyManager> CharacterPoisoned;
    public event Action<TurnManager, PartyManager> CharacterPlagueSick;
    public void ApplyTemporaryEffect(TurnManager turn, PartyManager party)
    {
        List<Character> opponentParty = turn.CurrentOpponentParty(party);
        int target = turn.Current.Target;
        switch (turn.Current.Attack.AttackTemporaryEffect)
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

    private void StealGear(TurnManager turn, Gear? gearToBeStolen)
    {
        if (turn.CurrentParty(this) == HeroParty.PartyList)
            HeroParty.AddGear(gearToBeStolen);
        else
            MonsterParty.AddGear(gearToBeStolen);
    }

    private void RemoveGearFromTarget(TurnManager turn, PartyManager party) =>
        turn.CurrentOpponentParty(party)[turn.Current.Target].Weapon = null;

    public event Action<TurnManager> ConsumableItemUsed;
    public void UseConsumableItem(TurnManager turn) => ConsumableItemUsed.Invoke(turn);

    public event Action<Character> CharacterDied;
    public void DeathManager(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentOpponentParty(this).Count; index++)
        {
            if (CheckDeath(turn.CurrentOpponentParty(this)[index]))
            {
                CharacterDied.Invoke(turn.CurrentOpponentParty(this)[index]);
                ManageDeath(turn);
            }
        }
    }

    public bool CheckDeath(Character character)
    {
        return character.IsDeath();
    }

    public void ManagePartyDefeated(TurnManager turn)
    {
        if (CheckPartyDefeat(this, MonsterParty.PartyList)) ManageMonsterDefeated(this, turn);
        if (CheckPartyDefeat(this, HeroParty.PartyList)) ManageHeroesDefeated(this, turn);
    }

    public void ManageMonsterDefeated(PartyManager party, TurnManager turn)
    {
        TransferDeathMonsterPartyGear(party, turn);
        TransferDeathMonsterPartyItems(turn);
        NextMonsterParty(turn);
    }

    public event Action<PartyManager, TurnManager> PartyDefeated;
    public void ManageHeroesDefeated(PartyManager party, TurnManager turn)
    {
        if (party.IsPartyEmpty(HeroParty.PartyList)) PartyDefeated.Invoke(party, turn);
    }

    public void NextMonsterParty(TurnManager turn)
    {
        if (HasAdditionalRounds(turn) && HasAdditionalMonsters())
        {
            AddAdditionalMonsterList();
            AddAdditionalItemList();
            AddAdditionalGearList();
        }
        PartyDefeated?.Invoke(this, turn);
        turn.AdditionalBattleRoundUsed();
    }

    public event Action<TurnManager, PartyManager> GearObtained;
    private void TransferDeathMonsterPartyGear(PartyManager party, TurnManager turn)
    {
        for (int index = 0; index < MonsterParty.GearInventory.Count; index++)
        {
            HeroParty.AddGear(MonsterParty.GearInventory[index]);
            MonsterParty.RemoveGear(MonsterParty.GearInventory[index]);
        }

        GearObtained?.Invoke(turn, party);
    }

    public event Action<TurnManager, PartyManager> ItemsObtained;
    private void TransferDeathMonsterPartyItems(TurnManager turn)
    {
        string message = "";
        for (int index = 0; index < MonsterParty.ItemInventory.Count; index++)
        {
            HeroParty.AddItem(MonsterParty.ItemInventory[index]);
            MonsterParty.RemoveItem(MonsterParty.ItemInventory[index]);
        }
            
        ItemsObtained?.Invoke(turn, this);
    }

    private bool HasAdditionalRounds(TurnManager turn) => turn.Round.Battles > 0;
    private bool HasAdditionalMonsters() => AdditionalMonsters.CharacterLists.Count > 0; // can be reused

    private void AddAdditionalItemList()
    {
        if (AdditionalMonsters.ItemLists.Count > 0)
            foreach (Consumables items in AdditionalMonsters.ItemLists[0])
                MonsterParty.AddItem(items);

        AdditionalMonsters.RemoveItemListAt(0);
    }
    private void AddAdditionalGearList()
    {
        if (AdditionalMonsters.GearLists.Count > 0)
            foreach (Gear? gear in AdditionalMonsters.GearLists[0])
                MonsterParty.AddGear(gear);

        AdditionalMonsters.RemoveGearListAt(0);
    }

    private void AddAdditionalMonsterList()
    {
        foreach (Character character in AdditionalMonsters.CharacterLists[0])
            MonsterParty.AddCharacter(character);

        AdditionalMonsters.RemoveCharacterListAt(0);
    }

    public void ManageDeath(TurnManager turn)
    {
        ManageDeathCharacterGear(turn);
        ManageDeathCharacterSoul(turn);
        RemoveCharacter(turn);
    }

    private void RemoveCharacter(TurnManager turn)
    {
        turn.CurrentOpponentParty(this).Remove(turn.CurrentOpponentParty(this)[turn.Current.Target]);
    }

    public void ManageDeathCharacterGear(TurnManager turn)
    {
        if (turn.CurrentTargetHasGear(turn.CurrentOpponentParty(this))) AddDeathGearToOpponentInventory(turn);
    }

    public event Action<TurnManager, PartyManager> SoulObtained;
    public void ManageDeathCharacterSoul(TurnManager turn)
    {
        if (turn.Current.Character is Hero && turn.CurrentOpponentParty(this)[turn.Current.Target].SoulsXP > 0)
        {
            UpdateSoulValue(turn);
            SoulObtained?.Invoke(turn, this);
        }
    }

    public void UpdateSoulValue(TurnManager turn)
    {
        if (turn.CurrentOpponentParty(this)[turn.Current.Target].SoulsXP >= 1)
            turn.Current.Character.SoulsValue += turn.CurrentOpponentParty(this)[turn.Current.Target].SoulsXP;
    }

    public event Action<PartyManager, TurnManager> DeathOpponentGearObtained;
    public void AddDeathGearToOpponentInventory(TurnManager turn)
    {
        turn.Current.GearInventory.Add(turn.CurrentOpponentParty(this)[turn.Current.Target].Weapon!);
        DeathOpponentGearObtained.Invoke(this, turn);
    }

    public bool CheckPartyDefeat(PartyManager party, List<Character> partyList) => party.IsPartyEmpty(partyList);

    public bool OptionAvailable(int? choice, TurnManager turn) => !OptionNotAvailable(choice, turn);

    public bool OptionNotAvailable(int? choice, TurnManager turn) => 
        choice == 3 && turn.Current.GearInventory.Count == 0;

    public bool ActionGearAvailable(AttackActions action, TurnManager turn)
    {
        if (action != AttackActions.Nothing)
            if (action == turn.Current.Character.Weapon?.Execute()) return true; 
        // assuming there is no same attack for gears we are okay, else: add && for extra condition
        return false;
    }

    public bool ActionAvailable(AttackActions action, TurnManager turn)
    {        
        if (action != AttackActions.Nothing)
        {
            if (action == turn.Current.Character.StandardAttack) return true;
            if (action == turn.Current.Character.AdditionalStandardAttack) return true;
        }
        return false;
    }

    public event Action<TurnManager> GearEquipped;

    public void ManageEquipGear(TurnManager turn)
    {
        EquipGear(turn.Current.GearInventory[turn.Current.GearChoice], turn);
        GearEquipped.Invoke(turn);
        RemoveGearFromInventory(turn);
    }

    private void RemoveGearFromInventory(TurnManager turn) => turn.Current.GearInventory.RemoveAt(turn.Current.GearChoice);

    private void EquipGear(Gear gear, TurnManager turn)
    {
        if (gear is Armor) turn.Current.Character.Armor = turn.Current.GearInventory[turn.Current.GearChoice];
        else turn.Current.Character.Weapon = turn.Current.GearInventory[turn.Current.GearChoice];
    }
}