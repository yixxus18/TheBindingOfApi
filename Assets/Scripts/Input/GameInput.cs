using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnRebindBinding;

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Attack,
        Dodge,
        Interact,
        Pause,
        ToggleMenu
    }

    private KeyCode moveUpKey = KeyCode.W;
    private KeyCode moveDownKey = KeyCode.S;
    private KeyCode moveLeftKey = KeyCode.A;
    private KeyCode moveRightKey = KeyCode.D;
    private KeyCode attackKey = KeyCode.K;
    private KeyCode dodgeKey = KeyCode.J;
    private KeyCode interactKey = KeyCode.E;
    private KeyCode pauseKey = KeyCode.Escape;
    private KeyCode toggleMenuKey = KeyCode.Tab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = new Vector2(0, 0);
        if (Input.GetKey(moveUpKey)) inputVector.y = +1;
        if (Input.GetKey(moveDownKey)) inputVector.y = -1;
        if (Input.GetKey(moveLeftKey)) inputVector.x = -1;
        if (Input.GetKey(moveRightKey)) inputVector.x = +1;
        return inputVector.normalized;
    }

    public bool GetAttackPressed() => Input.GetKeyDown(attackKey);
    public bool GetDodgePressed() => Input.GetKeyDown(dodgeKey);
    public bool GetInteractPressed() => Input.GetKeyDown(interactKey);
    public bool GetPausePressed() => Input.GetKeyDown(pauseKey);
    public bool GetToggleMenuPressed() => Input.GetKeyDown(toggleMenuKey);

    public void RebindBinding(Binding binding, KeyCode newKey)
    {
        switch (binding)
        {
            case Binding.MoveUp: moveUpKey = newKey; break;
            case Binding.MoveDown: moveDownKey = newKey; break;
            case Binding.MoveLeft: moveLeftKey = newKey; break;
            case Binding.MoveRight: moveRightKey = newKey; break;
            case Binding.Attack: attackKey = newKey; break;
            case Binding.Dodge: dodgeKey = newKey; break;
            case Binding.Interact: interactKey = newKey; break;
            case Binding.Pause: pauseKey = newKey; break;
            case Binding.ToggleMenu: toggleMenuKey = newKey; break;
        }
        SaveBindings();
        OnRebindBinding?.Invoke(this, EventArgs.Empty);
    }

    public KeyCode GetBinding(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.MoveUp: return moveUpKey;
            case Binding.MoveDown: return moveDownKey;
            case Binding.MoveLeft: return moveLeftKey;
            case Binding.MoveRight: return moveRightKey;
            case Binding.Attack: return attackKey;
            case Binding.Dodge: return dodgeKey;
            case Binding.Interact: return interactKey;
            case Binding.Pause: return pauseKey;
            case Binding.ToggleMenu: return toggleMenuKey;
        }
    }

    private void SaveBindings()
    {
        PlayerPrefs.SetString("MoveUpKey", moveUpKey.ToString());
        PlayerPrefs.SetString("MoveDownKey", moveDownKey.ToString());
        PlayerPrefs.SetString("MoveLeftKey", moveLeftKey.ToString());
        PlayerPrefs.SetString("MoveRightKey", moveRightKey.ToString());
        PlayerPrefs.SetString("AttackKey", attackKey.ToString());
        PlayerPrefs.SetString("DodgeKey", dodgeKey.ToString());
        PlayerPrefs.SetString("InteractKey", interactKey.ToString());
        PlayerPrefs.SetString("PauseKey", pauseKey.ToString());
        PlayerPrefs.SetString("ToggleMenuKey", toggleMenuKey.ToString());
        PlayerPrefs.Save();
    }

    private void LoadBindings()
    {
        moveUpKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveUpKey", KeyCode.W.ToString()));
        moveDownKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveDownKey", KeyCode.S.ToString()));
        moveLeftKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveLeftKey", KeyCode.A.ToString()));
        moveRightKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveRightKey", KeyCode.D.ToString()));
        attackKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("AttackKey", KeyCode.K.ToString()));
        dodgeKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DodgeKey", KeyCode.J.ToString()));
        interactKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractKey", KeyCode.E.ToString()));
        pauseKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PauseKey", KeyCode.Escape.ToString()));
        toggleMenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ToggleMenuKey", KeyCode.Tab.ToString()));
    }
}