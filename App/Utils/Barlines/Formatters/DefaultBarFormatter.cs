namespace Barlines;

public class DefaultBarFormatter : BaseBarFormatter, IBarFormatter
{
    public DefaultBarFormatter() : base() { }

    public override void DisplayBar(float value)
    {
        var displayBar = BarLineGenerator.GetBarValue(value, DisplayWidth);
        var displayString = string.Format("{0}{1}{2}{3} {4:00.00}% {5}", _leaderString, BarLeadCharacter, displayBar, BarFollowCharacter, value * 100f, _followingString);

        Console.WriteLine(displayString);
    }
}
