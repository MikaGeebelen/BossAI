using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsedInCombo : MonoBehaviour
{
    private bool _has2Comps = false;
    public bool Has2Comps()
    {
        return _has2Comps;
    }

    public void ClaimBuddy()
    {
        _has2Comps = true;
    }
}
