using CustomRules;
using Samples;

namespace App;

internal static class SampleFactory {

    public static IRulesHandler CreateSimpleRulesSample (params string[]? useRuleFile){
        var generator = new SampleRecordGenerator();

        var engine = Core.BrokerFactory.CreateEngine();

        if (useRuleFile == null || useRuleFile.Length == 0)
            useRuleFile = new string[] {"simple", "matureCompanyRules"};

        engine.AddCustomRule(typeof(ComplexRules));

        var simpleSample = new SimpleRulesSample(engine, generator, useRuleFile);

        return simpleSample;
    }

    public static IRulesHandler CreateBulkSample(string? useRuleFile = null){
        var generator = new SampleRecordGenerator();

        var engine = Core.BrokerFactory.CreateEngine();

        engine.AddCustomRule(typeof(ComplexRules));

        var dataSample = new BulkRulesSample(engine, generator);

        return dataSample;
    }
}