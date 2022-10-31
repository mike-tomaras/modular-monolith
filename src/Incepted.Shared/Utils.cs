using Optional;
using Optional.Collections;
using System.Collections.Immutable;
using System.Text.Json;

namespace Incepted.Shared;

public static class Utils
{
    public static T? DeepClone<T>(this T source)
    {
        var serialized = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(serialized);
    }

    public static IImmutableList<T> ToImmutable<T>(this IEnumerable<T> list)
    {
        return ImmutableList.CreateRange(list);
    }
    public static IImmutableList<T> ToImmutable<T>(this T[] list)
    {
        return ImmutableList.CreateRange(list);
    }
    public static IImmutableList<T> Clone<T>(this IImmutableList<T> list)
    {
        return ImmutableList.CreateRange(list);
    }
    public static IImmutableList<T> Replace<T>(this IImmutableList<T> list, Func<T, bool> predicate, T replacement)
    {
        var itemToReplace = list.SingleOrDefault(predicate);

        if (itemToReplace == null) 
            throw new ArgumentNullException(nameof(itemToReplace), "No item to replace in the list was found.");
        
        return list.Replace(itemToReplace, replacement);
    }
    public static void ReplaceInList<T>(this IList<T> list, Func<T, bool> predicate, T replacement)
    {
        var itemToReplace = list.Single(predicate);
        var index = list.IndexOf(itemToReplace);
        list[index] = replacement;
    }

    public static IImmutableList<T> Remove<T>(this IImmutableList<T> list, Func<T, bool> predicate)
    {
        var itemToRemove = list.SingleOrDefault(predicate);

        if (itemToRemove == null)
            throw new ArgumentNullException(nameof(itemToRemove), "No item to remove in the list was found.");

        return list.Remove(itemToRemove);
    }

    public static Option<T, ErrorCode> GetLastErrorOrLastValue<T>(this IEnumerable<Option<T, ErrorCode>> optionals)
    {
        var exceptions = optionals.Exceptions();
        if (exceptions.Any()) return Option.None<T, ErrorCode>(exceptions.Last());

        var values = optionals.Values();
        if (!values.Any()) return Option.Some<T, ErrorCode>(default);

        return Option.Some<T, ErrorCode>(values.Last());
    }

    public static string FileExtension(this string fileName) => fileName.Substring(fileName.LastIndexOf('.')).ToLower();

    public static IImmutableList<string> Countries = new List<string>
    {        
        "Afghanistan",
        "Albania",
        "Algeria",
        "Andorra",
        "Angola",
        "Antigua and Barbuda",
        "Argentina",
        "Armenia",
        "Australia",
        "Austria",
        "Azerbaijan",
        "Bahamas",
        "Bahrain",
        "Bangladesh",
        "Barbados",
        "Belarus",
        "Belgium",
        "Belize",
        "Benin",
        "Bhutan",
        "Bolivia",
        "Bosnia and Herzegovina",
        "Botswana",
        "Brazil",
        "Brunei",
        "Bulgaria",
        "Burkina Faso",
        "Burundi",
        "Cabo Verde",
        "Cambodia",
        "Cameroon",
        "Canada",
        "Central African Republic",
        "Chad",
        "Chile",
        "China",
        "Colombia",
        "Comoros",
        "Congo",
        "Costa Rica",
        "Croatia",
        "Cuba",
        "Cyprus",
        "Czech Republic (Czechia)",
        "Côte d'Ivoire",
        "Denmark",
        "Djibouti",
        "Dominica",
        "Dominican Republic",
        "DR Congo",
        "Ecuador",
        "Egypt",
        "El Salvador",
        "Equatorial Guinea",
        "Eritrea",
        "Estonia",
        "Eswatini",
        "Ethiopia",
        "Fiji",
        "Finland",
        "France",
        "Gabon",
        "Gambia",
        "Georgia",
        "Germany",
        "Ghana",
        "Greece",
        "Grenada",
        "Guatemala",
        "Guinea",
        "Guinea-Bissau",
        "Guyana",
        "Haiti",
        "Holy See",
        "Honduras",
        "Hungary",
        "Iceland",
        "India",
        "Indonesia",
        "Iran",
        "Iraq",
        "Ireland",
        "Israel",
        "Italy",
        "Jamaica",
        "Japan",
        "Jordan",
        "Kazakhstan",
        "Kenya",
        "Kiribati",
        "Kuwait",
        "Kyrgyzstan",
        "Laos",
        "Latvia",
        "Lebanon",
        "Lesotho",
        "Liberia",
        "Libya",
        "Liechtenstein",
        "Lithuania",
        "Luxembourg",
        "Madagascar",
        "Malawi",
        "Malaysia",
        "Maldives",
        "Mali",
        "Malta",
        "Marshall Islands",
        "Mauritania",
        "Mauritius",
        "Mexico",
        "Micronesia",
        "Moldova",
        "Monaco",
        "Mongolia",
        "Montenegro",
        "Morocco",
        "Mozambique",
        "Myanmar",
        "Namibia",
        "Nauru",
        "Nepal",
        "Netherlands",
        "New Zealand",
        "Nicaragua",
        "Niger",
        "Nigeria",
        "North Korea",
        "North Macedonia",
        "Norway",
        "Oman",
        "Pakistan",
        "Palau",
        "Panama",
        "Papua New Guinea",
        "Paraguay",
        "Peru",
        "Philippines",
        "Poland",
        "Portugal",
        "Qatar",
        "Romania",
        "Russia",
        "Rwanda",
        "Saint Kitts & Nevis",
        "Saint Lucia",
        "Samoa",
        "San Marino",
        "Sao Tome & Principe",
        "Saudi Arabia",
        "Senegal",
        "Serbia",
        "Seychelles",
        "Sierra Leone",
        "Singapore",
        "Slovakia",
        "Slovenia",
        "Solomon Islands",
        "Somalia",
        "South Africa",
        "South Korea",
        "South Sudan",
        "Spain",
        "Sri Lanka",
        "St. Vincent & Grenadines",
        "State of Palestine",
        "Sudan",
        "Suriname",
        "Sweden",
        "Switzerland",
        "Syria",
        "Tajikistan",
        "Tanzania",
        "Thailand",
        "Timor-Leste",
        "Togo",
        "Tonga",
        "Trinidad and Tobago",
        "Tunisia",
        "Turkey",
        "Turkmenistan",
        "Tuvalu",
        "Uganda",
        "Ukraine",
        "United Arab Emirates",
        "United Kingdom",
        "United States",
        "Uruguay",
        "Uzbekistan",
        "Vanuatu",
        "Venezuela",
        "Vietnam",
        "Yemen",
        "Zambia",
        "Zimbabwe"
    }.ToImmutable();
}
