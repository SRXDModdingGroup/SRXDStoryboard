using BepInEx.Logging;
using SpinCore;

namespace SRXDStoryboard.Plugin; 

public class Plugin : SpinPlugin {
    public static ManualLogSource Logger { get; private set; }
    
    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;
    }
}