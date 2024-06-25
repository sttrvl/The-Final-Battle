using System.Reflection.Metadata.Ecma335;

public abstract class Character
{
    public Guid ID { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Character";
    public int MaxHP { get; set; } = 0;
    public int CurrentHP { get; set; }
    public int SoulsXP { get; set; }
    public int SoulsValue { get; set; } = 0;
    public override string ToString() => Name;
    public int HealthClamp() => Math.Clamp(CurrentHP, 0, MaxHP);
    public bool IsAlive() => CurrentHP > 0;
    public bool IsDeath() => CurrentHP == 0;

    public AttackActions StandardAttack { get; set; } = AttackActions.Nothing;
    public AttackActions AdditionalStandardAttack = AttackActions.Nothing;

    public DefensiveAttackModifier? DefensiveAttackModifier;
    public OffensiveAttackModifiers? OffensiveAttackModifier;

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

    public int ExecuteAction(PartyManager party, TurnManager turn)
    {
        InputManager input = new InputManager();
        int targetsCount = 0;
        foreach (Character c in turn.CurrentOpponentParty(party))
            targetsCount++;

        turn.CurrentTarget = new Random().Next(0, targetsCount);

        int optionsCount = 1;
        List<int> normalAttacks = new List<int>();
        List<int> gearAttacks = new List<int>();

        List<AttackActions> availableActions = input.ActionAvailableCheck(party, turn);
        foreach (AttackActions action in Enum.GetValues(typeof(AttackActions)))
        {
            if (action == AttackActions.Nothing)
                continue;

            if (party.ActionAvailable(action, turn))
            {
                normalAttacks.Add(optionsCount);
                optionsCount++;
            }
                
            if (party.ActionGearAvailable(action, turn))
            {
                gearAttacks.Add(optionsCount);
                optionsCount++;
            }

            if (optionsCount > 2)
                break; // restricted it to 2 so It's only 1 or 2
        }

        int randomNumber = new Random().Next(100);

        if (randomNumber < 80 && gearAttacks.Count > 0)
        {
            int gearIndex = new Random().Next(gearAttacks.Count);
            Console.WriteLine("Count" + gearAttacks[gearIndex]);
            return gearAttacks[gearIndex];
        }
        else if (randomNumber < 70) // I put a high chance for computer to choose this, which excludes nothing
        {
            int attackIndex = new Random().Next(normalAttacks.Count);
            Console.WriteLine("Count" + normalAttacks[attackIndex]);
            return normalAttacks[attackIndex];
        }
        else
        {
            Console.WriteLine("Count" + normalAttacks[0]);
            return normalAttacks[0];
        }
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

    public int MenuOption(List<Consumables> itemList, TurnManager turn)
    {
        int randomNumber = new Random().Next(100);

        if (itemList.Any(x => x is HealthPotion) && turn.SelectedCharacter.CurrentHP < turn.SelectedCharacter.MaxHP / 2)
            if (randomNumber < 25)
                return 2; // use item

        if (turn.SelectedCharacter.Weapon == null && turn.CurrentGearInventory.Count >= 1)
            if (randomNumber < 50)
                return 3; // equip gear

        if (randomNumber < 80) // attack
            return 1;
        else
            return 0;
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

    public TrueProgrammer()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Weapon = new Sword();
        InputManager manageInput = new InputManager();
        Name = manageInput.AskUser("What's your character's name?");
        DefensiveAttackModifier = new ObjectSight();
        StandardAttack = AttackActions.Punch;
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

    public MylaraAndSkorin()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Mylara and Skorin";
        StandardAttack = AttackActions.Punch;
    }
}

public abstract class Monsters : Character
{

}

public class Skeleton : Monsters
{
    public int DefaultMaxHP { get; } = 5;

    public Skeleton()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "Skeleton";
        StandardAttack = AttackActions.BoneCrunch;
        SoulsXP = 1;
    }
}

public class UncodedOne : Monsters
{
    public int DefaultMaxHP { get; } = 15;

    public UncodedOne()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;
        Name = "UncodedOne";
        StandardAttack = AttackActions.Unraveling;
        SoulsXP = 999;
    }
}

public class StoneAmarok : Monsters
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

public class ShadowOctopoid : Monsters
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