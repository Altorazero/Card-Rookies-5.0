using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card Definition", menuName = "Card Rookies/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string CardName = "New Card";
    [TextArea] public string Description = "";
    
    [Header("Graph")]
    public CardGraph CardGraph;
}
