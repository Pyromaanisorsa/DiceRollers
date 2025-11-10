using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChoice", menuName = "Dialog/ChoiceNode")]
public class ChoiceNode : DialogNode
{
    [SerializeField] private ChoiceNodeEntry[] choices;

    public ChoiceNodeEntry Choices(int index) => choices[index];
    public int Length => choices.Length;
}

[System.Serializable]
public class ChoiceNodeEntry
{
    [SerializeField] private ChoiceNodeAction nodeType;
    [TextArea][SerializeField] private string dialog;

    public ChoiceNodeAction NodeType => nodeType;
    public string Dialog => dialog;
}

[System.Serializable]
public enum ChoiceNodeAction
{
    Normal,
    RollDice
}