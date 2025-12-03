using UnityEngine;
using UnityEngine.UIElements;

public class InventoryPanel : UIPanel
{
    private VisualElement _root;

    override public void Show()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        _root.style.display = DisplayStyle.None;
    }

    override protected void RegisterUIElements() {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory");
    }

    override protected void SubcribeToUIEvents() { }

    override protected void UnsubscriveFromUIEvents() { }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }
}
