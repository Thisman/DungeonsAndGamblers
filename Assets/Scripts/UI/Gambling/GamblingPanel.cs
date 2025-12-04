using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GamblingPanel : UIPanel
{
    [SerializeField]
    private float _spinDuration = 3f;

    [SerializeField]
    private int _visibleItems = 8;

    [SerializeField]
    private List<Sprite> _availableIcons = new();

    private VisualElement _root;
    private VisualElement _betList;
    private Label _selectedLabel;
    private Button _rollButton;
    private List<VisualElement> _betItems = new();
    private List<VisualElement> _slots = new();
    private List<IGamblingBet> _bets = new();
    private int _startIndex;
    private int _selectedIndex = -1;
    private bool _isSpinning;
    private Coroutine _spinCoroutine;

    public void Render(List<IGamblingBet> bets)
    {
        _bets = bets ?? new List<IGamblingBet>();
        _selectedIndex = GetFirstUsableBetIndex();
        _startIndex = _selectedIndex >= 0 ? _selectedIndex : 0;
        UpdateSelectedLabel();
        RenderBetItems();
        UpdateRollButtonState();
    }

    override public void Show()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.None;
    }

    override protected void RegisterUIElements()
    {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "gambling");
        _betList = _root?.Q<VisualElement>("gambling__bet-list");
        _selectedLabel = _root?.Q<Label>("gambling__selected");
        _rollButton = _root?.Q<Button>("gambling__roll-button");
        _slots = _root?.Q<VisualElement>("gambling__slots")?.Query<VisualElement>(className: "gambling__slot").ToList() ?? new List<VisualElement>();

        CreateBetItems();
        RenderBetItems();
        UpdateSelectedLabel();
    }

    override protected void SubcribeToUIEvents()
    {
        if (_betList != null)
        {
            _betList.RegisterCallback<WheelEvent>(HandleWheel); 
        }

        foreach (var item in _betItems)
        {
            item.RegisterCallback<PointerDownEvent>(HandleBetClick);
        }

        if (_rollButton != null)
        {
            _rollButton.clicked += HandleRollClicked;
        }
    }

    override protected void UnsubscriveFromUIEvents()
    {
        if (_betList != null)
        {
            _betList.UnregisterCallback<WheelEvent>(HandleWheel);
        }

        foreach (var item in _betItems)
        {
            item.UnregisterCallback<PointerDownEvent>(HandleBetClick);
        }

        if (_rollButton != null)
        {
            _rollButton.clicked -= HandleRollClicked;
        }
    }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }

    private void CreateBetItems()
    {
        if (_betList == null)
        {
            return;
        }

        _betList.Clear();
        _betItems.Clear();

        for (int i = 0; i < Mathf.Max(1, _visibleItems); i++)
        {
            var betItem = new VisualElement();
            betItem.AddToClassList("gambling__bet-item");

            var label = new Label
            {
                pickingMode = PickingMode.Ignore
            };
            betItem.Add(label);

            _betList.Add(betItem);
            _betItems.Add(betItem);
        }
    }

    private void HandleWheel(WheelEvent evt)
    {
        if (_bets.Count == 0)
        {
            return;
        }

        _startIndex = WrapIndex(_startIndex + (evt.delta.y > 0 ? 1 : -1), _bets.Count);
        RenderBetItems();
        evt.StopPropagation();
    }

    private void HandleBetClick(PointerDownEvent evt)
    {
        if (_bets.Count == 0)
        {
            return;
        }

        VisualElement target = evt.currentTarget as VisualElement;
        int localIndex = _betItems.IndexOf(target);
        if (localIndex < 0)
        {
            return;
        }

        int betIndex = WrapIndex(_startIndex + localIndex, _bets.Count);
        if (!_bets[betIndex].CanUse)
        {
            return;
        }

        _selectedIndex = betIndex;
        UpdateSelectedLabel();
        RenderBetItems();
    }

    private void HandleRollClicked()
    {
        if (_isSpinning || _selectedIndex < 0 || _bets.Count == 0 || _availableIcons.Count == 0)
        {
            return;
        }

        _spinCoroutine ??= StartCoroutine(Spin());
    }

    private IEnumerator Spin()
    {
        _isSpinning = true;
        _rollButton?.SetEnabled(false);

        float elapsed = 0f;
        while (elapsed < _spinDuration)
        {
            SetRandomIcons();
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }

        var finalIcons = new List<Sprite>();
        for (int i = 0; i < _slots.Count; i++)
        {
            Sprite icon = GetRandomIcon();
            finalIcons.Add(icon);
            _slots[i].style.backgroundImage = icon != null ? new StyleBackground(icon) : new StyleBackground();
        }

        bool allEqual = finalIcons.Count > 0 && finalIcons.All(s => s == finalIcons[0]);
        IGamblingBet bet = _selectedIndex >= 0 && _selectedIndex < _bets.Count ? _bets[_selectedIndex] : null;
        bet?.ApplyResult(allEqual);

        _isSpinning = false;
        _rollButton?.SetEnabled(true);
        _spinCoroutine = null;
        RenderBetItems();
        UpdateRollButtonState();
    }

    private void SetRandomIcons()
    {
        if (_slots == null || _slots.Count == 0)
        {
            return;
        }

        foreach (var slot in _slots)
        {
            Sprite icon = GetRandomIcon();
            slot.style.backgroundImage = icon != null ? new StyleBackground(icon) : new StyleBackground();
        }
    }

    private Sprite GetRandomIcon()
    {
        if (_availableIcons == null || _availableIcons.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, _availableIcons.Count);
        return _availableIcons[index];
    }

    private void RenderBetItems()
    {
        if (_betItems == null || _betItems.Count == 0)
        {
            return;
        }

        if (_bets == null || _bets.Count == 0)
        {
            foreach (var item in _betItems)
            {
                if (item.Q<Label>() is { } label)
                {
                    label.text = "Нет ставок";
                }
                item.EnableInClassList("gambling__bet-item--selected", false);
                item.EnableInClassList("gambling__bet-item--disabled", true);
            }
            return;
        }

        for (int i = 0; i < _betItems.Count; i++)
        {
            var item = _betItems[i];
            int betIndex = WrapIndex(_startIndex + i, _bets.Count);
            var bet = _bets[betIndex];

            if (item.Q<Label>() is { } label)
            {
                label.text = bet.Name;
            }

            bool isSelected = betIndex == _selectedIndex;
            item.EnableInClassList("gambling__bet-item--selected", isSelected);
            item.EnableInClassList("gambling__bet-item--disabled", !bet.CanUse);
        }
    }

    private int GetFirstUsableBetIndex()
    {
        for (int i = 0; i < _bets.Count; i++)
        {
            if (_bets[i].CanUse)
            {
                return i;
            }
        }

        return -1;
    }

    private int WrapIndex(int index, int length)
    {
        if (length == 0)
        {
            return 0;
        }

        int value = index % length;
        return value < 0 ? value + length : value;
    }

    private void UpdateSelectedLabel()
    {
        if (_selectedLabel == null)
        {
            return;
        }

        if (_selectedIndex >= 0 && _selectedIndex < _bets.Count)
        {
            _selectedLabel.text = $"Ваша ставка {_bets[_selectedIndex].Name}";
        }
        else
        {
            _selectedLabel.text = "Ваша ставка";
        }
    }

    private void UpdateRollButtonState()
    {
        bool canRoll = !_isSpinning && _selectedIndex >= 0 && _selectedIndex < _bets.Count && _availableIcons.Count > 0;
        _rollButton?.SetEnabled(canRoll);
    }
}

public interface IGamblingBet
{
    string Name { get; }

    bool CanUse { get; }

    void ApplyWin();

    void ApplyLose();
}

public static class GamblingBetExtensions
{
    public static void ApplyResult(this IGamblingBet bet, bool isWin)
    {
        if (bet == null)
        {
            return;
        }

        if (isWin)
        {
            bet.ApplyWin();
        }
        else
        {
            bet.ApplyLose();
        }
    }
}
