using System.ComponentModel;

namespace App;



public record Shareholder {
    public Shareholder(string name, float holding){
        Name =  name;
        Holding = holding;
    }
    public string Name {get;set;}
    public float Holding {get;set;}
}

public record Profits {
    public Profits(int year, decimal profit){
        Year = year;
        Profit = profit;
    }

    public int Year {get;set;}
    public decimal Profit {get;set;}
}

// ----------------------------------------------------------------------------------------------------
// Sample record for holding information to be evaluated by the rules
public record SimpleRecord : Core.IRulesRecord{
    public SimpleRecord (){
        Id = Guid.NewGuid().ToString();
        Directors = new string[]{};
        ShareHolders = new Shareholder[]{};
        AnnualProfit = new Profits[]{};
   }

    public string Id {get; set;}
    public required string CompanyName {get;init;}
    public required DateTime IncorporationDate {get;init;}
    public required string IncorporationCountryCode {get;init;}
    public string? RegistrationNumber {get;set;}
    public string? TaxRegistration {get;set;}
    public decimal CreditRating {get;set;}
    
    public string[] Directors {get;set;}
    public Shareholder[] ShareHolders {get;set;}
    public required DateTime LastFiledAccountsDate {get;init;}
    public Profits[] AnnualProfit {get;set;}
    public string Descriptor => CompanyName;
}
