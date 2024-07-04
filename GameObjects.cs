public interface IAction
{
    public void Execute();
}

public interface IInventoryAction : IAction
{
    string Name { get; internal set; }
}

public abstract class InventoryItem : IInventoryAction
{
    public string Name { get; set; } = "Inventory Object";
    public abstract void Use();
    void IAction.Execute() => Use();
}

public abstract class Consumables : InventoryItem
{
    public int? Heal { get; internal set; } = null;
    public abstract ConsumableItem Execute();
}

public class HealthPotion : Consumables
{
    public HealthPotion()
    {
        Name = "Health Potion";
        Heal = 10;
    }

    public override void Use() => Execute();
    public override ConsumableItem Execute() => ConsumableItem.HealthPotion;
}

public class SimulasSoup : Consumables
{
    public SimulasSoup(TurnManager turn)
    {
        Name = "Simulas Soup";
        Heal = 999; // this was the easy way ok
    }

    public override void Use() => Execute();
    public override ConsumableItem Execute() => ConsumableItem.SimulasSoup;
}

public abstract class AttackAction : InventoryItem
{
    public AttackTypes AttackType { get; internal set; } = AttackTypes.Normal;
    public abstract AttackActions Execute();
    public int AttackDamage { get; internal set; } = 0;
    public double AttackProbability { get; internal set; } = 0;
    public AttackSideEffects? AttackSideEffect { get; internal set; } = null;
    public AttackTemporaryEffects? AttackTemporaryEffect { get; internal set; } = null;
}

public class Nothing : AttackAction
{
    public Nothing()
    {
        AttackDamage = 0;
        AttackProbability = 1;
        Name = "Nothing";
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Nothing;
}

public class Punch : AttackAction
{
    public Punch()
    {
        AttackDamage = 1;
        AttackProbability = 1;
        Name = "Punch";
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Punch;
}

public class BoneCrunch : AttackAction
{
    public BoneCrunch()
    {
        AttackDamage = new Random().Next(2);
        AttackProbability = 1;
        Name = "Bone Crunch";
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.BoneCrunch;
}

public class Unraveling : AttackAction
{
    public Unraveling()
    {
        AttackType = AttackTypes.Decoding;
        AttackDamage = new Random().Next(0, 5);
        AttackProbability = 1;
        Name = "Unraveling";
    }

    public override AttackActions Execute() => AttackActions.Unraveling;
    public override void Use() => Execute();
}

public class Bite : AttackAction
{
    public Bite()
    {
        AttackDamage = 1;
        AttackProbability = 1;
        Name = "Bite";
    }

    public override AttackActions Execute() => AttackActions.Bite;
    public override void Use() => Execute();
}

public class Grapple : AttackAction
{
    public Grapple()
    {
        AttackDamage = 2;
        AttackProbability = 0.5;
        Name = "Grapple";
        AttackSideEffect = AttackSideEffects.Steal;
    }

    public override AttackActions Execute() => AttackActions.Grapple;
    public override void Use() => Execute();
}

public class Whip : AttackAction
{
    public Whip()
    {
        AttackDamage = 1;
        AttackProbability = 0.5;
        AttackTemporaryEffect = AttackTemporaryEffects.Poison;
        Name = "Whip";
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Whip;
}

public class Scratch : AttackAction
{
    public Scratch()
    {
        AttackDamage = 1;
        AttackProbability = 1;
        AttackTemporaryEffect = AttackTemporaryEffects.RotPlague;
        Name = "Scratch";
    }

    public override AttackActions Execute() => AttackActions.Scratch;
    public override void Use() => Execute();
}

public abstract class AreaAttack : AttackAction
{

}

public class SmartRockets : AreaAttack
{
    public SmartRockets()
    {
        AttackDamage = 1;
        AttackProbability = 0.75;
        Name = "Smart Rockets";
    }
    
    public override AttackActions Execute() => AttackActions.SmartRockets;
    public override void Use() => Execute();
}

public abstract class Gear : AttackAction
{
    public int DefensiveValue { get; internal set; } = 0;
    public int OffensiveValue { get; internal set; } = 0;

