using static TurnManager;

public class PartyManager
{

    // PartyPlayer and List can be a struct, also with both of their inventories

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

    public List<List<Gear>> AdditionalGearLists { get; set; } = new List<List<Gear>>();

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

    public bool CheckForPlagueSickCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentSickPlagueCharacters.Count; index++)
        {
            SickPlaguedCharacterInfo sick = turn.CurrentSickPlagueCharacters[index];
            if (sick.TurnsSick == 0)
                turn.CurrentSickPlagueCharacters.Remove(sick);
        }
        return turn.CurrentSickPlagueCharacters.Count > 0;
    }

    public void PoisonCharacter(TurnManager turn)
    {
        for (int index = 0; index < turn.CurrentPoisonedCharacters.Count; index++)
        {
            PoisonedCharacterInfo poisoned = turn.CurrentPoisonedCharacters[index]; 
            poisoned.Character.CurrentHP -= 1; // we could also pass the poison damage
            poisoned.Character.CurrentHP = poisoned.Character.HealthClamp();
            poisoned.TurnsPoisoned -= 1;
            turn.CurrentPoisonedCharacters[index] = poisoned;
            PoisonDamage.Invoke(poisoned.Character);
        }   
    }

    public event Action<Character> PoisonDamage;

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

    public event Action<Character> PlagueSickDamage;

    public void UpdateCharacterHealth(TurnManager turn)
    {
        turn.SelectedCharacter.CurrentHP += turn.CurrentHealValue;
        turn.SelectedCharacter.CurrentHP = Math.Clamp(turn.SelectedCharacter.CurrentHP, 0, turn.SelectedCharacter.MaxHP);

        if (turn.GetCurrentItemInventory(this).Count > 0) turn.GetCurrentItemInventory(this).RemoveAt(turn.ConsumableSelectedNumber);
    }

    public void SetUpParties(List<MenuOption> menu, DisplayInformation info, TurnManager turn, PartyManager party)
    {
        PartySetUpSettings(menu, info, turn, party);
        Console.Clear();
    }

    public record Level(Consumables? ExtraItemType, int itemAmount, Gear? ExtraGearType, int gearAmount,
                        Gear? equippedGearChoice,          params Character[] characters);

    public void PartySetUpSettings(List<MenuOption> menu, DisplayInformation info, TurnManager turn, PartyManager party)
    {
        InputManager input = new InputManager();
        (HeroPlayer, MonsterPlayer) = input.MenuSetter(input.InputMenuOption(menu, info));
        turn.SelectedPlayerType = HeroPlayer;

        //CreateHeroParty(HeroPlayer, new TrueProgrammer(HeroPlayer), new VinFletcher(), new MylaraAndSkorin(turn));
        //CreateMonsterParty(MonsterPlayer, new Skeleton(), new StoneAmarok(), new EvilRobot());

        List<Level> levels = LoadLevelsFromFile("Levels.txt", turn);
        Dictionary<Character, Gear> noGear = new Dictionary<Character, Gear>();

        foreach (Level level in levels)
        {
            List<Dictionary<Character, Gear?>> currentGearChoices = new List<Dictionary<Character, Gear?>>();
            if (level.equippedGearChoice != null)
                currentGearChoices.Add(SetUpCharacterWithGear(level.equippedGearChoice, level.characters));
            else
                currentGearChoices.Add(SetUpCharacterWithGear(null, level.characters));

            // ok so HeroGearInventoryGets added

            for (int index = 0; index < currentGearChoices.Count; index++)
            {
                AddRound(currentGearChoices[0], RetriveCharactersWithGear(currentGearChoices[0]), party, 
                         level.ExtraItemType, level.itemAmount, level.ExtraGearType, level.gearAmount);
            }
        }
    }

    public List<Level> LoadLevelsFromFile(string filePath, TurnManager turn)
    {
        List<Level> levels = new List<Level>();

        string[] levelStrings = File.ReadAllLines(filePath);
        List<string> levelStringList =levelStrings.ToList();
        levelStringList.RemoveAt(0);
        levelStringList.RemoveAt(0);
        foreach (string levelString in levelStringList)
        {
            List<Character> characters = new List<Character>();
            string[] tokens = levelString.Split(',');

            for (int index = 5; index < tokens.Length; index++)
                characters.Add(GetCharacter(tokens[index].Trim(), turn));

            int extraItemAmount = Convert.ToInt32(tokens[1].Trim());
            int extraGearAmount = Convert.ToInt32(tokens[3].Trim());

            levels.Add(new Level(GetItem(tokens[0].Trim(), turn), extraItemAmount, GetGear(tokens[2].Trim(), turn),
                       extraGearAmount, GetGear(tokens[4].Trim(), turn), characters.ToArray()));
        }
        return levels;
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
            "default"          => default, // DEBUG
            _                  => null
        };
    }

    public Character GetCharacter(string character, TurnManager turn)
    {
        return character.ToLower() switch
        {   // It's a problem if we use it before It's been set, since even if It's a Computer it will ask for a name.
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

    public (List<Character>, Character) CreatePlayer(List<Character> newParty, Character partyType) => (newParty, partyType);

    public void AddRound(Dictionary<Character, Gear?> gearChoices, List<Character> characterList, PartyManager party, 
                         Consumables itemType, int itemAmount, Gear? gearType, int gearAmount)
    {
        AddGearChoices(gearChoices, characterList);

        if (party.IsPartyEmpty(party.HeroPartyList))
        {
            foreach (Character character in characterList)
                HeroPartyList.Add(character);

            if (gearType != null && gearAmount > 0)
                for (int index = 0; index < gearAmount; index++)
                    party.HeroGearInventory.Add(gearType);

            if (itemType != null && itemAmount > 0)
                for (int index = 0; index < itemAmount; index++)
                    party.HeroItemInventory.Add(itemType);
        }
        else if (party.IsPartyEmpty(party.MonsterPartyList))
        {
            foreach (Character character in characterList)
                MonsterPartyList.Add(character);

            if (gearType != null && gearAmount > 0)
                for (int index = 0; index < gearAmount; index++)
                    party.MonsterGearInventory.Add(gearType);

            if (itemType != null && itemAmount > 0)
                for (int index = 0; index < itemAmount; index++)
                    party.MonsterItemInventory.Add(itemType);
        }
        else
        {
            List<Gear> gearList = new List<Gear>();
            for (int index = 0; index < gearAmount; index++)
                gearList.Add(gearType);

            if (gearType != null && gearAmount > 0)
                party.AdditionalGearLists.Add(gearList);
            else
                AdditionalGearLists.Add(new List<Gear>());

            List<Consumables> itemList = new List<Consumables>();
            for (int index = 0; index < itemAmount; index++)
                itemList.Add(itemType);

            if (itemType != null && itemAmount > 0)
                party.AdditionalItemLists.Add(itemList);
            else
                AdditionalItemLists.Add(new List<Consumables>());


            AdditionalMonsterLists.Add(characterList);
            AdditionalMonsterRound?.Invoke();
        }
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
        if (turn.CurrentAttack is AreaAttack)
            foreach (Character character in turn.CurrentOpponentParty(party))
            {
                character.CurrentHP -= turn.CurrentDamage;
                character.HealthClamp();
            }

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
        switch (turn.CurrentAttack.AttackTemporaryEffect)
        {
            case AttackTemporaryEffects.Poison:
                Character poisonedTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
                turn.CurrentPoisonedCharacters.Add(new PoisonedCharacterInfo(poisonedTarget, turn.CurrentOpponentParty(party), 3));
                CharacterPoisoned?.Invoke(turn, party);
                break;
            case AttackTemporaryEffects.RotPlague:
                Character sickPlagueTarget = turn.CurrentOpponentParty(party)[turn.CurrentTarget];
                turn.CurrentSickPlagueCharacters.Add(new SickPlaguedCharacterInfo(sickPlagueTarget, turn.CurrentOpponentParty(party), 1));
                CharacterPlagueSick?.Invoke(turn, party);
                break;
        }
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

    public void DeathManager(PartyManager party, DisplayInformation info, TurnManager turn)
    {
        if (CheckDeath(turn, party))
        {
            info.DisplayCharacterDeath(turn.CurrentOpponentParty(party), turn.CurrentTarget);
            ManageDeath(party, turn);
        }
    }

    public bool CheckDeath(TurnManager turn, PartyManager party)
    {
        if (turn.CurrentTarget >= 0 && turn.CurrentTarget < turn.CurrentOpponentParty(party).Count)
            return turn.CurrentOpponentParty(party)[turn.CurrentTarget].IsDeath();

        return false;
    }

    public void ManagePartyDefeated(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        if (CheckPartyDefeat(party, party.MonsterPartyList))
            ManageMonsterDefeated(party, turn);
        if (CheckPartyDefeat(party, party.HeroPartyList))
            ManageHeroesDefeated(party, turn, info);
    }

    public void ManageMonsterDefeated(PartyManager party, TurnManager turn)
    {
        TransferDeathMonsterPartyGear(party, turn);
        TransferDeathMonsterPartyItems(party, turn);
        NextMonsterParty(turn, party);
    }

    public void ManageHeroesDefeated(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        if (party.IsPartyEmpty(party.HeroPartyList))
            PartyDefeated.Invoke(party, turn);
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
        if (turn.NumberBattleRounds > 0)
        {
            if (AdditionalMonsterLists.Count > 0)
            {
                foreach (Character character in AdditionalMonsterLists[0])
                    MonsterPartyList.Add(character);

                AdditionalMonsterLists.RemoveAt(0); // removes "used" monster list

                if (AdditionalGearLists.Count > 0) // count is 4
                {
                    foreach (Gear gear in AdditionalGearLists[0]) // good, all count 0
                        MonsterGearInventory.Add(gear);
                }
                AdditionalGearLists.RemoveAt(0);

                if (AdditionalItemLists.Count > 0) // 6 when there should be 4
                {
                    foreach (Consumables items in AdditionalItemLists[0]) 
                        MonsterItemInventory.Add(items);
                }
                AdditionalItemLists.RemoveAt(0);
            }
        }
        PartyDefeated?.Invoke(party, turn);
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

    public bool CheckPartyDefeat(PartyManager party, List<Character> partyList) => party.IsPartyEmpty(partyList);

    public bool OptionAvailable(int? choice, TurnManager turn) => !OptionNotAvailable(choice, turn);

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