using App;
using Newtonsoft.Json;

namespace CustomRules;

public static class ProfitUtils {
    public static Profits[]  ToProfitArray(this object obj){
        var intermediateJson = JsonConvert.SerializeObject(obj);
        var profits = JsonConvert.DeserializeObject<Profits[]>(intermediateJson);

        return profits;
    }
}