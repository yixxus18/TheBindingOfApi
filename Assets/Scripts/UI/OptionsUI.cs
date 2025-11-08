using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OptionsUI : MonoBehaviour
{
    [Header("Keybinding UI")]
    [SerializeField] private Button moveUpButton;
    [SerializeField] private TMP_Text moveUpText;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private TMP_Text moveDownText;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private TMP_Text moveLeftText;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private TMP_Text moveRightText;
    [SerializeField] private Button attackButton;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private Button dodgeButton;
    [SerializeField] private TMP_Text dodgeText;
    [SerializeField] private Button interactButton;
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private TMP_Text pauseText;
    [SerializeField] private Button toggleMenuButton;
    [SerializeField] private TMP_Text toggleMenuText;

    [Header("Rebind Prompt")]
    [SerializeField] private GameObject pressToRebindKeyUI;

    private bool isWaitingForKey = false;
    private GameInput.Binding currentBinding;

    private void Awake()
    {
        Debug.Log("OptionsUI Awake! Asignando listeners...");
        // Ya no hay botón de cierre, solo se asignan los listeners de keybinding.
        moveUpButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.MoveUp));
        moveDownButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.MoveDown));
        moveLeftButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.MoveLeft));
        moveRightButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.MoveRight));
        attackButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.Attack));
        dodgeButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.Dodge));
        interactButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.Interact));
        pauseButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.Pause));
        toggleMenuButton.onClick.AddListener(() => StartRebinding(GameInput.Binding.ToggleMenu));
    }

    private void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnRebindBinding += (sender, e) => UpdateVisual();
        }
        HidePressToRebindKey();
    }

    private void OnEnable()
    {
        // Cada vez que su panel contenedor se active, actualizará los textos.
        UpdateVisual();
    }

    private void Update()
    {
        if (!isWaitingForKey) return;

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                GameInput.Instance.RebindBinding(currentBinding, key);
                isWaitingForKey = false;
                HidePressToRebindKey();
                break;
            }
        }
    }

    private void StartRebinding(GameInput.Binding binding)
    {
        Debug.Log("StartRebinding llamado para: " + binding.ToString());
        currentBinding = binding;
        isWaitingForKey = true;
        ShowPressToRebindKey();
    }

    private void UpdateVisual()
    {
        if (GameInput.Instance == null) return;
        Debug.Log("Actualizando visual...");
        moveUpText.text = GameInput.Instance.GetBinding(GameInput.Binding.MoveUp).ToString();
        moveDownText.text = GameInput.Instance.GetBinding(GameInput.Binding.MoveDown).ToString();
        moveLeftText.text = GameInput.Instance.GetBinding(GameInput.Binding.MoveLeft).ToString();
        moveRightText.text = GameInput.Instance.GetBinding(GameInput.Binding.MoveRight).ToString();
        attackText.text = GameInput.Instance.GetBinding(GameInput.Binding.Attack).ToString();
        dodgeText.text = GameInput.Instance.GetBinding(GameInput.Binding.Dodge).ToString();
        interactText.text = GameInput.Instance.GetBinding(GameInput.Binding.Interact).ToString();
        pauseText.text = GameInput.Instance.GetBinding(GameInput.Binding.Pause).ToString();
        toggleMenuText.text = GameInput.Instance.GetBinding(GameInput.Binding.ToggleMenu).ToString();
    }

    private void ShowPressToRebindKey() => pressToRebindKeyUI.SetActive(true);
    private void HidePressToRebindKey() => pressToRebindKeyUI.SetActive(false);
}