using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void TCPConnenctinCheckReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connencted successfully and is now player {_fromClient}");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumes the wrong client ID({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void UDPTestReceive(int _fromClient, Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"{_msg}");
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        // 패킷에서 길이를 읽어와서 그 길이 만큼의 부울 배열을 만든다.
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void playerThrowItem(int _fromClient, Packet _packet)
    {
        Debug.Log($"get gerenade from {_fromClient}");
        Vector3 _throwDirection = _packet.ReadVector3();
        Server.clients[_fromClient].player.ThrowItem(_throwDirection);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }


    public static void PlayerStartVaccume(int _fromClient, Packet _packet)
    {
        Vector3 _vaccumeDirection = _packet.ReadVector3();
        Server.clients[_fromClient].player.StartVaccume(_vaccumeDirection);
    }

    public static void PlayerEndVaccume(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.EndVaccume();
    }

    public static void ItemCollide(int _fromClient, Packet _packet)
    {
        int _ItemID = _packet.ReadInt();
        Item.items[_ItemID].DeleteItem();
        ServerSend.ItemCollide(_ItemID, _fromClient);
    }
}
