using UnityEngine;

public class PlayerActivityEmitter : MonoBehaviour
{
    public ParticleSystem activityParticleSystem;
    public float emitInterval = 0.2f;

    float timer;

    void Update()
    {
        if (activityParticleSystem == null) return;

        timer += Time.deltaTime;
        if (timer >= emitInterval)
        {
            timer = 0f;

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position;

            activityParticleSystem.Emit(emitParams, 1);
        }
    }
}
