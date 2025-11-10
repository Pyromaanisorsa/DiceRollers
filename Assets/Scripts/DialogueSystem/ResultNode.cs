using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResult", menuName = "Dialog/ResultNode")]
public class ResultNode : DialogNode
{
    [TextArea][SerializeField] private string success;
    [TextArea][SerializeField] private string failure;

    public string Success => success;
    public string Failure => failure;
}
