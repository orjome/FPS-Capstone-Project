using Sandbox;

public sealed class ZombieAI : Component
{
	[Property] public float MoveSpeed { get; set; } = 120f;
	[Property] public float AttackRange { get; set; } = 60f;
	[Property] public float AttackDamage { get; set; } = 10f;
	[Property, Sync] public float Health { get; set; } = 100f;
	[Property] public SoundEvent IdleSound { get; set; }
	[Property] public SoundEvent AttackSound { get; set; }
	[Property] public float IdleSoundInterval { get; set; } = 5f;

	NavMeshAgent _agent;
	float _nextIdleSound = 0f;
	float _nextAttackTime = 0f;

	protected override void OnStart()
	{
		_agent = GetComponent<NavMeshAgent>();
		_nextIdleSound = Time.Now + Game.Random.Float( 0f, IdleSoundInterval );
	}

	protected override void OnFixedUpdate()
	{
		var player = Scene.GetAllComponents<PlayerHealth>().FirstOrDefault();
		if ( player == null ) return;

		// Always face the player
		var direction = (player.WorldPosition - WorldPosition).WithZ( 0 ).Normal;
		if ( direction.Length > 0 )
		{
			WorldRotation = Rotation.LookAt( direction, Vector3.Up );
		}

		float dist = Vector3.DistanceBetween( WorldPosition, player.WorldPosition );

		if ( dist > AttackRange )
		{
			_agent.MoveTo( player.WorldPosition );
		}
		else
		{
			_agent.Stop();
			TryAttack( player );
		}

		// Play idle sound periodically
		if ( Time.Now >= _nextIdleSound )
		{
			if ( IdleSound != null )
				Sound.Play( IdleSound, WorldPosition );

			_nextIdleSound = Time.Now + Game.Random.Float( IdleSoundInterval * 0.5f, IdleSoundInterval * 1.5f );
		}
	}

	void TryAttack( PlayerHealth player )
	{
		if ( Time.Now < _nextAttackTime ) return;
		_nextAttackTime = Time.Now + 1f;

		player.TakeDamage( AttackDamage );

		if ( AttackSound != null )
			Sound.Play( AttackSound, WorldPosition );
	}

	public void TakeDamage( float dmg )
	{
		Health -= dmg;
		Log.Info( $"Zombie took {dmg} damage! Health: {Health}" );

		if ( Health <= 0 )
		{
			var playerXP = Scene.GetAllComponents<PlayerXP>().FirstOrDefault();
			playerXP?.AddXP( 100 );

			var skillTree = Scene.GetAllComponents<SkillTree>().FirstOrDefault();
			if ( skillTree != null && skillTree.IsUnlocked( SkillType.Adrenaline ) )
			{
				var playerHealth = Scene.GetAllComponents<PlayerHealth>().FirstOrDefault();
				if ( playerHealth != null )
				{
					playerHealth.Health = System.Math.Min( playerHealth.Health + 10f, playerHealth.MaxHealth );
					Log.Info( $"Adrenaline! Healed 10hp. Current health: {playerHealth.Health}" );
				}
			}

			var manager = Scene.GetAllComponents<ZombieGameManager>().FirstOrDefault();
			manager?.OnZombieDied();

			Log.Info( "Zombie died! Player awarded 100 XP." );
			GameObject.Destroy();
		}
	}
}
