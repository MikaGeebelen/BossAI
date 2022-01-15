using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGun : MonoBehaviour
{
    [SerializeField] private List<Transform> _bulletSpawns = new List<Transform>();
    [SerializeField] private GameObject _bulletType = null;
    
    virtual public void Fire(GameObject owner)
    {
        foreach (Transform pos in _bulletSpawns)
        {
            GameObject bullet = Instantiate(_bulletType, pos.position, pos.rotation);
            bullet.GetComponent<Damage>().AddOwner(owner);
        }
    }
}
