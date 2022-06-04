using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryboardSystem.Editor; 

public class LaneInfoBox : MonoBehaviour {
    [SerializeField] private TMP_Text rigInfoText;
    [SerializeField] private Button button;

    public TMP_Text RigInfoText => rigInfoText;
    
    public Button Button => button;
}