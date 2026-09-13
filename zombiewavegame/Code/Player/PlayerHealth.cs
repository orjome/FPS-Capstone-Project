using Sandbox;

public sealed class PlayerHealth : Component
{
	[Property, Sync] public float Health { get; set; } = 100f;
	[Property, Sync] public float MaxHealth { get; set; } = 100f;
	[Property, Sync] public bool IsDead { get; set; } = false;
	bool _lastStandUsed = false;

	public void TakeDamage( float amount )
	{
		if ( IsDead ) return;

		Health -= amount;
		Log.Info( $"Player took {amount} damage! Health: {Health}" );

		if ( Health <= 0 )
		{
			// Check Last Stand skill
			var skillTree = Scene.GetAllComponents<SkillTree>().FirstOrDefault();
			if ( skillTree != null && skillTree.IsUnlocked( SkillType.LastStand ) && !_lastStandUsed )
			{
				_lastStandUsed = true;
				Health = 1f;
				Log.Info( "LAST STAND TRIGGERED! Survived a fatal hit with 1hp!" );
				return;
			}

			Health = 0;
			IsDead = true;
			OnDeath();
		}
	}

	void OnDeath()
	{
		Log.Info( "Player has died!" );

		// Disable player controller so they cant move
		var controller = GetComponent<PlayerController>();
		if ( controller != null )
			controller.Enabled = false;

		// Disable all weapons
		var weapons = GetComponents<WeaponBase>();
		foreach ( var weapon in weapons )
			weapon.Enabled = false;

		// Notify the game manager
		var manager = Scene.GetAllComponents<ZombieGameManager>().FirstOrDefault();
		manager?.OnPlayerDied();
	}

	public void Respawn()
	{
		_lastStandUsed = false;
		Health = MaxHealth;
		IsDead = false;

		var controller = GetComponent<PlayerController>();
		if ( controller != null )
			controller.Enabled = true;

		var weapons = Scene.GetAllComponents<WeaponBase>();
		foreach ( var weapon in weapons )
			weapon.Enabled = true;

		Log.Info( "Player respawned!" );
	}
}
