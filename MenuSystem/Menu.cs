namespace MenuSystem;

public class Menu
{
    private string? Title { get; set; }
    private List<MenuItem> MenuItems { get; set; }
    private List<MenuItem> ReservedItems { get; set; }
    private const string Separator = "--------------------------------";
    private int CurrentIndex { get; set; }
    private int ReservedIndex { get; set; }

    public Menu(string? title, List<MenuItem> menuItems, List<MenuItem> reservedItems = default!)
    {
        Title = title;
        MenuItems = menuItems;
        ReservedItems = reservedItems;
        CurrentIndex = 0;
        ReservedIndex = 0;
    }

    private void Draw()
    {
        Console.Clear();
        Console.WriteLine(Separator);
        Console.WriteLine(Title);
        Console.WriteLine(Separator);

        for (int i = 0; i < MenuItems.Count; i++)
        {
            if (i == CurrentIndex)
            {
                Console.WriteLine($"<{MenuItems[i].MenuLabel}>");
            }
            else
            {
                Console.WriteLine(MenuItems[i].MenuLabel);
            }
        }

        Console.WriteLine(Separator);

        for (int i = 0; i < ReservedItems.Count; i++)
        {
            if (MenuItems.Count == CurrentIndex && i == ReservedIndex)
            {
                Console.Write($"<{ReservedItems[i].MenuLabel}>");
            }
            else
            {
                Console.Write(ReservedItems[i].MenuLabel);
            }
            Console.Write("     ");
        }
        Console.WriteLine();
        
        Console.WriteLine(Separator);
    }

    public string? Run()
    {
        ConsoleKeyInfo keyInfo;
        
        Draw();

        while ((keyInfo = Console.ReadKey()).Key != ConsoleKey.Enter)
        {
            Console.WriteLine(CurrentIndex);
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    CurrentIndex = CurrentIndex == 0 ? MenuItems.Count : CurrentIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    CurrentIndex = CurrentIndex == MenuItems.Count ? 0 : CurrentIndex + 1;
                    break;
                case ConsoleKey.LeftArrow:
                    if (CurrentIndex == MenuItems.Count)
                    {
                        ReservedIndex = ReservedIndex == 0 ? ReservedItems.Count - 1 : ReservedIndex - 1;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (CurrentIndex == MenuItems.Count)
                    {
                        ReservedIndex = ReservedIndex == ReservedItems.Count - 1 ? 0 : ReservedIndex + 1;
                    }
                    break;
            }
            Draw();
        }

        if (CurrentIndex == MenuItems.Count)
        {
            ReservedItems[ReservedIndex].MethodToRun!();
        }
        else
        {
            MenuItems[CurrentIndex].MethodToRun!();
        }
        return null;
    }
}

