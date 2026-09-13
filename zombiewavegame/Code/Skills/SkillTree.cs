using Sandbox;
using System.Collections.Generic;

public enum SkillType
{
	// Combat
	FastHands,
	HollowPoint,
	Overkill,

	// Survival
	IronSkin,
	Adrenaline,
	LastStand,

	// Mobility
	LightFoot,
	Parkour,
	Slippery
}

public class Skill
{
	public SkillType Type { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int XPCost { get; set; }
	public bool IsUnlocked { get; set; } = false;
	public SkillType? Requires { get; set; } = null;

	public bool CanUnlock( List<Skill> allSkills )
	{
		if ( IsUnlocked ) return false;
		if ( Requires == null ) return true;
		var required = allSkills.Find( s => s.Type == Requires );
		return required != null && required.IsUnlocked;
	}
}

public sealed class SkillTree : Component
{
	public List<Skill> Skills { get; set; } = new()
	{
        // Combat
        new Skill { Type = SkillType.FastHands,   Name = "Fast Hands",   Description = "50% faster reload",          XPCost = 500,  Requires = null },
		new Skill { Type = SkillType.HollowPoint,  Name = "Hollow Point", Description = "25% increased damage",       XPCost = 1000, Requires = SkillType.FastHands },
		new Skill { Type = SkillType.Overkill,     Name = "Overkill",     Description = "Double reserve ammo",        XPCost = 2000, Requires = SkillType.HollowPoint },

        // Survival
        new Skill { Type = SkillType.IronSkin,     Name = "Iron Skin",    Description = "+50 max health",             XPCost = 500,  Requires = null },
		new Skill { Type = SkillType.Adrenaline,   Name = "Adrenaline",   Description = "Regen 10hp per kill",        XPCost = 1000, Requires = SkillType.IronSkin },
		new Skill { Type = SkillType.LastStand,    Name = "Last Stand",   Description = "Survive one fatal hit",      XPCost = 2000, Requires = SkillType.Adrenaline },

        // Mobility
        new Skill { Type = SkillType.LightFoot,    Name = "Light Foot",   Description = "25% faster movement",       XPCost = 500,  Requires = null },
		new Skill { Type = SkillType.Parkour,      Name = "Parkour",      Description = "50% higher jump",           XPCost = 1000, Requires = SkillType.LightFoot },
		new Skill { Type = SkillType.Slippery,     Name = "Slippery",     Description = "Zombies can't slow you",    XPCost = 2000, Requires = SkillType.Parkour },
	};

	PlayerXP _playerXP;

	protected override void OnStart()
	{
		_playerXP = Scene.GetAllComponents<PlayerXP>().FirstOrDefault();
	}

	public bool TryUnlock( SkillType type )
	{
		var skill = Skills.Find( s => s.Type == type );
		if ( skill == null ) return false;
		if ( !skill.CanUnlock( Skills ) ) return false;
		if ( _playerXP == null ) return false;
		if ( _playerXP.XP < skill.XPCost ) return false;

		_playerXP.XP -= skill.XPCost;
		skill.IsUnlocked = true;

		ApplySkill( skill );
		Log.Info( $"Unlocked skill: {skill.Name}!" );
		return true;
	}

	void ApplySkill( Skill skill )
	{
		var playerHealth = Scene.GetAllComponents<PlayerHealth>().FirstOrDefault();
		var playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault();
		var weapons = Scene.GetAllComponents<WeaponBase>().ToList();

		switch ( skill.Type )
		{
			case SkillType.FastHands:
				foreach ( var w in weapons )
				{
					w.FireRate *= 0.5f;
					w.ReloadTime *= 0.5f;
				}
				break;

			case SkillType.HollowPoint:
				foreach ( var w in weapons )
					w.Damage *= 1.25f;
				break;

			case SkillType.Overkill:
				foreach ( var w in weapons )
					w.ReserveAmmo *= 2;
				break;

			case SkillType.IronSkin:
				if ( playerHealth != null )
				{
					playerHealth.MaxHealth += 50f;
					playerHealth.Health += 50f;
				}
				break;

			case SkillType.Adrenaline:
				break;

			case SkillType.LastStand:
				break;

			case SkillType.LightFoot:
				if ( playerController != null )
					playerController.WalkSpeed *= 1.25f;
				break;

			case SkillType.Parkour:
				if ( playerController != null )
					playerController.JumpSpeed *= 1.5f;
				break;

			case SkillType.Slippery:
				break;
		}
	}

	public bool IsUnlocked( SkillType type )
	{
		return Skills.Find( s => s.Type == type )?.IsUnlocked ?? false;
	}

	public Skill GetSkill( SkillType type )
	{
		return Skills.Find( s => s.Type == type );
	}
}
