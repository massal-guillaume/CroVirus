using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Hexagon Graphic")]
[RequireComponent(typeof(CanvasRenderer))]
public class HexagonGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Vector2 center = rect.center;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

        // Create center vertex
        UIVertex centerVertex = UIVertex.simpleVert;
        centerVertex.color = color;
        centerVertex.position = center;
        vh.AddVert(centerVertex);

        // Create 6 perimeter vertices
        for (int i = 0; i < 6; i++)
        {
            float angleRad = Mathf.Deg2Rad * (60f * i);
            Vector2 point = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = point;
            vh.AddVert(vertex);
        }

        // Create triangles for filled hexagon
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            vh.AddTriangle(0, i + 1, next + 1);
        }

        // Create border: thin gray line around hexagon edges
        Color borderColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray border

        // For each edge of the hexagon, add border vertices and triangles
        int borderVertexStart = 7; // Start after center + 6 perimeter vertices

        for (int i = 0; i < 6; i++)
        {
            float angleRad = Mathf.Deg2Rad * (60f * i);
            Vector2 point = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;

            float offsetX = Mathf.Cos(angleRad) * 1.5f;
            float offsetY = Mathf.Sin(angleRad) * 1.5f;
            Vector2 outsetPoint = point + new Vector2(offsetX, offsetY);

            UIVertex borderVert = UIVertex.simpleVert;
            borderVert.color = borderColor;
            borderVert.position = outsetPoint;
            vh.AddVert(borderVert);
        }

        // Create border triangles
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            int v1 = i + 1;       // Current outer vertex
            int v2 = next + 1;    // Next outer vertex
            int v3 = borderVertexStart + i;       // Current border vertex
            int v4 = borderVertexStart + next;    // Next border vertex

            vh.AddTriangle(v1, v3, v4);
            vh.AddTriangle(v1, v4, v2);
        }
    }
}
