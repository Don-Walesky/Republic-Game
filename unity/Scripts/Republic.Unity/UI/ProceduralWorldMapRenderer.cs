namespace Republic.Unity.UI;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Republic.Core.World.Models;

/// <summary>
/// Unity UI MonoBehaviour rendering procedural hex grid map tiles, provincial control colors, and biome overlays.
/// </summary>
public sealed class ProceduralWorldMapRenderer : MonoBehaviour
{
    [Header("Grid Layout Parameters")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float hexRadius = 40f;

    [Header("UI Containers")]
    [SerializeField] private RectTransform mapCanvasContainer = null!;
    [SerializeField] private Text hoveredTileTooltipText = null!;

    private readonly Dictionary<Vector2Int, GameObject> _hexTileMap = new();

    private void Start()
    {
        GenerateProceduralWorldMap();
    }

    public void GenerateProceduralWorldMap()
    {
        if (mapCanvasContainer == null) return;

        foreach (Transform child in mapCanvasContainer)
        {
            Destroy(child.gameObject);
        }
        _hexTileMap.Clear();

        float xSpacing = hexRadius * 1.732f; // sqrt(3)
        float ySpacing = hexRadius * 1.5f;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float xPos = x * xSpacing + ((y % 2 == 1) ? xSpacing * 0.5f : 0f);
                float yPos = y * ySpacing;

                var tileObj = new GameObject($"HexTile_{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                tileObj.transform.SetParent(mapCanvasContainer, false);

                var rect = tileObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(xPos, yPos);
                rect.sizeDelta = new Vector2(hexRadius * 1.8f, hexRadius * 1.8f);

                var image = tileObj.GetComponent<Image>();
                image.color = DetermineBiomeColor(x, y);

                var btn = tileObj.GetComponent<Button>();
                int tileX = x, tileY = y;
                btn.onClick.AddListener(() => OnTileSelected(tileX, tileY));

                _hexTileMap[new Vector2Int(x, y)] = tileObj;
            }
        }
    }

    private Color DetermineBiomeColor(int x, int y)
    {
        if (y < 2) return new Color(0.2f, 0.4f, 0.8f); // Maritime Coastal Water
        if (y > 6) return new Color(0.8f, 0.8f, 0.9f); // Mountain Snow Peak
        if (x % 3 == 0) return new Color(0.3f, 0.7f, 0.3f); // Fertile Lowland Plains
        return new Color(0.8f, 0.6f, 0.3f); // Arid Plateau
    }

    public void OnTileSelected(int x, int y)
    {
        if (hoveredTileTooltipText != null)
        {
            hoveredTileTooltipText.text = $"PROVINCIAL SECTOR [{x}, {y}] - Biome Sector Initialized.";
        }
    }
}
