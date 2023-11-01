using System.Data;
using Barlines;

namespace App;

public static class RulesDisplay
{

    public static void PresentRulesResults(Core.RulesResult[] results, TimeSpan runTime, bool groupResults = false)
    {
        presentResultStatistics(runTime, results);

        if (results.Length > 100)
        {
            presentBulkMatches(results);
            presentResultDistribution(runTime, results);
        }
        else
        {
            var lastDescription = string.Empty;
            // If we have less that 100 results then roll them out
            foreach (var r in results)
            {
                if (lastDescription != r.RecordDescription)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0}", r.RecordDescription);
                    lastDescription = r.RecordDescription;
                    Console.ResetColor();
                }
                presentRuleResult(r);
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // Present basic statistics on the execution run
    private static void presentResultStatistics(TimeSpan runTime, Core.RulesResult[] results)
    {
        var executionAverage = (from r in results
                                select r.ApproxExecutionMilliseconds).Average();

        var minAverage = (from r in results
                          select r.ApproxExecutionMilliseconds).Min();

        var maxAverage = (from r in results
                          select r.ApproxExecutionMilliseconds).Max();

        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine("Execution Results:");
        Console.ResetColor();

        Console.WriteLine("Run Time: {0}\tRules Executed: {1}", runTime.ToString(@"hh\:mm\:ss\.ff"), results.Length);
        Console.WriteLine("Average Rule Time (ms): {0:000.00}\tMin: {1:000.00}/Max: {2:000.00}\n", executionAverage, minAverage, maxAverage);
    }

    // ----------------------------------------------------------------------------------------------------
    // Presents information on the results from a single rule
    private static void presentRuleResult(Core.RulesResult result)
    {
        var resultString = result.Success ? "OK" : $"FAIL ({result.LastError})";

        Console.Write(" \u2514 {0}", result.RuleName.PadRight(30));

        if (result.Success)
        {
            if (result.SubResults?.Length > 0)
            {
                presentRuleSubResult(result.SubResults);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("\t- {0}", resultString);
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("\t- {0}", resultString);
            Console.ResetColor();
        }

        if (result.RuleOutput != null)
            Console.Write(" - Additional Info: {0}", result.RuleOutput);

        Console.Write("\n");
    }

    private static void presentRuleSubResult(Core.RulesResult[] subResults)
    {
        var trafficLights = new string[] { "Red", "Amber", "Green" };

        Console.Write("\t- ");

        foreach (var sr in subResults)
        {
            if (sr.Success)
            {
                if (!string.IsNullOrEmpty(sr.SuccessResult))
                {
                    if (trafficLights.Contains(sr.SuccessResult))
                    {
                        switch (sr.SuccessResult)
                        {
                            case "Red":
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                break;
                            case "Amber":
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.BackgroundColor = ConsoleColor.DarkYellow;
                                break;
                            case "Green":
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                                break;
                        }
                        Console.Write("{0}", sr.SuccessResult);
                    }
                    else {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("OK ({0})", sr.SuccessResult);
                    }

                    Console.ResetColor();
                }
                else{
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("OK");
                    Console.ResetColor();
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    private static void presentBulkMatches(Core.RulesResult[] results)
    {
        var formatter = BarLineFactory.CreateBarLine(BarLineFactory.BarType.Colour);
        formatter.LeadStringFormat = "{0} ";
        formatter.FollowingStringFormat = "\t- {0}/{1}\n";

        var subFormatter = BarLineFactory.CreateBarLine(BarLineFactory.BarType.Default);
        subFormatter.LeadStringFormat = "{0} ";
        subFormatter.FollowingStringFormat = "\t- {0}/{1}\n";
        
        var dist = from r in results
                   let rule = r.RuleName
                   group r by rule into g
                   orderby g.Key
                   select new
                   {
                       Rule = g.Key,
                       Successes= g.Where(x=>x.Success == true).Count(),
                       Failures = g.Where(x=>x.Success == false).Count(),
                       SubResults = (from psr in g
                                    where psr.SubResults != null
                                        from sr in psr.SubResults
                                        let subRuleName = sr.RuleName
                                        group sr by sr.RuleName into srg
                                        select new {
                                            ResultName = srg.Key,
                                            Count = srg.Where(x=>x.Success == true).Count()
                                        }
                                    ).ToList()
                   };

        foreach (var d in dist)
        {
            var ruleTotal = (float)d.Successes + d.Failures;
            var value = (float)d.Successes / ruleTotal;

            formatter.SetLeadData(d.Rule.PadRight(28));
            formatter.SetFollowingData(d.Successes, ruleTotal);
            formatter.DisplayBar(value);
            if (d.SubResults?.Count > 0){
                foreach (var sr in d.SubResults){
                    var count = sr.Count;
                    var subValue = (float)count/ruleTotal;

                    subFormatter.SetLeadData($"    {sr.ResultName.PadRight(24)}");
                    subFormatter.SetFollowingData(count, ruleTotal);
                    subFormatter.DisplayBar(subValue);
                }
            }
        }

        var countries = from r in results
                    let rule = $"{r.RuleName} - {r.RuleOutput??"OK"}"
                    where r.RuleName == "CheckCountry" && r.Success == false
                   group r by rule into g
                   orderby g.Key
                   select new
                   {
                       Rule = g.Key,
                       Entries = g.Count()
                   };

        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine("\nFailed Countries:");
        Console.ResetColor();

        var total = (from r in results
                     where r.RuleName == "CheckCountry"
                     select r).Count();

        formatter = BarLineFactory.CreateBarLine(BarLineFactory.BarType.Colour);
        formatter.FollowingStringFormat = "{0}";

        foreach (var d in countries)
        {
            var value = (float)d.Entries / (float)total;
            formatter.SetLeadData(d.Rule.PadRight(28));
            formatter.SetFollowingData(d.Entries.ToString().PadLeft(6));
            formatter.DisplayBar(value);
        }
    }

    private static void presentResultDistribution(TimeSpan runTime, Core.RulesResult[] results, float factor = 1.0f)
    {
        var formatter = BarLineFactory.CreateBarLine(BarLineFactory.BarType.Colour);
        formatter.LeadStringFormat = "{0}ms ";
        formatter.FollowingStringFormat = "{0}";

        var resultSet = from r in results
                        group r by r.RulesScript into g
                        orderby g.Key
                        select new
                        {
                            Group = g.Key,
                            Results = g.ToArray()
                        };

        foreach (var rs in resultSet)
        {
            Console.WriteLine("\nRules Timing Distribution\n");
            presentResultStatistics(runTime, rs.Results);

            var dist = from r in results
                       group r by r.RuleName into rn

                       select new {
                        RuleName = rn.Key,
                        Stats = (from sr in rn
                                 let range = ((int)(Math.Round(sr.ApproxExecutionMilliseconds, 2, MidpointRounding.ToEven) / factor)) * factor
                                 group sr by range into g
                                 orderby g.Key
                                 select new
                                 {
                                     Range = g.Key,
                                     Entries = g.Count()
                                 }).ToList()
                       };


            foreach (var d in dist)
            {
                Console.WriteLine("{0}", d.RuleName);

                var total = d.Stats.Select(x => x.Entries).Sum();

                foreach (var s in d.Stats){
                    var value = (float)s.Entries / (float)total;
                    var wholeRange = Math.Floor(s.Range);
                    formatter.SetLeadData(wholeRange.ToString().PadLeft(26));
                    formatter.SetFollowingData(s.Entries.ToString().PadLeft(6));
                    formatter.DisplayBar(value);
                }
            }
        }
    }
}