using Domain;

namespace UNOEngine;

public class GameCard
{
    public ECardSuite CardSuite { get; set; } = default!;
    public ECardValue CardValue { get; set; } = default!;

    public GameCard(int suite, int value)
    {
        CardSuite = (ECardSuite)suite;
        CardValue = (ECardValue)value;
    }

    public override string ToString()
    {
        return $"{CardSuite}, {CardValue}";
    }

    public int GetIntValue()
    {
        return (int)CardValue;
    }
    
    public int GetIntSuite()
    {        
        return (int)CardSuite;
    }

    public string GetValue()
    {
        return CardValue.ToString().ToLower()
            .Replace("value", "")
            .Replace("reverse", "/")
            .Replace("skip", "->")
            .Replace("plus2", "+2")
            .Replace("changecolor", "Ø")
            .Replace("draw4", "+4");
    }

    public string AsCard()
    {
        return $"|-{CardValue}-|".Replace("Value", "");
    }

    public string GetCss()
    {
        return $"card num-{CardValue} {CardSuite}".ToString().Replace("Value","").ToLower();
    }
}