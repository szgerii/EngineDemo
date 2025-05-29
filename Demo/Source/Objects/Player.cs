using Engine;
using Engine.Components;
using Engine.Collision;
using Engine.Input;
using Microsoft.Xna.Framework;
using Demo.Components;
using Engine.Source.Interfaces;

namespace Demo.Objects;

public class Player : GameObject, IHasCenter {
	public const int WALK_SPEED = 200;
	public static Player? Instance { get; private set; }

	public Vector2 Center => Position + new Vector2(sprite.FrameWidth * 0.5f, sprite.FrameHeight * 0.5f);

	private readonly AnimatedSpriteCmp sprite;
	private readonly DirectionalAnimationCmp dirAnimCmp;
	private readonly CollisionCmp collCmp;
	private readonly ColliderCollectionCmp hitboxes;

	public Player(Vector2 pos) : base(pos) {
		// player collider
		collCmp = new CollisionCmp(8, 24, 16, 8);
		collCmp.Tags = CollisionTags.Player;
		collCmp.Targets = CollisionTags.World;
		Attach(collCmp);

		// sets up hitboxes cmp
		hitboxes = new ColliderCollectionCmp();
		hitboxes.DefaultTargets = CollisionTags.Damageable;
		Attach(hitboxes);

		sprite = new(Game.LoadTexture("assets/sprites/entities/player"), 3, 4, 10);
		sprite.LayerDepth = 0.5f;
		sprite.YSortEnabled = true;
		sprite.YSortOffset = sprite.FrameHeight;

		// Idle animations
		sprite.Animations["IdleDown"] = new Animation(0, 1, true, 1);
		sprite.Animations["IdleUp"] = new Animation(3, 1, true, 1);
		sprite.Animations["IdleLeft"] = new Animation(1, 1, true, 1);
		sprite.Animations["IdleRight"] = new Animation(2, 1, true, 1);

		// Walk animations
		sprite.Animations["WalkDown"] = new Animation(0, 3, true);
		sprite.Animations["WalkUp"] = new Animation(3, 3, true);
		sprite.Animations["WalkLeft"] = new Animation(1, 3, true);
		sprite.Animations["WalkRight"] = new Animation(2, 3, true);

		Attach(sprite);

		// sets up directional animations
		dirAnimCmp = new DirectionalAnimationCmp();
		Attach(dirAnimCmp);

		HealthCmp healthCmp = new HealthCmp(10);
		Attach(healthCmp);

		Instance = this;
	}

	public override void Update(GameTime gameTime) {
		Vector2 delta = Vector2.Zero;
		if (InputManager.Actions.IsDown("up")) {
			delta -= Vector2.UnitY;
		}
		if (InputManager.Actions.IsDown("down")) {
			delta += Vector2.UnitY;
		}
		if (InputManager.Actions.IsDown("left")) {
			delta -= Vector2.UnitX;
		}
		if (InputManager.Actions.IsDown("right")) {
			delta += Vector2.UnitX;
		}

		if (delta != Vector2.Zero) {
			delta.Normalize();
			dirAnimCmp.AnimationName = "Walk";
		} else {
			dirAnimCmp.AnimationName = "Idle";
		}

		delta *= (float)(WALK_SPEED * gameTime.ElapsedGameTime.TotalSeconds);
		Vector2 effectiveDelta = collCmp.MoveOwner(delta);

		if (effectiveDelta != Vector2.Zero) {
			dirAnimCmp.SetDirectionFromVector(effectiveDelta);
		} else if (delta != Vector2.Zero) {
			dirAnimCmp.SetDirectionFromVector(delta);
		}

		base.Update(gameTime);
	}
}
