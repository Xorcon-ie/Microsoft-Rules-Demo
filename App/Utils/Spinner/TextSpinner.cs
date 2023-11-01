
namespace Spinners;

public class TextSpinner
{
    #region Constants
    private const string TL = "\u2598";
    private const string TR = "\u259D";
    private const string BL = "\u2596";
    private const string BR = "\u2597";

    private const string TOP = "\u2580";
    private const string BOTTOM = "\u2584";
    private const string LEFT = "\u258C";
    private const string RIGHT = "\u2590";


    private const string SC_TOP = "\u23BA";
    private const string SC_HIGH = "\u23BB";
    private const string SC_LOW = "\u23BC";
    private const string SC_BOT = "\u23BD";
    #endregion

    public enum SpinnerStyle
    {
        Corners,
        Growing,
        Spinning,
        Scanning,
        Rolling,
        Bounce,
        Numbers
    }

    public enum SpinnerSpeed
    {
        Sprinting,
        Running,
        Jogging,
        Walking,
        Strolling,
        Crawling,
    }

    public enum SpinnerJustification
    {
        None,
        Left,
        Centre,
        Right
    }

    private static readonly object _consoleSync = new object();

    private readonly string[] _spinnerChars;
    private int _spinnerDelay;
    private Task? _t;
    private int _left, _top;
    private SpinnerJustification _justification;

    private string _preLabel, _postLabel;
    private ConsoleColor _spinnerColour;


    public TextSpinner(SpinnerStyle requestedStyle)
    {
        _spinnerChars = getSpinnerStyle(requestedStyle);
        _spinnerDelay = getSpinnerAnimationDelay(SpinnerSpeed.Jogging);
        _justification = SpinnerJustification.None;
        setDefaultPosition();
        _preLabel = "[";
        _postLabel = "]";
        _spinnerColour = ConsoleColor.White;
    }


    public object SyncRoot => _consoleSync;

    public void SetSpinnerSpeed(SpinnerSpeed s)
    {
        var newSpeed = getSpinnerAnimationDelay(s);

        Interlocked.Exchange(ref _spinnerDelay, newSpeed);
    }

    public void SetSpinnerSpeed(int animationStepDelay)
    {
        Interlocked.Exchange(ref _spinnerDelay, animationStepDelay);
    }

    public void SetPosition(int left, int top)
    {
        Interlocked.Exchange(ref _left, left);
        Interlocked.Exchange(ref _top, top);
    }

    public void SetPosition(SpinnerJustification j, int top)
    {
        _justification = j;
        Interlocked.Exchange(ref _top, top);
    }

    public void SetSpinnerColour(ConsoleColor c)
    {
        _spinnerColour = c;
    }

    public void SetPreLabel(string l)
    {
        _preLabel = l;
    }

    public void SetPostLabel(string l)
    {
        _postLabel = l;
    }

    public Task Start(CancellationToken token)
    {
        _t = Task.Run(async () =>
        {
            var templateSpinner = string.Format("{0}{1}{2}", _preLabel, _spinnerChars[0], _postLabel);
            var templateLen = templateSpinner.Length;

            var stepMod = _spinnerChars.Length;
            var step = 0;

            while (!token.IsCancellationRequested)
            {
                var icon = step % stepMod;

                displaySpinner(templateSpinner, icon);

                step++;
                await Task.Delay(_spinnerDelay);
            }

            var blankLen = new string(' ', templateSpinner.Length);
            displaySpinner(blankLen, 0, true);
        });

        return _t;
    }

