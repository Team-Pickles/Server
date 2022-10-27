using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public static int dataBufferSize = 4096;
    public int id;
    public TCP tcp;
    public UDP udp;
    public Player player;
    

    public Client(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnencted");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisnconnect(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;

        private Packet receiveData;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Disconnect()
        {
            socket.Close();
            receiveBuffer = null;
            receiveData = null;
            socket = null;
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error Sending data to Player{id} via TCP:{_ex}");
            }
        }


        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receiveData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "TCP Connection Attept arrived to Server");
        }

        public static string ToReadbleByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }



        public void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                //NetworkStream에서 읽은 바이트 수 반환
                int _byteLength = stream.EndRead(_result);
                
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receiveData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
                Server.clients[id].Disconnect();
            }
        }

        public bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receiveData.SetBytes(_data);
            //TCP 패킷의 맨처음에는 data의 길이를 알려주는 int형 정수가 들어있음
            //그래서 먼저 int 크기 만큼이 있는지 확인하고
            //_packetLength에 data의 크기를 저장함.
            if (receiveData.UnreadLength() >= 4)
            {
                _packetLength = receiveData.ReadInt();
                if (_packetLength <= 0)
                    return true;
            }

            while (_packetLength > 0 && _packetLength <= receiveData.UnreadLength())
            {
                byte[] _packetBytes = receiveData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        
                        Server.packetHandler[_packetId](id, _packet);
                    }
                });

                _packetLength = 0;
                if (receiveData.UnreadLength() >= 4)
                {
                    _packetLength = receiveData.ReadInt();
                    if (_packetLength <= 0)
                        return true;
                }
            }

            if (_packetLength <= 1)
                return true;

            return false;
        }


    }

    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
            ServerSend.UDPTest(id);
        }

        public void Disconnect()
        {
            endPoint = null;
        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData)
        {
            int _packetLenght = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLenght);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandler[_packetId](id, _packet);
                }
            });
        }

    }

    public void SendIntoGame(string _username)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, _username);

        //새로 접속하는 클라이언트에게 
        //기존의 플레이어 정보를 넘겨줌
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                    ServerSend.SpawnPlayer(id, _client.player);
            }
        }

        //모든 클라이언트에게 새로 접속하는 클라이언트의 정보를 넘겨줌
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        foreach (Item _item in Item.items.Values)
        {
            ServerSend.SpawnItem(id, _item);
        }



    }
}