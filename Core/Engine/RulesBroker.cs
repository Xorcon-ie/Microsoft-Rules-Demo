using Newtonsoft.Json;
using RulesEngine.Models;
using System.Diagnostics;

namespace Core.Engine;

internal class RulesBroker : Core.IRulesBroker
{
    private readonly Core.IRulesRepository? _repo;
    private readonly Stopwatch _executionTimer, _runTimer;
    private readonly Core.DataAdapter _adapter;
    private readonly List<Type> _customRules;

    private RulesEngine.RulesEngine? _engine;

    public RulesBroker(Core.IRulesRepository? repo)
    {
        if (repo is null)
            throw new ArgumentException("Engine cannot be constructed with an empty repository");

        _repo = repo;
        _executionTimer = new Stopwatch();
        _runTimer = new Stopwatch();
        _adapter = new Core.DataAdapter();
        _customRules = new List<Type>();

        DefaultWorkflow = "not set";
    }

    public string DefaultWorkflow {get; internal set;}

    public void AddCustomRule (Type ruleType){
        _customRules.Add(ruleType);
    }

    //  ----------------------------------------------------------------------------------------------------
    // Set up the Rules engine to execute the rules file
    public virtual bool ConfigureEngine(params string[] workflows)
    {
        var rulesEngineSettings = new ReSettings { CustomTypes = _customRules.ToArray() };
        var workflowDefinitions = readAllRules(workflows);

        var engine = new RulesEngine.RulesEngine(workflowDefinitions.ToArray(), rulesEngineSettings);

        _engine = engine;
        return true;
    }


    // ----------------------------------------------------------------------------------------------------
    // Evaluate the rules against the set of records, using the identified workflow as the root 
    public async Task<Core.RulesResult[]> Execute<T>(T[] records, string workflow) where T : Core.IRulesRecord
    {
        if (_engine == null)
            throw new Exception("Engine not configured");

        var results = new List<Core.RulesResult>();

        _runTimer.Start();
        foreach (var rec in records)
        {
            var rulesInput = _adapter.AdaptDataToDynamic(rec);

            Processed = Processed + 1;
            _executionTimer.Reset();
            _executionTimer.Start();
            var resultData = await _engine.ExecuteAllRulesAsync(workflow, rulesInput);
            _executionTimer.Stop();

            var recordResults = adaptRulesTreeResults(rec, workflow, resultData);

            var averageExecutionTime = (float)_executionTimer.ElapsedMilliseconds / (float)recordResults.Count;

            foreach (var r in recordResults)
                r.ApproxExecutionMilliseconds = averageExecutionTime;

            results.AddRange(recordResults);
        }

        _runTimer.Stop();
        return results.ToArray();
    }

    public int Processed {get;set;}

    public TimeSpan RunTime => _runTimer.Elapsed;

    // ----------------------------------------------------------------------------------------------------
    // Adapt results from the rules engine execution to results for returning
    private List<Core.RulesResult>? adaptRulesTreeResults<T>(T rec, string workflow, IEnumerable<RuleResultTree> results) where T : Core.IRulesRecord
    {
        if (results?.Count() > 0)
        {
            var rulesResults = (from r in results
                                select new Core.RulesResult
                                {
                                    RecordId = rec.Id,
                                    RecordDescription = rec.Descriptor,
                                    RuleName = r.Rule.RuleName,
                                    RulesScript = workflow,
                                    Success = r.IsSuccess,
                                    SuccessResult = r.Rule.SuccessEvent,
                                    LastError = r.IsSuccess ? null : r.ExceptionMessage,
                                    RuleOutput = r.ActionResult?.Output,
                                    SubResults = adaptRulesTreeResults(rec, workflow, r.ChildResults)?.ToArray()
                                }).ToList();
            return rulesResults;
        }
        return null;
    }

    // ----------------------------------------------------------------------------------------------------
    // Read all the individual rules definitions
    private List<string>? readAllRules(params string[] workflows)
    {
        if (_repo == null)
            return null;

        var ruleDefinitions = new List<string>();

        var workflowDefinitions = _repo.ReadRules(workflows);

        DefaultWorkflow = workflowDefinitions[0].WorkflowName;

        var rules = from wf in workflowDefinitions
                    select JsonConvert.SerializeObject(wf);

        ruleDefinitions.AddRange(rules);

        return ruleDefinitions;
    }

}
