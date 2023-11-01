using App;
using Core;

namespace Samples;

internal class SimpleRulesSample : BaseRulesSample, App.IRulesHandler
{
    public SimpleRulesSample(IRulesBroker engine, ISampleRecordsGenerator generator, params string[] rulesScripts) :
        base(engine, generator, rulesScripts)
    {
    }

    public string SampleDisplayName => "Simple Rules Sample";
    public bool GroupResults => false;

    // ----------------------------------------------------------------------------------------------------
    // Create the sample data for this test
    protected override void CreateRecords()
    {
        var records = base.Generator.CreateRecords(10);
        AddRecords(records.ToArray());
    }
}