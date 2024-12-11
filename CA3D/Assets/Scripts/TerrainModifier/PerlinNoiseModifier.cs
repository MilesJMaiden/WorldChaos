using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// A height modifier that applies layered Perlin noise to a terrain heightmap.
/// </summary>
public class PerlinNoiseModifier : IHeightModifier
{
    #region Public Methods

    /// <summary>
    /// Modifies the terrain heights using Perlin noise based on the specified settings.
    /// </summary>
    /// <param name="heights">The 2D array of terrain heights to modify.</param>
    /// <param name="settings">The terrain generation settings used for Perlin noise generation.</param>
    public void ModifyHeight(float[,] heights, TerrainGenerationSettings settings)
    {
        int width = heights.GetLength(0);
        int length = heights.GetLength(1);

        // Cache reusable values from settings
        float baseScale = settings.perlinBaseScale;
        float amplitudeDecay = settings.perlinAmplitudeDecay;
        float frequencyGrowth = settings.perlinFrequencyGrowth;
        Vector2 offset = settings.perlinOffset;

        Parallel.For(0, width, x =>
        {
            for (int y = 0; y < length; y++)
            {
                float normalizedX = x / (float)width;
                float normalizedY = y / (float)length;

                float heightValue = GeneratePerlinNoiseHeight(normalizedX, normalizedY, baseScale, amplitudeDecay, frequencyGrowth, offset, settings.perlinLayers);
                heights[x, y] += Mathf.Clamp(heightValue, 0f, 1f);
            }
        });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Generates the height value for a single point using layered Perlin noise.
    /// </summary>
    private float GeneratePerlinNoiseHeight(float normalizedX, float normalizedY, float baseScale, float amplitudeDecay, float frequencyGrowth, Vector2 offset, int layers)
    {
        float heightValue = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int layer = 0; layer < layers; layer++)
        {
            float xCoord = (normalizedX * baseScale * frequency) + offset.x;
            float yCoord = (normalizedY * baseScale * frequency) + offset.y;

            heightValue += Mathf.PerlinNoise(xCoord, yCoord) * amplitude;
            amplitude *= amplitudeDecay;
            frequency *= frequencyGrowth;
        }

        return heightValue;
    }

    #endregion
}
