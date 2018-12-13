using System;
using System.Collections.Generic;
using System.Text;

namespace Stocks
{
    class ListCompanies
    {
        public static List<Company> Companies = new List<Company>
        {
            new Company("AMD", "AMD", "amd_logo.png"),
            new Company("Apple", "AAPL", "apple_logo.png"),
            new Company("Facebook", "FB", "facebook_logo.png"),
            new Company("Google", "GOOGL", "google_logo.png"),
            new Company("Hewlett Packard", "HPE", "hp_logo.png"),
            new Company("IBM", "IBM", "ibm_logo.png"),
            new Company("Intel", "INTC", "intel_logo.png"),
            new Company("Microsoft", "MSFT", "microsoft_logo.png"),
            new Company("Oracle", "ORCL", "oracle_logo.gif"),
            new Company("Twitter", "TWTR", "twitter_logo.png")
        };
    }
}
