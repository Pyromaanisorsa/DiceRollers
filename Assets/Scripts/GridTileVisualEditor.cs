using UnityEngine;
using TMPro;

public class GridTileVisualEditor : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool occupied = false;
    [SerializeField] private TMP_Text coordinateText;
    [SerializeField] public Renderer rend;
    private MaterialPropertyBlock materialProperty;
    private static Color openColor = new Color(1, 1, 1, 0.2705882f);
    private static Color occupiedColor = new Color(1, 0, 0, 0.7f);
    [SerializeField] private static bool readyEditor = false;

    // Print tile's coordinates to tile's text
    public void Initialize(Vector2Int pos)
    {
        GridPosition = pos;
        gameObject.name = $"Tile{pos.x}-{pos.y}";
        coordinateText.text = $"{pos.x}-{pos.y}";
        materialProperty = new MaterialPropertyBlock();
    }

    // Enables OnValidate logic once every tile is ready
    public void ReadyToEnable()
    {
        readyEditor = true;
    }

    // Change tile color on occupied-boolean value change
    private void OnValidate()
    {
        if (readyEditor)
        {
            if (occupied)
            {
                materialProperty.SetColor("_Color", occupiedColor);
                rend.SetPropertyBlock(materialProperty);
            }
            else
            {
                materialProperty.SetColor("_Color", openColor);
                rend.SetPropertyBlock(materialProperty);
            }
        }
    }

    // Set occupied tile from LevelTileData to occupied state when creating editorTile grid
    public void LoadOccupied()
    {
        occupied = true;
        materialProperty.SetColor("_Color", occupiedColor);
        rend.SetPropertyBlock(materialProperty);
    }
}