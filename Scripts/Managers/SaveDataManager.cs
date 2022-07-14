using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// public class SaveDataManager : Singleton<SaveDataManager>
public class SaveDataManager : MonoBehaviour
{
    private GameData _gameData = new GameData();
    public string path;

    private void Awake()
    {
        CreatePath();
    }

    [ContextMenu("Create directory")]
    public void CreatePath()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsEditor)
        {
            path = Path.Combine(Application.dataPath, "UsageData");
            Directory.CreateDirectory(path); 
            File.WriteAllText(Path.Combine(path, "Database.json"), "");
        }
        else
        {
            path = Path.Combine(Application.persistentDataPath, "UsageData");
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "Database.json"), "");
        }
    }


    public void AddPresentation(SoundPresentationData soundPresentationData)
    {
        _gameData.tutorialData.soundPresentationsData.Add(soundPresentationData);
        
        SaveJson();
    }

    public void AddTarget(TargetData target)
    {
        if (!_gameData.levelDatas.Exists(l => l.name == target.levelName))
            _gameData.levelDatas.Add(new LevelData(target.levelName));

        LevelData level = _gameData.levelDatas.Find(l => l.name == target.levelName);
        level.targets.Add(target);
        
        SaveJson();
    }

    public void SaveLevelOverallData(OverallLevelData overallLevelData)
    {
        if (!_gameData.levelDatas.Exists(l => l.name == overallLevelData.levelName))
        {
            Debug.LogError("You tried to save overall level data, but level data does not exists yet.");
            return;
        }

        LevelData level = _gameData.levelDatas.Find(l => l.name == overallLevelData.levelName);
        level.overall = overallLevelData;
        
        SaveJson();
    }

    private void SaveJson()
    {
        //sovrascrivo tutto
        string json = JsonUtility.ToJson(_gameData, true);
        
        // System.IO.File.WriteAllText(Application.persistentDataPath + "/LevelData.json", json);

        // if (Application.platform == RuntimePlatform.WindowsPlayer
        //     || Application.platform == RuntimePlatform.WindowsEditor)
        //     File.WriteAllText(Path.Combine(path , "Database.json"), json);
        // else
            File.WriteAllText(Path.Combine(path , "Database.json"), json);
    }
}

[Serializable]
public class GameData
{
    public TutorialData tutorialData = new TutorialData();
    public List<LevelData> levelDatas = new List<LevelData>();
}

[Serializable]
public class TutorialData
{
    public List<SoundPresentationData> soundPresentationsData = new List<SoundPresentationData>();
}

[Serializable]
public class SoundPresentationData
{
    public string name;
    public int attempts; //Attempts number for the current feedback

    public SoundPresentationData(string name, int attempts)
    {
        this.name = name;
        this.attempts = attempts;
    }
}

[Serializable]
public class LevelData
{
    public string name;
    public List<TargetData> targets = new List<TargetData>();
    public OverallLevelData overall = new OverallLevelData();

    public LevelData(string name)
    {
        this.name = name;
    }
}

[Serializable]
public class OverallLevelData
{
    [NonSerialized] public string levelName;
    public float totalTimeToFinishLevel;
    public int totalSupportsAmount;
    public int totalWrongDirectionsAmount;
    public int totalDangerousObstaclesHitAmount;
    public int totalDangerousMovingObstaclesHitAmount;
    public int totalSafeObstaclesHitAmount;
    public int totalSafeMovingObstaclesHitAmount;
    public int totalEnvironmentObstacleHitAmount;
    public int totalResetAreaHitAmount;
    public int totalSupportAreaResetAmount;
    public int totalManualResetAmount;
}

[Serializable]
public class TargetData
{
    [NonSerialized] public string levelName;
    public string name;
    public float timeToReach;
    public int supportsAmount;
    public int wrongDirectionsAmount;
    public int dangerousObstaclesHitAmount;
    public int dangerousMovingObstaclesHitAmount;
    public int safeObstaclesHitAmount;
    public int safeMovingObstaclesHitAmount;
    public int environmentObstacleHitAmount;
    public int resetAreaHitAmount;
    public int supportAreaResetAmount;
    public int manualResetAmount;
}