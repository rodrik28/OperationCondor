using UnityEngine;

public static class OC_VFX
{
    public static void Burst(
        Vector3 position,
        Color color,
        int count = 12)
    {
        GameObject vfxObject = new GameObject("VFX_Burst");
        vfxObject.transform.position = position;

        ParticleSystem ps =
            vfxObject.AddComponent<ParticleSystem>();

        // IMPORTANTE:
        // detener antes de modificar Duration.
        ps.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        var main = ps.main;

        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.35f;

        main.startLifetime =
            new ParticleSystem.MinMaxCurve(0.2f, 0.45f);

        main.startSpeed =
            new ParticleSystem.MinMaxCurve(2f, 5f);

        main.startSize =
            new ParticleSystem.MinMaxCurve(0.08f, 0.18f);

        main.startColor = color;
        main.gravityModifier = 0.25f;
        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        ps.Emit(count);
        ps.Play();

        Object.Destroy(vfxObject, 1.5f);
    }
}