    public OffensiveAttackModifier? OffensiveAttackModifier;
}

public abstract class Weapon : Gear
{

}

public class Sword : Weapon
{
    public Sword()
    {
        Name = "Sword";
        AttackDamage = 2;
        AttackProbability = 1;
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Slash;
}

public class Dagger : Weapon
{
    public Dagger()
    {
        Name = "Dagger";
        AttackDamage = 1;
        AttackProbability = 1;
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Stab;
}

public class VinsBow : Weapon
{
    public VinsBow()
    {
        Name = "Vin's Bow";
        AttackDamage = 3;
        AttackProbability = 0.5;
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.QuickShot;
}

public class CannonOfConsolas : Weapon
{
    public CannonOfConsolas(TurnManager turn)
    {
        Name = "Cannon Of Consolas";
        AttackDamage = GetTurnDamage(turn);
        AttackProbability = 1;
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.CannonBall;

    public int GetTurnDamage(TurnManager turn)
    {
        int value = 1;
        if (turn.Round.CurrentRound % 3 == 0 && turn.Round.CurrentRound % 5 == 0) value = 5;
        else if (turn.Round.CurrentRound % 3 == 0 || turn.Round.CurrentRound % 5 == 0) value = 2;
        
        return value;
    }
}

public abstract class Armor : Gear
{

}

public class BinaryHelm : Armor
{
    public BinaryHelm()
    {
        Name = "Binary Helm";
        DefensiveValue = 1;
        OffensiveAttackModifier = new Binary();
    }

    public override void Use() => Execute();
    public override AttackActions Execute() => AttackActions.Nothing;
}

public abstract class AttackModifier<T> : IAction
{
    public string Name { get; set; } = "None";
    public int Value { get; set; } = 0;
    public abstract T Execute();
    void IAction.Execute() => Execute();
}

public abstract class DefensiveAttackModifier : AttackModifier<DefensiveAttackModifiers>
{

}

public class StoneArmor : DefensiveAttackModifier
{
    public StoneArmor()
    {
        Name = "Stone Armor";
        Value = -1;
    }

    public override DefensiveAttackModifiers Execute() => DefensiveAttackModifiers.StoneArmor;
}

public class ObjectSight : DefensiveAttackModifier
{
    public ObjectSight()
    {
        Name = "Object Sight";
        Value = -2;
    }

    public override DefensiveAttackModifiers Execute() => DefensiveAttackModifiers.ObjectSight;
}

public abstract class OffensiveAttackModifier : AttackModifier<OffensiveAttackModifiers>
{

}

public class Binary : OffensiveAttackModifier
{
    public Binary()
    {
        Name = "Binary";
        Value = new Random().Next(1);
    }

    public override OffensiveAttackModifiers Execute() => OffensiveAttackModifiers.Binary;
}

public interface IMenuAction<T> : IAction
{

}

public abstract class MenuOption : IMenuAction<MenuOptions>
{
    public string Name { get; set; } = "Menu Option";
    public abstract MenuOptions Execute();
    void IAction.Execute() => Execute();
}

public class ComputerVsComputer : MenuOption
{
    public ComputerVsComputer()
    {
        Name = "Computer Vs. Computer";
    }
    public override MenuOptions Execute() => MenuOptions.ComputerVsComputer;
}

public class PlayerVsComputer : MenuOption
{
    public PlayerVsComputer()
    {
        Name = "Player Vs. Computer";
    }

    public override MenuOptions Execute() => MenuOptions.PlayerVsComputer;
}

public class PlayerVsPlayer : MenuOption
{
    public PlayerVsPlayer()
    {
        Name = "Player Vs. Player";
    }

    public override MenuOptions Execute() => MenuOptions.PlayerVsPlayer;
}

public class SkipTurn : MenuOption
{
    public SkipTurn()
    {
        Name = "Skip Turn.";
    }

    public override MenuOptions Execute() => MenuOptions.SkipTurn;
}

public class Attack : MenuOption
{
    public Attack()
    {
        Name = "Attack.";
    }

    public override MenuOptions Execute() => MenuOptions.Attack;
}

public class UseItem : MenuOption
{
    public UseItem()
    {
        Name = "Use Item.";
    }

    public override MenuOptions Execute() => MenuOptions.UseItem;
}

public class EquipGear : MenuOption
{
    public EquipGear()
    {
        Name = "Equip Gear.";
    }

    public override MenuOptions Execute() => MenuOptions.EquipGear;
}

public enum AttackActions
{
    Nothing,
    Punch,
    BoneCrunch,
    Unraveling,
    Slash,
    Stab,
    QuickShot,
    Bite,
    Grapple,
    Whip,
    Scratch,
    SmartRockets,
    CannonBall
}

public enum DefensiveAttackModifiers
{
    StoneArmor,
    ObjectSight
}

public enum OffensiveAttackModifiers
{
    Binary
}

public enum AttackTypes
{
    Normal,
    Decoding
}

public enum AttackSideEffects
{
    Steal
}

public enum AttackTemporaryEffects
{
    Poison,
    RotPlague
}

public enum ConsumableItem
{
    HealthPotion,
    SimulasSoup
}

public enum MenuOptions
{
    ComputerVsComputer,
    PlayerVsComputer,
    PlayerVsPlayer,
    SkipTurn,
    Attack,
    UseItem,
    EquipGear
}

public enum CharacterOptions
{
    Skip,
    Attack,
    Item,
    Gear
}