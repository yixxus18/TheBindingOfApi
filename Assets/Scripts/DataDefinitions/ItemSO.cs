using UnityEngine;

public enum ApiItemType
{
    Consumable,
    Method,
    Fragment,
    Header,
    Token,
    Body
}

[CreateAssetMenu(fileName = "New API Item", menuName = "BindingOfApi/Item")]
public class ItemSO : ScriptableObject
{
    [Tooltip("ID único para este item (ej: 'HEALTH_POTION', 'API_KEY_GET')")]
    public string itemID;
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite icon;
    public int stackSize = 10;

    [Header("API Properties")]
    public ApiItemType itemType;
    [Tooltip("El valor real de texto para la petici�n. Ej: 'GET', '/api/v1'")]
    public string apiValue;

    [Header("Currency")]
    public bool isGold;
    public int goldAmount;

    [Header("Consumable Stats")]
    public int healAmount;
    public int damageInfo;
}