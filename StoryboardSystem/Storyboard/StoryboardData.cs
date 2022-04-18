using System.Collections.Generic;
using System.IO;

namespace StoryboardSystem; 

internal class StoryboardData {
    public List<LoadedObjectReference> ObjectReferences { get; }
    
    public Dictionary<Identifier, HashSet<Identifier>> BindingIdentifiers;
    
    public Dictionary<string, object> OutParams { get; }

    public StoryboardData(
        List<LoadedObjectReference> objectReferences,
        Dictionary<Identifier, HashSet<Identifier>> bindingIdentifiers,
        Dictionary<string, object> outParams) {
        ObjectReferences = objectReferences;
        BindingIdentifiers = bindingIdentifiers;
        OutParams = outParams;
    }

    public bool TrySerialize(BinaryWriter writer) {
        writer.Write(ObjectReferences.Count);

        foreach (var reference in ObjectReferences) {
            if (!reference.TrySerialize(writer))
                return false;
        }
        
        writer.Write(BindingIdentifiers.Count);

        foreach (var pair in BindingIdentifiers) {
            var properties = pair.Value;
            
            pair.Key.Serialize(writer);
            writer.Write(properties.Count);

            foreach (var property in properties)
                property.Serialize(writer);
        }

        writer.Write(OutParams.Count);

        foreach (var pair in OutParams) {
            writer.Write(pair.Key);

            if (!SerializationUtility.TrySerialize(pair.Value, writer))
                return false;
        }

        return true;
    }

    public static bool TryDeserialize(BinaryReader reader, out StoryboardData data) {
        int objectReferenceCount = reader.ReadInt32();
        var objectReferences = new List<LoadedObjectReference>(objectReferenceCount);

        for (int i = 0; i < objectReferenceCount; i++) {
            if (!LoadedObjectReference.TryDeserialize(reader, out var reference)) {
                data = null;
                
                return false;
            }

            objectReferences.Add(reference);
        }

        int bindingIdentifiersCount = reader.ReadInt32();
        var bindingIdentifiers = new Dictionary<Identifier, HashSet<Identifier>>();

        for (int i = 0; i < bindingIdentifiersCount; i++) {
            var controller = Identifier.Deserialize(reader);
            int propertiesCount = reader.ReadInt32();
            var properties = new HashSet<Identifier>();

            for (int j = 0; j < propertiesCount; j++)
                properties.Add(Identifier.Deserialize(reader));
            
            bindingIdentifiers.Add(controller, properties);
        }

        int outParamsCount = reader.ReadInt32();
        var outParams = new Dictionary<string, object>();

        for (int i = 0; i < outParamsCount; i++) {
            string key = reader.ReadString();

            if (!SerializationUtility.TryDeserialize(reader, out object value)) {
                data = null;

                return false;
            }
            
            outParams.Add(key, value);
        }

        data = new StoryboardData(objectReferences, bindingIdentifiers, outParams);

        return true;
    }
}