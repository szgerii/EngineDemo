using SoLoud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Engine.Audio.Types;

namespace Engine.Audio;

public static class AudioManager {
	/// <summary>
	/// The relative path to the audio resources
	/// </summary>
	public const string REL_AUDIO_PATH = $"Content/assets/audio/";

	/// <summary>
	/// The main Soloud object used to interact with soloud
	/// </summary>
	public static Soloud Soloud { get; private set; } = new Soloud();
	public static Bus MasterBus { get; private set; } = new Bus("master", targetBus: null);

	private static Dictionary<string, AudioSource> audio = new Dictionary<string, AudioSource>();
	/// <summary>
	/// The audio clips managed by this AudioManager
	/// </summary>
	public static IReadOnlyDictionary<string, AudioSource> Audio => audio;

	private static Dictionary<string, Bus> buses = new Dictionary<string, Bus>();
	/// <summary>
	/// The audio buses manager by this AudioManager
	/// </summary>
	public static IReadOnlyDictionary<string, Bus> Buses => buses;

	public static void Initialize() {
		string path = new Uri(typeof(AudioManager).Assembly.Location).LocalPath;
		string? folder = Path.GetDirectoryName(path);
		if (folder == null)
			throw new AudioLoadException("Couldn't determine a valid path for the current directory");
		
		string subfolder = Environment.Is64BitProcess ? "\\win64\\" : "\\win32\\";

		NativeLibrary.Load(folder + subfolder + "soloud.dll");

		int status = Soloud.init();
		if (status != 0) {
			throw new AudioLoadException(
				"An error occurred when attempting to initialize soloud.\n" +
				$"Error code: {status}; error message: {Soloud.getErrorString(status)}");
		}

		AddBus(MasterBus);
		MasterBus.Load();
		MasterBus.Init();
	}

	/// <summary>
	/// Adds an audio clip to the manager
	/// </summary>
	public static void AddAudio(AudioSource source) {
		audio.Add(source.Name, source);
	}

	public static void RemoveAudio(string name) {
		AudioSource src = audio[name];
		src.Unload();
		audio.Remove(name);
	}

	public static T GetAudio<T>(string name) where T : AudioSource => (T)audio[name];

	public static void AddBus(Bus bus) {
		buses.Add(bus.Name, bus);
	}

	public static void RemoveBus(string name) {
		Bus b = buses[name];
		b.Unload();
		buses.Remove(name);
	}
}

public class AudioInitializationException : Exception {
	public AudioInitializationException() { }
	public AudioInitializationException(string message) : base(message) { }
}