using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMU.Utilities;
using SpinCore;
using SpinCore.UI;

namespace SRXDStoryboard; 

[BepInDependency("com.pink.spinrhythm.moddingutils", "1.0.6")]
[BepInDependency("com.pink.spinrhythm.spincore")]
[BepInDependency("SRXD.PostProcessing")]
[BepInPlugin("SRXD.Storyboard", "Storyboard", "1.0.0.0")]
public class Plugin : SpinPlugin {
    public static Bindable<bool> EnableStoryboards { get; private set; }

    public new static ManualLogSource Logger { get; private set; }
    
    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;

        var harmony = new Harmony("Storyboard");
        
        harmony.PatchAll(typeof(Patches));
        EnableStoryboards = AddBindableConfig("EnableStoryboards", true);
    }

    protected override void CreateMenus() {
        var root = CreateOptionsTab("Storyboard").UIRoot;

        SpinUI.CreateToggle("Enable Storyboards", root).Bind(EnableStoryboards);
    }
}