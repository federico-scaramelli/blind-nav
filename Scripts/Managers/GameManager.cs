using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Static;
using Utilities.Time;
using Debug = UnityEngine.Debug;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private List<string> levels;
    private int _currentLevel;
    
    [Space]
    //Public access
    public TimersHandler timersHandler;
    public StopwatchHandler stopwatchHandler;
    public SaveDataManager saveDataManager;

    public override void Awake()
    {
        base.Awake();

        levels = new List<string>();

        timersHandler = GetComponent<TimersHandler>();
        if (!timersHandler)
            timersHandler = gameObject.AddComponent<TimersHandler>();
        
        stopwatchHandler = GetComponent<StopwatchHandler>();
        if (!stopwatchHandler)
            stopwatchHandler = gameObject.AddComponent<StopwatchHandler>();
        
        saveDataManager = GetComponent<SaveDataManager>();
        if (!saveDataManager)
            saveDataManager = gameObject.AddComponent<SaveDataManager>();

        for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // Debug.Log(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            levels.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }
        // levels = new[] { "Level 1", "Level 2", "Level 3"};
        _currentLevel = levels.IndexOf(SceneManager.GetActiveScene().name);
        
        // EndGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        if(EditorApplication.isPlaying)
            EditorApplication.ExitPlaymode();
        else
#endif
            Application.Quit();
    }

    public void NextLevel()
    {
        stopwatchHandler.ResetHandler();
        timersHandler.ResetHandler();

        if (++_currentLevel < levels.Count)
            SceneManager.LoadScene(levels[_currentLevel]);
        else
            EndGame();
    }

    public void EndGame()
    {
        Application.OpenURL("https://forms.gle/J1kEsFQCeakddBEGA");
        Application.OpenURL("https://forms.gle/di18HbkBHJAhZTnk8");
        
        Process process = new Process();
        // process.StartInfo.FileName = "explorer.exe";
        process.StartInfo.FileName = (Application.platform == RuntimePlatform.WindowsPlayer 
                                        || Application.platform == RuntimePlatform.WindowsEditor) 
                                        ? "explorer.exe" : "open";
        // Debug.Log("Trying to open " + Application.persistentDataPath + " with " + process.StartInfo.FileName);
        // process.StartInfo.Arguments = "file://"+Application.persistentDataPath;
        process.StartInfo.Arguments = "file://"+saveDataManager.path;
        process.Start();
        
        QuitGame();
    }

    [ContextMenu("End Game")]
    public void ForceEndGame()
    {
        saveDataManager.CreatePath();
        EndGame();
    }
}
