using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public Tilemap tilemap;
    public List<GameObject> players;

    private List<List<int>> _map = new List<List<int>>();
    private int _mapX, _mapY;
    public List<Vector3Int> playerPositions = new List<Vector3Int>();

    public Vector3Int GetTopLeftBasePosition(Vector3 position)
    {
        Vector3Int temp = tilemap.WorldToCell(position);
        return new Vector3Int(temp.x + tilemap.size.x, tilemap.size.y - temp.y);
    }
    IEnumerator SetPlayerPosition()
    {
        List<Vector3Int> oldPositions = new List<Vector3Int>();
        for(int _playerIdx = 0; _playerIdx < players.Count; _playerIdx++)
        {
            playerPositions[_playerIdx] = GetTopLeftBasePosition(players[_playerIdx].transform.position);
            oldPositions.Add(playerPositions[_playerIdx]);
        }
        while (true)
        {
            for(int _playerIdx = 0; _playerIdx < players.Count; _playerIdx++)
            {
                if(players[_playerIdx] == null) {
                    break;
                }
                _map[oldPositions[_playerIdx].y][oldPositions[_playerIdx].x] = 0;
                playerPositions[_playerIdx] = GetTopLeftBasePosition(players[_playerIdx].transform.position);
                _map[playerPositions[_playerIdx].y][playerPositions[_playerIdx].x] = 5;
                oldPositions[_playerIdx] = playerPositions[_playerIdx];
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void Init()
    {
        for(int _playerIdx = 0; _playerIdx < players.Count; _playerIdx++) {
            playerPositions.Add(GetTopLeftBasePosition(players[_playerIdx].transform.position));
        }
        _mapX = tilemap.size.x * 2 + 1;
        _mapY = tilemap.size.y * 2 + 1;
        for (int i = tilemap.size.y; i >= -tilemap.size.y; i--)
        {
            _map.Add(new List<int>());
            for (int j = -tilemap.size.x; j <= tilemap.size.x; j++)
            {
                _map[tilemap.size.y-i].Add(tilemap.HasTile(new Vector3Int(j, i, 0)) == true ? 1 : 0);
            }
        }
        StartCoroutine(SetPlayerPosition());
    }
}
