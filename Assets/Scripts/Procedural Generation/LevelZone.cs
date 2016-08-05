using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class LevelZone
{
    public string name;
    public int lengthInCells;
    public string spriteSetPrefix;
    public RoomGeneratorConfiguration roomConfig;

    // @TODO Background
    // @TODO Music

    public static LevelZone LoadFromJSONNode(JSONNode node) {
        string name = node["name"];
        int lengthInCells = node ["lengthInCells"].AsInt;
        string spriteSetPrefix = node["spriteSetPrefix"];
        JSONNode roomConfiguration = node ["roomConfiguration"];
        RoomGeneratorConfiguration roomConfig = RoomGeneratorConfiguration.LoadFromJSONNode (roomConfiguration);

        return new LevelZone (name, lengthInCells, spriteSetPrefix, roomConfig);
    }

    public LevelZone(string name, int lengthInCells, string spriteSetPrefix, RoomGeneratorConfiguration roomConfig) {
        this.name = name;
        this.lengthInCells = lengthInCells;
        this.spriteSetPrefix = spriteSetPrefix;
        this.roomConfig = roomConfig;
    }

    public override string ToString() {
        return string.Format ("{0}: {1} cells long.  Sprite Set '{2}'", name, lengthInCells, spriteSetPrefix);
    }
}

