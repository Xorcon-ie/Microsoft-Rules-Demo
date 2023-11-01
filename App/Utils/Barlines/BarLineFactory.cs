using Microsoft.VisualBasic;

namespace Barlines;


public static class BarLineFactory{
    public enum BarType{
        Default,
        Colour
    };

    public static IBarFormatter CreateBarLine (BarType bt = BarType.Default, int preferredWidth = -1, Dictionary<float, ConsoleColor>? colours = null){
        if (preferredWidth == -1)
            preferredWidth = (int)Math.Floor(Console.WindowWidth * .6);

        IBarFormatter formatter;

        if (bt == BarType.Colour){
            var barLine = new ColourBarFormatter();
            if (colours != null)
                barLine.Colours = colours;
            
            formatter = barLine;
        } else {
            formatter = new DefaultBarFormatter();
        }

        formatter.DisplayWidth = preferredWidth;
        formatter.LeadStringFormat = "{0} ";
        formatter.FollowingStringFormat = " - {0}\n";

        return formatter;
    }
}