using Core;
using App;
using Spinners;

namespace Samples;

internal abstract class BaseRulesSample 
{
    private readonly List<SimpleRecord> _records;
    private readonly IRulesBroker _broker;
    private readonly ISampleRecordsGenerator _generator;

    private string[] _rulesScripts;

    private readonly CancellationTokenSource _cts;
    private TextSpinner? _spinner;
    private Task? _spinnerTask;

    public BaseRulesSample(IRulesBroker broker, ISampleRecordsGenerator recordsGenerator, params string[] rulesScripts) 
    {
        if (rulesScripts?.Length > 0)
            _rulesScripts = rulesScripts;
        else
            _rulesScripts = new string[]{"Simple"};

        _broker = broker;
        _generator = recordsGenerator;
        _records = new List<SimpleRecord>();
        _cts = new CancellationTokenSource();
        _spinnerTask = null;
    }

    // ----------------------------------------------------------------------------------------------------
    // Rules parameters
    public void SetRulesScript (params string[] scripts){
        _rulesScripts = scripts;
    }

    public TimeSpan RunTime => _broker.RunTime;

    // ----------------------------------------------------------------------------------------------------
    // Run the sample against the rules engine
    public virtual async Task<Core.RulesResult[]> Execute(string? rootWorkflow = null)
    {
        if (_broker is null)
            throw new Exception("Engine is not configured");

        startSpinner();
        CreateRecords();
        displayCtr();

        if (_rulesScripts == null || _rulesScripts.Length < 1)
            throw new Exception("No rules configured");
        else
            _broker.ConfigureEngine(_rulesScripts);

        rootWorkflow ??= _broker.DefaultWorkflow;

        var results = await _broker.Execute<SimpleRecord>(_records.ToArray(), rootWorkflow);

        _cts.Cancel();

        _spinnerTask?.Wait(500);
        return results;
    }

    // ----------------------------------------------------------------------------------------------------
    // Create the sample data for this test
    protected abstract void CreateRecords ();

    protected ISampleRecordsGenerator Generator => _generator;

    protected void AddRecords(params SimpleRecord[] records){
        _records.AddRange(records);
    }

    protected SimpleRecord? GetById (string id){
        return _records.FirstOrDefault(x=> x.Id == id);
    }

    // ----------------------------------------------------------------------------------------------------
    // Start a wait indicator
    private void startSpinner(){
        var centreSpinner = new TextSpinner(TextSpinner.SpinnerStyle.Rolling);
        centreSpinner.SetPreLabel("Working ");
        centreSpinner.SetPostLabel("");
        centreSpinner.SetSpinnerColour(ConsoleColor.DarkGreen);
        centreSpinner.SetSpinnerSpeed(150);

        _spinnerTask = centreSpinner.Start(_cts.Token);

        _spinnerTask.Wait(500);

        _spinner = centreSpinner;

    }

    private void displayCtr() {
        if (_spinner == null)
            return;

        void displayCounter (int ctr) {
            lock (_spinner.SyncRoot)
            {
                var windowWidth = Console.WindowWidth;
                var popCursor = Console.GetCursorPosition();
                var message = $"{ctr}";
                Console.SetCursorPosition(windowWidth - message.Length, 1);
                Console.Write("{0}", _broker.Processed);
                Console.SetCursorPosition(popCursor.Left, popCursor.Top);
            }
        }

        var t = Task.Run(async ()=>{
            while (!_cts.IsCancellationRequested){
                displayCounter(_broker.Processed);

                await Task.Delay(1000);
            }
            displayCounter(_broker.Processed);
        });

        t.Wait(100);
    }
}
