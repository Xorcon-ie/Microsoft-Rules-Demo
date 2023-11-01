using RulesEngine.Models;

namespace Core;

public interface IRulesRepository {
    List<Workflow> ReadRules(params string[] ruleName);
}