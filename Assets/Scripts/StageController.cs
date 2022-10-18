using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField]
    private Tilemap2d _tilemap2D;

    private void Awake()
    {
        MapDataLoader _mapDataLoader = new MapDataLoader();
        MapData _mapData = _mapDataLoader.Load("Stage01");

        _tilemap2D.GenerateTilemap(_mapData);
    }
}
