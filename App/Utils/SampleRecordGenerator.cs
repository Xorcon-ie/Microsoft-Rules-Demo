using Newtonsoft.Json;

namespace App;


public interface ISampleRecordsGenerator {
    List<SimpleRecord> CreateRecords (int count = 10000);
}

public class SampleRecordGenerator : ISampleRecordsGenerator{
    private static string[] Countries = { "IE", "GB", "FR", "DE", "NL", "ES", "IT", "CH", "PL" };
    private static string[] FirstNames = {
        "John", "William", "James", "Charles", "George", "Frank", "Joseph", "Thomas",
        "Henry", "Robert", "Edward", "Harry", "Walter", "Arthur", "Fred", "Albert",
        "David", "Joe", "Charlie", "Richard", "Andrew",
        "Mary", "Anna", "Emma", "Elizabeth", "Margaret", "Alice", "Sarah", "Annie",
        "Ciara", "Ella", "Laura", "Carrie", "Julia", "Edith", "Mattie", "Catherine",
        "Helen", "Louise", "Eva", "Frances", "Lucy"
    };
    private static string[] LastNames = { 
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson",
        "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Thompson",
        "White", "Harris", "Clark", "Lewis", "Robinson", "Young", "King", "Hill"
    };


    private readonly Random _random;

    public SampleRecordGenerator(){
        _random = new Random();
    }

    public static string JsonEncodeRecords(List<SimpleRecord> records)
    {
        var response = JsonConvert.SerializeObject(records, Formatting.Indented);
        return response;
    }

    // ----------------------------------------------------------------------------------------------------
    // Create the sample data for this test
    public List<SimpleRecord> CreateRecords(int count = 10000)
    {
        var response = new List<SimpleRecord>();

        for (var ctr = 0; ctr < count; ctr++)
        {
            var incorporationDate = GenerateDate();
            var lastAccountingDate = GenerateDate(incorporationDate.Year);

            var testRecord = new SimpleRecord
            {
                CompanyName = $"Test Company {ctr}",
                IncorporationDate = incorporationDate,
                IncorporationCountryCode = GenerateCountryCode(),
                RegistrationNumber = GenerateRegistrationNumber(),
                TaxRegistration = GenerateRegistrationNumber(),
                CreditRating = (Decimal)_random.Next(10,95),
                Directors = GenerateDirectors(),
                ShareHolders = GenerateShareholders(),
                LastFiledAccountsDate = lastAccountingDate,
                AnnualProfit = GenerateProfits(incorporationDate.Year, lastAccountingDate.Year),
            };

            response.Add(testRecord);
        }
        return response;
    }

    // ----------------------------------------------------------------------------------------------------
    // Generators
    private string GenerateCountryCode()
    {
        var country = _random.Next(Countries.Length);
        return Countries[country];
    }

    private string GenerateRegistrationNumber () {
        var conumber = _random.Next(12345, 99999);
        return string.Format("{0:00000}-G", conumber);
    }

    private DateTime GenerateDate (int startFrom = 1980) {
        var incorporationYear = _random.Next(startFrom, DateTime.Now.Year);
        var incorporationMonth = _random.Next(1, 13);

        return new DateTime(incorporationYear, incorporationMonth, 1);
    }

    private string GenerateName (){
        var firstNameIdx = _random.Next(FirstNames.Length);
        var lastNameIdx = _random.Next(LastNames.Length);

        var name = string.Format("{0} {1}", FirstNames[firstNameIdx], LastNames[lastNameIdx]);

        return name;
    }

    private Profits[] GenerateProfits(int startFrom, int lastDate)
    {
        var response = new Dictionary<int, Profits>();

        for (var profitYear = startFrom + 1; profitYear <= lastDate; profitYear++)
        {
            var profit = _random.Next(-150000, 1000000);
            var p = new Profits (profitYear, (decimal)profit);
            response.Add(profitYear, p);
        }
        return response.Values.ToArray();
    }

    private string[] GenerateDirectors (){
        var directorsCount = _random.Next(2, 8);
        var directors = new List<string>();

        for (int ctr = 0; ctr < directorsCount; ctr++){
            var newDirector = GenerateName();
            directors.Add(newDirector);
        }

        return directors.ToArray();
    }

    private Shareholder[] GenerateShareholders(){
        var cumulativeShareholding = 0.0f;

        var shareholders = new Dictionary<string, Shareholder>();

        do 
        {
            var newShareholder = GenerateName();

            if (shareholders.ContainsKey(newShareholder))
                continue;

            var holding = 100f - cumulativeShareholding;

            if (holding > 8.0f){
                while (true)
                {
                    holding = (float)Math.Round((_random.NextSingle() * 100f), 2);
                    if (holding > 55f)
                        continue;

                    if ((holding + cumulativeShareholding) < 100f)
                        break;
                }
            }
            else
                holding = (float)Math.Round(holding, 2);

            var newRec = new Shareholder(newShareholder, holding);
            shareholders.Add(newShareholder, newRec);
            cumulativeShareholding += holding;

        } while (cumulativeShareholding < 98.0f);

        return shareholders.Values.ToArray();
    }
}