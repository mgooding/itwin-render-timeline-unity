using System.IO;
using UnityEngine;

public static class ExampleDataConfig
{
    public static void GetExampleFilePaths(out string gltfPath, out string animationPath)
    {
        // Find the sample data included in the repository. Will only work in the editor.
        var projectRootDirectory = Directory.GetParent(Application.dataPath);
        var exampleDataDirectory = Path.Combine(projectRootDirectory.FullName, "example-data");

        gltfPath = Path.Combine(exampleDataDirectory, "test-imodel.gltf");
        animationPath = Path.Combine(exampleDataDirectory, "animation.json");
    }
}
