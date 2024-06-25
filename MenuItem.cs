public class MenuManager
{
    public MenuItem CurrentMenu { get; set; }
    public record MenuItem(String description, MenuOptions action, bool isEnabled);
    
    public void CreateMenu()
    {
        InputManager input = new InputManager();
        foreach (MenuOptions option in Enum.GetValues(typeof(MenuOptions))) ;
            //MenuItems.Add(new MenuItem(input.MenuDescription(option), option, false));  
    }
}