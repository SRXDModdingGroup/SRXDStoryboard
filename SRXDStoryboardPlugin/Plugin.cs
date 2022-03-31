using System.IO;
using BepInEx.Logging;
using SpinCore;

namespace SRXDStoryboard.Core; 

public class Plugin : SpinPlugin {
    public static ManualLogSource Logger { get; private set; }
    
    public static string CustomAssetBundlePath { get; private set; }
    
    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;
        CustomAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(CustomAssetBundlePath))
            Directory.CreateDirectory(CustomAssetBundlePath);
    }
}