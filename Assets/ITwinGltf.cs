using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ITwinGltf
{
    public Dictionary<string, List<MeshRenderer>> IdToMeshRendererMap { get; private set; }

    private ITwinGltf() { }

    public static async Task<ITwinGltf> FromPath(string gltfPath)
    {
        // Use glTFast to load our glTF exported from https://developer.bentley.com/apis/mesh-export/.
        var gltfImport = new GLTFast.GltfImport();
        bool success = await gltfImport.Load(new System.Uri(gltfPath));
        if (!success)
            throw new System.Exception($"Failed to load {gltfPath}");

        // Default instantiation is fine for the purposes of this example.
        var gltfParent = new GameObject(System.IO.Path.GetFileName(gltfPath));
        gltfImport.InstantiateMainScene(gltfParent.transform);

        // The GameObject hierarchy matches the glTF hierarchy.
        // The first child is the one and only Scene in the iTwin glTF file.
        // Its children are the GameObjects for the individual elements.
        Transform scene = gltfParent.transform.GetChild(0);
        return new ITwinGltf
        {
            IdToMeshRendererMap = BuildIdToMeshRendererMap(scene),
        };
    }

    private static Dictionary<string, List<MeshRenderer>> BuildIdToMeshRendererMap(Transform scene)
    {
        var result = new Dictionary<string, List<MeshRenderer>>();

        foreach (Transform child in scene)
        {
            // GameObject names should always be Id64String from the source iModel.
            // The scene hierarchy should be flat so there should not be any need to recurse.
            // There can be multiple GameObjects for an element ID for a variety of reasons (usually if the element uses multiple materials),
            // so the map needs its values to be Lists.
            List<MeshRenderer> renderersForId;
            if (!result.TryGetValue(child.gameObject.name, out renderersForId))
            {
                renderersForId = new List<MeshRenderer>();
                result[child.gameObject.name] = renderersForId;
            }
            var childRenderer = child.GetComponent<MeshRenderer>();
            Debug.Assert(childRenderer != null);
            renderersForId.Add(childRenderer);
        }

        return result;
    }
}
