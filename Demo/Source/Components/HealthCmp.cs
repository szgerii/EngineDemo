using Demo.Scenes;
using Engine;
using Engine.Components;
using Engine.Debug;
using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Demo.Components;

public class HealthCmp : Component {
	private int health;
	public int Health {
		get => health;
		set {
			health = value;
			if (RemoveOnDeath && health <= 0) {
				Die();
			}
			if (health > MaxHealth) {
				health = MaxHealth;
			}
		}
	}

	private int maxHealth;
	public int MaxHealth {
		get => maxHealth;
		set {
			maxHealth = value;
			if (Health > maxHealth) {
				Health = maxHealth;
			}
		}
	}

	private bool removeOnDeath = true;
	public bool RemoveOnDeath {
		get => removeOnDeath;
		set {
			removeOnDeath = value;
			if (removeOnDeath && Health <= 0) {
				Die();
			}
		}
	}

	private static ITexture2D debugHealthTex = Utils.GenerateTexture(1, 1, new Color(Color.Red, 0.2f));

	static HealthCmp() {
		DebugMode.AddFeature(new LoopedDebugFeature("draw-health", (object? s, GameTime gt) => {
			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj.GetComponent(out HealthCmp? h)) {
					h.DebugDrawHealth();
				}
			}
		}, GameLoopStage.PRE_DRAW));
	}

	public HealthCmp(int health, int maxHealth = -1) {
		MaxHealth = maxHealth == -1 ? health : maxHealth;
		Health = health;
	}

	public void Die() {
		Game.RemoveObject(Owner!);
	}

	public void DebugDrawHealth() {
		int fullWidth, fullHeight;
		if (Owner!.GetComponent(out SpriteCmp? sprite)) {
			fullWidth = sprite.SourceRectangle?.Width ?? sprite.Texture?.Width ?? 0;
			fullHeight = sprite.SourceRectangle?.Height ?? sprite.Texture?.Height ?? 0;
		} else if (Owner.GetComponent(out AnimatedSpriteCmp? animSprite)) {
			fullWidth = animSprite.FrameWidth;
			fullHeight = animSprite.FrameHeight;
		} else {
			return;
		}

		float scale = Health / (float)MaxHealth;
		int height = Utils.Round(fullHeight * scale);
		Rectangle destRec = new Rectangle((int)Owner.ScreenPosition.X, (int)Owner.ScreenPosition.Y + (fullHeight - height), fullWidth, height);
		Game.SpriteBatch!.Draw(debugHealthTex.ToTexture2D(), Owner.Position, debugHealthTex.Bounds, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.9f);
	}
}
