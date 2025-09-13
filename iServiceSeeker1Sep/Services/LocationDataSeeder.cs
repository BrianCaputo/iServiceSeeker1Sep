using ServiceSeeker.Data;
using System.Collections.Generic;

namespace ServiceSeeker.Services
{
    public static class LocationDataSeeder
    {
        public static List<Country> GetCountries()
        {
            return new List<Country>
            {
                new Country { ID = 1, Name = "United States", Iso2Code = "US" },
                new Country { ID = 2, Name = "Canada", Iso2Code = "CA" },
                new Country {ID = 3, Name = "United Kingdom", Iso2Code = "GB" }
            };
        }

        public static List<StateProvince> GetStateProvinces()
        {
            var states = new List<StateProvince>();

            // United States (CountryId = 1) - States
            states.AddRange(new List<StateProvince>
            {
                new StateProvince { ID = 1, CountryID = 1, Name = "Alabama", Abbreviation = "AL" },
                new StateProvince { ID = 2, CountryID = 1, Name = "Alaska", Abbreviation = "AK" },
                new StateProvince { ID = 3, CountryID = 1, Name = "Arizona", Abbreviation = "AZ" },
                new StateProvince { ID = 4, CountryID = 1, Name = "Arkansas", Abbreviation = "AR" },
                new StateProvince { ID = 5, CountryID = 1, Name = "California", Abbreviation = "CA" },
                new StateProvince { ID = 6, CountryID = 1, Name = "Colorado", Abbreviation = "CO" },
                new StateProvince { ID = 7, CountryID = 1, Name = "Connecticut", Abbreviation = "CT" },
                new StateProvince { ID = 8, CountryID = 1, Name = "Delaware", Abbreviation = "DE" },
                new StateProvince { ID = 9, CountryID = 1, Name = "Florida", Abbreviation = "FL" },
                new StateProvince { ID = 10, CountryID = 1, Name = "Georgia", Abbreviation = "GA" },
                new StateProvince { ID = 11, CountryID = 1, Name = "Hawaii", Abbreviation = "HI" },
                new StateProvince { ID = 12, CountryID = 1, Name = "Idaho", Abbreviation = "ID" },
                new StateProvince { ID = 13, CountryID = 1, Name = "Illinois", Abbreviation = "IL" },
                new StateProvince { ID = 14, CountryID = 1, Name = "Indiana", Abbreviation = "IN" },
                new StateProvince { ID = 15, CountryID = 1, Name = "Iowa", Abbreviation = "IA" },
                new StateProvince { ID = 16, CountryID = 1, Name = "Kansas", Abbreviation = "KS" },
                new StateProvince { ID = 17, CountryID = 1, Name = "Kentucky", Abbreviation = "KY" },
                new StateProvince { ID = 18, CountryID = 1, Name = "Louisiana", Abbreviation = "LA" },
                new StateProvince { ID = 19, CountryID = 1, Name = "Maine", Abbreviation = "ME" },
                new StateProvince { ID = 20, CountryID = 1, Name = "Maryland", Abbreviation = "MD" },
                new StateProvince { ID = 21, CountryID = 1, Name = "Massachusetts", Abbreviation = "MA" },
                new StateProvince { ID = 22, CountryID = 1, Name = "Michigan", Abbreviation = "MI" },
                new StateProvince { ID = 23, CountryID = 1, Name = "Minnesota", Abbreviation = "MN" },
                new StateProvince { ID = 24, CountryID = 1, Name = "Mississippi", Abbreviation = "MS" },
                new StateProvince { ID = 25, CountryID = 1, Name = "Missouri", Abbreviation = "MO" },
                new StateProvince { ID = 26, CountryID = 1, Name = "Montana", Abbreviation = "MT" },
                new StateProvince { ID = 27, CountryID = 1, Name = "Nebraska", Abbreviation = "NE" },
                new StateProvince { ID = 28, CountryID = 1, Name = "Nevada", Abbreviation = "NV" },
                new StateProvince { ID = 29, CountryID = 1, Name = "New Hampshire", Abbreviation = "NH" },
                new StateProvince { ID = 30, CountryID = 1, Name = "New Jersey", Abbreviation = "NJ" },
                new StateProvince { ID = 31, CountryID = 1, Name = "New Mexico", Abbreviation = "NM" },
                new StateProvince { ID = 32, CountryID = 1, Name = "New York", Abbreviation = "NY" },
                new StateProvince { ID = 33, CountryID = 1, Name = "North Carolina", Abbreviation = "NC" },
                new StateProvince { ID = 34, CountryID = 1, Name = "North Dakota", Abbreviation = "ND" },
                new StateProvince { ID = 35, CountryID = 1, Name = "Ohio", Abbreviation = "OH" },
                new StateProvince { ID = 36, CountryID = 1, Name = "Oklahoma", Abbreviation = "OK" },
                new StateProvince { ID = 37, CountryID = 1, Name = "Oregon", Abbreviation = "OR" },
                new StateProvince { ID = 38, CountryID = 1, Name = "Pennsylvania", Abbreviation = "PA" },
                new StateProvince { ID = 39, CountryID = 1, Name = "Rhode Island", Abbreviation = "RI" },
                new StateProvince { ID = 40, CountryID = 1, Name = "South Carolina", Abbreviation = "SC" },
                new StateProvince { ID = 41, CountryID = 1, Name = "South Dakota", Abbreviation = "SD" },
                new StateProvince { ID = 42, CountryID = 1, Name = "Tennessee", Abbreviation = "TN" },
                new StateProvince { ID = 43, CountryID = 1, Name = "Texas", Abbreviation = "TX" },
                new StateProvince { ID = 44, CountryID = 1, Name = "Utah", Abbreviation = "UT" },
                new StateProvince { ID = 45, CountryID = 1, Name = "Vermont", Abbreviation = "VT" },
                new StateProvince { ID = 46, CountryID = 1, Name = "Virginia", Abbreviation = "VA" },
                new StateProvince { ID = 47, CountryID = 1, Name = "Washington", Abbreviation = "WA" },
                new StateProvince { ID = 48, CountryID = 1, Name = "West Virginia", Abbreviation = "WV" },
                new StateProvince { ID = 49, CountryID = 1, Name = "Wisconsin", Abbreviation = "WI" },
                new StateProvince { ID = 50, CountryID = 1, Name = "Wyoming", Abbreviation = "WY" },
                new StateProvince { ID = 51, CountryID = 1, Name = "District of Columbia", Abbreviation = "DC" }
            });

            // United States (CountryId = 1) - Territories
            states.AddRange(new List<StateProvince>
            {
                new StateProvince { ID = 69, CountryID = 1, Name = "Puerto Rico", Abbreviation = "PR" },
                new StateProvince { ID = 70, CountryID = 1, Name = "Guam", Abbreviation = "GU" },
                new StateProvince { ID = 71, CountryID = 1, Name = "U.S. Virgin Islands", Abbreviation = "VI" },
                new StateProvince { ID = 72, CountryID = 1, Name = "American Samoa", Abbreviation = "AS" },
                new StateProvince { ID = 73, CountryID = 1, Name = "Northern Mariana Islands", Abbreviation = "MP" }
            });

            // Canada (CountryId = 2)
            states.AddRange(new List<StateProvince>
            {
                new StateProvince { ID = 52, CountryID = 2, Name = "Alberta", Abbreviation = "AB" },
                new StateProvince { ID = 53, CountryID = 2, Name = "British Columbia", Abbreviation = "BC" },
                new StateProvince { ID = 54, CountryID = 2, Name = "Manitoba", Abbreviation = "MB" },
                new StateProvince { ID = 55, CountryID = 2, Name = "New Brunswick", Abbreviation = "NB" },
                new StateProvince { ID = 56, CountryID = 2, Name = "Newfoundland and Labrador", Abbreviation = "NL" },
                new StateProvince { ID = 57, CountryID = 2, Name = "Nova Scotia", Abbreviation = "NS" },
                new StateProvince { ID = 58, CountryID = 2, Name = "Ontario", Abbreviation = "ON" },
                new StateProvince { ID = 59, CountryID = 2, Name = "Prince Edward Island", Abbreviation = "PE" },
                new StateProvince { ID = 60, CountryID = 2, Name = "Quebec", Abbreviation = "QC" },
                new StateProvince { ID = 61, CountryID = 2, Name = "Saskatchewan", Abbreviation = "SK" },
                new StateProvince { ID = 62, CountryID = 2, Name = "Northwest Territories", Abbreviation = "NT" },
                new StateProvince { ID = 63, CountryID = 2, Name = "Nunavut", Abbreviation = "NU" },
                new StateProvince { ID = 64, CountryID = 2, Name = "Yukon", Abbreviation = "YT" }
            });

            // United Kingdom (CountryId = 3) - Constituent Countries
            states.AddRange(new List<StateProvince>
            {
                new StateProvince { ID = 65, CountryID = 3, Name = "England", Abbreviation = "ENG" },
                new StateProvince { ID = 66, CountryID = 3, Name = "Scotland", Abbreviation = "SCT" },
                new StateProvince { ID = 67, CountryID = 3, Name = "Wales", Abbreviation = "WLS" },
                new StateProvince { ID = 68, CountryID = 3, Name = "Northern Ireland", Abbreviation = "NIR" }
            });

            // United Kingdom (CountryId = 3) - Crown Dependencies & Overseas Territories
            states.AddRange(new List<StateProvince>
            {
                new StateProvince { ID = 74, CountryID = 3, Name = "Isle of Man", Abbreviation = "IM" },
                new StateProvince { ID = 75, CountryID = 3, Name = "Jersey", Abbreviation = "JE" },
                new StateProvince { ID = 76, CountryID = 3, Name = "Guernsey", Abbreviation = "GG" },
                new StateProvince { ID = 77, CountryID = 3, Name = "Gibraltar", Abbreviation = "GI" },
                new StateProvince { ID = 78, CountryID = 3, Name = "Bermuda", Abbreviation = "BM" }
            });
            return states;
        }
        public static List<StateProvince> GetStateProvincesByCountryId(int country)
        {
            var allStates = GetStateProvinces();
            return allStates.Where(sp => sp.CountryID == country).ToList();
        }
    }
}