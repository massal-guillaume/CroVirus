using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Charge un fichier GeoJSON et génère les GameObjects pays.
/// Utilise PolygonCollider2D (compatible Physics2D.Raycast pour mobile+PC).
///
/// SETUP :
/// 1. https://geojson-maps.ash.ms → Medium resolution → countries.geojson
/// 2. Placer dans Assets/StreamingAssets/countries.geojson
/// 3. Attacher sur le même GO que WorldMap + MapCameraController
/// </summary>
public class GeoJsonLoader : MonoBehaviour
{
    [Header("Fichier GeoJSON")]
    public string geoJsonFileName = "countries.geojson";

    [Header("Visuel")]
    public Material countryMaterial;   // Unlit/Color (Built-in) ou URP/Unlit
    public float mapScale = 0.01f;     // Échelle de la map (0.01 = degree → Unity unit conversion)

    [Header("Debug")]
    public bool showLogs = true;

    public static GeoJsonLoader Instance { get; private set; }
    public float LoadProgress { get; private set; }
    public bool IsLoaded { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Le chargement ne démarre PAS automatiquement — appeler StartLoading() explicitement
    public void StartLoading() => StartCoroutine(LoadGeoJson());

    IEnumerator LoadGeoJson()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, geoJsonFileName);
        
        // Sur PC/Editor : lire directement le fichier
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"[GeoJsonLoader] Fichier non trouvé : {path}");
                yield break;
            }
            string json = System.IO.File.ReadAllText(path);
            yield return StartCoroutine(ParseAndBuildAsync(json));
            yield break;
        }

        // Sur Android/autres : utiliser UnityWebRequest
        string url = Application.platform == RuntimePlatform.Android ? path : "file:///" + path;

        using var req = UnityEngine.Networking.UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[GeoJsonLoader] Erreur lecture : {req.error}");
            yield break;
        }

        yield return StartCoroutine(ParseAndBuildAsync(req.downloadHandler.text));
    }

    IEnumerator ParseAndBuildAsync(string json)
    {
        List<CountryData> allData = GeoJsonParser.Parse(json);
        int total = Mathf.Max(1, allData.Count);
        int built = 0;

        for (int idx = 0; idx < allData.Count; idx++)
        {
            var data = allData[idx];
            if (!string.IsNullOrEmpty(data.name))
            {
                GameObject go = new GameObject(data.name);
                go.transform.parent = transform;

                Country comp = go.AddComponent<Country>();
                comp.countryName = data.name;
                comp.countryCode = data.isoCode ?? "";

                if (data.isMultiPolygon)
                    BuildMultiPolygon(go, data.multiPolygon);
                else
                    BuildPolygon(go, data.polygon);

                WorldMap.Instance?.RegisterCountry(comp);
                built++;
            }

            LoadProgress = (float)(idx + 1) / total;
            // Yield every 5 pays pour rester fluide
            if (idx % 5 == 0) yield return null;
        }

        LoadProgress = 1f;
        if (showLogs) Debug.Log($"[GeoJsonLoader] {built} pays générés.");

        var cam = Camera.main;
        if (cam != null)
        {
            var camController = cam.GetComponent<MapCameraController>();
            if (camController != null)
                camController.ResetView();
            else
                Debug.LogWarning("[GeoJsonLoader] MapCameraController not found on Main Camera!");
        }
        else
            Debug.LogWarning("[GeoJsonLoader] Main Camera not found!");

        if (showLogs)
        {
            var allCountries = GetComponentsInChildren<MeshRenderer>(false);
            Debug.Log($"[GeoJsonLoader] Total MeshRenderers: {allCountries.Length}");
        }

        IsLoaded = true;
    }

    void BuildPolygon(GameObject parent, List<List<Vector2>> rings)
    {
        if (rings == null || rings.Count == 0) return;
        var outerRing = ScaleRing(rings[0]);
        if (outerRing.Count < 3) return;
        AttachPolygon(parent, outerRing);
        
        // IMPORTANT: Also draw ALL inner rings (holes) as separate borders
        for (int i = 1; i < rings.Count; i++)
        {
            var holeRing = ScaleRing(rings[i]);
            if (holeRing.Count < 3) continue;
            DrawBorder(parent, holeRing, "Hole_" + i);
        }
    }

    void BuildMultiPolygon(GameObject parent, List<List<List<Vector2>>> polygons)
    {
        if (polygons == null) return;

        var col = parent.AddComponent<PolygonCollider2D>();
        col.pathCount = 0;
        var meshFilter = parent.AddComponent<MeshFilter>();
        var meshRenderer = parent.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(countryMaterial);

        var combinedMeshes = new List<CombineInstance>();
        int pathIndex = 0;

        foreach (var polygon in polygons)
        {
            if (polygon == null || polygon.Count == 0) continue;
            var ring = ScaleRing(polygon[0]);
            if (ring.Count < 3) continue;

            col.pathCount = pathIndex + 1;
            col.SetPath(pathIndex, ring.ToArray());
            pathIndex++;

            var mesh = Triangulate(ring);
            if (mesh != null)
                combinedMeshes.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.identity });
            
            // Also draw holes as separate line renderers
            for (int i = 1; i < polygon.Count; i++)
            {
                var holeRing = ScaleRing(polygon[i]);
                if (holeRing.Count < 3) continue;
                DrawBorder(parent, holeRing, "MultiHole_" + pathIndex + "_" + i);
            }
        }

        if (combinedMeshes.Count > 0)
        {
            var combined = new Mesh { name = parent.name };
            combined.CombineMeshes(combinedMeshes.ToArray(), true, false);
            combined.RecalculateNormals();
            meshFilter.sharedMesh = combined;
        }
        
        // Draw borders for ALL outer rings in the multi-polygon too
        pathIndex = 0;
        foreach (var polygon in polygons)
        {
            if (polygon == null || polygon.Count == 0) continue;
            var ring = ScaleRing(polygon[0]);
            if (ring.Count < 3) continue;
            DrawBorder(parent, ring, "Outer_" + pathIndex);
            pathIndex++;
        }
    }

    void AttachPolygon(GameObject go, List<Vector2> ring)
    {
        var col = go.AddComponent<PolygonCollider2D>();
        col.SetPath(0, ring.ToArray());

        var mesh = Triangulate(ring);
        if (mesh == null) 
        {
            Debug.LogWarning($"[GeoJsonLoader] {go.name}: Triangulate returned null!");
            return;
        }

        var filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = new Material(countryMaterial);
        
        // Add outline/border - draw polygon edges in black
        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.positionCount = ring.Count + 1; // +1 to close the loop
        var positions = new Vector3[ring.Count + 1];
        for (int i = 0; i < ring.Count; i++)
            positions[i] = new Vector3(ring[i].x, ring[i].y, -0.01f); // slightly in front
        positions[ring.Count] = positions[0]; // close the polygon
        lineRenderer.SetPositions(positions);
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        
        if (showLogs)
            Debug.Log($"[GeoJsonLoader] {go.name}: Created mesh with {mesh.vertices.Length} verts, {mesh.triangles.Length/3} triangles + border outline");
    }

    void DrawBorder(GameObject parent, List<Vector2> ring, string ringName)
    {
        // Just draw borders for holes/inner rings (no mesh)
        var lineGo = new GameObject(parent.name + "_" + ringName);
        lineGo.transform.parent = parent.transform;
        lineGo.transform.localPosition = Vector3.zero;
        
        var lineRenderer = lineGo.AddComponent<LineRenderer>();
        lineRenderer.positionCount = ring.Count + 1;
        var positions = new Vector3[ring.Count + 1];
        for (int i = 0; i < ring.Count; i++)
            positions[i] = new Vector3(ring[i].x, ring[i].y, -0.01f);
        positions[ring.Count] = positions[0];
        lineRenderer.SetPositions(positions);
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.0015f;
        lineRenderer.endWidth = 0.0015f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
    }

    List<Vector2> ScaleRing(List<Vector2> points)
    {
        var result = new List<Vector2>(points.Count);
        foreach (var p in points)
            result.Add(new Vector2(p.x * mapScale, p.y * mapScale));
        
        if (result.Count > 0 && showLogs && result[0].magnitude > 0.5f) // Gros pays
        {
            // Log first 20 points with HIGH PRECISION to see variation
            Debug.Log($"[GeoJsonLoader] === Scaled Ring ({result.Count} points total) ===");
            for (int i = 0; i < Mathf.Min(20, result.Count); i++)
            {
                string dist = (i > 0) ? $" dist={Vector2.Distance(result[i], result[i-1]):F4}" : "";
                Debug.Log($"  [{i:D3}] = ({result[i].x:F6}, {result[i].y:F6}){dist}");
            }
        }
        
        return result;
    }

    Mesh Triangulate(List<Vector2> points)
    {
        if (points == null || points.Count < 3) return null;
        if (points[0] == points[points.Count - 1])
            points.RemoveAt(points.Count - 1);
        if (points.Count < 3) return null;

        var indices = EarClip(points);
        if (indices == null || indices.Count < 3) return null;

        var mesh = new Mesh();
        var verts = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            verts[i] = new Vector3(points[i].x, points[i].y, 0f);

        mesh.vertices = verts;
        
        // IMPORTANT: Reverse triangle order so normals face camera (CCW = front-facing)
        var reversedTriangles = new List<int>(indices);
        for (int i = 0; i < reversedTriangles.Count; i += 3)
        {
            // Swap first and third to reverse winding order
            int temp = reversedTriangles[i];
            reversedTriangles[i] = reversedTriangles[i + 2];
            reversedTriangles[i + 2] = temp;
        }
        
        mesh.triangles = reversedTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Debug: vérifier la taille du mesh
        var bounds = mesh.bounds;
        if (showLogs && bounds.size.magnitude > 0.01f)
            Debug.Log($"[GeoJsonLoader] Mesh bounds: {bounds.center} size: {bounds.size}");
        
        return mesh;
    }

    List<int> EarClip(List<Vector2> poly)
    {
        var result = new List<int>();
        var idx = new List<int>();
        for (int i = 0; i < poly.Count; i++) idx.Add(i);
        if (SignedArea(poly) < 0) idx.Reverse();

        int safety = poly.Count * poly.Count + 10;
        while (idx.Count > 3 && safety-- > 0)
        {
            bool found = false;
            for (int i = 0; i < idx.Count; i++)
            {
                int a = idx[(i - 1 + idx.Count) % idx.Count];
                int b = idx[i];
                int c = idx[(i + 1) % idx.Count];
                if (!IsEar(poly, idx, a, b, c)) continue;
                result.Add(a); result.Add(b); result.Add(c);
                idx.RemoveAt(i);
                found = true;
                break;
            }
            if (!found) break;
        }
        if (idx.Count == 3) { result.Add(idx[0]); result.Add(idx[1]); result.Add(idx[2]); }
        return result;
    }

    bool IsEar(List<Vector2> poly, List<int> idx, int a, int b, int c)
    {
        if (Cross(poly[a], poly[b], poly[c]) <= 0) return false;
        foreach (int i in idx)
        {
            if (i == a || i == b || i == c) continue;
            if (PointInTriangle(poly[i], poly[a], poly[b], poly[c])) return false;
        }
        return true;
    }

    float Cross(Vector2 a, Vector2 b, Vector2 c) =>
        (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

    float SignedArea(List<Vector2> p)
    {
        float area = 0;
        for (int i = 0; i < p.Count; i++)
        {
            int j = (i + 1) % p.Count;
            area += p[i].x * p[j].y - p[j].x * p[i].y;
        }
        return area / 2f;
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(a, b, p), d2 = Cross(b, c, p), d3 = Cross(c, a, p);
        return !(((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0)));
    }
}
