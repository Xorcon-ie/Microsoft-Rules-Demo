namespace Barlines;

public class ColourBarFormatter : BaseBarFormatter, IBarFormatter
{
    private Dictionary<float, ConsoleColor> _colours;

    public ColourBarFormatter() : base()
    {
        _colours = new Dictionary<float, ConsoleColor> {
           {0.00f, ConsoleColor.DarkRed},
           {0.20f, ConsoleColor.Red},
           {0.40f, ConsoleColor.DarkYellow},
           {0.60f, ConsoleColor.Yellow},
           {0.80f, ConsoleColor.Green},
           {0.95f, ConsoleColor.DarkGreen}
        };
    }

    public Dictionary<float, ConsoleColor> Colours {
        get => _colours;
        set {
            _colours = value;
        }
    }
    public override void DisplayBar(float value)
    {
        var displayColour = (from c in _colours
                             where c.Key <= value
                             select c.Value).Last();

        var displayBar = BarLineGenerator.GetBarValue(value, DisplayWidth);

        if (!string.IsNullOrEmpty(_leaderString))
            Console.Write(_leaderString);

        Console.Write(BarLeadCharacter);

        Console.ForegroundColor = displayColour;
        Console.Write(displayBar);
        Console.ResetColor();

        Console.Write("{0} {1:00.00}%", BarFollowCharacter, value * 100f);

        if (!string.IsNullOrEmpty(_followingString))
            Console.WriteLine("{0}", _followingString);
        else
            Console.Write("\n");
    }
}
