using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Parser GeoJSON minimal pour Unity.
/// JsonUtility ne gère pas les tableaux de tableaux dynamiques,
/// donc on parse les coordonnées manuellement.
/// 
/// Remplace l'appel JsonUtility.FromJson<GeoJsonRoot> dans GeoJsonLoader
/// par GeoJsonParser.Parse(json).
/// </summary>
public static class GeoJsonParser
{
    /// <summary>
    /// Parse rapide qui extrait uniquement les noms de pays (pour autocomplete).
    /// Beaucoup plus rapide que Parse() car on évite le parsing des coordonnées.
    /// </summary>
    public static List<string> ParseCountryNames(string json)
    {
        var names = new List<string>();
        var namePattern = new Regex(@"""name""\s*:\s*""([^""]+)""");
        var matches = namePattern.Matches(json);
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            string name = m.Groups[1].Value;
            if (seen.Add(name))
                names.Add(name);
        }
        names.Sort(System.StringComparer.OrdinalIgnoreCase);
        return names;
    }

    public static List<CountryData> Parse(string json)
    {
        var countries = new List<CountryData>();

        // Extrait chaque "feature" du FeatureCollection
        var featurePattern = new Regex(@"\{[^{}]*""type""\s*:\s*""Feature""[^{}]*(?:\{[^{}]*\}[^{}]*)*\}", RegexOptions.Singleline);

        // Approche plus robuste : split sur les features manuellement
        var features = SplitFeatures(json);

        foreach (var featureJson in features)
        {
            var data = ParseFeature(featureJson);
            if (data != null)
                countries.Add(data);
        }

        return countries;
    }

    // ─── Découpe le JSON en blocs feature ────────────────────
    static List<string> SplitFeatures(string json)
    {
        var features = new List<string>();
        int depth = 0;
        int start = -1;
        bool inString = false;
        bool escape = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (escape) { escape = false; continue; }
            if (c == '\\' && inString) { escape = true; continue; }
            if (c == '"') { inString = !inString; continue; }
            if (inString) continue;

            if (c == '{')
            {
                if (depth == 1) start = i; // Début d'une feature (depth 1 = dans "features":[...])
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 1 && start != -1)
                {
                    string block = json.Substring(start, i - start + 1);
                    if (block.Contains("\"Feature\""))
                        features.Add(block);
                    start = -1;
                }
            }
        }

        return features;
    }

    // ─── Parse une feature individuelle ──────────────────────
    static CountryData ParseFeature(string featureJson)
    {
        var data = new CountryData();

        // Nom du pays
        data.name = ExtractStringField(featureJson, "name");
        if (string.IsNullOrEmpty(data.name))
            data.name = ExtractStringField(featureJson, "NAME");

        // Code ISO
        data.isoCode = ExtractStringField(featureJson, "iso_a3");

        // Type de géométrie
        string geoType = ExtractStringField(featureJson, "type", afterKey: "geometry");

        // Extrait le bloc "coordinates"
        string coordsJson = ExtractCoordinatesBlock(featureJson);
        if (string.IsNullOrEmpty(coordsJson)) return null;

        if (geoType == "Polygon")
        {
            data.isMultiPolygon = false;
            data.polygon = ParsePolygon(coordsJson);
        }
        else if (geoType == "MultiPolygon")
        {
            data.isMultiPolygon = true;
            data.multiPolygon = ParseMultiPolygon(coordsJson);
        }
        else
        {
            return null; // On ignore Points, LineStrings, etc.
        }

        return data;
    }

    // ─── Extraction de champs simples ─────────────────────────
    static string ExtractStringField(string json, string key, string afterKey = null)
    {
        int searchFrom = 0;
        if (!string.IsNullOrEmpty(afterKey))
        {
            int afterIdx = json.IndexOf($"\"{afterKey}\"");
            if (afterIdx >= 0) searchFrom = afterIdx;
        }

        string pattern = $"\"{key}\"\\s*:\\s*\"([^\"]+)\"";
        var m = Regex.Match(json.Substring(searchFrom), pattern);
        return m.Success ? m.Groups[1].Value : null;
    }

    static string ExtractCoordinatesBlock(string json)
    {
        int coordIdx = json.IndexOf("\"coordinates\"");
        if (coordIdx < 0) return null;

        int bracketStart = json.IndexOf('[', coordIdx);
        if (bracketStart < 0) return null;

        int depth = 0;
        for (int i = bracketStart; i < json.Length; i++)
        {
            if (json[i] == '[') depth++;
            else if (json[i] == ']')
            {
                depth--;
                if (depth == 0)
                    return json.Substring(bracketStart, i - bracketStart + 1);
            }
        }
        return null;
    }

    // ─── Parse Polygon : [ [ [lng,lat], ... ] ] ───────────────
    static List<List<Vector2>> ParsePolygon(string json)
    {
        var rings = new List<List<Vector2>>();
        var ringStrings = SplitTopLevelArrays(json);
        foreach (var ring in ringStrings)
            rings.Add(ParseRing(ring));
        return rings;
    }

    // ─── Parse MultiPolygon : [ [ [ [lng,lat], ... ] ] ] ──────
    static List<List<List<Vector2>>> ParseMultiPolygon(string json)
    {
        var polygons = new List<List<List<Vector2>>>();
        var polyStrings = SplitTopLevelArrays(json);
        foreach (var poly in polyStrings)
            polygons.Add(ParsePolygon(poly));
        return polygons;
    }

    // ─── Parse un ring de coordonnées ─────────────────────────
    static List<Vector2> ParseRing(string json)
    {
        var points = new List<Vector2>();
        var pointPattern = new Regex(@"\[\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)");
        foreach (Match m in pointPattern.Matches(json))
        {
            if (float.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float lng) &&
                float.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float lat))
            {
                points.Add(new Vector2(lng, lat));
            }
        }
        return points;
    }

    // ─── Découpe un tableau JSON en sous-tableaux top-level ───
    static List<string> SplitTopLevelArrays(string json)
    {
        var result = new List<string>();
        int depth = 0;
        int start = -1;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '[')
            {
                if (depth == 1) start = i;
                depth++;
            }
            else if (c == ']')
            {
                depth--;
                if (depth == 1 && start != -1)
                {
                    result.Add(json.Substring(start, i - start + 1));
                    start = -1;
                }
            }
        }
        return result;
    }
}

// ─── Data container ───────────────────────────────────────────
public class CountryData
{
    public string name;
    public string isoCode;
    public bool isMultiPolygon;
    public List<List<Vector2>> polygon;
    public List<List<List<Vector2>>> multiPolygon;
}
