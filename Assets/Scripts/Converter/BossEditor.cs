using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEditor : MonoBehaviour
{
    [SerializeField] private Transform _content = null;
    [SerializeField] private GameObject _layoutPrefab = null;
    [SerializeField] private GameObject _layoutHealthPrefab = null;
    [SerializeField] private BossBuilder _bossBuilder = null;

    private void Start()
    {
        _bossBuilder.OnBehaviorComplete.AddListener(FillTable);
    }

    private void FillTable()
    {
        List<NodeInfo> infoList = _bossBuilder.InfoList;
        foreach (NodeInfo nodeInfo in infoList)
        {
            if (nodeInfo.IsHealth)
            {
                GameObject newLayout = Instantiate(_layoutHealthPrefab, _content, false);
                newLayout.transform.localScale = Vector3.one;

                BossDefinition def = newLayout.GetComponent<BossDefinition>();
                def.Setup(_bossBuilder,nodeInfo,new List<object>());
            }
            else
            {
                GameObject newLayout = Instantiate(_layoutPrefab, _content, false);
                newLayout.transform.localScale = Vector3.one;

                BossDefinition def = newLayout.GetComponent<BossDefinition>();
                switch (nodeInfo.Type)
                {
                    case NodeFill.MainMovement:
                        def.Setup(_bossBuilder, nodeInfo, _bossBuilder.MovementTypes);
                        break;
                    case NodeFill.SecundaryMovement:
                        def.Setup(_bossBuilder, nodeInfo, _bossBuilder.SecundaryMovementTypes);
                        break;
                    case NodeFill.Ability:
                        def.Setup(_bossBuilder, nodeInfo, _bossBuilder.AbilityTypes);
                        break;
                    case NodeFill.Condition:
                        def.Setup(_bossBuilder, nodeInfo, _bossBuilder.ConditionTypes);
                        break;
                }
            }
        }
    }
}
