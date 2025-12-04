using UnityEngine;

[System.Serializable]
public class ItemDefinition
{
    [SerializeField]
    private string _displayName = "Item";

    public string DisplayName => _displayName;
}
