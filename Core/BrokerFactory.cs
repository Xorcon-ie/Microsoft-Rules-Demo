namespace Core;

public static class BrokerFactory {
    public static IRulesBroker CreateEngine (bool useFileRepository = true){
        IRulesRepository? repo = useFileRepository ? new Repository.Files.FileRulesRepository("Rules") : null;

        var engine = new Engine.RulesBroker(repo);

        return engine;
    }
}