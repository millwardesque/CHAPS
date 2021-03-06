﻿using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class LevelZone
{
    public string name;
    public int lengthInCells;
    public string spriteSetPrefix;
    public RoomGeneratorConfiguration roomConfig;
    public bool needsCeiling;
    public AudioClip backgroundMusic;
    public float intelSpawnPercentage;

    // @TODO Background
    // @TODO Music

    public static LevelZone LoadFromJSONNode(JSONNode node) {
        string name = node["name"];
        int lengthInCells = node ["lengthInCells"].AsInt;
        string spriteSetPrefix = node["spriteSetPrefix"];
        bool needsCeiling = node["needsCeiling"].AsBool;
        float intelSpawnPercentage = node ["intelSpawnPercentage"].AsFloat;

        string backgroundMusicName = node ["backgroundMusic"];
        AudioClip backgroundMusic = null;
        if (backgroundMusicName != "") {
            backgroundMusic = Resources.Load<AudioClip>(backgroundMusicName);
        }
        
        JSONNode roomConfiguration = node ["roomConfiguration"];
        RoomGeneratorConfiguration roomConfig = RoomGeneratorConfiguration.LoadFromJSONNode (roomConfiguration);

        return new LevelZone (name, lengthInCells, spriteSetPrefix, needsCeiling, backgroundMusic, intelSpawnPercentage, roomConfig);
    }

    public LevelZone(string name, int lengthInCells, string spriteSetPrefix, bool needsCeiling, AudioClip backgroundMusic, float intelSpawnPercentage, RoomGeneratorConfiguration roomConfig) {
        this.name = name;
        this.lengthInCells = lengthInCells;
        this.spriteSetPrefix = spriteSetPrefix;
        this.needsCeiling = needsCeiling;
        this.backgroundMusic = backgroundMusic;
        this.intelSpawnPercentage = intelSpawnPercentage;
        this.roomConfig = roomConfig;
    }

    public override string ToString() {
        return string.Format ("{0}: {1} cells long. Sprite Set '{2}'. Needs ceiling: {3}. Background Music: {4}. Intel Spawn: {5}%.", name, lengthInCells, spriteSetPrefix, (needsCeiling ? "Yes" : "No"), (backgroundMusic == null ? "None" : backgroundMusic.name), intelSpawnPercentage * 100f);
    }
}

