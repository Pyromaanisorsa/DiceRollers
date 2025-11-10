using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remove tiles that are not empty (occupied or has occupier)
[CreateAssetMenu(fileName = "ExcludeNotEmptyFilter", menuName = "CombatData/TargetFilter/ExcludeNotEmptyFilter")]
public class ExcludeNotEmptyFilter: TargetFilterBase
{
    public override bool IsValidTarget(GridTile tile, CombatParticipant caster)
    {
        return tile.Occupier == null;
    }
}
