using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

public abstract class Character
{
    public Guid ID { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Character";
    public string? TauntText { get; set; }
    public int MaxHP { get; set; } = 0;
    public int CurrentHP { get; set; }
    public int SoulsXP { get; set; }
    public int SoulsValue { get; set; } = 0;
    public int? ForcedChoice { get; set; }
    public override string ToString() => Name;
    public int HealthClamp() => Math.Clamp(CurrentHP, 0, MaxHP);
    public bool IsAlive() => CurrentHP > 0;
    public bool IsDeath() => CurrentHP == 0;

    public AttackActions StandardAttack { get; set; } = AttackActions.Nothing;
    public AttackActions AdditionalStandardAttack = AttackActions.Nothing;

    public DefensiveAttackModifier? DefensiveAttackModifier;

    public Gear? Weapon;
    public Gear? Armor;
}

public class HumanPlayer : Character
{
    public override string ToString() => Name;
}

public class Computer : Character
{
    private string _defaultName = "Computer";

    public Computer()
    {
        Name = _defaultName;
    }

    public void ExecuteAction(PartyManager party, TurnManager turn)
    {
        InputManager input = new InputManager();
        int targetsCount = 0;
        foreach (Character c in turn.CurrentOpponentParty(party))
            targetsCount++;

        turn.CurrentTarget = new Random().Next(0, targetsCount);

        List<int> normalAttacks = new List<int>();
        List<int> gearAttacks = new List<int>();
        int actionNumber;

        List<AttackActions> availableActions = input.ActionAvailableCheck(party, turn);

        for (int index = 0; index < availableActions.Count; index++)
        {
            if (party.ActionAvailable(availableActions[index], turn)) normalAttacks.Add(index);    
            if (party.ActionGearAvailable(availableActions[index], turn)) gearAttacks.Add(index);
        }

        int randomNumber = new Random().Next(100);

        if (randomNumber < 80 && gearAttacks.Count > 0)
        {
            int gearIndex = new Random().Next(gearAttacks.Count);
            actionNumber = gearAttacks[gearIndex];
        }
        else if (randomNumber < 70) // I put a high chance for computer to choose this, which excludes nothing
        {
            int attackIndex = new Random().Next(normalAttacks.Count);
            actionNumber = normalAttacks[attackIndex];
        }
        else
            actionNumber = normalAttacks[0];

        input.InputAction(party, turn, availableActions[actionNumber]);
    }

    public void SelectItem(List<Consumables> itemList, TurnManager turn)
    {
        int optionsCount = 0;
        foreach (Consumables item in itemList)
            optionsCount++;

        Random random = new Random();

        turn.ConsumableSelectedNumber = random.Next(0, optionsCount);
        turn.ConsumableSelected = itemList[turn.ConsumableSelectedNumber];
    }

    public int MenuOption(PartyManager party, TurnManager turn, DisplayInformation info)
    {
        InputManager input = new InputManager();
        int randomNumber = new Random().Next(100);
        int computerChoice = 0;

        Character character = turn.SelectedCharacter;
        List<Consumables> itemInventory = turn.GetCurrentItemInventory(party);
        if (itemInventory.Any(x => x is Consumables) && character?.CurrentHP < character?.MaxHP / 4 && randomNumber < 90)
        { // Testing: / 4
            computerChoice = 2; // use item
        }
        else if (character?.Weapon == null && turn.CurrentGearInventory.Count >= 1 && randomNumber < 50)
        {
            computerChoice = 3; // equip gear  
        }
        else if (randomNumber < 90)
            computerChoice = 1; // attack

        info.DisplayCorrectMenu(computerChoice, party, turn, info);
        return computerChoice;
    }

    public void SelectGear(TurnManager turn)
    {
        int count = turn.CurrentGearInventory.Count;
        int randomNumber = new Random().Next(0, count);
        turn.SelectedGear = randomNumber;
    }
}

public abstract class Hero : Character
{

}

public class TrueProgrammer : Hero // if this is a computer, default name should be "Computer"
{
    public int DefaultMaxHP { get; } = 25;

    public TrueProgrammer(Character characterType)
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Weapon = new Sword();
        Name = DefineName(characterType);
        DefensiveAttackModifier = new ObjectSight();
        StandardAttack = AttackActions.Punch;
    }

    public string DefineName(Character characterType)
    {
        InputManager manageInput = new InputManager();
        if (characterType is Computer)
            return new Computer().Name;
        else
            return manageInput.AskUser("What's your character's name?");
    }
}

public class VinFletcher : Hero
{
    AttackActions _standard = AttackActions.Punch;

    int DefaultMaxHP { get; } = 15;

    public VinFletcher()
    {

        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Weapon = new VinsBow();
        Name = "Vin Fletcher";
        StandardAttack = AttackActions.Punch;
    }
}

public class MylaraAndSkorin : Hero // consider separating these
{
    int DefaultMaxHP { get; } = 10;

    public MylaraAndSkorin(TurnManager turn)
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Mylara And Skorin";
        StandardAttack = AttackActions.Punch;
        Weapon = new CannonOfConsolas(turn);
    }
}

public abstract class Monster : Character
{

}

public class Skeleton : Monster
{
    public int DefaultMaxHP { get; } = 5;

    public Skeleton()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Skeleton";
        StandardAttack = AttackActions.BoneCrunch;
        SoulsXP = 1;
        TauntText = "“We will repel your spineless assault!”";
    }
}

public class UncodedOne : Monster
{
    public int DefaultMaxHP { get; } = 15;

    public UncodedOne()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "UncodedOne";
        StandardAttack = AttackActions.Unraveling;
        SoulsXP = 999;
        TauntText = "<<THE UNRAVELLING OF ALL THINGS IS INEVITABLE>>";
    }
}

public class Amarok : Monster
{
    int DefaultMaxHP { get; } = 8;

    public Amarok()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        
        Name = "Amarok";
        StandardAttack = AttackActions.Bite;
        AdditionalStandardAttack = AttackActions.Scratch;
        SoulsXP = 1;
        TauntText = ">>You smell a familiar rotten stench<<";
    }
}

public class StoneAmarok : Monster
{
    int DefaultMaxHP { get; } = 4;

    public StoneAmarok()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        DefensiveAttackModifier = new StoneArmor();
        Name = "Stone Amarok";
        StandardAttack = AttackActions.Bite;
        SoulsXP = 0;
    }
}

public class ShadowOctopoid : Monster
{
    int DefaultMaxHP { get; } = 15;
    public ShadowOctopoid()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Shadow Octopoid";
        StandardAttack = AttackActions.Grapple;
        AdditionalStandardAttack = AttackActions.Whip;
        SoulsXP = 2;
    }
}

public class EvilRobot : Monster
{
    int DefaultMaxHP { get; } = 15;
    public EvilRobot()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Evil Robot";
        StandardAttack = AttackActions.SmartRockets;
        SoulsXP = 2;
    }
}