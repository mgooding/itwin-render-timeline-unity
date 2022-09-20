using System.Collections.Generic;
using UnityEngine;

public class ITwinTimelineBatch
{
    public long EpochTimeBeginInSeconds { get; private set; }
    public long EpochTimeEndInSeconds { get; private set; }

    private List<MeshRenderer> RenderersInBatch { get; set; }
    private Material[] OriginalBatchMaterials { get; set; }

    private ColorEntryProps[] ColorTimeline { get; set; }
    private VisibilityEntryProps[] VisibilityTimeline { get; set; }

    private ITwinTimelineBatch() { }

    public static ITwinTimelineBatch FromElementTimelineProps(ElementTimelineProps elementTimeline, Dictionary<string, List<MeshRenderer>> idToMeshRendererMap)
    {
        // Store references to the MeshRenderer components referred to by this batch.
        var renderersInBatch = new List<MeshRenderer>();
        foreach (var elementId in elementTimeline.elementIds)
        {
            // We may not have renderers for animated elements if they weren't exported for whatever reason.
            List<MeshRenderer> renderersForThisElement;
            if (idToMeshRendererMap.TryGetValue(elementId, out renderersForThisElement))
                renderersInBatch.AddRange(renderersForThisElement);
        }

        // If a colorTimeline entry has no value defined, it means that the animation should revert to the
        // original element material.
        // This stores references to the original materials for each MeshRenderer in a parallel array to OriginalBatchMaterials.
        var originalBatchMaterials = new Material[renderersInBatch.Count];
        for (int i = 0; i < renderersInBatch.Count; ++i)
            originalBatchMaterials[i] = renderersInBatch[i].sharedMaterial;

        var result = new ITwinTimelineBatch()
        {
            EpochTimeBeginInSeconds = long.MaxValue,
            EpochTimeEndInSeconds = 0,
            RenderersInBatch = renderersInBatch,
            OriginalBatchMaterials = originalBatchMaterials,
        };

        // Store colorTimeline entries, ensure they are sorted for playback, and accumulate the range of this entry.
        if (elementTimeline.colorTimeline != null && elementTimeline.colorTimeline.Length != 0)
        {
            result.ColorTimeline = elementTimeline.colorTimeline;
            System.Array.Sort(result.ColorTimeline, (a, b) => a.time.CompareTo(b.time));

            if (result.EpochTimeBeginInSeconds > result.ColorTimeline[0].time)
                result.EpochTimeBeginInSeconds = result.ColorTimeline[0].time;

            if (result.EpochTimeEndInSeconds < result.ColorTimeline[result.ColorTimeline.Length - 1].time)
                result.EpochTimeEndInSeconds = result.ColorTimeline[result.ColorTimeline.Length - 1].time;
        }

        // Store visibilityTimeline entries, ensure they are sorted for playback, and accumulate the range of this entry.
        if (elementTimeline.visibilityTimeline != null && elementTimeline.visibilityTimeline.Length != 0)
        {
            result.VisibilityTimeline = elementTimeline.visibilityTimeline;
            System.Array.Sort(result.VisibilityTimeline, (a, b) => a.time.CompareTo(b.time));

            if (result.EpochTimeBeginInSeconds > result.VisibilityTimeline[0].time)
                result.EpochTimeBeginInSeconds = result.VisibilityTimeline[0].time;

            if (result.EpochTimeEndInSeconds < result.VisibilityTimeline[result.VisibilityTimeline.Length - 1].time)
                result.EpochTimeEndInSeconds = result.VisibilityTimeline[result.VisibilityTimeline.Length - 1].time;
        }

        return result;
    }

    public void ApplyAtEpochTimeInSeconds(long epochTimeInSeconds)
    {
        // This method assumes no prior state and will apply the animated properties when called for
        // arbitrary points within the timeline.
        // If playing linearly through, it would be much more efficient to not re-apply all this state,
        // particularly since we're just using individual GameObjects and creating new materials to set
        // colors.

        if (VisibilityTimeline != null)
        {
            int visibility = EvaluateVisibility(epochTimeInSeconds);
            SetElementVisibility(visibility);
        }

        if (ColorTimeline != null)
        {
            // If the current colorTimeline entry does not have a color, we should revert to the original
            // element appearance.
            Color color;
            bool useColor = TryEvaluateColor(epochTimeInSeconds, out color);
            if (useColor)
                SetElementColor(color);
            else
                ResetElementColor();
        }
    }

    private bool TryEvaluateColor(long time, out Color outColor)
    {
        // Entries are sorted by time, so find the first entry at or earlier than our current time.
        // This example only implements step animation, but extending for linear interpolation is
        // straightforward - see README.md.

        ColorEntryRgb? lastColor = null;
        outColor = Color.white;

        for (int i = 0; i < ColorTimeline.Length; ++i)
        {
            if (time < ColorTimeline[i].time)
            {
                if (lastColor.HasValue)
                    outColor = lastColor.Value.ToColor();
                return lastColor.HasValue;
            }

            lastColor = ColorTimeline[i].value;
        }

        if (lastColor.HasValue)
            outColor = lastColor.Value.ToColor();

        return lastColor.HasValue;
    }

    private int EvaluateVisibility(long time)
    {
        // Entries are sorted by time, so find the first entry at or earlier than our current time.
        // This example only implements step animation, but extending for linear interpolation is
        // straightforward - see README.md.

        int lastVisibility = 100;

        for (int i = 0; i < VisibilityTimeline.Length; ++i)
        {
            if (time < VisibilityTimeline[i].time)
                return lastVisibility;

            lastVisibility = VisibilityTimeline[i].value;
        }

        return lastVisibility;
    }

    private void ResetElementColor()
    {
        for (int i = 0; i < RenderersInBatch.Count; ++i)
            RenderersInBatch[i].sharedMaterial = OriginalBatchMaterials[i];
    }

    private void SetElementColor(Color color)
    {
        // Very inefficient to create new materials like this all the time, but attempting to keep the example
        // as simple as possible.
        var baseShader = RenderersInBatch[0].sharedMaterial.shader;
        var elementColorMaterial = new Material(baseShader) { color = color };

        foreach (var renderer in RenderersInBatch)
            renderer.sharedMaterial = elementColorMaterial;
    }

    private void SetElementVisibility(int visibility)
    {
        // Not using transparent shaders, so just set binary visibility when we cross the 50% threshold.
        bool isVisible = visibility >= 50;
        foreach (var renderer in RenderersInBatch)
            renderer.enabled = isVisible;
    }
}
