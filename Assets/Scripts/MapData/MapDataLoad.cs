using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MapDataLoad : MonoBehaviour
{
    [SerializeField] string filePath = "MapData/";
    [SerializeField] string fileName = "MyMap";
    [SerializeField] GameObject TileGroup;
    [SerializeField] GameObject ItemGroup;
    [SerializeField] GameObject EnemyGroup;
    [SerializeField] GameObject PlayerGroup;
    [SerializeField] List<GameObject> TilePrefabs;
    [SerializeField] List<GameObject> ItemPrefabs;
    [SerializeField] List<GameObject> PlayerPrefabs;
    [SerializeField] List<GameObject> EnemyPrefabs;

    public void Load() {
        Refresh();

        string path = filePath + fileName + ".json";
        if(File.Exists(filePath + fileName + ".json") == false){
            Debug.LogError("Load failed. There is no file(" + fileName + ".json).");
            return;
        }
        string fromJson = File.ReadAllText(filePath + fileName + ".json");
        Dictionary<Vector3, DataClass> loaded = JsonUtility.FromJson<Serialization<Vector3, DataClass>>(fromJson).ToDictionary();

        int[] tileIds = new int[TilePrefabs.Count];
        int[] itemIds = new int[ItemPrefabs.Count];
        int[] playerIds = new int[PlayerPrefabs.Count];
        int[] enemyIds = new int[EnemyPrefabs.Count];
        foreach(DataClass data in loaded.Values) {
            switch(data.GetInfoType()){
                case InfoTypes.player:
                    int playerType = data.GetAdditionalInfo();
                    GameObject playerPrefab = PlayerPrefabs[playerType];
                    GameObject playerClone = Instantiate(playerPrefab, data.GetPos(), Quaternion.identity);
                    playerClone.name = ((TileTypes)playerType).ToString() + "_" + playerIds[playerType];
                    playerClone.transform.parent = PlayerGroup.transform;
                    ++playerIds[playerType];
                    break;
                case InfoTypes.tile:
                    int tileType = data.GetAdditionalInfo();
                    GameObject tilePrefab = TilePrefabs[tileType];
                    GameObject tileClone = Instantiate(tilePrefab, data.GetPos(), Quaternion.identity);
                    tileClone.name = ((TileTypes)tileType).ToString() + "_" + tileIds[tileType];
                    tileClone.transform.parent = TileGroup.transform;
                    ++tileIds[tileType];
                    break;
                case InfoTypes.item:
                    int itemType = data.GetAdditionalInfo();
                    GameObject itemPrefab = ItemPrefabs[itemType];
                    GameObject itemClone = Instantiate(itemPrefab, data.GetPos(), Quaternion.identity);
                    itemClone.name = ((ItemTypes)itemType).ToString() + "_" + itemIds[itemType];
                    itemClone.transform.parent = ItemGroup.transform;
                    ++itemIds[itemType];
                    break;
                case InfoTypes.enemy:
                    int enemyType = data.GetAdditionalInfo();
                    GameObject enemyPrefab = EnemyPrefabs[enemyType];
                    GameObject enemyClone = Instantiate(enemyPrefab, data.GetPos(), Quaternion.identity);
                    enemyClone.name = ((ItemTypes)enemyType).ToString() + "_" + enemyIds[enemyType];
                    enemyClone.transform.parent = EnemyGroup.transform;
                    ++enemyIds[enemyType];
                    break;
            }
        }

        Debug.Log("load done");
    }

    public void Refresh() {
        List<GameObject> forDestroy = new List<GameObject>();

        int tileCnt = TileGroup.transform.childCount;
        int itemCnt = ItemGroup.transform.childCount;
        int enemyCnt = EnemyGroup.transform.childCount;
        int playerCnt = PlayerGroup.transform.childCount;
        for (int i = 0; i < tileCnt; ++i) {
            forDestroy.Add(TileGroup.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < itemCnt; ++i) {
            forDestroy.Add(ItemGroup.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < enemyCnt; ++i) {
            forDestroy.Add(EnemyGroup.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < playerCnt; ++i) {
            forDestroy.Add(PlayerGroup.transform.GetChild(i).gameObject);
        }

        foreach (GameObject obj in forDestroy) {
            DestroyImmediate(obj);
        }
    }
}
