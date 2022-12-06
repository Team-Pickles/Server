using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

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

    private class MapPosSizeData
    {
        public Vector3 pos;
        public Vector2 size;
        public MapPosSizeData(Vector3 _pos, Vector2 _size)
        {
            this.pos = _pos;
            this.size = _size;
        }
    }

    public List<TileBase> TileBases;
    
    public List<GameObject> ItemPrefabs;
    public List<GameObject> PlayerPrefabs;
    public List<GameObject> EnemyPrefabs;
    public GameObject ProjectilePrefab;
    public GameObject BulletPrefab;
    
    public Dictionary<Vector3, Vector2> DeletedPosList = new Dictionary<Vector3, Vector2>();
    private Vector3[] RoomPosList = new Vector3[] {new Vector3(0, 1f, 0), new Vector3(0, -1f, 0)};
    private MapPosSizeData[] mapPosSizeList = new MapPosSizeData[]
    {
        new MapPosSizeData(new Vector3(0, 0, 0), new Vector2(0, 0)),
        new MapPosSizeData(new Vector3(0, 0, 0), new Vector2(0, 0)),
    };
    private Vector3 spawnPosition = new Vector3(0,0,0);
    private int roomCnt = 0;

    public string CreateRoom(string _roomName, int _serverPort, int _mapId)
    {
        string _roomId = GenerateRoomId();
        Vector2 mapSize = APIMapDataLoader.instance.mapListItems[_mapId].map_size;
        Debug.Log(mapSize);
        Vector3 _roomPos = new Vector3(0, 0, 0);
        if(DeletedPosList.Count > 0)
        {
            _roomPos = DeletedPosList.Keys.First();
            Vector2 _data = APIMapDataLoader.instance.mapListItems[_mapId].map_size;
            if(DeletedPosList[_roomPos].y - _data.y < 0)
            {
                _roomPos = mapPosSizeList[roomCnt % 2].pos;
                float _roomPosY = mapPosSizeList[roomCnt % 2].size.y * RoomPosList[roomCnt % 2].y;
                _roomPos += new Vector3(0, _roomPosY, 0);
                mapPosSizeList[roomCnt % 2] = new MapPosSizeData(_roomPos, mapSize);
            }
            else {
                DeletedPosList.Remove(_roomPos);
                mapPosSizeList[roomCnt % 2] = new MapPosSizeData(_roomPos, mapSize);
            }
        } else {
            if(roomCnt != 0)
            {
                _roomPos = mapPosSizeList[roomCnt % 2].pos;
                float _roomPosY = mapPosSizeList[roomCnt % 2].size.y * 2 * RoomPosList[roomCnt % 2].y;
                _roomPos += new Vector3(0, _roomPosY, 0);
                mapPosSizeList[roomCnt % 2] = new MapPosSizeData(_roomPos, mapSize);
                Debug.Log(mapPosSizeList[roomCnt % 2].pos + "_" + mapPosSizeList[roomCnt % 2].size);
            } else {
                mapPosSizeList[0] = new MapPosSizeData(_roomPos, mapSize);
                mapPosSizeList[1] = new MapPosSizeData(_roomPos, mapSize);
            }
        }
        
        try {
            ThreadManager.createRoomOnMainThread.Add(new CreateRoomData(_roomId, _roomName, _serverPort, _roomPos, mapSize, _mapId));
            ++roomCnt;
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
        _server.serverSend.JoinDone(_clientId);
    }

    public void LoadMap(Room _room, int map_id)
    {
        Dictionary<int, DataClass> loaded = APIMapDataLoader.instance.mapListItems[map_id].map_info;
        
        int[] itemIds = new int[ItemPrefabs.Count];
        int[] enemyIds = new int[EnemyPrefabs.Count];
        int[] _deathZone = new int[4];

        foreach(DataClass data in loaded.Values) {
            int _infoType = data.GetInfoType();
            if(_infoType == (int)TileTypes.Empty / 100) {
                int tileType = data.GetAdditionalInfo();
                Vector3 _pos = data.GetPos();
                Vector3Int _intPos = new Vector3Int((int)_pos.x, (int)_pos.y, (int)_pos.z);
                _room.TileGroup.SetTile(_intPos, TileBases[tileType - 1]);
            }
            else if(_infoType == (int)TileTypes.Item / 100)
            {
                int itemType = data.GetAdditionalInfo();
                Debug.Log($"{itemType}, {ItemPrefabs.Count}");
                int itemIdx = itemType - (int)TileTypes.Item - 1;
                GameObject itemClone = InstatiateItem(_room.ItemGroup, itemIdx);
                Item _item = itemClone.GetComponent<Item>();

                _item.Init(_room.roomId, itemType);
                _item.server = NetworkManager.instance.servers[_room.serverPort];
                _room.items.Add(_item.id, _item);

                itemClone.name = ((TileTypes)itemType).ToString() + "_" + itemIds[itemIdx];
                itemClone.transform.localPosition = data.GetPos();
                ++itemIds[itemIdx];
            }
            else if(_infoType == (int)TileTypes.Enemy / 100)
            {
                int enemyType = data.GetAdditionalInfo();
                Debug.Log($"{enemyType}, {enemyType - (int)TileTypes.Enemy - 1}, {EnemyPrefabs.Count}");
                int enemyIdx = enemyType - (int)TileTypes.Enemy - 1;
                GameObject enemyClone = InstantiateEnemy(_room.EnemyGroup, enemyIdx);
                Enemy _enemy = enemyClone.GetComponent<Enemy>();
                _enemy.Initialize(NetworkManager.instance.servers[_room.serverPort], _room);
                _room.enemies.Add(_enemy.id, _enemy);

                enemyClone.name = ((TileTypes)enemyType).ToString() + "_" + enemyIds[enemyIdx];
                enemyClone.transform.localPosition = data.GetPos();
                ++enemyIds[enemyIdx];
            }
            else if(_infoType == (int)TileTypes.Player / 100)
            {
                _room.spawnPoint = data.GetPos();
            }
            else if(_infoType == (int)TileTypes.MapSize / 100)
            {
                int _minMaxType = data.GetAdditionalInfo();
                Vector3 _mapSize = data.GetPos();
                if(_minMaxType == (int)TileTypes.minSize)
                {
                    _deathZone[0] = (int)_mapSize.x;
                    _deathZone[1] = (int)_mapSize.y;
                }
                else if(_minMaxType == (int)TileTypes.maxSize)
                {
                    _deathZone[2] = (int)_mapSize.x;
                    _deathZone[3] = (int)_mapSize.y;
                }
            }
        }
        for(int _x = _deathZone[0] - 2; _x <= _deathZone[2] + 2; ++_x)
        {
            _room.DeathZone.SetTile(new Vector3Int(_x, _deathZone[1] - 2, 0), TileBases[0]);
        }
        for(int _y = _deathZone[1] - 1; _y <= _deathZone[3] + 1; ++_y)
        {
            _room.DeathZone.SetTile(new Vector3Int(_deathZone[0] - 2, _y, 0), TileBases[0]);
            _room.DeathZone.SetTile(new Vector3Int(_deathZone[2] + 2, _y, 0), TileBases[0]);
        }
        for(int _x = _deathZone[0] - 2; _x <= _deathZone[2] + 2; ++_x)
        {
            _room.DeathZone.SetTile(new Vector3Int(_x, _deathZone[3] + 2, 0), TileBases[0]);
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

    public GameObject InstatiateBullet(GameObject _playerGroup, Transform _shootOrigin, int _id)
    {
        GameObject _player = GameObject.Find("Player " + _id);
        GameObject _bullet = Instantiate(BulletPrefab, _player.transform);
        _bullet.transform.localPosition = _shootOrigin.localPosition;

        return _bullet;
    }

    public GameObject InstatiateGrenade(GameObject _playerGroup, Transform _shootOrigin, int _id)
    {
        GameObject _player = GameObject.Find("Player " + _id);
        GameObject _grenade = Instantiate(ProjectilePrefab, _player.transform);
        _grenade.transform.localPosition = _shootOrigin.localPosition;
        return _grenade;
    }

    public GameObject InstantiateEnemy(GameObject _enemyGroup, int _enemyType)
    {
        return Instantiate(EnemyPrefabs[_enemyType], _enemyGroup.transform);
    }
}
