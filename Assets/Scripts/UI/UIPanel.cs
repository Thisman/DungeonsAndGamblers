using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _root;

    protected virtual void Awake()
    {
        if (_root == null)
        {
            _root = gameObject;
        }
    }

    public virtual void Show()
    {
        if (_root != null)
        {
            _root.SetActive(true);
        }
    }

    public virtual void Hide()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }
}
