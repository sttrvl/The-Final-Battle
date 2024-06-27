public interface IGameObjects
{
    public void Execute();
}

public interface IAction : IGameObjects
{
}

public interface IMenuAction<T> : IAction
{
}

public interface IItemAction<T> : IAction
{
}

public interface IAttackAction : IAction
{
}

public interface IInventoryObject : IAction
{
}

public abstract class InventoryObjects<T> : IItemAction<T>
{
    public abstract T Execute();
    void IGameObjects.Execute() => Execute();
}

public abstract class Consumables : InventoryObjects<ConsumableItem>, IInventoryObject
{
    public string Name { get; set; } = "None";
    public int? Heal = null;
    public override abstract ConsumableItem Execute();
}

public class HealthPotion : Consumables
{
    public HealthPotion()
    {
        Name = "Health Potion";
        Heal = 10;
    }

    public override ConsumableItem Execute() => ConsumableItem.HealthPotion;
}

public class SimulasSoup : Consumables
{
    public SimulasSoup(TurnManager turn)
    {
        Name = "Simulas Soup";
        Heal = turn.SelectedCharacter.MaxHP;
    }

    public override ConsumableItem Execute() => ConsumableItem.HealthPotion;
}

public abstract class Gear : AttackAction, IInventoryObject
{
    public int DefensiveValue;
    public int OffensiveValue;
    public OffensiveAttackModifier? OffensiveAttackModifier;
}

public abstract class Weapon : Gear
{
}

public class Sword : Weapon
{
    public override AttackActions Execute() => AttackActions.Slash;

    public Sword()
    {
        Name = "Sword";
        AttackDamage = 2;
        AttackProbability = 1;
    }
}

public class Dagger : Weapon
{
    public override AttackActions Execute() => AttackActions.Stab;

    public Dagger()
    {
        Name = "Dagger";
        AttackDamage = 1;
        AttackProbability = 1;
    }
}

public class VinsBow : Weapon
{
    public override AttackActions Execute() => AttackActions.QuickShot;

    public VinsBow()
    {
        Name = "Vin's Bow";
        AttackDamage = 3;
        AttackProbability = 0.5;
    }
}

public class CannonOfConsolas : Weapon
{
    public override AttackActions Execute() => AttackActions.QuickShot;

    public CannonOfConsolas(TurnManager turn)
    {
        Name = "Cannon Of Consolas";
        AttackDamage = GetTurnDamage(turn);
        AttackProbability = 1;
    }

    public int GetTurnDamage(TurnManager turn)
    {
        int value = 1;
        if (turn.Round % 3 == 0 || turn.Round % 5 == 0) value = 2;
        if (turn.Round % 3 == 0 && turn.Round % 5 == 0) value = 5;

        return value;
    }
}

public abstract class Armor : Gear
{
}

public class BinaryHelm : Armor
{
    public override AttackActions Execute() => AttackActions.Nothing;

    public BinaryHelm()
    {
        Name = "Binary Helm";
        DefensiveValue = 1;
        OffensiveAttackModifier = new Binary();
    }
}

public abstract class AttackModifier<T> : IAction
{
    public string Name { get; set; } = "None";
    public int Value;
    public abstract T Execute();
    void IGameObjects.Execute() => Execute();
}

public abstract class DefensiveAttackModifier : AttackModifier<DefensiveAttackModifiers>
{
}

public class StoneArmor : DefensiveAttackModifier
{
    public override DefensiveAttackModifiers Execute() => DefensiveAttackModifiers.StoneArmor;
    public StoneArmor()
    {
        Name = "Stone Armor";
        Value = -1;
    }
}

public class ObjectSight : DefensiveAttackModifier
{
    public override DefensiveAttackModifiers Execute() => DefensiveAttackModifiers.ObjectSight;

    public ObjectSight()
    {
        Name = "Object Sight";
        Value = -2;
    }
}

public abstract class OffensiveAttackModifier : AttackModifier<OffensiveAttackModifiers>
{
}

public class Binary : OffensiveAttackModifier
{
    public override OffensiveAttackModifiers Execute() => OffensiveAttackModifiers.Binary;

    public Binary()
    {
        Name = "Binary";
        Value = new Random().Next(1);
    }
}

public abstract class AttackAction : IAttackAction
{
    public string Name { get; set; } = "Attack Action";
    public AttackTypes AttackType { get; set; } = AttackTypes.Normal;
    public abstract AttackActions Execute();
    void IGameObjects.Execute() => Execute();
    public int AttackDamage { get; set; }
    public double AttackProbability { get; set; }
    public AttackSideEffects? AttackSideEffect { get; set; }
    public AttackTemporaryEffects? AttackTemporaryEffect { get; set; }
}

public class Nothing : AttackAction
{
    public override AttackActions Execute() => AttackActions.Nothing;
    public Nothing()
    {
        AttackDamage = 0;
        AttackProbability = 1;
        Name = "Nothing";
    }
}

public class Punch : AttackAction
{
    public override AttackActions Execute() => AttackActions.Punch;

    public Punch()
    {
        AttackDamage = 1;
        AttackProbability = 1;
        Name = "Punch";
    }
}

public class BoneCrunch : AttackAction
{
    public override AttackActions Execute() => AttackActions.BoneCrunch;

    public BoneCrunch()
    {
        AttackDamage = new Random().Next(2);
        AttackProbability = 1;
        Name = "Bone Crunch";
    }
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
}

public class Grapple : AttackAction
{
    public Grapple()
    {
        AttackDamage = 2;
        AttackProbability = 1;
        Name = "Grapple";
        AttackSideEffect = AttackSideEffects.Steal;
    }
    public override AttackActions Execute() => AttackActions.Grapple;
}

public class Whip : AttackAction
{
    public Whip()
    {
        AttackDamage = 1;
        AttackProbability = 1; // change to 0.5
        AttackTemporaryEffect = AttackTemporaryEffects.Poison;
        Name = "Whip";
    }
    public override AttackActions Execute() => AttackActions.Whip;
}

public abstract class MenuOption : IMenuAction<MenuOptions>
{
    public string Name { get; set; } = "Menu Option";
    void IGameObjects.Execute() => Execute();
    public abstract MenuOptions Execute();
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

public enum GearType
{
    Sword,
    Dagger
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
    Whip
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
    Poison
}

public enum ConsumableItem
{
    HealthPotion
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