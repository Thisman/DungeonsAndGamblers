using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryPanel : UIPanel
{
    private VisualElement _root;
    private List<VisualElement> _playerInventorySlots = new();

    override public void Show()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        _root.style.display = DisplayStyle.None;
    }

    public void Render(List<ItemDefinition> playerItems)
    {
        for (int i = 0; i < _playerInventorySlots.Count; i++)
        {
            VisualElement slot = _playerInventorySlots[i];
            if (i < playerItems.Count)
            {
                ItemDefinition item = playerItems[i];
                slot.style.backgroundImage = new StyleBackground(item.Icon);
            }
            else
            {
                slot.style.backgroundImage = new StyleBackground();
            }
        }
    }

    override protected void RegisterUIElements() {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory");
        _playerInventorySlots = _root.Q<VisualElement>("player__inventory")
            .Query<VisualElement>(className:"inventory__cell").ToList();
    }

    override protected void SubcribeToUIEvents() { }

    override protected void UnsubscriveFromUIEvents() { }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }
}
