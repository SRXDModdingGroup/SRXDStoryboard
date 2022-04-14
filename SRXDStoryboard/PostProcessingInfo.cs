using UnityEngine;

namespace SRXDStoryboard; 

public readonly struct PostProcessingInfo {
    public Material Material { get; }
    
    public Camera Camera { get; }

    private readonly int hash;

    public PostProcessingInfo(Material material, Camera camera) {
        Material = material;
        Camera = camera;

        unchecked {
            uint uCameraId = (uint) camera.GetInstanceID();

            hash = (int) ((uint) Material.GetInstanceID() + (uCameraId << 16) | (uCameraId >> 16));
        }
    }

    public override bool Equals(object obj) => obj is PostProcessingInfo other && this == other;

    public override int GetHashCode() => hash;

    public static bool operator ==(PostProcessingInfo a, PostProcessingInfo b) => a.Material == b.Material && a.Camera == b.Camera;

    public static bool operator !=(PostProcessingInfo a, PostProcessingInfo b) => a.Material != b.Material || a.Camera != b.Camera;
}