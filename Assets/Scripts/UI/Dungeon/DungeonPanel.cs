using UnityEngine;
using UnityEngine.UIElements;

public class DungeonPanel : UIPanel
{
    private VisualElement _root;

    private VisualElement _playerInfo;
    private VisualElement _enemyInfo;

    private Label _playerHealthLabel;
    private Label _playerDamageLabel;
    private Label _playerDescriptionLabel;

    private Label _enemyHealthLabel;
    private Label _enemyDamageLabel;
    private Label _enemyDescriptionLabel;

    override public void Show()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        _root.style.display = DisplayStyle.None;
    }

    public void Render(UnitModel player, UnitModel enemy)
    {
        _playerDescriptionLabel.text = player.Description;
        _playerHealthLabel.text = $"Здоровье: {player.Health}";
        _playerDamageLabel.text = $"Урон: {player.Damage}";

        _enemyDescriptionLabel.text = enemy.Description;
        _enemyHealthLabel.text = $"Здоровье: {enemy.Health}";
        _enemyDamageLabel.text = $"Урон: {enemy.Damage}";
    }

    override protected void RegisterUIElements() {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "dungeon");
        _playerInfo = _uiDocument.rootVisualElement.Q<VisualElement>("player__info");
        _enemyInfo = _uiDocument.rootVisualElement.Q<VisualElement>("enemy__info");

        _playerHealthLabel = _playerInfo.Q<Label>("unit-health");
        _playerDamageLabel = _playerInfo.Q<Label>("unit-damage");
        _playerDescriptionLabel = _playerInfo.Q<Label>("unit-description");

        _enemyHealthLabel = _enemyInfo.Q<Label>("unit-health");
        _enemyDamageLabel = _enemyInfo.Q<Label>("unit-damage");
        _enemyDescriptionLabel = _enemyInfo.Q<Label>("unit-description");
    }

    override protected void SubcribeToUIEvents() { }

    override protected void UnsubscriveFromUIEvents() { }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }
}
