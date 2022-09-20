using UnityEngine;

public class MainLoop : MonoBehaviour
{
    // Animation state
    private ITwinTimeline _itwinTimeline;
    private long _currentAnimationTime;
    private bool _isAnimationPlaying;

    // Strings for UI
    private string _currentTimeString;
    private string _beginTimeString;
    private string _endTimeString;

    private async void Start()
    {
        _isAnimationPlaying = false;

        // Locate the example files on disk.
        string gltfPath, animationPath;
        ExampleDataConfig.GetExampleFilePaths(out gltfPath, out animationPath);

        // Load the glTF file, instantiate GameObjects for it and provide mapping from element IDs to them.
        ITwinGltf itwinGltf = await ITwinGltf.FromPath(gltfPath);

        // Load the animation file and connect it to the glTF representation.
        _itwinTimeline = ITwinTimeline.FromPath(itwinGltf.IdToMeshRendererMap, animationPath);

        // Set the initial state of the animation playback.
        UpdateCurrentAnimationTime(_itwinTimeline.EpochTimeBeginInSeconds);
        _beginTimeString = "Begin:\t" + System.DateTimeOffset.FromUnixTimeSeconds(_itwinTimeline.EpochTimeBeginInSeconds).ToString("MMMM dd, yyyy");
        _endTimeString = "End:\t" + System.DateTimeOffset.FromUnixTimeSeconds(_itwinTimeline.EpochTimeEndInSeconds).ToString("MMMM dd, yyyy");
    }

    private void Update()
    {
        if (_isAnimationPlaying && _itwinTimeline != null)
        {
            const int epochTimeStep = 60 * 60 * 24 * 7 * 2; // Advance two weeks per game second
            int step = (int)(Time.deltaTime * epochTimeStep);
            UpdateCurrentAnimationTime(_currentAnimationTime + step);
        }
    }

    private void OnGUI()
    {
        if (_itwinTimeline == null)
            return;

        const int GUI_WIDTH = 300, GUI_HEIGHT = 140;

        GUI.BeginGroup(new Rect(Screen.width - (GUI_WIDTH + 40),
                               Screen.height - (GUI_HEIGHT + 40),
                               GUI_WIDTH,
                               GUI_HEIGHT));

        GUI.Box(new Rect(0, 0, GUI_WIDTH, GUI_HEIGHT), "");

        GUI.Label(new Rect(10, 10, 200, 25), _beginTimeString);
        GUI.Label(new Rect(10, 35, 200, 25), _endTimeString);
        GUI.Label(new Rect(10, 60, 200, 25), _currentTimeString);

        string buttonLabel = _isAnimationPlaying ? "Pause" : "Play";
        if (GUI.Button(new Rect(100, 95, 100, 30), buttonLabel))
            _isAnimationPlaying = !_isAnimationPlaying;

        GUI.EndGroup();
    }

    private void UpdateCurrentAnimationTime(long time)
    {
        _currentAnimationTime = time;
        if (_currentAnimationTime >= _itwinTimeline.EpochTimeEndInSeconds)
        {
            // Stop the animation when we reach the end.
            _currentAnimationTime = _itwinTimeline.EpochTimeEndInSeconds;
            _isAnimationPlaying = false;
        }

        _itwinTimeline.ApplyAtEpochTimeInSeconds(_currentAnimationTime);
        _currentTimeString = "Current:\t" + System.DateTimeOffset.FromUnixTimeSeconds(_currentAnimationTime).ToString("MMMM dd, yyyy");
    }
}
