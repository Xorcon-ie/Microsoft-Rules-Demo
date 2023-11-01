using Newtonsoft.Json;
using RulesEngine.Models;


namespace Repository.Files;

internal class FileRulesRepository : Core.IRulesRepository
{
    private readonly string _rootPath;

    public FileRulesRepository(string rootPath)
    {
        _rootPath = rootPath;
    }

    public List<Workflow> ReadRules(params string[] ruleName)
    {
        var ruleDefinitions = new List<Workflow>();

        if (ruleName == null || ruleName.Length == 0)
            return ruleDefinitions;

        var rulesRoot = Path.Combine(Directory.GetCurrentDirectory(), _rootPath);

        foreach (var r in ruleName){
            var fullPath = r;

            if (!File.Exists(r)){
                fullPath = Path.Combine(rulesRoot, r);
                fullPath = Path.ChangeExtension(fullPath, "json");
            }

            var ruleFile = File.ReadAllText(fullPath);

            var response = JsonConvert.DeserializeObject<Workflow[]>(ruleFile);

            if (response != null)
                ruleDefinitions.AddRange(response);
        }

        return ruleDefinitions;
    }
}
