using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class PerfectTapManager : MonoBehaviour
{
    // ===================== UI PANELS =====================
    [Header("--- UI PANELS ---")]
    public GameObject startPanel;
    public GameObject gameplayPanel;
    public GameObject settingsPanel;
    public GameObject rewardButton;

    // ===================== UI TEXT =====================
    [Header("--- UI TEXT ---")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI gameOverText;

    // ===================== GAME OBJECTS =====================
    [Header("--- GAME OBJECTS ---")]
    public Transform centerLine;
    public Transform movingCircle;
    private Image circleImage;

    // ===================== AUDIO =====================
    [Header("--- AUDIO ---")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;

    // ===================== CHALLENGE SETTINGS =====================
    [Header("--- CHALLENGE SETTINGS ---")]
    public float startSpeed = 312f;            // Start Speed
    public float moveDistanceChallenge = 500f; // Move Distance
    public float perfectThresholdChallenge = 200f; // Perfect Threshold
    public float speedMultiplierChallenge = 0.5f;  // Speed Multiplier

    // ===================== RUNTIME VALUES =====================
    private float currentSpeed;
    private float moveDistance;
    private float perfectThreshold;
    private float speedMultiplier;

    private int score = 0;
    private int bestScore = 0;
    private int direction = 1;

    private bool isGameActive = false;
    private bool isInSettings = false;
    private bool isMuted = false;
    private bool canTap = false;

    private Vector3 circleStartPosition;

    // ===================== START =====================
    void Start()
    {
        if (movingCircle != null)
            circleImage = movingCircle.GetComponent<Image>();

        // ensure the ball is always white regardless of any prefs
        if (circleImage != null)
            circleImage.color = Color.white;

        // حفظ نقطة البداية الصحيحة
        if (movingCircle != null)
            circleStartPosition = movingCircle.localPosition;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        AudioListener.pause = isMuted;

        // grab a reference to the difficulty manager singleton if it exists
        difficultyManager = DifficultyManager.Instance;

        ShowMainMenu();
    }

    // ===================== UPDATE =====================
    void Update()
    {
        if (!isGameActive || isInSettings) return;

        // تحريك الدائرة
        movingCircle.localPosition += Vector3.right * direction * currentSpeed * Time.deltaTime;

        if (Mathf.Abs(movingCircle.localPosition.x - circleStartPosition.x) >= moveDistance)
            direction *= -1;

#if UNITY_EDITOR
        if (canTap &&
            Input.GetMouseButtonDown(0) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            HandleTap();
        }
#else
        if (canTap &&
            Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began &&
            !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            HandleTap();
        }
#endif
    }

    // ===================== GAME FLOW =====================
    public void StartGame()
    {
        score = 0;

        // let the difficulty manager adjust speeds, thresholds and line length
        if (difficultyManager != null)
        {
            difficultyManager.ApplySettings(this);
        }
        else
        {
            // fallback to previous values
            currentSpeed = startSpeed;
            moveDistance = moveDistanceChallenge;
            perfectThreshold = perfectThresholdChallenge;
            speedMultiplier = speedMultiplierChallenge;
        }

        direction = 1;
        isGameActive = true;
        isInSettings = false;
        canTap = false;

        startPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        settingsPanel.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        rewardButton?.SetActive(false);

        movingCircle.localPosition = circleStartPosition;
        UpdateUI();

        Invoke(nameof(EnableTap), 0.25f);
    }

    private void EnableTap()
    {
        canTap = true;
    }

    private void HandleTap()
    {
        if (!canTap) return;

        float distance = Mathf.Abs(
            movingCircle.localPosition.x - centerLine.localPosition.x
        );

        // compute hit zone exactly as half the centre line width; if we can't get the RectTransform fall back to cached threshold
        float threshold = perfectThreshold;
        if (centerLine != null)
        {
            RectTransform rt = centerLine.GetComponent<RectTransform>();
            if (rt != null)
                threshold = rt.sizeDelta.x * 0.5f;
        }

        if (distance <= threshold)
        {
            score++;
            currentSpeed += speedMultiplier;
            PlaySound(successSound);
            StartCoroutine(FlashColor(Color.green));
            UpdateUI();
        }
        else
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        canTap = false;

        PlaySound(failSound);
        StartCoroutine(FlashColor(Color.red));

        gameOverText.gameObject.SetActive(true);
        rewardButton?.SetActive(true);

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        Invoke(nameof(ShowMainMenu), 2.5f);
    }

    private void ShowMainMenu()
    {
        Time.timeScale = 1f;

        isGameActive = false;
        isInSettings = false;
        canTap = false;

        startPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        settingsPanel.SetActive(false);

        movingCircle.localPosition = circleStartPosition;
        UpdateUI();
    }

    // ===================== SETTINGS =====================
    public void OpenSettings()
    {
        isInSettings = true;
        canTap = false;
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        isInSettings = false;
        canTap = true;
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ===================== AUDIO =====================
    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.pause = isMuted;
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && !isMuted)
            audioSource.PlayOneShot(clip);
    }

    // ===================== DIFFICULTY BUTTONS =====================
    // called by UI buttons to change the difficulty
    public void SetDifficulty(int level)
    {
        if (difficultyManager == null)
            difficultyManager = DifficultyManager.Instance;

        if (difficultyManager != null)
        {
            difficultyManager.SetLevel((DifficultyManager.Level)level);
            difficultyManager.ApplySettings(this);
        }
    }

    // public helper used by DifficultyManager so it doesn't need to reach into our private fields
    public void ConfigureForDifficulty(float ballSpeed, float hitZoneSize, float centerLineLength)
    {
        startSpeed = ballSpeed;
        currentSpeed = ballSpeed;
        perfectThresholdChallenge = hitZoneSize;
        moveDistanceChallenge = centerLineLength * 0.5f;

        // also update runtime variables
        perfectThreshold = hitZoneSize;
        moveDistance = centerLineLength * 0.5f;

        // adjust the center line transform if available
        if (centerLine != null)
        {
            RectTransform rt = centerLine.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 size = rt.sizeDelta;
                size.x = centerLineLength;
                rt.sizeDelta = size;
            }
        }
    }

    // ===================== COLOR BUTTONS =====================
    // circle color is now always white; color selection removed
    // kept here for legacy, but it does nothing
    public void SetCircleColorByIndex(int index)
    {
        if (circleImage == null) return;
        circleImage.color = Color.white;
    }

    // allow UI to change background via the background manager
    public void SetBackgroundColor(int index)
    {
        if (BackgroundColorManager.Instance != null)
            BackgroundColorManager.Instance.SetBackground((BackgroundColorManager.BgColor)index);
    }

    // ===================== UI =====================
    private void UpdateUI()
    {
        scoreText.text = score.ToString();
        bestScoreText.text = "Best: " + bestScore;
    }

    private IEnumerator FlashColor(Color col)
    {
        if (circleImage == null) yield break;

        Color original = circleImage.color;
        circleImage.color = col;
        yield return new WaitForSecondsRealtime(0.1f);
        circleImage.color = original;
    }
}