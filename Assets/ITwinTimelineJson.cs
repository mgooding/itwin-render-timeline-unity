using System.ComponentModel;
using Newtonsoft.Json;

public static class ITwinTimelineJson
{
    public static ModelTimelineProps[] Deserialize(string json)
    {
        // The JSON format makes use of significant undefined fields, so the DefaultValueHandling here is important.
        return JsonConvert.DeserializeObject<ModelTimelineProps[]>(json, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Populate,
        });
    }
}

// https://www.itwinjs.org/reference/core-common/displaystyles/renderschedule/renderschedule.modeltimelineprops/
[System.Serializable]
public struct ModelTimelineProps
{
    public ElementTimelineProps[] elementTimelines;
}

// https://www.itwinjs.org/reference/core-common/displaystyles/renderschedule/renderschedule.elementtimelineprops/
[System.Serializable]
public struct ElementTimelineProps
{
    public string[] elementIds;

    // Only the fields relevant for this example are requested - see README.md.
    public ColorEntryProps[] colorTimeline;
    public VisibilityEntryProps[] visibilityTimeline;
}

// https://www.itwinjs.org/reference/core-common/displaystyles/renderschedule/renderschedule.colorentryprops/
[System.Serializable]
public struct ColorEntryProps
{
    // undefined means revert to original element material.
    [DefaultValue(null)]
    public ColorEntryRgb? value;

    public long time;
}

[System.Serializable]
public struct ColorEntryRgb
{
    public int red;
    public int green;
    public int blue;

    public UnityEngine.Color ToColor() { return new UnityEngine.Color(red / 255f, green / 255f, blue / 255f); }
}

// https://www.itwinjs.org/reference/core-common/displaystyles/renderschedule/renderschedule.visibilityentryprops/
[System.Serializable]
public struct VisibilityEntryProps
{
    // undefined means completely visible.
    [DefaultValue(100)]
    public int value;

    public long time;
}