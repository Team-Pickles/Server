
/*
타일: 위치, 타입
적 스포너: 위치, 적타입
아이템 스포너:  위치, 아이템 타입
플레이어 스포너: 위치, 캐릭터 타입
*/

public enum InfoTypes {
    tile = 0, enemy, item, player
}

public enum TileTypes {
    PlatformerTiles_0 = 0, PlatformerTiles_1, PlatformerTiles_2, tiles_packed_80, tiles_packed_81,
    tiles_packed_82, tiles_packed_83, tiles_packed_100, tiles_packed_101, tiles_packed_102,
    tiles_packed_103, tiles_packed_120, tiles_packed_121, tiles_packed_122, tiles_packed_123,
    tiles_packed_140, tiles_packed_141, tiles_packed_142, tiles_packed_143,
}

public enum ItemTypes {
    GoldCoin = 0, SilverCoin, BronzeCoin, Goal
}

public enum EnemyTypes {
    Enemy = 0
}

public enum PlayerTypes {
    player = 0
}