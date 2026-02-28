using UnityEngine;

public class BackgroundColorManager : MonoBehaviour
{
    public enum BgColor { Burgundy = 0, Olive = 1, Navy = 2 }

    public Color burgundy = new Color(0x7B / 255f, 0x2C / 255f, 0x39 / 255f);
    public Color olive = new Color(0x55 / 255f, 0x6B / 255f, 0x2F / 255f);
    public Color navy = new Color(0x0A / 255f, 0x1A / 255f, 0x3C / 255f);

    public BgColor currentColor = BgColor.Burgundy;

    public Camera mainCamera;
    public static BackgroundColorManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        ApplyColor();
    }

    public void SetBackground(BgColor color)
    {
        currentColor = color;
        ApplyColor();
    }

    public void ApplyColor()
    {
        if (mainCamera == null) return;

        switch (currentColor)
        {
            case BgColor.Burgundy:
                mainCamera.backgroundColor = burgundy;
                break;
            case BgColor.Olive:
                mainCamera.backgroundColor = olive;
                break;
            case BgColor.Navy:
                mainCamera.backgroundColor = navy;
                break;
        }
    }
}
