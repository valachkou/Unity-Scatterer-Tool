# Unity Scatterer Tool

## Overview
The **Unity Scatterer Tool** is a custom C# script for Unity that emulates the functionality of FMOD's scatterer instrument. It enables dynamic spatial audio effects by scattering audio sources in 3D space around a specified object, with customizable parameters for delay, volume, pitch, and spatial positioning. This tool is designed to enhance immersive audio experiences in game development, offering a lightweight and flexible alternative for spatial sound design within Unity's audio system.

## Features
- **Dynamic Audio Scattering**: Randomly positions audio sources within a defined 3D range (min/max scatter distance) to create spatial sound effects.
- **Customizable Audio Parameters**:
  - Configurable playback delay (min/max delay).
  - Randomized volume and pitch for varied audio playback.
  - Support for multiple audio clips with a non-repeating playback mechanism.
- **Audio Source Pooling**: Efficiently manages a pool of audio sources to optimize performance.
- **3D/2D Audio Support**: Toggle between 3D spatial audio and 2D playback.
- **Integration with Unity Audio Mixer**: Routes audio through a specified AudioMixerGroup for advanced mixing.
- **Smooth Audio Source Movement**: Implements smooth transitions for audio source positions using coroutines.
- **Configurable Attenuation**: Supports different audio rolloff modes for realistic sound falloff.

## Requirements
- **Unity Version**: 2019.4 or later.
- **Dependencies**: Unity's Audio Module (included by default).
- **Optional**: Unity Audio Mixer for advanced audio routing.

## Installation
1. Clone or download this repository to your local machine.
2. Copy the `AnalogScattererInstrumentForUnity.cs` script to your Unity project's `Assets/Scripts` folder.
3. In Unity, attach the `AnalogScattererInstrumentForUnity` component to a GameObject.
4. Configure the component in the Unity Inspector:
   - Assign an array of `AudioClips` to play.
   - Set the `AudioMixer` and `AudioMixerGroup` (optional) for audio routing.
   - Adjust `SpawnRate`, `minDelay`, `maxDelay`, `minVolume`, `maxVolume`, `minPitch`, `maxPitch`, `minScatterDistance`, `maxScatterDistance`, and `audioRolloff` as needed.
   - Enable or disable `enable3D` for 3D/2D audio playback.
5. Ensure the GameObject has an `AudioSource` component (automatically added if missing).

## Usage
1. Attach the `AnalogScattererInstrumentForUnity` script to a GameObject in your scene (e.g., an environment object like a forest or crowd).
2. Assign audio clips (e.g., ambient sounds, footsteps, or effects) to the `audioClips` array in the Inspector.
3. Configure spatial and audio settings to match your project's needs:
   - `SpawnRate`: Controls how often audio clips are played (0-500).
   - `minDelay`/`maxDelay`: Sets the range for random playback delays (0.1-300 seconds).
   - `minScatterDistance`/`maxScatterDistance`: Defines the 3D scattering range (0-500 units).
   - `minVolume`/`maxVolume`: Sets the volume randomization range (0-1).
   - `minPitch`/`maxPitch`: Sets the pitch randomization range (0.99-1.01).
4. Play the scene to hear the scattered audio effects.
5. Use the `StopPlaying()` method to halt playback programmatically if needed.

## Example
To create an ambient forest soundscape:
1. Attach the script to a GameObject representing the forest.
2. Add forest sounds (e.g., bird chirps, rustling leaves) to the `audioClips` array.
3. Set `minScatterDistance` to 10 and `maxScatterDistance` to 50 for a wide spatial effect.
4. Configure `minDelay` to 5 and `maxDelay` to 20 for natural timing.
5. Set `audioRolloff` to `Logarithmic` for realistic sound falloff.
6. Play the scene to experience randomized, spatialized forest sounds.

## Demo
Check out the tool in action: [Unity Scatterer Tool Demo](https://youtu.be/UJi7fI_hofU)

## Contributing
Feel free to fork this repository and submit pull requests with improvements or bug fixes. For feature requests or issues, please open a GitHub issue.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
Developed by Yan Valachkou.  
Email: [ercgercorg@gmail.com](mailto:ercgercorg@gmail.com)  
LinkedIn: [linkedin.com/in/yan-volochkov-41ab12208](https://www.linkedin.com/in/yan-volochkov-41ab12208)
