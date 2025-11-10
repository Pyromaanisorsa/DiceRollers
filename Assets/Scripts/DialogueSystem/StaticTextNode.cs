using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStaticText", menuName = "Dialog/StaticTextNode")]
public class StaticTextNode : DialogNode
{
    [TextArea]
    [SerializeField] private string dialog;

    public string Dialog => dialog;
}
