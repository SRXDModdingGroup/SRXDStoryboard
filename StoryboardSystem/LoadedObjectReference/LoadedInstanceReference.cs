using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem;

internal class LoadedInstanceReference : LoadedObjectReference {
    public override object LoadedObject => instance;
    
    private int assetReferenceIndex;
    private string name;
    private Identifier parentIdentifier;
    private int layer;
    private Object instance;

    public LoadedInstanceReference(int assetReferenceIndex, string name, Identifier parentIdentifier, int layer) {
        this.assetReferenceIndex = assetReferenceIndex;
        this.name = name;
        this.parentIdentifier = parentIdentifier;
        this.layer = layer;
    }

    public override void Serialize(BinaryWriter writer) {
        writer.Write((int) ObjectReferenceType.Instance);
        writer.Write(assetReferenceIndex);
        writer.Write(name);
        parentIdentifier.Serialize(writer);
        writer.Write(layer);
    }

    public override void Unload(ISceneManager sceneManager) {
        if (instance != null)
            Object.Destroy(instance);
        
        instance = null;
    }

    public override bool TryLoad(List<LoadedObjectReference> objectReferences, ISceneManager sceneManager, IStoryboardParams sParams, ILogger logger) {
        if (assetReferenceIndex < 0 || assetReferenceIndex >= objectReferences.Count) {
            logger.LogWarning($"Reference index for {name} is not valid");

            return false;
        }

        if (objectReferences[assetReferenceIndex] is not LoadedAssetReference assetReference) {
            logger.LogWarning($"{objectReferences[assetReferenceIndex]} is not an asset reference");

            return false;
        }
        
        if (assetReference.Asset == null) {
            logger.LogWarning($"Failed to create instance of {assetReference.AssetName}");
            
            return false;
        }
        
        instance = Object.Instantiate(assetReference.Asset);
        
        if (instance is GameObject gameObject) {
            Transform parentTransform;

            if (parentIdentifier == null)
                parentTransform = null;
            else if (Binder.TryResolveIdentifier(parentIdentifier, objectReferences, logger, out object parentObject)) {
                switch (parentObject) {
                    case Transform newParentTransform:
                        parentTransform = newParentTransform;
                        break;
                    case GameObject parentGameObject:
                        parentTransform = parentGameObject.transform;
                        break;
                    default:
                        logger.LogWarning($"Target parent {parentObject} is not a gameObject or transform");

                        return false;
                }
            }
            else {
                logger.LogWarning($"Could not resolve identifier {parentIdentifier}");

                return false;
            }
            
            gameObject.name = name;
        
            var transform = gameObject.transform;

            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            SetLayer(transform);
            
            void SetLayer(Transform subTransform) {
                subTransform.gameObject.layer = layer;

                for (int i = 0; i < subTransform.childCount; i++)
                    SetLayer(subTransform.GetChild(i));
            }
        }

        sceneManager.InitializeObject(instance);

        return true;
    }

    public static LoadedInstanceReference Deserialize(BinaryReader reader) {
        int assetReferenceIndex = reader.ReadInt32();
        string name = reader.ReadString();
        var parentIdentifier = Identifier.Deserialize(reader);
        int layer = reader.ReadInt32();

        return new LoadedInstanceReference(assetReferenceIndex, name, parentIdentifier, layer);
    }
}