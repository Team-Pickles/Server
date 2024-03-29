using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    private Server server;

    public ServerHandle(Server server)
    {
        this.server = server;
    }
    public void TCPConnenctinCheckReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();
        string _roomId = _packet.ReadString();

        Debug.Log($"{server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connencted successfully and is now player {_fromClient}");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumes the wrong client ID({_clientIdCheck})!");
        }
        server.clients[_fromClient].username = _username;
        server.clients[_fromClient].roomId = _roomId;
        RoomManager.instance.JoinRoom(_roomId, _fromClient, server.Port);
    }

    public void UDPTestReceive(int _fromClient, Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"{_msg}");
    }

    public void PlayerMovement(int _fromClient, Packet _packet)
    {
        // 패킷에서 길이를 읽어와서 그 길이 만큼의 부울 배열을 만든다.
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public void PlayerBullet(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        server.clients[_fromClient].player.ShootBullet(_shootDirection);
    }

    public void PlayerGrenade(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        server.clients[_fromClient].player.ShootGrenade(_shootDirection);
    }


    public void PlayerStartVaccume(int _fromClient, Packet _packet)
    {
        Vector3 _vaccumeDirection = _packet.ReadVector3();
        server.clients[_fromClient].player.StartVaccume(_vaccumeDirection);
    }

    public void PlayerEndVaccume(int _fromClient, Packet _packet)
    {
        server.clients[_fromClient].player.EndVaccume();
    }

    public void ItemCollide(int _fromClient, Packet _packet)
    {
        int _ItemID = _packet.ReadInt();
        string _roomId = server.clients[_fromClient].roomId;
        int _itemType = server.rooms[_roomId].items[_ItemID].itemType;
        server.rooms[_roomId].items[_ItemID].DeleteItem();
        int _cnt = 0;
        if (_itemType == (int)TileTypes.trash)
        {
            server.clients[_fromClient].player.BulletCount += 10;
            _cnt = server.clients[_fromClient].player.BulletCount;
        }
        else if (_itemType == (int)TileTypes.grenade2)
        {
            server.clients[_fromClient].player.GrenadeCount += 5;
            _cnt = server.clients[_fromClient].player.GrenadeCount;
        }

        server.serverSend.ItemCollide(_ItemID, _roomId, _fromClient);
        server.serverSend.ItemPickedUp(_itemType, _cnt, _fromClient);
    }

    public void PlayerJump(int _fromClient, Packet _packet)
    {
        server.clients[_fromClient].player.Jump();
    }

    public void PlayerRopeMove(int _fromClient, Packet _packet)
    {
        server.clients[_fromClient].player.OnRopeAction();
    }

    public void ReadyToStartGame(int _fromClient, Packet _packet)
    {
        string _roomId = _packet.ReadString();
        server.rooms[_roomId].readyPlayerCount += 1;
        Debug.Log("READY TO START: " + server.rooms[_roomId].readyPlayerCount + "/" + server.rooms[_roomId].members.Count);
        if (server.rooms[_roomId].readyPlayerCount == server.rooms[_roomId].members.Count)
        {
            server.serverSend.StartGame(_roomId, server.rooms[_roomId].mapId);
            server.rooms[_roomId].StartGame(server.rooms[_roomId].mapId);
            server.rooms[_roomId].readyPlayerCount = 0;
        }
        //
    }

    public void GoToNextPortal(int _fromClient, Packet _packet)
    {
        string _roomId = _packet.ReadString();
        int _doorId = _packet.ReadInt();
        Door _door = server.rooms[_roomId].doors[_doorId];
        _door.GoToNextPortal(server.clients[_fromClient]);
    }

    public void ReadyToRestartGame(int _fromClient, Packet _packet)
    {
        string _roomId = _packet.ReadString();
        bool _sayYes = _packet.ReadBool();
        if (_sayYes)
        {
            server.rooms[_roomId].readyPlayerCount += 1;
            Debug.Log("READY TO RESTART: " + server.rooms[_roomId].readyPlayerCount);
            if (server.rooms[_roomId].readyPlayerCount == server.rooms[_roomId].members.Count)
            {
                server.serverSend.RestartGame(_roomId, true);
                RoomManager.instance.ResetRoom(server.rooms[_roomId]);
                server.rooms[_roomId].StartGame(server.rooms[_roomId].mapId);
                server.rooms[_roomId].readyPlayerCount = 0;
            }
            else
            {
                server.serverSend.askToRestart(_roomId, _fromClient);
            }
        }
        else
        {
            server.rooms[_roomId].readyPlayerCount = 0;
            Debug.Log("Do not restart");
            server.serverSend.RestartGame(_roomId, false);
        }
    }
}
