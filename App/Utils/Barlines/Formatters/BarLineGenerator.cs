namespace Barlines;

public static class BarLineGenerator
{
    private static string[] blocks = { " ", "\u258f", "\u258e", "\u258d", "\u258c", "\u258b", "\u258a", "\u2589", "\u2588" };

    // ----------------------------------------------------------------------------------------------------
    // Build a string using the unicode blocks that is scaled to the desired displayWidth
    // displayWidth is the width of the display area in characters
    public static string GetBarValue(float value, int displayWidth)
    {
        value = Math.Min(1, Math.Max(0, value));

        var fullBlockWidth = (int)Math.Floor(value * displayWidth);
        var partialWidth = (value * displayWidth) % 1;
        var spaceBlockWidth = displayWidth - fullBlockWidth - 1;

        var blocksString = new String((char)blocks[8][0], fullBlockWidth);

        var trailingSpaces = "";
        var tail = "";
        // if we're not dealing with 100%
        if (fullBlockWidth != displayWidth){
            var partialBlockCharacterIndex = Math.Floor(partialWidth * 8);

            tail = blocks[(int)partialBlockCharacterIndex];
            trailingSpaces = spaceBlockWidth > 0 ? new String(' ', spaceBlockWidth) : "";

        }

        var resultString = string.Format("{0}{1}{2}", blocksString, tail, trailingSpaces);

        return resultString;
    }

}