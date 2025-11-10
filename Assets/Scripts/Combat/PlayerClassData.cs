using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "CombatData/PlayerClassData")]
public class PlayerClassData : CharacterData
{
    [field: SerializeField] public Sprite ClassIcon { get; private set; }
}