    private void displaySpinner(string spinnerString, int icon, bool useSpinnerString = false)
    {
        var templateLen = spinnerString.Length;

        if (_justification != SpinnerJustification.None)
            _left = positionForString(_justification, templateLen);

        lock (_consoleSync)
        {
            var currentPos = Console.GetCursorPosition();

            Console.SetCursorPosition(_left, _top);

            if (useSpinnerString)
            {
                Console.Write(spinnerString);
            }
            else
            {
                if (!string.IsNullOrEmpty(_preLabel))
                    Console.Write(_preLabel);

                Console.ForegroundColor = _spinnerColour;
                Console.Write("{0}", _spinnerChars[icon]);
                Console.ResetColor();

                if (!string.IsNullOrEmpty(_postLabel))
                    Console.Write(_postLabel);
            }

            Console.Write("\n");

            if (currentPos.Top == _top)
                currentPos.Top += 1;

            Console.SetCursorPosition(currentPos.Left, currentPos.Top);
        }
    }

    private void setDefaultPosition()
    {
        var curPos = Console.GetCursorPosition();
        _left = 0;
        _top = curPos.Top;
    }

    private static int positionForString(SpinnerJustification justification, int templateLen) =>
                    justification switch
                    {
                        SpinnerJustification.Left => 0,
                        SpinnerJustification.Right => Console.WindowWidth - templateLen,
                        SpinnerJustification.Centre => (int)((Console.WindowWidth - templateLen) / 2),
                        _ => 0
                    };

    private static string[] getSpinnerStyle(SpinnerStyle style)
    {
        var styles = new Dictionary<SpinnerStyle, string[]> {
            {SpinnerStyle.Corners, new string[]{ TL, TR, BR, BL, LEFT, TOP, RIGHT, BOTTOM, BL } },
            {SpinnerStyle.Growing, new string[]{ BR, BOTTOM, BL, LEFT, TL, TOP, TR, RIGHT }},
            {SpinnerStyle.Spinning, new string[] { "      ","      ","      ","      ","      ", "     /","    | ","   \\  ","  \u2015   "," /    ", "|     " }},
            {SpinnerStyle.Scanning, new string[] { SC_TOP, SC_HIGH, SC_LOW, SC_BOT, SC_LOW, SC_HIGH }},
            {SpinnerStyle.Rolling, new string[] {
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_BOT, SC_BOT),
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_BOT, SC_BOT),
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_BOT, SC_BOT),
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_BOT, SC_BOT),

                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_BOT, SC_LOW),
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_BOT, SC_LOW, SC_HIGH),
                string.Format("{0}{1}{2}{3}", SC_BOT, SC_LOW, SC_HIGH, SC_TOP),
                string.Format("{0}{1}{2}{3}", SC_LOW, SC_HIGH, SC_TOP, SC_HIGH),
                string.Format("{0}{1}{2}{3}", SC_HIGH, SC_TOP, SC_HIGH, SC_LOW),
                string.Format("{0}{1}{2}{3}", SC_TOP, SC_HIGH, SC_LOW, SC_BOT),
                string.Format("{0}{1}{2}{3}", SC_HIGH, SC_LOW, SC_BOT, SC_BOT),
                string.Format("{0}{1}{2}{3}", SC_LOW, SC_BOT, SC_BOT, SC_BOT),
            }},
            {SpinnerStyle.Bounce, new string[] { "\u2583     ", " \u2583    ", "  \u2583   ", "   \u2583  ", "    \u2583 ", "     \u2583", "    \u2583 ", "   \u2583  ", "  \u2583   ", " \u2583    "}},
            {SpinnerStyle.Numbers,new string[]{"-0-", "-1-", "-2-", "-3-", "-4-", "-5-", "-6-", "-7-", "-8-", "-9-" } },
        };

        return styles[style];
    }

    private static int getSpinnerAnimationDelay(SpinnerSpeed speed)
    {
        return speed switch
        {
            SpinnerSpeed.Crawling => 750,
            SpinnerSpeed.Strolling => 660,
            SpinnerSpeed.Walking => 500,
            SpinnerSpeed.Jogging => 400,
            SpinnerSpeed.Running => 200,
            SpinnerSpeed.Sprinting => 100,
            _ => 250
        };
    }

}
