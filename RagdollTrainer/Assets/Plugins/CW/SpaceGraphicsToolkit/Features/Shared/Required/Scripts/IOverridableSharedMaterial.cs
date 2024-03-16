namespace SpaceGraphicsToolkit
{
	public interface IOverridableSharedMaterial
	{
		void RegisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e);
		void UnregisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e);
	}
}