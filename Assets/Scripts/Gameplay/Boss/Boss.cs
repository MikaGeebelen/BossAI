using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public void SpawnProjectle(GameObject prefab, Vector3 pos,Vector3 rot)
    {
        GameObject item = Instantiate(prefab, pos, Quaternion.Euler(rot));
        item.GetComponent<Damage>().AddOwner(gameObject);
    }

}
