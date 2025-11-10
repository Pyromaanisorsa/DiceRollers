using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetFilter
{
    bool IsValidTarget(GridTile tile, CombatParticipant caster);
}
