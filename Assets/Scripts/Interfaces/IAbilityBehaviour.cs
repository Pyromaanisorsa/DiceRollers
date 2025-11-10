using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityBehaviour
{
    void Execute(AbilityData ability, CombatParticipant caster, List<GridTile> targets);
}
