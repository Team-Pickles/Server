using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region TCP
    private static void sendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void sendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayer; i++)
            Server.clients[i].tcp.SendData(_packet);
    }

    private static void sendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayer; i++)
            if (i != _exceptClient)
                Server.clients[i].tcp.SendData(_packet);
    }
    #endregion

    #region udp
    private static void sendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void sendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i=1; i<Server.MaxPlayer; i++) 
            Server.clients[i].udp.SendData(_packet);
    }

    private static void sendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayer; i++)
            if (i != _exceptClient)
            Server.clients[i].udp.SendData(_packet);
    }
    #endregion

    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.tcpConnenctinCheck))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);
            
            sendTCPData(_toClient, _packet);
        }
    }

    public static void UDPTest(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.udpTest))
        {
            _packet.Write("UDP pakcet for Test");
            sendUDPData(_toClient, _packet);
        }
    }

    public static void PlayerDisnconnect(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            sendTCPDataToAll(_packet);
        }
    }

    //소환할 때 한번만 사용하기 때문에 TCP로 전송
    //TCP는 도착이 보장됨
    public static void SpawnPlayer(int _toclient, Player _player)
    {
        
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            Debug.Log($"server send {(int)ServerPackets.spawnPlayer}, {_toclient}");
            sendTCPData(_toclient, _packet);

        }
    }

    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            sendUDPDataToAll(_packet);
        }
    }

    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);
            sendUDPDataToAll(_packet);
        }
    }

    public static void SpawnProjectile(Projectile _projectile, int _thrownByplayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            _packet.Write(_thrownByplayer);

            sendTCPDataToAll(_packet);
        }
    }

    public static void ProjectilesPosition(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectilePosition))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            sendUDPDataToAll(_packet);
        }
    }

    public static void ProjectilesExploded(Projectile _projectile, Collider2D[] _colliders)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExploded))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            _packet.Write(_colliders.Length);
            foreach (Collider2D _collider in _colliders) {
                _packet.Write(_collider.transform.position);
            }

            sendTCPDataToAll(_packet);
        }
    }

    public static void SpawnBullet(Bullet _bullet, int _thrownByplayer)
    {
        using(Packet _packet = new Packet((int)ServerPackets.spawnBullet))
        {
            _packet.Write(_bullet.id);
            _packet.Write(_bullet.transform.position);
            _packet.Write(_thrownByplayer);

            sendTCPDataToAll(_packet);
        }
    }

    public static void BulletPosition(Bullet _bullet)
    {
        using(Packet _packet = new Packet((int)ServerPackets.bulletPosition))
        {
            _packet.Write(_bullet.id);
            _packet.Write(_bullet.transform.position);

            sendUDPDataToAll(_packet);
        }
    }

    public static void BulletCollide(Bullet _bullet)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bulletCollide))
        {
            _packet.Write(_bullet.id);

            sendTCPDataToAll(_packet);
        }
    }

    public static void SpawnItem(int _toclient, Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnItem))
        {
            _packet.Write(_item.id);
            _packet.Write(_item.transform.position);

            sendTCPData(_toclient, _packet);
        }
    }

    public static void ItemPosition(Item _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.ItemPosition))
        {
            _packet.Write(_item.id);
            _packet.Write(_item.transform.position);

            sendUDPDataToAll(_packet);
        }
    }
    public static void ItemCollide(int _itemID, int _exceptClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemCollide))
        {
            _packet.Write(_itemID);

            sendTCPDataToAll(_exceptClient, _packet);
        }
    }


}
