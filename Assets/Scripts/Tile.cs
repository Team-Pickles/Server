using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty=0,
    Base,
    Broke,
    Boom,
    Jump,
    StraightLeft,
    StraithRight,
    Blink,
    LastIndex,

}


public class Tile : MonoBehaviour
{
    [SerializeField]
    private Sprite[] images;
    private SpriteRenderer spriteRenderer;
    private TileType tileType;

    public void Setup(TileType _tileType)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        TileType = _tileType;
    }

    public TileType TileType
    {
        set
        {
            tileType = value;
            spriteRenderer.sprite = images[(int)tileType - 1];
        }
        get => tileType;
    }
}
