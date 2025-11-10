using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialog/Dialogue")]
[System.Serializable]
public class Dialogue : ScriptableObject
{
    [SerializeField] private NodeEntry startNode;
    [SerializeField] private NodeEntry[] nodes;

    public NodeEntry StartNode => startNode;
    public NodeEntry Nodes(int index) => nodes[index];
}

[System.Serializable]
public class NodeEntry
{
    [SerializeField] private DialogNode node;
    [SerializeField] private NodeOptions[] nodeOptions;

    public DialogNode DialogNode => node;
    public NodeOptions NodeOptions(int index) => nodeOptions[index];
}

[System.Serializable]
public class NodeOptions
{
    [SerializeField] private int nextIndex;
    [SerializeField] private bool endNode;
    [SerializeField][Range(1,20)] private int rollRequired;
    [SerializeField] private AttributeType attributeType;

    public int Index => nextIndex;
    public bool EndNode => endNode;
    public int RollRequired => rollRequired;
    public AttributeType AttributeType => attributeType;
}