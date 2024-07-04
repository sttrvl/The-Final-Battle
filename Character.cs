using TheFinalBattle.InputManagement;
using TheFinalBattle.TurnSystem;
using TheFinalBattle.InformationDisplay;
using TheFinalBattle.GameObjects.Items;
using TheFinalBattle.GameObjects.Gear;
using TheFinalBattle.GameObjects.AttackModifiers;
using TheFinalBattle.PartyManagement;

namespace TheFinalBattle.Characters;

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

    public void ExecuteAction(TurnManager turn, PartyManager party)
    {
        int targetsCount = 0;
        foreach (Character c in turn.CurrentOpponentParty(party))
            targetsCount++;

        turn.Current.SetTarget(new Random().Next(0, targetsCount));

        List<int> normalAttacks = new List<int>();
        List<int> gearAttacks = new List<int>();
        int actionNumber;

        List<AttackActions> availableActions = new InputManager().ActionAvailableCheck(turn, party);

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
        else if (randomNumber < 70)
        {
            int attackIndex = new Random().Next(normalAttacks.Count);
            actionNumber = normalAttacks[attackIndex];
        }
        else
            actionNumber = normalAttacks[0];

        new InputManager().InputAction(turn, party, availableActions[actionNumber]);
    }

    public void ComputerSelectItem(List<Consumables> itemList, TurnManager turn)
    {
        int optionsCount = 0;
        foreach (Consumables item in itemList)
            optionsCount++;

        turn.Current.SetConsumableNumber(new Random().Next(0, optionsCount));
        turn.Current.SetConsumable(itemList[turn.Current.ConsumableNumber]);
    }

    public int ComputerMenuOption(TurnManager turn, PartyManager party, DisplayInformation info)
    {
        int randomNumber = new Random().Next(100);
        int computerChoice = 0;

        Character character = turn.Current.Character;
        List<Consumables> itemInventory = turn.GetCurrentItemInventory(party);
        
        if (itemInventory.Any(x => x is Consumables) && character?.CurrentHP < character?.MaxHP / 4 && randomNumber < 90)
        { // 1/4 health
            computerChoice = 2; // use item
        }
        else if (character?.Weapon == null && turn.Current.GearInventory.Count >= 1 && randomNumber < 50)
        {
            computerChoice = 3; // equip gear  
        }
        else if (randomNumber < 90)
            computerChoice = 1; // attack

        turn.CurrentMenu(party, info, computerChoice);
        return computerChoice;
    }

    public void ComputerSelectGear(TurnManager turn)
    {
        int count = turn.Current.GearInventory.Count;
        int randomNumber = new Random().Next(0, count);
        turn.Current.SetGear(randomNumber);
    }

    public override string ToString() => Name;
}

public abstract class Hero : Character
{
    public override string ToString() => Name;
}

public class TrueProgrammer : Hero
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
        if (characterType is Computer)
            return new Computer().Name;
        else
        {
            Console.Clear();
            new InputManager().InputPosition();
            return new InputManager().AskUser("What's your character's name?");
        }  
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

public class MylaraAndSkorin : Hero
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
    public override string ToString() => Name;
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