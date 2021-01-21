using Unity.Entities;
using Unity.Mathematics;

public class ResourceAuthoring : IComponentData
{
    public Entity prefab;
	public float amount;
	public float3 position;
	public bool stacked;
	public int stackIndex;
	public int gridX;
	public int gridY;
	public Entity holder;
	public float3 velocity;
}
