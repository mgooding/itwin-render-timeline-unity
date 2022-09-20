using System.Collections.Generic;
using UnityEngine;

public class ITwinTimeline
{
    public long EpochTimeBeginInSeconds { get; private set; }
    public long EpochTimeEndInSeconds { get; private set; }

    private readonly List<ITwinTimelineBatch> _batches = new List<ITwinTimelineBatch>();

    private ITwinTimeline(List<ITwinTimelineBatch> batches)
    {
        _batches = batches;
    }

    public void ApplyAtEpochTimeInSeconds(long epochTimeInSeconds)
    {
        foreach (var batch in _batches)
            batch.ApplyAtEpochTimeInSeconds(epochTimeInSeconds);
    }

    public static ITwinTimeline FromPath(Dictionary<string, List<MeshRenderer>> idToMeshRendererMap, string animationPath)
    {
        // Load animation JSON into our runtime data structure that connects to our imported glTF file.
        ModelTimelineProps[] modelTimelines = ITwinTimelineJson.Deserialize(System.IO.File.ReadAllText(animationPath)); 

        long epochBegin = long.MaxValue;
        long epochEnd = 0;
        var batches = new List<ITwinTimelineBatch>();

        foreach (var modelTimeline in modelTimelines)
        {
            foreach (var elementTimeline in modelTimeline.elementTimelines)
            {
                var batch = ITwinTimelineBatch.FromElementTimelineProps(elementTimeline, idToMeshRendererMap);
                epochBegin = System.Math.Min(epochBegin, batch.EpochTimeBeginInSeconds);
                epochEnd = System.Math.Max(epochEnd, batch.EpochTimeEndInSeconds);
                batches.Add(batch);
            }
        }

        return new ITwinTimeline(batches)
        {
            EpochTimeBeginInSeconds = epochBegin,
            EpochTimeEndInSeconds = epochEnd,
        };
    }
}
