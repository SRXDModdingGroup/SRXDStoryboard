using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore;

namespace SRXDStoryboard; 

[BepInDependency("com.pink.spinrhythm.moddingutils", "1.0.6")]
[BepInDependency("com.pink.spinrhythm.spincore")]
[BepInDependency("SRXD.PostProcessing")]
[BepInPlugin("SRXD.Storyboard", "Storyboard", "1.0.0.0")]
public class Plugin : SpinPlugin {
    public new static ManualLogSource Logger { get; private set; }
    
    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;

        var harmony = new Harmony("Storyboard");
        
        harmony.PatchAll(typeof(Patches));
    }
}