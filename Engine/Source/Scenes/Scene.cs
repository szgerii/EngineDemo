﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Scenes;

public class Scene : IUpdatable, IDrawable {
	public event EventHandler<GameTime> PreUpdate, PostUpdate;
	public event EventHandler<GameTime> PreDraw, PostDraw;

	public bool Loaded { get; protected set; } = false;

	public List<GameObject> GameObjects { get; protected set; } = new List<GameObject>();

	private Queue<GameObject> objAddBuffer = new Queue<GameObject>();
	private Queue<GameObject> objRemoveBuffer = new Queue<GameObject>();

	public virtual void Load() {
		foreach (GameObject obj in GameObjects) {
			obj.Load();
		}

		Loaded = true;
	}

	public virtual void Unload() {
		foreach (GameObject obj in GameObjects) {
			obj.Unload();
		}

		Loaded = false;
	}

	public virtual void Update(GameTime gameTime) {
		PreUpdate?.Invoke(null, gameTime);

		foreach (GameObject obj in GameObjects) {
			obj.Update(gameTime);
		}

		PostUpdate?.Invoke(null, gameTime);
	}

	public virtual void Draw(GameTime gameTime) {
		PreDraw?.Invoke(null, gameTime);

		foreach (GameObject obj in GameObjects) {
			obj.Draw(gameTime);
		}

		PostDraw?.Invoke(null, gameTime);
	}

	public virtual void AddObject(GameObject obj) {
		if (Loaded) {
			objAddBuffer.Enqueue(obj);
		} else {
			GameObjects.Add(obj);
		}
	}

	public virtual void RemoveObject(GameObject obj) {
		if (Loaded) {
			objRemoveBuffer.Enqueue(obj);
		} else {
			GameObjects.Remove(obj);
		}
	}

	public virtual void PerformObjectAdditions() {
		while (objAddBuffer.Count > 0) {
			GameObject obj = objAddBuffer.Dequeue();

			GameObjects.Add(obj);
			obj.Load();
		}
	}

	public virtual void PerformObjectRemovals() {
		while (objRemoveBuffer.Count != 0) {
			GameObject obj = objRemoveBuffer.Dequeue();

			obj.Unload();
			GameObjects.Remove(obj);
		}
	}
}
