using System.Collections.Generic;
using UnityEngine;

public class VoronoiBiomesModifier : IHeightModifier
{
    public void ModifyHeight(float[,] heights, TerrainGenerationSettings settings)
    {
        if (settings == null)
        {
            Debug.LogError("TerrainGenerationSettings is null in VoronoiBiomesModifier.");
            return;
        }

        if (settings.voronoiFalloffCurve == null)
        {
            Debug.LogError("VoronoiFalloffCurve is null. Ensure it is assigned in the settings.");
            return;
        }

        if (settings.voronoiDistributionMode == TerrainGenerationSettings.DistributionMode.Custom &&
            (settings.customVoronoiPoints == null || settings.customVoronoiPoints.Count == 0))
        {
            Debug.LogError("Custom Voronoi Points are null or empty. Ensure they are assigned.");
            return;
        }

        int width = heights.GetLength(0);
        int length = heights.GetLength(1);

        List<Vector2> points = GenerateVoronoiPoints(settings, width, length);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float minDist = float.MaxValue;

                // Find the closest Voronoi point
                foreach (var point in points)
                {
                    float dist = Vector2.Distance(point, new Vector2(x, y));
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }

                // Use the falloff curve and height range for the influence
                float normalizedDistance = minDist / Mathf.Max(width, length);
                float falloffValue = settings.voronoiFalloffCurve.Evaluate(1 - normalizedDistance);
                heights[x, y] = Mathf.Lerp(settings.voronoiHeightRange.x, settings.voronoiHeightRange.y, falloffValue);
            }
        }
    }

    private List<Vector2> GenerateVoronoiPoints(TerrainGenerationSettings settings, int width, int length)
    {
        List<Vector2> points = new List<Vector2>();

        switch (settings.voronoiDistributionMode)
        {
            case TerrainGenerationSettings.DistributionMode.Random:
                for (int i = 0; i < settings.voronoiCellCount; i++)
                {
                    points.Add(new Vector2(Random.Range(0, width), Random.Range(0, length)));
                }
                break;

            case TerrainGenerationSettings.DistributionMode.Grid:
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(settings.voronoiCellCount));
                float cellWidth = (float)width / gridSize;
                float cellHeight = (float)length / gridSize;

                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        if (points.Count >= settings.voronoiCellCount)
                            break;

                        float px = x * cellWidth + cellWidth / 2f;
                        float py = y * cellHeight + cellHeight / 2f;
                        points.Add(new Vector2(px, py));
                    }
                }
                break;

            case TerrainGenerationSettings.DistributionMode.Custom:
                if (settings.customVoronoiPoints != null && settings.customVoronoiPoints.Count > 0)
                {
                    points.AddRange(settings.customVoronoiPoints);
                }
                else
                {
                    Debug.LogWarning("Custom Voronoi Points are null or empty. Using default Random distribution.");
                    for (int i = 0; i < settings.voronoiCellCount; i++)
                    {
                        points.Add(new Vector2(Random.Range(0, width), Random.Range(0, length)));
                    }
                }
                break;
        }

        return points;
    }

}
