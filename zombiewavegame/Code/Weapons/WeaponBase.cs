using Sandbox;
using System.Threading.Tasks;

public sealed class WeaponBase : Component
{
	[Property] public string WeaponName { get; set; } = "Pistol";
	[Property] public float Damage { get; set; } = 25f;
	[Property] public int MaxAmmo { get; set; } = 12;
	[Property] public int ReserveAmmo { get; set; } = 60;
	[Property, Sync] public int Ammo { get; set; } = 12;
	[Property, Sync] public bool IsReloading { get; set; } = false;
	[Property] public float FireRate { get; set; } = 0.5f;
	[Property] public bool IsAutomatic { get; set; } = false;
	[Property] public int PelletCount { get; set; } = 1;
	[Property] public float Spread { get; set; } = 0f;
	[Property] public bool ShowDebugRays { get; set; } = false;
	[Property] public float ReloadTime { get; set; } = 2f;
	[Property] public Vector3 ViewModelPosition { get; set; } = new Vector3( 35, -10, -15 );
	[Property] public Vector3 ViewModelRotation { get; set; } = new Vector3( 0, 0, 0 );
	[Property] public ModelRenderer ViewModel { get; set; }

	// Sounds
	[Property] public SoundEvent ShootSound { get; set; }
	[Property] public SoundEvent ReloadSound { get; set; }
	[Property] public SoundEvent EmptySound { get; set; }

	// Recoil
	[Property] public float RecoilAmount { get; set; } = 1f;
	[Property] public float RecoilRecovery { get; set; } = 5f;

	float _nextFireTime = 0f;
	float _currentRecoil = 0f;
	PlayerXP _ownerXP;
	CameraComponent _camera;

	protected override void OnStart()
	{
		_ownerXP = GameObject.Root.GetComponentInChildren<PlayerXP>();
		Ammo = MaxAmmo;

		_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		if ( _camera != null )
		{
			GameObject.SetParent( _camera.GameObject );
			LocalPosition = ViewModelPosition;
			LocalRotation = Rotation.From( ViewModelRotation.x, ViewModelRotation.y, ViewModelRotation.z );
		}
	}

	protected override void OnUpdate()
	{
		var skillTreeUI = Scene.GetAllComponents<SkillTreeUI>().FirstOrDefault();
		if ( skillTreeUI != null && skillTreeUI.IsOpen ) return;

		bool triggerHeld = IsAutomatic ? Input.Down( "attack1" ) : Input.Pressed( "attack1" );

		if ( triggerHeld && !IsReloading && Time.Now >= _nextFireTime )
		{
			if ( Ammo <= 0 )
			{
				// Play empty click
				if ( EmptySound != null )
					Sound.Play( EmptySound, WorldPosition );
				_nextFireTime = Time.Now + FireRate;
			}
			else
			{
				Shoot();
			}
		}

		if ( Input.Pressed( "reload" ) && !IsReloading && Ammo < MaxAmmo )
		{
			_ = Reload();
		}	

		// Recover recoil
		if ( _currentRecoil > 0 )
		{
			_currentRecoil -= RecoilRecovery * Time.Delta;
			_currentRecoil = System.Math.Max( 0, _currentRecoil );

			if ( _camera != null )
			{
				var angles = _camera.WorldRotation.Angles();
				angles.pitch -= RecoilRecovery * Time.Delta;
				_camera.WorldRotation = Rotation.From( angles );
			}
		}
	}

	void Shoot()
	{
		if ( Ammo <= 0 ) return;

		Ammo--;
		_nextFireTime = Time.Now + FireRate;

		// Play shoot sound
		if ( ShootSound != null )
			Sound.Play( ShootSound, WorldPosition );

		// Apply recoil
		if ( _camera != null )
		{
			_currentRecoil += RecoilAmount;
			var angles = _camera.WorldRotation.Angles();
			angles.pitch -= RecoilAmount;
			_camera.WorldRotation = Rotation.From( angles );
		}

		for ( int i = 0; i < PelletCount; i++ )
		{
			var spreadVector = new Vector3(
				Game.Random.Float( -Spread, Spread ),
				Game.Random.Float( -Spread, Spread ),
				0
			);

			var direction = (_camera.WorldRotation.Forward + spreadVector).Normal;
			var ray = new Ray( _camera.WorldPosition, direction );
			var tr = Scene.Trace.Ray( ray, 5000f )
				.WithoutTags( "player" )
				.Run();

			if ( ShowDebugRays )
				DebugOverlay.Line( ray.Position, tr.EndPosition, Color.Red, 1f );

			if ( tr.Hit && tr.GameObject is not null )
			{
				var zombie = tr.GameObject.GetComponent<ZombieAI>();
				if ( zombie != null )
				{
					zombie.TakeDamage( Damage );

					var crosshair = Scene.GetAllComponents<Crosshair>().FirstOrDefault();
					crosshair?.TriggerHitmarker();

					if ( ShowDebugRays )
						DebugOverlay.Line( tr.EndPosition, tr.EndPosition + Vector3.Up * 10f, Color.Green, 1f );
				}
				else
				{
					if ( ShowDebugRays )
						DebugOverlay.Line( tr.EndPosition, tr.EndPosition + Vector3.Up * 10f, Color.Yellow, 1f );
				}
			}
		}
	}

	async Task Reload()
	{
		if ( ReserveAmmo <= 0 ) return;

		IsReloading = true;

		if ( ReloadSound != null )
			Sound.Play( ReloadSound, WorldPosition );

		await Task.DelaySeconds( ReloadTime );

		int ammoNeeded = MaxAmmo - Ammo;
		int ammoToLoad = System.Math.Min( ammoNeeded, ReserveAmmo );

		Ammo += ammoToLoad;
		ReserveAmmo -= ammoToLoad;

		IsReloading = false;
	}
}
