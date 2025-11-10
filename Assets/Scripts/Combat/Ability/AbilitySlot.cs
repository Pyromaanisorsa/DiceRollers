using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runtime container for abilities; contains reference to ability and cooldown per slot (future proofing)
[System.Serializable]
public class AbilitySlot
{
    public AbilityData abilityData;
    public int cooldown;

    public AbilitySlot(AbilityData abilityData) 
    {
        this.abilityData = abilityData;
    }
}
