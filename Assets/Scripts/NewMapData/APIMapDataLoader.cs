using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class MapListItem
{
    public int map_id;
    public string map_tag;
    public string map_info;
    public int map_grade;
    public int map_difficulty;
    public string map_maker;
}

[Serializable]
public class MapDataClass {
    [SerializeField] public int map_id;
    [SerializeField] public string map_info;
    [SerializeField] public string map_tag;
    [SerializeField] public int map_grade;
    [SerializeField] public int map_difficulty;
    [SerializeField] public string map_maker;
}

public class APIMapDataLoader : MonoBehaviour
{
    public Dictionary<int, MapListItem> mapListItems = new Dictionary<int, MapListItem>();

    public static APIMapDataLoader instance;

    private void Awake()
    {
        //Singleton ����
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists,destroying object!");
            Destroy(this);
        }
        string file = "MapData/" + "MyMap.json";
        if(File.Exists(file) == false){
            Debug.LogError("Load failed. There is no file(MyMap.json).");
            return;
        }
        string fromJson = File.ReadAllText(file);
        MapListItem _item = new MapListItem();
        _item.map_id = 0;
        _item.map_info = fromJson;
        _item.map_grade = 0;
        _item.map_difficulty = 0;
        _item.map_maker = "None";
        mapListItems.Add(0, _item);
        LoadAllProcess();
    }

    public void LoadAllProcess()
    {
        StartCoroutine(IneternetConnectCheck(isConnected => {
            if (isConnected)
            {
                Debug.Log("Server Available!");
                StartCoroutine(GetMapList(mapListItemCnt => {
                    if(mapListItemCnt == 0)
                    {
                        Debug.Log("There is no map.");
                    } else {
                        Debug.Log($"Map is loaded.(Count: {mapListItemCnt})");
                    }
                }));
            }
            else
            {
                Debug.Log("Internet or server Not Available");
            }
        }));

        IEnumerator GetMapList(Action<int> ResultHandler){
            using ( UnityWebRequest request = UnityWebRequest.Get("http://localhost:3001/api/map/getAllList"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();
                string _result = request.downloadHandler.text;
                string _forparse = "{\"Items\":" + _result + "}";
                MapDatas mapInfos = JsonUtility.FromJson<MapDatas>(_forparse);
                foreach(MapDataClass _mapInfo in mapInfos.Items)
                {
                    MapListItem _item = new MapListItem();
                    _item.map_id = _mapInfo.map_id;
                    _item.map_tag = _mapInfo.map_tag;
                    _item.map_info = _mapInfo.map_info;
                    _item.map_grade = _mapInfo.map_grade;
                    _item.map_difficulty = _mapInfo.map_difficulty;
                    _item.map_maker = _mapInfo.map_maker;
                    mapListItems.Add(_item.map_id, _item);
                }

                if (request.error != null)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    ResultHandler(mapListItems.Count);
                }
                request.downloadHandler.Dispose();
                request.Dispose();
            }
        }

        IEnumerator IneternetConnectCheck(Action<bool> action)
        {
            using (UnityWebRequest request = new UnityWebRequest("http://localhost:3001/"))
            {

                request.downloadHandler = new DownloadHandlerBuffer();
;
                yield return request.SendWebRequest();
                if (request.error != null)
                {
                    action(false);
                }
                else
                {
                    action(true);
                }
                request.downloadHandler.Dispose();
                request.Dispose();
            }
        }
    }
}
