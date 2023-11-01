namespace App;

class RulesSamples
{
    public static void Main(string[] args)
    {
        try
        {
            IRulesHandler? sample = SampleFactory.CreateSimpleRulesSample();
            List<string> rulesScript = new List<string>();

            if (args.Length > 0)
            {
                for (var ctr = 0; ctr < args.Length; ctr++)
                {
                    switch (args[ctr])
                    {
                        case "-b":
                            sample = SampleFactory.CreateBulkSample();
                            break;
                        case "-gen":
                            var generator = new SampleRecordGenerator();
                            var sampleData = SampleRecordGenerator.JsonEncodeRecords(generator.CreateRecords(10));
                            Console.WriteLine(sampleData);
                            sample = null;
                            break;
                        case "-use":
                            if (args.Length > ctr + 1)
                            {
                                rulesScript.Add(args[ctr + 1]);
                                ctr++;
                            }
                            else
                            {
                                Console.Error.WriteLine("\nNo Rule File Specified");
                                return;
                            }
                            break;
                        default:
                            Console.WriteLine("\n\nUnknown option {0}", args[ctr]);
                            break;
                    }
                }
            }


            if (sample != null)
            {
                if (rulesScript.Count > 0)
                    sample.SetRulesScript(rulesScript.ToArray());

                Console.Clear();
                Console.WriteLine("{0}", sample.SampleDisplayName);

                var t = Task.Run(async () =>
                {
                    try
                    {
                        var ruleResponse = await sample.Execute();

                        App.RulesDisplay.PresentRulesResults(ruleResponse, sample.RunTime, sample.GroupResults);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error executing rules sample: {ex}");
                    }
                });

                t.Wait();
            }
        }
        catch (NotImplementedException)
        {
            Console.Error.WriteLine("Sample is not implemented");
        }
    }
}