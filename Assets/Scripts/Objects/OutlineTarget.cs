using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class OutlineTarget : MonoBehaviour
{
    [Tooltip("Материал с шейдером Custom/SimpleOutline")]
    public Material outlineMaterial;

    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private bool _highlighted;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        _originalMaterials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].sharedMaterials;
        }
    }

    public void SetHighlighted(bool value)
    {
        if (_highlighted == value) return;
        _highlighted = value;

        if (outlineMaterial == null)
        {
            Debug.LogWarning($"[{name}] OutlineTarget: outlineMaterial не задан");
            return;
        }

        if (value)
        {
            // добавляем outline-материал как дополнительный
            for (int i = 0; i < _renderers.Length; i++)
            {
                var baseMats = _originalMaterials[i];
                var newMats = new Material[baseMats.Length + 1];
                baseMats.CopyTo(newMats, 0);
                newMats[newMats.Length - 1] = outlineMaterial;
                _renderers[i].materials = newMats;
            }
        }
        else
        {
            // возвращаем исходные материалы
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].materials = _originalMaterials[i];
            }
        }
    }
}
