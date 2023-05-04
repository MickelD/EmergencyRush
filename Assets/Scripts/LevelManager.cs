using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "LevelManager", menuName = "LevelManager")]
public class LevelManager : ScriptableObject
{
    public Level[] levelArray;
    public Level currentLevel;
}

[System.Serializable]
public struct Level
{
    public int ID;

    [Space(3),Header("Game Rules"), Space(1)]
    public Vector2Int minMaxNPCsOnScene;
    public float patientFuseTime;
    public Vector2 minMaxGenerationTime;
    public float startupDelay;
    public int maxActivePatients;
    public int patientGoal;

    [Space(3), Header("Visuals"), Space(1)]
    public VolumeProfile postProcessProfile;
    public GameObject light;
    public bool lightsOn;
}
