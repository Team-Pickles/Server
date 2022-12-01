using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    public static int dataBufferSize = 4096;
    public int id;
    public string username;
    public TCP tcp;
    public UDP udp;
    public Player player;
    
    private Server server;
    public string roomId;
    

    public Client(int _clientId, string _username, Server _server)
    {
        id = _clientId;
        server = _server;
        username = _username;
        tcp = new TCP(id, server);
        udp = new UDP(id, server);
    }

    
    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnencted");
        NetworkManager.instance.roomInfos.Remove(roomId);
        NetworkManager.instance.roomNameIds.Remove(roomId);
        
        ThreadManager.ExecuteOnMainThread(() =>
        {
            if(player != null)
                UnityEngine.Object.Destroy(player.gameObject);
            server.rooms[roomId].members.Remove(id);
            server.rooms[roomId].readyPlayerCount -= 1;
            if(server.rooms[roomId].members.Count == 0)
            {
                UnityEngine.Object.Destroy(server.rooms[roomId].room);
                RoomManager.instance.DeletedPosList.Add(server.rooms[roomId].mapAddPosition);
                server.rooms.Remove(roomId);
            }
            else
            {
                server.serverSend.PlayerDisnconnect(id, roomId);
            }
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();


    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;

        private Packet receiveData;
        private Server server;

        public TCP(int _id, Server server)
        {
            id = _id;
            this.server = server;
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

            server.serverSend.Welcome(id, "TCP Connection Attept arrived to Server");
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
                    server.clients[id].Disconnect();
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
                server.clients[id].Disconnect();
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
                        
                        server.packetHandler[_packetId](id, _packet);
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
        private Server server;

        public UDP(int _id, Server server)
        {
            id = _id;
            this.server = server;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
            server.serverSend.UDPTest(id);
        }

        public void Disconnect()
        {
            endPoint = null;
        }

        public void SendData(Packet _packet)
        {
            server.SendUDPData(endPoint, _packet);
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
                    server.packetHandler[_packetId](id, _packet);
                }
            });
        }

    }
    // SendIntoGame Delete.
}