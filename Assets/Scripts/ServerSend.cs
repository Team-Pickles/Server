using System;
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
        foreach (KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
            _member.Value.tcp.SendData(_packet);
    }

    private void sendTCPDataToAllInRoom(int _exceptClient, string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        foreach (KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
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
        if (_room != null)
        {
            foreach (KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
                _member.Value.udp.SendData(_packet);
        }
    }

    private void sendUDPDataToAllInRoom(int _exceptClient, string _roomId, Packet _packet)
    {
        _packet.WriteLength();
        Room _room = null;
        server.rooms.TryGetValue(_roomId, out _room);
        if (_room != null)
        {
            foreach (KeyValuePair<int, Client> _member in server.rooms[_roomId].members)
                if (_member.Key != _exceptClient)
                    _member.Value.udp.SendData(_packet);
        }
    }

    private void sendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < server.MaxPlayer; i++)
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
            foreach (Client _member in server.rooms[_roomId].members.Values)
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
            foreach (Client _member in server.rooms[_roomId].members.Values)
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
            try
            {
                _packet.Write(_player.id);
                _packet.Write(_player.transform.localPosition);
                if (_player.GetComponent<Rigidbody2D>().velocity.x == 0)
                    _packet.Write(false);
                else
                    _packet.Write(true);
                        
                sendUDPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
            }
            catch (Exception _e)
            {
                Debug.Log(_e.Message);
            }
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
            Debug.Log("Damaged : " + _player._hp);
            sendTCPDataToAllInRoom(server.clients[_player.id].roomId, _packet);
        }
    }
    #endregion

    #region 투사체
    public void SpawnProjectile(Projectile _projectile, int _thrownByplayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            int _cnt = server.clients[_thrownByplayer].player.GrenadeCount;
            if (_cnt > 0)
            {
                _packet.Write(_projectile.id);
                _packet.Write(_projectile.transform.localPosition);
                _packet.Write(_thrownByplayer);
                _packet.Write(server.clients[_thrownByplayer].player.GrenadeCount);
                sendTCPDataToAllInRoom(server.clients[_thrownByplayer].roomId, _packet);
            }

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

    public void ProjectilesExploded(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExploded))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.localPosition);
            sendTCPDataToAllInRoom(server.clients[_projectile.thrownByPlayer].roomId, _packet);
        }
    }

    public void SpawnBullet(Bullet _bullet, int _thrownByplayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnBullet))
        {
            int _cnt = server.clients[_thrownByplayer].player.BulletCount;
            if (_cnt > 0)
            {
                _packet.Write(_bullet.id);
                _packet.Write(_bullet.transform.localPosition);
                _packet.Write(_thrownByplayer);
                _packet.Write(server.clients[_thrownByplayer].player.BulletCount);
                sendTCPDataToAllInRoom(server.clients[_thrownByplayer].roomId, _packet);
            }
        }
    }

    public void BulletPosition(Bullet _bullet)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bulletPosition))
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
            sendTCPData(_toclient, _packet);
        }
    }

    public void SpawnItem(Room _room, Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnItem))
        {
            _packet.Write(_item.id);
            _packet.Write(_item.transform.localPosition);
            _packet.Write(_item.itemType);
            sendTCPDataToAllInRoom(_room.roomId, _packet);
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
            if (_exceptClient == -1)
            {
                sendTCPDataToAllInRoom(_roomId, _packet);
            }
            else
            {
                sendTCPDataToAllInRoom(_exceptClient, _roomId, _packet);
            }
        }
    } 
    public void DestroyItem(Item item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemDestroy))
        {
            _packet.Write(item.id);
            sendTCPDataToAllInRoom(item.roomId ,_packet);
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

    public void ItemPickedUp(int _itemType, int _cnt, int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_itemType);
            _packet.Write(_cnt);
            _packet.Write(_toClient);
            sendTCPData(_toClient, _packet);
        }
    }
    #endregion

    #region Enemy
    public void SpawnEnemy(Enemy _enemy, int _toClient, int type)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            _packet.Write(_enemy.id);
            _packet.Write(type);
            _packet.Write(_enemy.transform.localPosition);
            Debug.Log("spawn enemy");
            sendTCPData(_toClient, _packet);
        }
    }
    public void SpawnBoss(Boss1 _enemy, int _toClient, int type)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnBoss))
        {
            _packet.Write(_enemy.id);
            _packet.Write(type);
            _packet.Write(_enemy.transform.parent.transform.localPosition);
            Debug.Log("spawn enemy");
            sendTCPData(_toClient, _packet);
        }
    }

    public void BossDamaged(Boss1 boss)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bossHit))
        {
            _packet.Write(boss.id);
            sendUDPDataToAllInRoom(boss.room.roomId, _packet);
        }
    }
    public void BossClear(Boss1 boss)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bossClear))
        {
            _packet.Write(boss.id);
            sendUDPDataToAllInRoom(boss.room.roomId, _packet);
        }
    } 
    
    public void AttackIndeicator(Vector2 pos, Room room)
    {
        using (Packet _packet = new Packet((int)ServerPackets.attackIndeicator))
        {
            _packet.Write(pos);
            sendUDPDataToAllInRoom(room.roomId, _packet);
        }
    }

    public void SpawnEnemy(Room _room, Enemy _enemy, int type)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            _packet.Write(_enemy.id);
            _packet.Write(type);
            _packet.Write(_enemy.transform.localPosition);
            Debug.Log("spawn enemy");
            sendTCPDataToAllInRoom(_room.roomId, _packet);
        }
    }
    // public void SpawnBoss(Boss1 _boss, int _toClient)
    // {
    //     using (Packet _packet = new Packet((int)ServerPackets.spawnBoss))
    //     {
    //         _packet.Write(_boss.id);
    //         _packet.Write(_boss.transform.localPosition);
    //         Debug.Log("spawn Boss");
    //         sendTCPData(_toClient, _packet);
    //     }
    // }



    public void EnemyPosition(Enemy _enemy)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enemyPosition))
        {
            _packet.Write(_enemy.id);
            _packet.Write(_enemy.type);
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
    
    public void EnemyDestroy(Enemy _enemy)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enemyDestroy))
        {
            _packet.Write(_enemy.id);
            sendUDPDataToAllInRoom(_enemy.room.roomId, _packet);
        }
    }

    #endregion

    public void SpawnDoor(Door _door, int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnDoor))
        {
            _packet.Write(_door.id);
            _packet.Write(_door.transform.localPosition + new Vector3(0, 0, 1));
            _packet.Write(_door.isInDoor);
            Debug.Log("spawn enemy");
            sendTCPData(_toClient, _packet);
        }
    }

    public void StartGame(string _roomId, int _mapId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.startGame))
        {
            _packet.Write(_mapId);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }

    public void AllSpawned(string _roomId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.allSpawned))
        {
            _packet.Write(true);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }

    public void askToRestart(string _roomId, int _exceptClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.askToRestart))
        {
            sendTCPDataToAllInRoom(_exceptClient, _roomId, _packet);
        }
    }

    public void RestartGame(string _roomId, bool _isRestart)
    {
        using (Packet _packet = new Packet((int)ServerPackets.restart))
        {
            _packet.Write(server.rooms[_roomId].mapId);
            _packet.Write(_isRestart);
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }

    public void FragileBreak(string _roomId, List<Vector3Int> _positions)
    {
        Debug.Log("FragileBreak");
        using (Packet _packet = new Packet((int)ServerPackets.fragileBreak))
        {
            _packet.Write(_positions.Count);
            foreach(Vector3Int _pos in _positions)
            {
                _packet.Write(_pos);
            }
            sendTCPDataToAllInRoom(_roomId, _packet);
        }
    }
}
