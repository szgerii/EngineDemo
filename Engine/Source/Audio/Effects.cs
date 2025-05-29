using System.Collections.Generic;
using Engine.Audio.FX;

namespace Engine.Audio;

public class Effects {
	private Dictionary<uint, Effect> effects = new Dictionary<uint, Effect>();

	public uint TargetHandle { get; private set; } = 0;
	public SoLoud.Bus? BusObject { get; private set; } = null;
	public bool Active { get; private set; } = false;

	/// <summary>
	/// Returns a filter stored at a specific id
	/// </summary>
	public Effect this[uint id] => effects[id];

	/// <summary>
	/// Returns a effect stored at a specific id, with a specific type
	/// </summary>
	public T Get<T>(uint id) where T : Effect => (T)effects[id];

	/// <summary>
	/// Activates these effects
	/// </summary>
	public void Activate(SoLoud.Bus busObject, uint handle) {
		TargetHandle = handle;
		BusObject = busObject;
		foreach (uint key in effects.Keys) {
			Apply(key);
		}
		Active = true;
	}

	private void Apply(uint id) {
		effects[id].Activate(TargetHandle, id);
		BusObject!.setFilter(id, effects[id].EffectObject!);
		effects[id].InitializeParams();
		effects[id].Active = true;
	}

	/// <summary>
	/// Deactivates these filters
	/// </summary>
	public void Deactivate() {
		foreach (uint key in effects.Keys) {
			effects[key].Deactivate();
		}
	}

	private void Clear(uint id) {
		BusObject!.setFilter(id, new SoLoud.SoloudObject());
		effects[id].Deactivate();
	}

	/// <summary>
	/// Stores a effect at a given id
	/// </summary>
	public void Add(uint id, Effect effect) {
		effects.Add(id, effect);
		if (Active) {
			Apply(id);
		}
	}

	/// <summary>
	/// Removes a effect at a given id (replaces it with NoFilter, for active filters)
	/// </summary>
	public void Remove(uint id) {
		if (Active) {
			Clear(id);
		}
		effects.Remove(id);
	}
}
