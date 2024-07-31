using UnityEngine;

public static class ColorSettings
{
    public static readonly Color ContainerSlotDefault = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public static readonly Color ContainerSlotDefaultNormal = new Color(0.132f, 0.132f, 0.132f, 0.502f);
    public static readonly Color ContainerSlotHighligtNormal = new Color(0.601f, 0.601f, 0.601f, 0.502f);


    public static readonly Color SortColorMaterial = Color.cyan;
    public static readonly Color SortColorConsumable = Color.green;
    public static readonly Color SortColorWeapon = Color.red;
    public static readonly Color SortColorArmor = Color.yellow;
    public static readonly Color SortColorAmmo = new Color(1f, 0.75f, 0.8f, 1f);
    public static readonly Color SortColorMisc = new Color(1f, 0.5f, 0f, 1f);
    public static readonly Color SortColorTool = Color.magenta;
    public static readonly Color SortColorUtility = Color.blue;

    public static readonly Color DurabilityColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    public static readonly Color HealthColor = new Color(1f, 0.502f, 0.502f);
    public static readonly Color StaminaColor = new Color(1f, 1f, 0.502f);
    public static readonly Color EitrColor = new Color(0.565f, 0.565f, 1f);
}
