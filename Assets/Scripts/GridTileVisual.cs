using UnityEngine;
using System;

public class GridTileVisual : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    [SerializeField] private Renderer rend;
    private MaterialPropertyBlock materialProperty;
    public event Action<GridTileVisual> OnHover;
    public event Action<GridTileVisual> OnExit;
    public event Action<GridTileVisual> OnClick;

    private void Start()
    {
        materialProperty = new MaterialPropertyBlock();
        gameObject.SetActive(false);
    }

    // Initialize visualTile
    public void Initialize(Vector2Int pos, Color color)
    {
        if (materialProperty == null)
            Debug.Log($"MaterialProperty on null!?");
        GridPosition = pos;
        materialProperty.SetColor("_Color", color);
        rend.SetPropertyBlock(materialProperty);
    }

    // Swap color of visual tile
    public void SetColor(Color color)
    {
        if (materialProperty == null)
            Debug.Log($"MaterialProperty on null!?");
        materialProperty.SetColor("_Color", color);
        rend.SetPropertyBlock(materialProperty);
    }

    // Invoke events related to these functions on trigger
    private void OnMouseDown() => OnClick?.Invoke(this);
    private void OnMouseEnter() => OnHover?.Invoke(this);
    private void OnMouseExit() => OnExit?.Invoke(this);
}