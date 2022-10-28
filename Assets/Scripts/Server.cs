using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


public class Server
{
    public static int MaxPlayer { get; private set; }

    public static int Port { get; private set; }

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static Dictionary<int,Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandler;

    public static void Start(int _maxPlayer, int _port)
    {
        MaxPlayer = _maxPlayer;
        Port = _port;

        Debug.Log($"Starting Server....");
        InitalizeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();

        //비동기 연결
        tcpListener.BeginAcceptTcpClient(TCPConnectionCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    private static void TCPConnectionCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectionCallback), null);

        Debug.Log($"Incomming connenction from {_client.Client.RemoteEndPoint}...");
        for (int i = 1; i <= MaxPlayer; i++)
        {
            //해당 객체가 비어있다면
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }
        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server Full!");
    }

    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                //등록 안되어 있으면 등록하기
                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                // 기존의 연결인지 확인 (아무나 못보내게)
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error occured with : {_ex}");
        }
    }



    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error occured with : {_ex}");
        }
    }


    private static void InitalizeServerData()
    {
        for (int i=1; i<= MaxPlayer; i++)
        {
            clients.Add(i,new Client(i));
        }

        packetHandler = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.TCPConnenctinCheckReceived, ServerHandle.TCPConnenctinCheckReceived },
            { (int)ClientPackets.udpTestReceive, ServerHandle.UDPTestReceive },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.playerThrowItem, ServerHandle.playerThrowItem },
            { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
            { (int)ClientPackets.playerStartVacuume, ServerHandle.PlayerStartVaccume },
            { (int)ClientPackets.playerEndVacuume, ServerHandle.PlayerEndVaccume },
            { (int)ClientPackets.ItemCollide, ServerHandle.ItemCollide },
        };

        Debug.Log("Initialized pakcets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

}
