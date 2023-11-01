using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Diagnostics;

namespace Core;

public class DataAdapter
{
    private readonly Stopwatch _serialisationTimer;
    private readonly ExpandoObjectConverter _converter;

    public DataAdapter(){
        _serialisationTimer = new Stopwatch();
        _converter = new ExpandoObjectConverter();
    }

    // ----------------------------------------------------------------------------------------------------
    // Serialise input data as a dynamic type
    public dynamic AdaptDataToDynamic<T>(T r)
    {
        var raw = JsonConvert.SerializeObject(r);
        var response = JsonConvert.DeserializeObject<ExpandoObject>(raw, _converter);

        _serialisationTimer.Stop();

        return response;
    }
}