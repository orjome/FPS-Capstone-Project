using Sandbox;

public sealed class PlayerHealth : Component
{
	[Property, Sync] public float Health { get; set; } = 100f;
	[Property, Sync] public float MaxHealth { get; set; } = 100f;
	[Property, Sync] public bool IsDead { get; set; } = false;
	bool _lastStandUsed = false;

	[Rpc.Owner]
	public void TakeDamage( float amount, GameObject attacker = null )
	{
		if ( IsDead ) return;

		Health -= amount;
		Log.Info( $"{GameObject.Name} took {amount} damage from {attacker?.Name ?? "unknown"}! Health: {Health}" );

		if ( Health <= 0 )
		{
			// Check Last Stand skill
			var skillTree = GetComponent<SkillTree>();
			if ( skillTree != null && skillTree.IsUnlocked( SkillType.LastStand ) && !_lastStandUsed )
			{
				_lastStandUsed = true;
				Health = 1f;
				Log.Info( "LAST STAND TRIGGERED! Survived a fatal hit with 1hp!" );
				return;
			}

			Health = 0;
			IsDead = true;
			OnDeath( attacker );
		}
	}

	void OnDeath( GameObject attacker )
	{
		Log.Info( $"{GameObject.Name} has died!" );

		// Disable player controller so they cant move
		var controller = GetComponent<PlayerController>();
		if ( controller != null )
			controller.Enabled = false;

		// Disable all weapons (including nested/picked-up ones)
		var weapons = GetComponentsInChildren<WeaponBase>();
		foreach ( var weapon in weapons )
			weapon.Enabled = false;

		// Notify the game manager, crediting the killer
		var manager = Scene.GetAllComponents<ArenaGameManager>().FirstOrDefault();
		manager?.OnPlayerKilled( GameObject, attacker );
	}

	public void Respawn( Transform spawnPoint )
	{
		_lastStandUsed = false;
		Health = MaxHealth;
		IsDead = false;

		WorldTransform = spawnPoint;

		var controller = GetComponent<PlayerController>();
		if ( controller != null )
			controller.Enabled = true;

		GetComponent<WeaponManager>()?.ResetForRespawn();

		Log.Info( $"{GameObject.Name} respawned!" );
	}
}
