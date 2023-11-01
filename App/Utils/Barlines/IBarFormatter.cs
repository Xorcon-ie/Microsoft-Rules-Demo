namespace Barlines;

public interface IBarFormatter
{
    int DisplayWidth {get;set;}
    string LeadStringFormat {get;set;}
    string FollowingStringFormat {get;set;}
    string BarLeadCharacter { get; set; }
    string BarFollowCharacter { get; set; }

    void DisplayBar(float value);
    void SetLeadData(params object[] leadValues);
    void SetFollowingData(params object[] followValues);

}