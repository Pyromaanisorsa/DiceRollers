using UnityEngine;
using System;

public class GridTileHighlight : MonoBehaviour
{
    [SerializeField] private Renderer rend;
    //private MaterialPropertyBlock materialProperty;
    //public Vector2Int GridPosition { get; private set; }

    private void Start()
    {
        //materialProperty = new MaterialPropertyBlock();
        gameObject.SetActive(false);
    }

    //public void Initialize(Vector2Int pos, Color color)
    //{
    //    if (materialProperty == null)
    //        Debug.Log($"MaterialProperty on null!?");
    //    GridPosition = pos;
    //    materialProperty.SetColor("_Color", color);
    //    rend.SetPropertyBlock(materialProperty);
    //}

    //public void SetColor(Color color)
    //{
    //    if (materialProperty == null)
    //        Debug.Log($"MaterialProperty on null!?");
    //    //gameObject.transform.position = pos;
    //    materialProperty.SetColor("_Color", color);
    //    rend.SetPropertyBlock(materialProperty);
    //}
}