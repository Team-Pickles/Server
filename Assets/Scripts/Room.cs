using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public string roomId;
    public string roomName;
    public int maxPlayer;
    public int masterId;
    public int serverPort;
    
    public Vector3 mapAddPosition;

    public GameObject room;
    public GameObject PlayerGroup;
    public GameObject TileGroup;
    public GameObject EnemyGroup;
    public GameObject ItemGroup;

    public Dictionary<int, Client> members = new Dictionary<int, Client>();
    public Item items;

    public Room(string _roomId, string _roomName, int _serverPort, Vector3 _mapAddPosition, GameObject _room)
    {
        roomId = _roomId;
        roomName = _roomName;
        serverPort = _serverPort;
        mapAddPosition = _mapAddPosition;
        room = _room;
        room.transform.localPosition = new Vector3(0, 0, 0) + _mapAddPosition;
        for(int i = 0; i < room.transform.childCount; ++i)
        {
            GameObject now = room.transform.GetChild(i).gameObject;
            switch(now.name)
            {
                case "TileGroup":
                    TileGroup = now;
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
            }
        }
    }

    public void StartGame()
    {
        if(room == null)
        {
            Debug.Log("There is no mapInfo.");
            return;
        }
        Debug.Log("Spawn");
        ThreadManager.ExecuteOnMainThread(() => 
            {
                SetMapInfo(-1);
                SpawnPlayerAndItems();
            }
        );
        
    }

    public void SpawnPlayerAndItems()
    {
        foreach(Client _member in members.Values)
        {
            GameObject playerClone = RoomManager.instance.InstatiatePlayer(PlayerGroup, 0);
            playerClone.name = "Player " + _member.id;
            playerClone.transform.localPosition = new Vector3(0, 0, 0);

            Player _player = playerClone.GetComponent<Player>();
            _player.Initialize(_member.id, "tester");
            _player.server = NetworkManager.instance.servers[serverPort];
            _player.room = this;
            _member.player = _player;
        }

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
                foreach (Item _item in items.items.Values)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnItem(_member.id, _item);
                }
            }
        }
    }

    private void SetMapInfo(int map_id)
    {
        // for test
        if(map_id == -1)
        {
            RoomManager.instance.LoadTestMap(this);
        }
    }
}