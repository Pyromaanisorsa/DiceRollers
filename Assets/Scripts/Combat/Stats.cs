using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stats
{
    public StatBlock maxStats;
    public StatBlock currentStats;
    public List<AbilitySlot> abilities = new();

    // Get Attribute value with enum
    public virtual int GetAttributeValue(AttributeType attribute) 
    {
        switch(attribute)
        {
            case AttributeType.Strength:
                return currentStats.STR;
            case AttributeType.Dexterity:
                return currentStats.DEX;
            case AttributeType.Constitution:
                return currentStats.CON;
            case AttributeType.Intelligence:
                return currentStats.INT;
            case AttributeType.Wisdom:
                return currentStats.WIS;
            case AttributeType.Charisma:
                return currentStats.CHA;
        }
        return 0;
    }
}

[System.Serializable]
public struct StatBlock
{
    public int HP, AP, MOVE;
    public int STR, DEX, CON, INT, WIS, CHA;
}
