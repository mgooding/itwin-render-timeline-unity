# itwin-render-timeline-unity

Copyright © Bentley Systems, Incorporated. All rights reserved.

A simple example showing how to play back 4D animation from an iModel in Unity after exporting the iModel to glTF.

## Prerequisites

- Unity 2020.3 or later

## Getting Started

- Open this project in the Unity Editor.
- Open the Main scene.
- Push Play in the Unity Editor to start the application.
- Push Play in the game UI to start the animation playback.

Camera controls match Unity's scene view:

- WASD to fly
- Hold right-click for mouse-look
- Middle mouse to pan
- Mouse wheel to zoom

## How It Works

The [example-data directory](./example-data/) contains:

- A SYNCHRO example file saved as a Snapshot iModel: [test.bim](./example-data/test.bim)
- The SYNCHRO example exported to glTF using iTwin Platform's [Mesh Export](https://developer.bentley.com/apis/mesh-export/) API: [test-imodel.gltf](./example-data/test-imodel.gltf)
- The [RenderSchedule.Script](https://www.itwinjs.org/reference/core-common/displaystyles/renderschedule/renderschedule.script/)
  controlling the example's 4D animation extracted to JSON: [animation.json](./example-data/animation.json)

The example application loads the glTF model using [glTFast](https://github.com/atteneder/glTFast) and builds a mapping from
the iModel's [element IDs](https://www.itwinjs.org/learning/backend/accesselements/) to the Unity GameObject representation.

The application then parses the animation script and prepares it for playback inside Unity.

## Notes

### Viewing The Included test.bim Example File

I recommend using the [iTwin Exporter for Datasmith](https://developer.bentley.com/unreal) to be able to see the 4D playback
inside an iTwin Viewer. Simply drag and drop it onto the window after signing in.

### Optimization

This example is focused on simplicity and is decidedly not optimized. Depending on your hardware, you may see low framerates
even with this modest example file.

A real application would likely want to combine the elements in each batch together to lower the number of draw calls and
reduce the number of updates required during animation playback. There are also plenty of opportunities for caching in the
playback logic.

Finally, this example does not provide a separate path for linear playback; `ITwinTimeline` and related classes make no
assumptions about prior state and do a complete repaint of scene state on each animation update.

### Importing Other SYNCHRO iModels

Use [Mesh Export](https://developer.bentley.com/apis/mesh-export/) to export the iModel to glTF.

> TODO: include a frontend snippet for extracting `RenderSchedule.ModelTimelineProps[]` from the iModel. Only have a backend snippet at the moment, but no blocker to having a frontend one as well.

Make [ExampleDataConfig](./Assets/ExampleDataConfig.cs) point to your glTF and animation JSON on disk.

### Missing Features

This example is just a starting point and does not include several animation features available in iTwin Viewer.

[RenderSchedule.ts](https://github.com/iTwin/itwinjs-core/blob/master/core/common/src/RenderSchedule.ts) in the iTwin.js monorepo
has a full implementation of playing back this animation. Please refer to it as the authoritative reference on any questions.

Missing features in this Unity implementation include:

#### Linear interpolation for animations

Evaluating the linear interpolation is straightforward, but playing it back in Unity will depend on your shader and optimization strategy.
Please see `RenderSchedule.ts` as mentioned above if a reference is needed on evaluating the interpolation.

#### Transform animation and cutting plane animation

There are several challenges in implementing these.

Actually animating the cutting plane will depend on your shader strategy, which is outside the scope of this document.

Coordinates for both the cutting planes and the transforms are in the iModel coordinate system (right-handed with Z up)
and need to be translated into the Unity coordinate system (left-handed with Y up).

Transform animations are based on the element geometry without its [Placement3d](https://www.itwinjs.org/reference/core-common/geometry/placement3d/)
applied, but glTF geometry is in world space. For elements with transform animation, the inverse of their placement should
be prepended to the animation transform.
