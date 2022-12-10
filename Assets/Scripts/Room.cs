using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public string roomId;
    public int mapId;
    public Vector2 mapSize = new Vector2(0, 0);
    public string roomName;
    public int maxPlayer;
    public int masterId;
    public int serverPort;
    public int readyPlayerCount = 0;
    
    public Vector3 mapAddPosition;

    public GameObject room;
    public GameObject PlayerGroup;
    public Grid TileGroupGrid;
    public Tilemap TileGroup;
    public GameObject EnemyGroup;
    public GameObject ItemGroup;
    public Tilemap DeathZone;
    public Tilemap FragileGroup;
    public Tilemap BlockGroup;
    public GameObject MapUtilGroup;

    public Dictionary<int, Client> members = new Dictionary<int, Client>();
    public Dictionary<Vector3, int> ItemInfos = new Dictionary<Vector3, int>();
    public Dictionary<Vector3, int> EnemyInfos = new Dictionary<Vector3, int>();
    public Vector3 spawnPoint = new Vector3(0, 0, 0);
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    public Dictionary<int, Door> doors = new Dictionary<int, Door>();
    public Dictionary<int, Boss1> boss = new Dictionary<int, Boss1>();

    public Room(string _roomId, string _roomName, int _serverPort, GameObject _room)
    {
        roomId = _roomId;
        roomName = _roomName;
        serverPort = _serverPort;

        room = _room;

    }

    public void StartGame(int map_id)
    {
        if(room == null)
        {
            Debug.Log("There is no mapInfo.");
            return;
        }
        Debug.Log("Spawn");
        ThreadManager.ExecuteOnMainThread(() => 
            {
                SetMapInfo(map_id);
                SpawnPlayerAndItems();
            }
        );

    }

    public void GoToNextPortal(Vector3 _nextPos)
    {
        spawnPoint = _nextPos;
        foreach (Client _client in members.Values)
        {
            if (_client.player != null)
            {
                _client.player.isHanging = false;
                _client.player.isJumping = false;
                _client.player.server.serverSend.RopeACK(_client.player);
                _client.player.gameObject.transform.localPosition = spawnPoint;
            }
        }
    }

    public void InitRoomPos(Vector3 _mapAddPosition, Vector2 _mapsize) {
        mapAddPosition = _mapAddPosition;
        mapSize = _mapsize;
        room.transform.localPosition = new Vector3(0, 0, 0) + _mapAddPosition;
        for(int i = 0; i < room.transform.childCount; ++i)
        {
            GameObject now = room.transform.GetChild(i).gameObject;
            switch(now.name)
            {
                case "TileGroup":
                    TileGroupGrid = now.GetComponent<Grid>();
                    TileGroup = now.transform.GetChild(0).GetComponent<Tilemap>();
                    DeathZone = now.transform.GetChild(1).GetComponent<Tilemap>();
                    FragileGroup = now.transform.GetChild(2).GetComponent<Tilemap>();
                    BlockGroup = now.transform.GetChild(3).GetComponent<Tilemap>();
                    break;
                case "ItemGroup":
                    ItemGroup = now;
                    break;
                case "EnemyGroup":
                    EnemyGroup = now;
                    break;
                case "PlayerGroup":
                    PlayerGroup = now;
                    break;
                case "MapUtilGroup":
                    MapUtilGroup = now;
                    break;
            }
        }
    }

//

    public void SpawnPlayerAndItems()
    {
        foreach(Client _member in members.Values)
        {
            GameObject playerClone = RoomManager.instance.InstatiatePlayer(PlayerGroup, 0);
            playerClone.name = "Player " + _member.id;
            playerClone.transform.localPosition = spawnPoint;

            Player _player = playerClone.GetComponent<Player>();
            _player.Initialize(_member.id, "tester");
            _player.server = NetworkManager.instance.servers[serverPort];
            _player.room = this;
            _member.player = _player;

            // TileGroupGrid.GetComponent<MapManager>().players.Add(playerClone);
        }

        // TileGroupGrid.GetComponent<MapManager>().Init();

        foreach(Client _member in members.Values)
        {
            // 내 클론을 스폰
            foreach (Client _client in members.Values)
            {
                Debug.Log(_member.id + ", " + _client.id);

                if (_client.player != null)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnPlayer(_client.id, _member.player);
                }
            }
            if(items != null) {
                foreach (Item _item in items.Values)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnItem(_member.id, _item);
                }
            }
            if(enemies != null) {
                foreach (Enemy _enemy in enemies.Values)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnEnemy(_enemy, _member.id);
                }
            }
            //if (enemies != null)
            //{
            //    foreach (Boss1 _boss in boss.Values)
            //    {
            //        NetworkManager.instance.servers[serverPort].serverSend.SpawnBoss(_boss, _member.id);
            //    }
            //}
            if (doors != null) {
                foreach(KeyValuePair<int, Door> _door in doors)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnDoor(_door.Value, _member.id);
                }
            }
        }
        NetworkManager.instance.servers[serverPort].serverSend.AllSpawned(roomId);
    }

    private void SetMapInfo(int map_id)
    {
        RoomManager.instance.LoadMap(this, map_id);
    }

    public Vector3Int GetTopLeftBasePosition(Vector3 position)
    {
        // local X. Calculate with world position.
        Vector3Int temp = TileGroup.WorldToCell(position);
        return new Vector3Int(temp.x + TileGroup.size.x, TileGroup.size.y - temp.y);
    }
}