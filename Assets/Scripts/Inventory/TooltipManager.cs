using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Vector2 offset;

    private RectTransform panelRectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        }

        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            panelRectTransform.position = (Vector2)Input.mousePosition + offset;
        }
    }

    public void ShowTooltip(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
        tooltipPanel.SetActive(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}