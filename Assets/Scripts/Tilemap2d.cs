using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilemap2d : MonoBehaviour
{
    [Header("Tile")]
    [SerializeField]
    private GameObject tilePrefab;

    public void GenerateTilemap(MapData _mapData)
    {
        int _width = _mapData.mapSize.x;
        int _height = _mapData.mapSize.y;

        for (int y=0; y< _height; ++y)
        {
            for (int x=0; x< _width; ++x)
            {
                int _index = y * _width + x;

                if (_mapData.mapData[_index] == (int)TileType.Empty)
                    continue;
                Vector3 _position = new Vector3(x, y);

                if (_mapData.mapData[_index] > (int)TileType.Empty && _mapData.mapData[_index] < (int)TileType.LastIndex)
                {
                    SpawnTile((TileType)_mapData.mapData[_index], _position);
                }
            }
        }

    }

    public void SpawnTile(TileType _tileType, Vector3 _position)
    {
        GameObject clone = Instantiate(tilePrefab, _position, Quaternion.identity);

        clone.name = "Tile";
        clone.transform.SetParent(transform);

        Tile tile = clone.GetComponent<Tile>();
        tile.Setup(_tileType);
    }
}
