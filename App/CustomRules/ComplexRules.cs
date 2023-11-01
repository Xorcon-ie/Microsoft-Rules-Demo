using System.Security.Cryptography;

namespace CustomRules;

public static class ComplexRules
{
    public static int NumAverages = 4;

    public static decimal ProfitMovingAverage(object profits, int minAverages)
    {
        var ma = new DecimalMovingAverage(NumAverages);
        var result = 0.0m;

        var allProfits = profits.ToProfitArray();

        if (allProfits.Length < minAverages)
            return 0.0m;

        foreach (var v in allProfits)
        {
            result = ma.Update(v.Profit);
        }

        return result;
    }
}

// ----------------------------------------------------------------------------------------------------
// Calculate a simple moving average based on 'k' instances
internal class DecimalMovingAverage
{
    private readonly int _k;
    private readonly decimal[] _values;

    private int _index = 0;
    private decimal _sum = 0;

    public DecimalMovingAverage(int k)
    {
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

        _k = k;
        _values = new decimal[k];
    }

    public decimal Update(decimal nextInput)
    {
        // calculate the new sum
        _sum = _sum - _values[_index] + nextInput;

        // overwrite the old value with the new one
        _values[_index] = nextInput;

        // increment the index (wrapping back to 0)
        _index = (_index + 1) % _k;

        // calculate the average
        return ((decimal)_sum) / _k;
    }
}
