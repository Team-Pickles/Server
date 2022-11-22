using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public List<GameObject> TilePrefabs;
    public List<GameObject> ItemPrefabs;
    public List<GameObject> PlayerPrefabs;
    public List<GameObject> EnemyPrefabs;
    public GameObject ProjectilePrefab;
    public GameObject BulletPrefab;
    
    public List<Vector3> DeletedPosList = new List<Vector3>();
    private Vector3[] RoomPosList = new Vector3[] {new Vector3(0, 16f, 0), new Vector3(0, -16f, 0)};
    private Vector3 spawnPosition = new Vector3(0,0,0);

    public string CreateRoom(string _roomName, int _serverPort)
    {
        string _roomId = GenerateRoomId();
        int roomCnt = NetworkManager.instance.roomInfos.Count;
        Vector3 _roomPos;
        if(DeletedPosList.Count > 0)
        {
            _roomPos = DeletedPosList[0];
            DeletedPosList.Remove(_roomPos);
        } else {
            _roomPos = RoomPosList[roomCnt % 2] * Mathf.Ceil(roomCnt / 2.0f);
        }
        
        try {
            ThreadManager.createRoomOnMainThread.Add(new CreateRoomData(_roomId, _roomName, _serverPort, _roomPos));
            return _roomId;
        } catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
        return "None";
    }

    public void JoinRoom(string _roomId, int _clientId, int _serverPort)
    {
        Server _server = NetworkManager.instance.servers[_serverPort];
        _server.rooms[_roomId].members.Add(_clientId, _server.clients[_clientId]);
        if(_server.rooms[_roomId].members.Count >= 2)
            _server.rooms[_roomId].StartGame();
    }

    public void LoadTestMap(Room _room) {
        string file = "MapData/" + "Stagetest.json";
        if(File.Exists(file) == false){
            Debug.LogError("Load failed. There is no file(Stagetest.json).");
            return;
        }
        string fromJson = File.ReadAllText(file);
        Dictionary<Vector3, DataClass> loaded = JsonUtility.FromJson<Serialization<Vector3, DataClass>>(fromJson).ToDictionary();
        
        int[] tileIds = new int[TilePrefabs.Count];
        int[] itemIds = new int[ItemPrefabs.Count];
        int[] enemyIds = new int[EnemyPrefabs.Count];
        foreach(DataClass data in loaded.Values) {
            switch(data.GetInfoType()){
                case InfoTypes.player:
                    spawnPosition = data.GetPos();
                    break;
                case InfoTypes.tile:
                    int tileType = data.GetAdditionalInfo();
                    GameObject tilePrefab = TilePrefabs[tileType];
                    Vector3 _tilePos = data.GetPos();
                    GameObject tileClone = Instantiate(tilePrefab, _room.TileGroup.transform);
                    tileClone.name = ((TileTypes)tileType).ToString() + "_" + tileIds[tileType];
                    tileClone.transform.localPosition = _tilePos;
                    tileClone.tag = "floor";
                    ++tileIds[tileType];
                    break;
                case InfoTypes.item:
                    int itemType = data.GetAdditionalInfo();
                    GameObject itemPrefab = ItemPrefabs[itemType];
                    GameObject itemClone = RoomManager.instance.InstatiateItem(_room.ItemGroup, 0);
                    Item _item = itemClone.GetComponent<Item>();
                    _item.Init(_room.roomId);
                    _item.server = NetworkManager.instance.servers[_room.serverPort];

                    itemClone.name = ((ItemTypes)itemType).ToString() + "_" + itemIds[itemType];
                    itemClone.transform.localPosition = new Vector3(5f, -2.5f, 0f);
                    ++itemIds[itemType];
                    break;
                case InfoTypes.enemy:
                    int enemyType = data.GetAdditionalInfo();
                    GameObject enemyPrefab = EnemyPrefabs[enemyType];
                    Vector3 _enemyPos = data.GetPos();
                    GameObject enemyClone = Instantiate(enemyPrefab, _room.EnemyGroup.transform);
                    enemyClone.name = ((ItemTypes)enemyType).ToString() + "_" + enemyIds[enemyType];
                    enemyClone.transform.localPosition = _enemyPos;
                    ++enemyIds[enemyType];
                    break;
            }
        }

        Debug.Log("load done");
    }

    private string GenerateRoomId()
    {
        string allCharacter = "*ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string _roomId = "";
        while (true)
        {
            for (int i = 0; i < 8; ++i)
            {
                System.Random rand = new System.Random();
                var temp = rand.Next(0, allCharacter.Length);
                _roomId += allCharacter[temp];
            }
            
            if (!CheckDuplicateRoomName(_roomId))
                break;
            
            _roomId = "";
        }
        return _roomId;
    }

    private bool CheckDuplicateRoomName(string _roomName)
    {
        foreach (string roomName in NetworkManager.instance.roomInfos.Keys)
        {
            if (roomName == _roomName)
            {
                return true;
            }
        }
        return false;
    }

    public GameObject InstatiatePlayer(GameObject _playerGroup, int _playerType)
    {
        return Instantiate(PlayerPrefabs[_playerType], _playerGroup.transform);
    }

    public GameObject InstatiateItem(GameObject _itemGroup, int _itemType)
    {
        return Instantiate(ItemPrefabs[_itemType], _itemGroup.transform);
    }

    public GameObject InstatiateBullet(GameObject _playerGroup, Transform _shootOrigin)
    {
        GameObject _bullet = Instantiate(BulletPrefab, _playerGroup.transform);
        if(_shootOrigin.rotation.y == -1f){
            _bullet.transform.localPosition = _shootOrigin.localPosition - new Vector3(_shootOrigin.localScale.x / 2.0f * 1.1f, 0, 0);
        } else {
            _bullet.transform.localPosition = _shootOrigin.localPosition + new Vector3(_shootOrigin.localScale.x / 2.0f * 1.1f, 0, 0);
        }
        return _bullet;
    }

    public GameObject InstatiateGrenade(GameObject _playerGroup, Transform _shootOrigin)
    {
        GameObject _grenade = Instantiate(ProjectilePrefab, _playerGroup.transform);
        _grenade.transform.localPosition = _shootOrigin.localPosition + new Vector3(_shootOrigin.localPosition.x * 0.3f, 0, 0);
        return _grenade;
    }
}
