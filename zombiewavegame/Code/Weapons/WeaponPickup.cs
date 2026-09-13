using Sandbox;

public sealed class WeaponPickup : Component
{
	[Property] public string WeaponName { get; set; } = "Rifle";
	[Property] public float Damage { get; set; } = 15f;
	[Property] public int MaxAmmo { get; set; } = 30;
	[Property] public int ReserveAmmo { get; set; } = 120;
	[Property] public float FireRate { get; set; } = 0.1f;
	[Property] public bool IsAutomatic { get; set; } = true;
	[Property] public int PelletCount { get; set; } = 1;
	[Property] public float Spread { get; set; } = 0f;
	[Property] public float PickupRadius { get; set; } = 50f;
	[Property] public bool ShowDebugRays { get; set; } = false;
	[Property] public float ReloadTime { get; set; } = 2f;
	[Property] public Vector3 ViewModelPosition { get; set; } = new Vector3( 35, -10, -15 );
	[Property] public Vector3 ViewModelRotation { get; set; } = new Vector3( 0, 0, 0 );
	[Property] public SoundEvent ShootSound { get; set; }
	[Property] public SoundEvent ReloadSound { get; set; }
	[Property] public SoundEvent EmptySound { get; set; }
	[Property] public float RecoilAmount { get; set; } = 1f;
	[Property] public float RecoilRecovery { get; set; } = 5f;

	protected override void OnUpdate()
	{
		var player = Scene.GetAllComponents<PlayerHealth>().FirstOrDefault();
		if ( player == null ) return;

		float dist = Vector3.DistanceBetween( WorldPosition, player.WorldPosition );

		if ( dist <= PickupRadius )
		{
			DebugOverlay.Text( WorldPosition + Vector3.Up * 20f, $"Press E to pick up {WeaponName}", 0.1f );

			if ( Input.Pressed( "use" ) )
			{
				TryPickup( player );
			}
		}
	}

	void TryPickup( PlayerHealth player )
	{
		var weaponManager = player.GameObject.GetComponent<WeaponManager>();
		if ( weaponManager == null ) return;

		var weaponGo = new GameObject();
		weaponGo.Name = WeaponName;

		// Copy the model first
		var existingModel = GetComponent<ModelRenderer>();
		ModelRenderer newModel = null;
		if ( existingModel != null )
		{
			newModel = weaponGo.AddComponent<ModelRenderer>();
			newModel.Model = existingModel.Model;
		}

		// Create weapon after model
		var weapon = weaponGo.AddComponent<WeaponBase>();
		weapon.WeaponName = WeaponName;
		weapon.Damage = Damage;
		weapon.MaxAmmo = MaxAmmo;
		weapon.ReserveAmmo = ReserveAmmo;
		weapon.FireRate = FireRate;
		weapon.IsAutomatic = IsAutomatic;
		weapon.PelletCount = PelletCount;
		weapon.Spread = Spread;
		weapon.Ammo = MaxAmmo;
		weapon.ShowDebugRays = ShowDebugRays;
		weapon.ReloadTime = ReloadTime;
		weapon.ViewModelPosition = ViewModelPosition;
		weapon.ViewModelRotation = ViewModelRotation;
		weapon.ViewModel = newModel; // now set after weapon is declared
		weapon.ShootSound = ShootSound;
		weapon.ReloadSound = ReloadSound;
		weapon.EmptySound = EmptySound;
		weapon.RecoilAmount = RecoilAmount;
		weapon.RecoilRecovery = RecoilRecovery;

		var skillTree = Scene.GetAllComponents<SkillTree>().FirstOrDefault();
		if ( skillTree != null )
		{
			if ( skillTree.IsUnlocked( SkillType.FastHands ) )
			{
				weapon.FireRate *= 0.5f;
				weapon.ReloadTime *= 0.5f;
			}
			if ( skillTree.IsUnlocked( SkillType.HollowPoint ) )
				weapon.Damage *= 1.25f;
			if ( skillTree.IsUnlocked( SkillType.Overkill ) )
				weapon.ReserveAmmo *= 2;
		}

		bool picked = weaponManager.PickupWeapon( weapon );

		if ( picked )
		{
			GameObject.Destroy();
		}
	}
}
