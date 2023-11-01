namespace Barlines;

public abstract class BaseBarFormatter 
{
    protected string? _leaderString;
    protected string? _followingString;

    public BaseBarFormatter()
    {
        BarLeadCharacter = "[";
        BarFollowCharacter = "]";
        DisplayWidth = 75;
        LeadStringFormat = "";
        FollowingStringFormat = "";
        _leaderString = "";
        _followingString = "";
    }

    public string BarLeadCharacter { get; set; }
    public string BarFollowCharacter { get; set; }
    public int DisplayWidth { get; set; }
    public string LeadStringFormat {get;set;}
    public string FollowingStringFormat {get;set;}

    public void SetLeadData(params object[] leadValues)
    {
        if (leadValues != null && leadValues.Length > 0)
            _leaderString = string.Format(LeadStringFormat, leadValues);
        else
            _leaderString = null;
    }

    public void SetFollowingData(params object[] followValues)
    {
        if (followValues != null && followValues.Length > 0)
            _followingString = string.Format(FollowingStringFormat, followValues);
        else
            _followingString = null;
    }

    public abstract void DisplayBar(float value);
}
