namespace App;

public interface IRulesHandler
{
    string SampleDisplayName {get;}
    bool GroupResults {get;}

    void SetRulesScript(params string[] script);
    TimeSpan RunTime {get;}

    Task<Core.RulesResult[]> Execute(string? rootWorkflow = null);
}
