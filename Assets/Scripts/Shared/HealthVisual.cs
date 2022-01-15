using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthVisual : MonoBehaviour
{
    [SerializeField]private Slider _slider = null;
    [SerializeField] private GameObject _phaseIndictor = null;
    [SerializeField] private GameObject _healthBar = null;

    [SerializeField] private Transform _healthStart = null;
    [SerializeField] private Transform _healthEnd = null;

    private Health _health = null;

    private void Awake()
    {
        _health = gameObject.GetComponentInParent<Health>();
        _health.AttachVisual(this);
        _health.Damaged.AddListener(VisualUpdate);
    }

    private void VisualUpdate()
    {
        _slider.value = (float)_health.CurrentHealth / _health.MaxHealth;
    }

    public void SetHealthCondition(float healthPercent)
    {
        Instantiate(_phaseIndictor, _healthBar.transform);
        _phaseIndictor.GetComponent<RectTransform>().anchoredPosition3D = Vector3.Lerp(_healthEnd.localPosition, _healthStart.localPosition, healthPercent);
    }
}
