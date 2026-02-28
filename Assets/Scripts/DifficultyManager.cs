using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum Level { Easy = 0, Medium = 1, Hard = 2 }
    public Level currentLevel = Level.Medium;

    [Header("Easy Settings")]
    public float easyLineLength = 800f;
    public float easyBallSpeed = 150f;
    public float easyHitZoneSize = 400f;

    [Header("Medium Settings")]
    public float mediumLineLength = 330f;
    public float mediumBallSpeed = 312f;
    public float mediumHitZoneSize = 200f;

    [Header("Hard Settings")]
    public float hardLineLength = 100f;
    public float hardBallSpeed = 500f;
    public float hardHitZoneSize = 100f;

    // singleton pattern for easy access
    public static DifficultyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetLevel(Level level)
    {
        currentLevel = level;
    }

    public void ApplySettings(PerfectTapManager pm)
    {
        if (pm == null) return;

        float speed, hitZone, lineLength;
        switch (currentLevel)
        {
            case Level.Easy:
                speed = easyBallSpeed;
                hitZone = easyHitZoneSize;
                lineLength = easyLineLength;
                break;
            case Level.Medium:
                speed = mediumBallSpeed;
                hitZone = mediumHitZoneSize;
                lineLength = mediumLineLength;
                break;
            case Level.Hard:
                speed = hardBallSpeed;
                hitZone = hardHitZoneSize;
                lineLength = hardLineLength;
                break;
            default:
                speed = mediumBallSpeed;
                hitZone = mediumHitZoneSize;
                lineLength = mediumLineLength;
                break;
        }

        // delegate configuration to the manager itself (keeps its internals private)
        pm.ConfigureForDifficulty(speed, hitZone, lineLength);
    }
}
