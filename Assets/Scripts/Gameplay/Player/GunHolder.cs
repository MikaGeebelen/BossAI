using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHolder : MonoBehaviour
{
    [SerializeField] private GameObject _owner = null;
    [SerializeField] private GameObject _gunPrefab = null;
    private BaseGun _currentGun = null;
    private void Start()
    {
        GameObject gun = Instantiate(_gunPrefab, transform);
        _currentGun = gun.GetComponent<BaseGun>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire"))
        {
            _currentGun.Fire(_owner);
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = mousePos - transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, mouseDir);

        transform.eulerAngles = new Vector3(0,0,angle);
    }


}
