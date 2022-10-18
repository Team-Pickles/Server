using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapDataLoader 
{
    public MapData Load(string _fileName)
    {
        if (_fileName.Contains(".json") == false)
        {
            _fileName += ".json";
        }

        _fileName = Path.Combine(Application.streamingAssetsPath, _fileName);

        string _dataAsJson = File.ReadAllText(_fileName);

        MapData _mapData = new MapData();
        _mapData = JsonUtility.FromJson<MapData>(_fileName);

        return _mapData;
    }
}