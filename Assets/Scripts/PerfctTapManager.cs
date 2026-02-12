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

        // حفظ نقطة البداية الصحيحة
        if (movingCircle != null)
            circleStartPosition = movingCircle.localPosition;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        AudioListener.pause = isMuted;

        // استرجاع لون الدائرة
        int savedColor = PlayerPrefs.GetInt("CircleColorIndex", 0);
        SetCircleColorByIndex(savedColor);

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

        currentSpeed = startSpeed;
        moveDistance = moveDistanceChallenge;
        perfectThreshold = perfectThresholdChallenge;
        speedMultiplier = speedMultiplierChallenge;

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

        if (distance <= perfectThreshold)
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
    public void SetDifficulty(int level)
    {
        switch (level)
        {
            case 0: // Easy
                startSpeed = 150f;
                perfectThresholdChallenge = 400f;
                break;

            case 1: // Medium
                startSpeed = 312f;
                perfectThresholdChallenge = 200f;
                break;

            case 2: // Hard
                startSpeed = 500f;
                perfectThresholdChallenge = 100f;
                break;
        }

        currentSpeed = startSpeed;
        perfectThreshold = perfectThresholdChallenge;
    }

    // ===================== COLOR BUTTONS =====================
    public void SetCircleColorByIndex(int index)
    {
        if (circleImage == null) return;

        switch (index)
        {
            case 0: circleImage.color = Color.white; break;
            case 1: circleImage.color = Color.red; break;
            case 2: circleImage.color = Color.green; break;
            case 3: circleImage.color = Color.blue; break;
            case 4: circleImage.color = Color.yellow; break;
            default: circleImage.color = Color.white; break;
        }

        PlayerPrefs.SetInt("CircleColorIndex", index);
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