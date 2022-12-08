using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private Server server;

    public ServerSend(Server server)
    {
        this.server = server;
    }

    #region TCP
    private void sendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        server.clients[_toClient].tcp.SendData(_packet);
    }

    private void sendTCPDataToAllInRoom(string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        foreach(KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
            _member.Value.tcp.SendData(_packet);
    }

    private void sendTCPDataToAllInRoom(int _exceptClient, string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        foreach(KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
            if (_member.Key != _exceptClient)
                _member.Value.tcp.SendData(_packet);
    }

    private void sendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= server.MaxPlayer; i++)
            server.clients[i].tcp.SendData(_packet);
    }

    private void sendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= server.MaxPlayer; i++)
            if (i != _exceptClient)
                server.clients[i].tcp.SendData(_packet);
    }
    #endregion

    #region udp
    private void sendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        server.clients[_toClient].udp.SendData(_packet);
    }

    private void sendUDPDataToAllInRoom(string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        Room _room = null;
        server.rooms.TryGetValue(_roomId, out _room);
        if(_room!=null){
            foreach(KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
            _member.Value.udp.SendData(_packet);
        }
    }

    private void sendUDPDataToAllInRoom(int _exceptClient, string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        Room _room = null;
        server.rooms.TryGetValue(_roomId, out _room);
        if(_room!=null){
            foreach(KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
                if (_member.Key != _exceptClient)
                    _member.Value.udp.SendData(_packet);
        }
    }

    private void sendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i=1; i< server.MaxPlayer; i++)
            server.clients[i].udp.SendData(_packet);
    }

    private void sendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < server.MaxPlayer; i++)
            if (i != _exceptClient)
                server.clients[i].udp.SendData(_packet);
    }
    #endregion

    public void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.tcpConnenctinCheck))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);
            
            sendTCPData(_toClient, _packet);
        }
    }

    public void JoinDone(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.roomJoined))
        {
            string _roomId = server.clients[_toClient].roomId;
            _packet.Write(server.rooms[_roomId].mapId);
            string memberNames = "";
            foreach(Client _member in server.rooms[_roomId].members.Values)
            {
                memberNames += _member.username + ",";
            }
            _packet.Write(memberNames);
            _packet.Write(server.rooms[_roomId].roomName);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }

    public void UDPTest(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.udpTest))
        {
            _packet.Write("UDP pakcet for Test");
            sendUDPData(_toClient, _packet);
        }
    }

    #region PlayerSend
    public void PlayerDisnconnect(int _playerId, string _roomId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            string memberNames = "";
            foreach(Client _member in server.rooms[_roomId].members.Values)
            {
                memberNames += _member.username + ",";
            }
            _packet.Write(memberNames);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }


    //소환할 때 한번만 사용하기 때문에 TCP로 전송
    //TCP는 도착이 보장됨
    public void SpawnPlayer(int _toclient, Player _player)
    {
        
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.localPosition);
            _packet.Write(_player.transform.rotation);

            Debug.Log($"server send {(int)ServerPackets.spawnPlayer}, {_toclient}");
            sendTCPData(_toclient, _packet);

        }
    }

    public void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.localPosition);

            sendUDPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }

    public void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);
            sendUDPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }

    public void SendFlip(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.charactorFlip))
        {
            _packet.Write(_player.id);
            _packet.Write(_player._flip);
            sendUDPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }

    public void RopeACK(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.ropeACK))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.isHanging);
            sendUDPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }

    public void PlayerDamaged(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player._hp);
            Debug.Log("Damaged : "+_player._hp);
            sendTCPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }
    #endregion

    #region 투사체
    public void SpawnProjectile(Projectile _projectile, int _thrownByplayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.localPosition);
            _packet.Write(_thrownByplayer);
            sendTCPDataToAllInRoom(server.clients[_thrownByplayer].roomId, _packet);
        }
    }

    public void ProjectilesPosition(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectilePosition))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.localPosition);
            sendUDPDataToAllInRoom(server.clients[_projectile.thrownByPlayer].roomId, _packet);
        }
    }

    public void ProjectilesExploded(Projectile _projectile, List<Vector3Int> _positions)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExploded))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.localPosition);
            _packet.Write(_positions.Count);
            foreach (Vector3Int _position in _positions) {
                _packet.Write(_position);
            }
            sendTCPDataToAllInRoom(server.clients[_projectile.thrownByPlayer].roomId, _packet);
        }
    }

    public void SpawnBullet(Bullet _bullet, int _thrownByplayer)
    {
        using(Packet _packet = new Packet((int)ServerPackets.spawnBullet))
        {
            _packet.Write(_bullet.id);
            _packet.Write(_bullet.transform.localPosition);
            _packet.Write(_thrownByplayer);

            sendTCPDataToAllInRoom(server.clients[_thrownByplayer].roomId, _packet);
        }
    }

    public void BulletPosition(Bullet _bullet)
    {
        using(Packet _packet = new Packet((int)ServerPackets.bulletPosition))
        {
            _packet.Write(_bullet.id);
            _packet.Write(_bullet.transform.localPosition);

            sendUDPDataToAllInRoom(server.clients[_bullet.thrownByPlayer].roomId, _packet);
        }
    }

    public void BulletCollide(Bullet _bullet)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bulletCollide))
        {
            _packet.Write(_bullet.id);
            sendTCPDataToAllInRoom(server.clients[_bullet.thrownByPlayer].roomId, _packet);
        }
    }
    #endregion

    #region ITem
    public void SpawnItem(int _toclient, Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnItem))
        {
            _packet.Write(_item.id);
            _packet.Write(_item.transform.localPosition);
            _packet.Write(_item.itemType);
            Debug.Log("spawn item");
            sendTCPData(_toclient, _packet);
        }
    }

    public void ItemPosition(Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.ItemPosition))
        {
            _packet.Write(_item.id);
            _packet.Write(_item.transform.localPosition);

            sendUDPDataToAllInRoom(_item.roomId, _packet);
        }
    }
    public void ItemCollide(int _itemID, string _roomId, int _exceptClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemCollide))
        {
            _packet.Write(_itemID);
            sendTCPDataToAllInRoom(_exceptClient, _roomId, _packet);
        }
    }

    public void SpringColorChange(Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.springColorChange))
        {
            _packet.Write(_item.id);
            sendTCPDataToAllInRoom(_item.roomId, _packet);
        }
    }
    #endregion

    #region Enemy
    public void SpawnEnemy(Enemy _enemy, int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            _packet.Write(_enemy.id);
            _packet.Write(_enemy.transform.localPosition);
            Debug.Log("spawn enemy");
            sendTCPData(_toClient, _packet);
        }
    }

    public void EnemyPosition(Enemy _enemy)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enemyPosition))
        {
            _packet.Write(_enemy.id);
            _packet.Write(_enemy.transform.localPosition);
            _packet.Write(_enemy._isMove);

            sendUDPDataToAllInRoom(_enemy.room.roomId, _packet);
        }
    }

    public void EnemyHit(Enemy _enemy)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enemyHit))
        {
            _packet.Write(_enemy.id);
            Debug.Log("EnemyHit_" + _enemy.id);
            sendUDPDataToAllInRoom(_enemy.room.roomId, _packet);
        }
    }

    #endregion

    public void StartGame(string _roomId, int _mapId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.startGame))
        {
            _packet.Write(_mapId);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }
}
