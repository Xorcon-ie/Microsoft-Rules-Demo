namespace Core;

public interface IRulesBroker {
    bool ConfigureEngine(params string[] workflows);

    public string DefaultWorkflow {get;}

    public void AddCustomRule(Type ruleType);

    Task<RulesResult[]> Execute<T>(T[] records, string workflow) where T: IRulesRecord;
    int Processed {get;}
    TimeSpan RunTime {get;}
}