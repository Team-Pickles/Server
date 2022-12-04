using System;
using UnityEngine;

[Serializable]
public class DataClass {
    [SerializeField] int infoType;
    [SerializeField] Vector3 pos;
    [SerializeField] int additionalInfo = -1;

    public DataClass(int infoType, Vector3 pos) {
        this.infoType = infoType;
        this.pos = pos;
    }

    public DataClass(int infoType, Vector3 pos, int additionalInfo) {
        this.infoType = infoType;
        this.pos = pos;
        this.additionalInfo = additionalInfo;
    }

    public Vector3 GetPos() {
        return pos;
    }

    public int GetInfoType() {
        return infoType;
    }

    public int GetAdditionalInfo() {
        return additionalInfo;
    }

    public override string ToString()
    {
        string datatype;
        string addInfo = "";
        switch(infoType) {
            case (int)TileTypes.Empty / 100:
                datatype = "Tile";
                addInfo = " - TileType: [" + additionalInfo + "]";
                break;
            case (int)TileTypes.Enemy / 100:
                datatype = "Enemy";
                addInfo = " - EnemyType: [" + additionalInfo + "]";
                break;
            case (int)TileTypes.Item / 100:
                datatype = "Item";
                addInfo = " - ItemType: [" + additionalInfo + "]";
                break;
            case (int)TileTypes.Player / 100:
                datatype = "Player";
                addInfo = " - PlayerType: [" + additionalInfo + "]";
                break;
            default:
                datatype = "Unkown";
                break;
        }

        return datatype + "(" + pos + ")" + addInfo;
    }
}