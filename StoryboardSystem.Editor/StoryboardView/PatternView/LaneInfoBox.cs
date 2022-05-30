using System;
using TMPro;
using UnityEngine;

namespace StoryboardSystem.Editor; 

public class LaneInfoBox : MonoBehaviour {
    [SerializeField] private TMP_Text rigInfoText;

    public void UpdateInfo(RigSetup rigSetup, Lane lane) => rigInfoText.SetText(rigSetup.Name);
}