using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Damage : MonoBehaviour
{
    private GameObject _originator = null;
    private string _tag = "Untagged";
    [SerializeField] private int _damage = 10;

    public void AddOwner(GameObject originator)
    {
        _originator = originator;
        _tag = _originator.tag;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(_tag))
        {
            return; //can't hit self
        }

        Health healthComponent = col.GetComponent<Health>();
        if (healthComponent == null || col.CompareTag("Trap")) //skip floor traps and unhittable Targets
        {
            return;
        }

        healthComponent.TakeDamage(_damage);
        Destroy(gameObject);
    }
}
