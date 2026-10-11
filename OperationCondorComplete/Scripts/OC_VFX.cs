using UnityEngine;
public static class OC_VFX
{
    public static void Burst(Vector3 pos, Color color, int count=16)
    {
        var go=new GameObject("VFX_Burst"); go.transform.position=pos;
        var ps=go.AddComponent<ParticleSystem>(); var main=ps.main; main.duration=.35f; main.startLifetime=.35f; main.startSpeed=3.2f; main.startSize=.08f; main.startColor=color; main.simulationSpace=ParticleSystemSimulationSpace.World; main.maxParticles=64;
        var emission=ps.emission; emission.enabled=false; var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=.15f;
        ps.Emit(count); Object.Destroy(go,1f);
    }
}
