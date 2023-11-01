using App;
using Core;

namespace Samples;

internal class BulkRulesSample : BaseRulesSample, IRulesHandler
{
    public BulkRulesSample(IRulesBroker broker, ISampleRecordsGenerator generator, string rulesScript = "Simple") : 
        base(broker, generator, rulesScript) {
    }

    public string SampleDisplayName => "Bulk Rules Sample";
    public bool GroupResults => false;

    protected override void CreateRecords()
    {
        var records = base.Generator.CreateRecords();
        AddRecords(records.ToArray());
    }
}