using System.Collections.Generic;
using UnityEngine;

public static class CountryManager
{
    private static List<CountryObject> countries = new List<CountryObject>();
    private static bool initialized = false;

    // Helper compact : (name, population, temp°C, wealth 0-1, hygiene 0-1, healthcare 0-1, climate)
    private static void A(string n, int p, float t, float w, float h, float d, string c)
        => countries.Add(new CountryObject(n, p, t, w, h, d, c));

    public static void Initialize()
    {
        if (initialized) return;
        countries.Clear();

        // ── Europe ────────────────────────────────────────────
        A("France",               67000000,  12f, 0.85f, 0.88f, 0.88f, "Temperate");
        A("Germany",              83000000,   9f, 0.88f, 0.90f, 0.92f, "Temperate");
        A("Italy",                58000000,  15f, 0.80f, 0.82f, 0.82f, "Mediterranean");
        A("Spain",                47000000,  14f, 0.78f, 0.83f, 0.82f, "Mediterranean");
        A("United Kingdom",       67000000,  10f, 0.84f, 0.86f, 0.88f, "Temperate");
        A("Poland",               38000000,   8f, 0.68f, 0.78f, 0.74f, "Continental");
        A("Netherlands",          17000000,  10f, 0.88f, 0.92f, 0.90f, "Temperate");
        A("Belgium",              11000000,  10f, 0.85f, 0.88f, 0.86f, "Temperate");
        A("Sweden",               10000000,   3f, 0.88f, 0.93f, 0.91f, "Continental");
        A("Norway",                5400000,   1f, 0.95f, 0.94f, 0.92f, "Continental");
        A("Denmark",               5800000,   8f, 0.90f, 0.93f, 0.91f, "Temperate");
        A("Finland",               5500000,   2f, 0.87f, 0.93f, 0.90f, "Continental");
        A("Switzerland",           8700000,   6f, 0.94f, 0.94f, 0.94f, "Temperate");
        A("Austria",               9000000,   7f, 0.84f, 0.88f, 0.87f, "Temperate");
        A("Portugal",             10000000,  16f, 0.74f, 0.80f, 0.78f, "Mediterranean");
        A("Czechia",              10700000,   8f, 0.74f, 0.82f, 0.78f, "Continental");
        A("Hungary",              10000000,  10f, 0.66f, 0.78f, 0.74f, "Continental");
        A("Romania",              19000000,   9f, 0.56f, 0.72f, 0.66f, "Continental");
        A("Ukraine",              44000000,   9f, 0.38f, 0.68f, 0.56f, "Continental");
        A("Greece",               10700000,  18f, 0.65f, 0.78f, 0.74f, "Mediterranean");
        A("Serbia",                7000000,  11f, 0.48f, 0.72f, 0.68f, "Continental");
        A("Croatia",               4000000,  13f, 0.60f, 0.78f, 0.74f, "Mediterranean");
        A("Slovakia",              5500000,   8f, 0.62f, 0.78f, 0.74f, "Continental");
        A("Bulgaria",              7000000,  11f, 0.52f, 0.72f, 0.66f, "Continental");
        A("Belarus",               9400000,   7f, 0.40f, 0.72f, 0.66f, "Continental");
        A("Albania",               2900000,  15f, 0.34f, 0.68f, 0.60f, "Mediterranean");
        A("Bosnia and Herz.",      3300000,  10f, 0.42f, 0.68f, 0.62f, "Continental");
        A("North Macedonia",       2100000,  12f, 0.38f, 0.68f, 0.60f, "Continental");
        A("Montenegro",             620000,  15f, 0.54f, 0.74f, 0.66f, "Mediterranean");
        A("Moldova",               2600000,  10f, 0.30f, 0.65f, 0.56f, "Continental");
        A("Lithuania",             2800000,   7f, 0.70f, 0.82f, 0.78f, "Continental");
        A("Latvia",                1800000,   6f, 0.66f, 0.80f, 0.76f, "Continental");
        A("Estonia",               1300000,   5f, 0.70f, 0.82f, 0.80f, "Continental");
        A("Luxembourg",             640000,   9f, 0.94f, 0.92f, 0.92f, "Temperate");
        A("Malta",                  500000,  19f, 0.72f, 0.82f, 0.78f, "Mediterranean");
        A("Cyprus",                1200000,  20f, 0.68f, 0.80f, 0.76f, "Mediterranean");
        A("Iceland",                370000,   2f, 0.90f, 0.93f, 0.91f, "Temperate");
        A("Ireland",               5000000,  10f, 0.86f, 0.88f, 0.88f, "Temperate");
        A("Kosovo",                1800000,  12f, 0.30f, 0.64f, 0.56f, "Continental");
        A("Slovenia",              2100000,  10f, 0.72f, 0.84f, 0.82f, "Temperate");
        A("San Marino",              34000,  14f, 0.88f, 0.90f, 0.88f, "Mediterranean");
        A("Monaco",                  39000,  16f, 0.98f, 0.96f, 0.95f, "Mediterranean");
        A("Andorra",                 77000,   5f, 0.84f, 0.88f, 0.86f, "Temperate");
        A("Liechtenstein",           38000,   8f, 0.96f, 0.94f, 0.92f, "Temperate");
        A("N. Cyprus",              350000,  20f, 0.52f, 0.74f, 0.68f, "Mediterranean");
        A("Russia",              144000000,  -5f, 0.52f, 0.66f, 0.62f, "Continental");
        A("Åland",                   30000,   5f, 0.88f, 0.92f, 0.88f, "Temperate");
        A("Greenland",               56000, -10f, 0.70f, 0.82f, 0.78f, "Polar");
        A("Faeroe Is.",              55000,   5f, 0.82f, 0.88f, 0.84f, "Temperate");
        A("Guernsey",                63000,  11f, 0.84f, 0.88f, 0.86f, "Temperate");
        A("Jersey",                 100000,  12f, 0.86f, 0.88f, 0.86f, "Temperate");
        A("Isle of Man",             85000,  10f, 0.80f, 0.86f, 0.84f, "Temperate");

        // ── Asie ──────────────────────────────────────────────
        A("China",              1400000000,   8f, 0.52f, 0.62f, 0.64f, "Continental");
        A("India",              1380000000,  24f, 0.28f, 0.48f, 0.50f, "Tropical");
        A("Japan",               126000000,  14f, 0.82f, 0.86f, 0.90f, "Temperate");
        A("South Korea",          52000000,  12f, 0.78f, 0.84f, 0.86f, "Temperate");
        A("Indonesia",           273000000,  26f, 0.34f, 0.52f, 0.52f, "Tropical");
        A("Pakistan",            220000000,  22f, 0.22f, 0.42f, 0.42f, "Arid");
        A("Bangladesh",          166000000,  26f, 0.20f, 0.44f, 0.40f, "Tropical");
        A("Vietnam",              97000000,  24f, 0.32f, 0.56f, 0.52f, "Tropical");
        A("Thailand",             70000000,  27f, 0.40f, 0.60f, 0.62f, "Tropical");
        A("Myanmar",              54000000,  25f, 0.22f, 0.46f, 0.42f, "Tropical");
        A("Malaysia",             32000000,  27f, 0.46f, 0.64f, 0.64f, "Tropical");
        A("Philippines",         109000000,  26f, 0.30f, 0.54f, 0.52f, "Tropical");
        A("Uzbekistan",           34000000,  14f, 0.26f, 0.56f, 0.50f, "Continental");
        A("Kazakhstan",           19000000,   8f, 0.44f, 0.64f, 0.60f, "Continental");
        A("Afghanistan",          39000000,  12f, 0.12f, 0.32f, 0.28f, "Arid");
        A("Tajikistan",            9500000,  10f, 0.16f, 0.48f, 0.42f, "Continental");
        A("Kyrgyzstan",            6600000,   4f, 0.18f, 0.52f, 0.46f, "Continental");
        A("Turkmenistan",          6000000,  17f, 0.36f, 0.58f, 0.52f, "Arid");
        A("Azerbaijan",           10000000,  14f, 0.44f, 0.66f, 0.62f, "Continental");
        A("Armenia",               3000000,   8f, 0.36f, 0.66f, 0.62f, "Continental");
        A("Georgia",               3700000,  12f, 0.38f, 0.66f, 0.62f, "Temperate");
        A("Mongolia",              3300000,  -2f, 0.30f, 0.56f, 0.52f, "Continental");
        A("Singapore",             5700000,  27f, 0.92f, 0.96f, 0.94f, "Tropical");
        A("Cambodia",             16000000,  27f, 0.22f, 0.50f, 0.44f, "Tropical");
        A("Laos",                  7300000,  26f, 0.22f, 0.50f, 0.44f, "Tropical");
        A("Nepal",                29000000,  15f, 0.18f, 0.48f, 0.44f, "Tropical");
        A("Sri Lanka",            21000000,  28f, 0.32f, 0.58f, 0.58f, "Tropical");
        A("North Korea",          25000000,  10f, 0.18f, 0.40f, 0.36f, "Continental");
        A("Taiwan",               23000000,  22f, 0.78f, 0.86f, 0.86f, "Subtropical");
        A("Hong Kong",             7500000,  23f, 0.90f, 0.92f, 0.92f, "Subtropical");
        A("Macao",                  650000,  23f, 0.86f, 0.90f, 0.88f, "Subtropical");
        A("Bhutan",                 800000,  10f, 0.30f, 0.62f, 0.56f, "Tropical");
        A("Timor-Leste",           1300000,  26f, 0.18f, 0.44f, 0.38f, "Tropical");
        A("Brunei",                 440000,  28f, 0.74f, 0.78f, 0.76f, "Tropical");
        A("Maldives",               540000,  29f, 0.52f, 0.70f, 0.66f, "Tropical");
        // Moyen-Orient
        A("Saudi Arabia",         35000000,  25f, 0.82f, 0.76f, 0.78f, "Arid");
        A("Turkey",               84000000,  12f, 0.56f, 0.72f, 0.68f, "Mediterranean");
        A("Iran",                 84000000,  17f, 0.40f, 0.64f, 0.60f, "Arid");
        A("Iraq",                 40000000,  22f, 0.36f, 0.54f, 0.52f, "Arid");
        A("Israel",                9000000,  20f, 0.80f, 0.82f, 0.86f, "Mediterranean");
        A("Jordan",               10000000,  18f, 0.48f, 0.68f, 0.64f, "Arid");
        A("Lebanon",               6800000,  20f, 0.46f, 0.68f, 0.64f, "Mediterranean");
        A("Syria",                17500000,  17f, 0.22f, 0.48f, 0.42f, "Mediterranean");
        A("Yemen",                33000000,  25f, 0.14f, 0.38f, 0.32f, "Arid");
        A("Kuwait",                4300000,  26f, 0.82f, 0.78f, 0.80f, "Arid");
        A("United Arab Emirates",  9900000,  28f, 0.88f, 0.82f, 0.84f, "Arid");
        A("Qatar",                 2800000,  28f, 0.90f, 0.82f, 0.84f, "Arid");
        A("Oman",                  4600000,  26f, 0.70f, 0.74f, 0.72f, "Arid");
        A("Bahrain",               1700000,  27f, 0.82f, 0.78f, 0.80f, "Arid");
        A("Palestine",             5000000,  20f, 0.28f, 0.58f, 0.52f, "Mediterranean");

        // ── Afrique ───────────────────────────────────────────
        A("Nigeria",             211000000,  26f, 0.26f, 0.44f, 0.42f, "Tropical");
        A("Ethiopia",            115000000,  20f, 0.14f, 0.36f, 0.34f, "Tropical");
        A("Dem. Rep. Congo",      90000000,  24f, 0.10f, 0.32f, 0.28f, "Tropical");
        A("Tanzania",             61000000,  22f, 0.16f, 0.44f, 0.40f, "Tropical");
        A("South Africa",         60000000,  18f, 0.40f, 0.56f, 0.60f, "Subtropical");
        A("Kenya",                54000000,  22f, 0.20f, 0.46f, 0.44f, "Tropical");
        A("Algeria",              44000000,  15f, 0.34f, 0.58f, 0.54f, "Arid");
        A("Sudan",                44000000,  25f, 0.14f, 0.36f, 0.32f, "Arid");
        A("Uganda",               46000000,  22f, 0.16f, 0.44f, 0.40f, "Tropical");
        A("Morocco",              37000000,  18f, 0.38f, 0.60f, 0.56f, "Mediterranean");
        A("Angola",               33000000,  24f, 0.20f, 0.42f, 0.38f, "Tropical");
        A("Mozambique",           32000000,  24f, 0.12f, 0.38f, 0.34f, "Tropical");
        A("Ghana",                32000000,  26f, 0.24f, 0.48f, 0.46f, "Tropical");
        A("Madagascar",           27000000,  24f, 0.12f, 0.40f, 0.36f, "Tropical");
        A("Cameroon",             27000000,  24f, 0.18f, 0.42f, 0.40f, "Tropical");
        A("Côte d'Ivoire",        26000000,  26f, 0.22f, 0.44f, 0.40f, "Tropical");
        A("Niger",                24000000,  28f, 0.10f, 0.32f, 0.28f, "Arid");
        A("Mali",                 20000000,  28f, 0.10f, 0.32f, 0.28f, "Arid");
        A("Malawi",               19000000,  22f, 0.12f, 0.40f, 0.36f, "Tropical");
        A("Burkina Faso",         21000000,  28f, 0.12f, 0.34f, 0.30f, "Arid");
        A("Zambia",               18000000,  21f, 0.16f, 0.42f, 0.38f, "Tropical");
        A("Senegal",              17000000,  28f, 0.22f, 0.46f, 0.42f, "Tropical");
        A("Chad",                 16000000,  28f, 0.10f, 0.30f, 0.26f, "Arid");
        A("Somalia",              16000000,  28f, 0.08f, 0.28f, 0.24f, "Arid");
        A("Zimbabwe",             15000000,  22f, 0.16f, 0.46f, 0.42f, "Tropical");
        A("Guinea",               13000000,  26f, 0.12f, 0.36f, 0.32f, "Tropical");
        A("Rwanda",               13000000,  18f, 0.18f, 0.48f, 0.46f, "Tropical");
        A("Burundi",              12000000,  20f, 0.12f, 0.40f, 0.36f, "Tropical");
        A("Benin",                12000000,  27f, 0.16f, 0.40f, 0.36f, "Tropical");
        A("Tunisia",              12000000,  18f, 0.42f, 0.62f, 0.58f, "Mediterranean");
        A("S. Sudan",             11000000,  26f, 0.10f, 0.30f, 0.24f, "Tropical");
        A("Egypt",               102000000,  21f, 0.44f, 0.58f, 0.56f, "Arid");
        A("Togo",                  8300000,  27f, 0.16f, 0.40f, 0.36f, "Tropical");
        A("Sierra Leone",          8000000,  26f, 0.12f, 0.36f, 0.30f, "Tropical");
        A("Libya",                 7000000,  22f, 0.38f, 0.54f, 0.50f, "Arid");
        A("Liberia",               5000000,  26f, 0.14f, 0.38f, 0.32f, "Tropical");
        A("Central African Rep.",  5000000,  26f, 0.08f, 0.28f, 0.22f, "Tropical");
        A("Congo",                 5500000,  24f, 0.18f, 0.44f, 0.38f, "Tropical");
        A("Eritrea",               3500000,  26f, 0.12f, 0.36f, 0.32f, "Arid");
        A("Mauritania",            4600000,  30f, 0.14f, 0.38f, 0.34f, "Arid");
        A("Gabon",                 2200000,  25f, 0.38f, 0.54f, 0.52f, "Tropical");
        A("Gambia",                2400000,  28f, 0.14f, 0.38f, 0.34f, "Tropical");
        A("Botswana",              2600000,  21f, 0.36f, 0.56f, 0.60f, "Arid");
        A("Namibia",               2600000,  20f, 0.30f, 0.54f, 0.56f, "Arid");
        A("Guinea-Bissau",         2000000,  26f, 0.10f, 0.34f, 0.28f, "Tropical");
        A("Lesotho",               2100000,  15f, 0.16f, 0.46f, 0.40f, "Temperate");
        A("Eq. Guinea",            1400000,  25f, 0.34f, 0.46f, 0.44f, "Tropical");
        A("Mauritius",             1300000,  24f, 0.48f, 0.70f, 0.70f, "Tropical");
        A("Djibouti",              1000000,  29f, 0.28f, 0.46f, 0.44f, "Arid");
        A("eSwatini",              1200000,  20f, 0.22f, 0.50f, 0.46f, "Subtropical");
        A("Cabo Verde",             560000,  24f, 0.36f, 0.66f, 0.60f, "Tropical");
        A("Comoros",                870000,  26f, 0.20f, 0.46f, 0.40f, "Tropical");
        A("Seychelles",              98000,  27f, 0.58f, 0.78f, 0.74f, "Tropical");
        A("São Tomé and Principe",  220000,  26f, 0.20f, 0.50f, 0.44f, "Tropical");
        A("Somaliland",            4000000,  28f, 0.08f, 0.28f, 0.22f, "Arid");
        A("W. Sahara",              600000,  22f, 0.12f, 0.36f, 0.30f, "Arid");
        A("Saint Helena",             6000,  18f, 0.60f, 0.74f, 0.70f, "Tropical");

        // ── Amériques du Nord & Caraïbes ──────────────────────
        A("United States of America", 331000000, 12f, 0.88f, 0.88f, 0.92f, "Temperate");
        A("Canada",               38000000,   2f, 0.88f, 0.90f, 0.90f, "Continental");
        A("Mexico",              130000000,  20f, 0.46f, 0.60f, 0.60f, "Subtropical");
        A("Guatemala",            17000000,  20f, 0.28f, 0.50f, 0.46f, "Tropical");
        A("Honduras",             10000000,  22f, 0.24f, 0.48f, 0.44f, "Tropical");
        A("El Salvador",           6500000,  24f, 0.30f, 0.52f, 0.48f, "Tropical");
        A("Nicaragua",             6600000,  25f, 0.22f, 0.48f, 0.44f, "Tropical");
        A("Costa Rica",            5100000,  22f, 0.44f, 0.70f, 0.68f, "Tropical");
        A("Panama",                4300000,  26f, 0.46f, 0.66f, 0.64f, "Tropical");
        A("Cuba",                 11000000,  25f, 0.28f, 0.66f, 0.72f, "Tropical");
        A("Haiti",                11000000,  26f, 0.14f, 0.38f, 0.28f, "Tropical");
        A("Dominican Rep.",       11000000,  25f, 0.32f, 0.58f, 0.56f, "Tropical");
        A("Jamaica",               3000000,  26f, 0.32f, 0.60f, 0.58f, "Tropical");
        A("Trinidad and Tobago",   1400000,  26f, 0.48f, 0.70f, 0.68f, "Tropical");
        A("Bahamas",                390000,  26f, 0.60f, 0.76f, 0.72f, "Tropical");
        A("Barbados",               287000,  27f, 0.46f, 0.74f, 0.72f, "Tropical");
        A("Belize",                 400000,  26f, 0.34f, 0.58f, 0.54f, "Tropical");
        A("Grenada",                113000,  27f, 0.42f, 0.68f, 0.64f, "Tropical");
        A("Saint Lucia",            183000,  27f, 0.40f, 0.68f, 0.64f, "Tropical");
        A("St. Vin. and Gren.",     110000,  27f, 0.38f, 0.66f, 0.62f, "Tropical");
        A("Dominica",                72000,  26f, 0.34f, 0.64f, 0.60f, "Tropical");
        A("St. Kitts and Nevis",     53000,  27f, 0.50f, 0.72f, 0.68f, "Tropical");
        A("Antigua and Barb.",       98000,  27f, 0.46f, 0.70f, 0.66f, "Tropical");
        A("Puerto Rico",           3200000,  25f, 0.54f, 0.74f, 0.76f, "Tropical");
        A("U.S. Virgin Is.",        104000,  26f, 0.56f, 0.74f, 0.72f, "Tropical");
        A("British Virgin Is.",      30000,  26f, 0.52f, 0.72f, 0.70f, "Tropical");
        A("Aruba",                  107000,  28f, 0.58f, 0.76f, 0.72f, "Tropical");
        A("Curaçao",                160000,  28f, 0.52f, 0.72f, 0.68f, "Tropical");
        A("Sint Maarten",            42000,  27f, 0.54f, 0.72f, 0.68f, "Tropical");
        A("St-Barthélemy",            10000,  27f, 0.82f, 0.82f, 0.80f, "Tropical");
        A("St-Martin",               36000,  27f, 0.68f, 0.76f, 0.72f, "Tropical");
        A("Turks and Caicos Is.",    38000,  27f, 0.60f, 0.74f, 0.72f, "Tropical");
        A("Cayman Is.",              65000,  27f, 0.74f, 0.80f, 0.78f, "Tropical");
        A("Bermuda",                 64000,  21f, 0.80f, 0.84f, 0.82f, "Subtropical");
        A("Montserrat",               5000,  27f, 0.42f, 0.68f, 0.64f, "Tropical");
        A("Anguilla",                18000,  27f, 0.56f, 0.74f, 0.70f, "Tropical");
        A("St. Pierre and Miquelon",  5700,   4f, 0.70f, 0.82f, 0.80f, "Temperate");

        // ── Amériques du Sud ──────────────────────────────────
        A("Brazil",              212000000,  25f, 0.40f, 0.56f, 0.58f, "Tropical");
        A("Argentina",            45000000,  14f, 0.46f, 0.66f, 0.68f, "Temperate");
        A("Colombia",             51000000,  22f, 0.38f, 0.60f, 0.60f, "Tropical");
        A("Chile",                19000000,  10f, 0.54f, 0.74f, 0.74f, "Temperate");
        A("Peru",                 33000000,  18f, 0.34f, 0.56f, 0.56f, "Tropical");
        A("Venezuela",            28000000,  26f, 0.28f, 0.52f, 0.44f, "Tropical");
        A("Ecuador",              17000000,  22f, 0.36f, 0.58f, 0.58f, "Tropical");
        A("Bolivia",              12000000,  15f, 0.26f, 0.50f, 0.48f, "Tropical");
        A("Paraguay",              7000000,  22f, 0.28f, 0.52f, 0.50f, "Subtropical");
        A("Uruguay",               3500000,  17f, 0.56f, 0.74f, 0.72f, "Temperate");
        A("Guyana",                 788000,  26f, 0.28f, 0.56f, 0.52f, "Tropical");
        A("Suriname",               590000,  26f, 0.36f, 0.60f, 0.56f, "Tropical");
        A("Falkland Is.",             3500,   6f, 0.60f, 0.76f, 0.70f, "Temperate");
        A("Fr. Polynesia",          280000,  26f, 0.62f, 0.76f, 0.72f, "Tropical");

        // ── Océanie ───────────────────────────────────────────
        A("Australia",            26000000,  20f, 0.86f, 0.90f, 0.90f, "Temperate");
        A("New Zealand",           5000000,  12f, 0.86f, 0.90f, 0.90f, "Temperate");
        A("Papua New Guinea",      9000000,  26f, 0.20f, 0.44f, 0.38f, "Tropical");
        A("Fiji",                   930000,  25f, 0.34f, 0.62f, 0.58f, "Tropical");
        A("Vanuatu",                320000,  24f, 0.22f, 0.54f, 0.48f, "Tropical");
        A("Solomon Is.",            690000,  26f, 0.18f, 0.50f, 0.44f, "Tropical");
        A("Samoa",                  200000,  27f, 0.24f, 0.58f, 0.52f, "Tropical");
        A("Tonga",                  100000,  25f, 0.24f, 0.58f, 0.52f, "Tropical");
        A("Kiribati",               119000,  28f, 0.16f, 0.48f, 0.42f, "Tropical");
        A("Marshall Is.",            42000,  28f, 0.18f, 0.50f, 0.44f, "Tropical");
        A("Micronesia",             115000,  27f, 0.22f, 0.52f, 0.46f, "Tropical");
        A("Palau",                   18000,  28f, 0.30f, 0.60f, 0.56f, "Tropical");
        A("Nauru",                   10800,  28f, 0.22f, 0.52f, 0.48f, "Tropical");
        A("Tuvalu",                  11000,  29f, 0.18f, 0.50f, 0.44f, "Tropical");
        A("Cook Is.",                17000,  24f, 0.32f, 0.62f, 0.56f, "Tropical");
        A("New Caledonia",          270000,  23f, 0.66f, 0.78f, 0.74f, "Tropical");
        A("N. Mariana Is.",          57000,  27f, 0.52f, 0.70f, 0.66f, "Tropical");
        A("Guam",                   163000,  28f, 0.56f, 0.74f, 0.70f, "Tropical");
        A("American Samoa",          55000,  27f, 0.50f, 0.68f, 0.64f, "Tropical");
        A("Niue",                     1600,  24f, 0.28f, 0.60f, 0.54f, "Tropical");
        A("Norfolk Island",           1800,  18f, 0.52f, 0.72f, 0.68f, "Subtropical");
        A("Wallis and Futuna Is.",   11600,  26f, 0.26f, 0.58f, 0.52f, "Tropical");
        A("Pitcairn Is.",               50,  20f, 0.34f, 0.64f, 0.60f, "Temperate");

        // ── Territoires spéciaux ──────────────────────────────
        A("Antarctica",            1000, -40f, 0.70f, 0.80f, 0.90f, "Polar");
        A("Vatican",                800,  16f, 0.90f, 0.92f, 0.90f, "Mediterranean");
        A("Ashmore and Cartier Is.",  10,  26f, 0.70f, 0.80f, 0.80f, "Tropical");
        A("Br. Indian Ocean Ter.",  3500,  28f, 0.64f, 0.76f, 0.74f, "Tropical");
        A("Fr. S. Antarctic Lands",  200, -10f, 0.68f, 0.78f, 0.80f, "Polar");
        A("Heard I. and McDonald Is.", 10,  2f, 0.68f, 0.78f, 0.80f, "Polar");
        A("Indian Ocean Ter.",        10,  20f, 0.70f, 0.80f, 0.80f, "Tropical");
        A("S. Geo. and the Is.",    300,   2f, 0.60f, 0.72f, 0.70f, "Temperate");
        A("Siachen Glacier",          10,  -5f, 0.20f, 0.40f, 0.40f, "Polar");

        initialized = true;
        Debug.Log($"[CountryManager] {countries.Count} pays initialisés.");
    }

    public static CountryObject GetCountry(string name)
    {
        // Auto-init si pas encore fait
        if (!initialized)
            Initialize();
        
        foreach (CountryObject country in countries)
        {
            if (country.name == name)
                return country;
        }
        Debug.LogWarning($"Pays '{name}' non trouvé dans CountryManager");
        return null;
    }

    public static List<CountryObject> GetAllCountries()
    {
        return new List<CountryObject>(countries);  // Retourne une copie pour éviter modifications
    }

    public static void AddCountry(string name, int population, float temperature = 15f)
    {
        if (GetCountry(name) != null)
        {
            Debug.LogWarning($"Le pays '{name}' existe déjà");
            return;
        }
        countries.Add(new CountryObject(name, population, temperature));
        Debug.Log($"Pays '{name}' ajouté au CountryManager");
    }
}
