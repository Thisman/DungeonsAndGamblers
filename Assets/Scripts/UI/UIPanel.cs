using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField]
    protected UIDocument _uiDocument;

    [Inject]
    protected GameEventBus _sceneEventBusService;

    protected bool _isAttached;

    protected void OnEnable()
    {
        TryRegisterLifecycleCallbacks();
        SubscriveToGameEvents();
    }

    protected void OnDisable()
    {
        DetachFromPanel();
        TryUnregisterLifecycleCallbacks();
    }

    protected void OnDestroy()
    {
        DetachFromPanel();
        TryUnregisterLifecycleCallbacks();
    }

    virtual protected void AttachToPanel(UIDocument document)
    {
        if (_isAttached)
            return;

        RegisterUIElements();
        SubcribeToUIEvents();
        SubscriveToGameEvents();

        _isAttached = true;
        Debug.Log($"{GetType().Name} attached to panel.");
    }

    virtual protected void DetachFromPanel()
    {
        if (!_isAttached)
            return;

        UnsubscriveFromUIEvents();
        UnsubscribeFromGameEvents();

        _isAttached = false;
    }

    protected void TryRegisterLifecycleCallbacks()
    {
        if (_uiDocument.rootVisualElement is { } root)
        {
            root.RegisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            root.RegisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);

            if (!_isAttached && root.panel != null)
                AttachToPanel(_uiDocument);
        }
    }

    protected void TryUnregisterLifecycleCallbacks()
    {
        if (_uiDocument.rootVisualElement is { } root)
        {
            root.UnregisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            root.UnregisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);
        }
    }

    abstract public void Show();

    abstract public void Hide();

    abstract protected void RegisterUIElements();

    abstract protected void SubcribeToUIEvents();

    abstract protected void UnsubscriveFromUIEvents();

    abstract protected void SubscriveToGameEvents();

    abstract protected void UnsubscribeFromGameEvents();

    protected void HandleAttachToPanel(AttachToPanelEvent _)
    {
        if (!_isAttached)
            AttachToPanel(_uiDocument);
    }

    protected void HandleDetachFromPanel(DetachFromPanelEvent _)
    {
        DetachFromPanel();
    }
}
