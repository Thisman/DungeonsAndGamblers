// Manages switching between different input action maps for major gameplay modes.
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public enum GameMode
{
    Hub,
    Dungeon,
    Inventory,
    Shop,
}

public class GameInputSystem
{
    [Inject]
    private readonly InputActionAsset _actions;

    public InputActionAsset Actions => _actions;

    public void EnableOnly(params string[] maps)
    {
        foreach (var m in _actions.actionMaps)
        {
            m.Disable();
        }

        foreach (var name in maps)
        {
            var map = _actions.FindActionMap(name, throwIfNotFound: true);
            map.Enable();
        }
    }

    public void ClearBindingMask() => _actions.bindingMask = null;

    public void EnterDungeon() => SetMode(GameMode.Dungeon);

    public void EnterHub() => SetMode(GameMode.Hub);

    public void EnterInventory() => SetMode(GameMode.Inventory);

    public void EnterShop() => SetMode(GameMode.Shop);

    private void SetMode(GameMode mode)
    {
        ClearBindingMask();
        Debug.Log($"Enter {mode} mode");

        switch (mode)
        {
            case GameMode.Dungeon:
                EnableOnly("Dungeon");
                break;
            case GameMode.Hub:
                EnableOnly("Hub");
                break;
            case GameMode.Inventory:
                EnableOnly("Inventory");
                break;
            case GameMode.Shop:
                EnableOnly("Shop");
                break;
        }
    }
}
