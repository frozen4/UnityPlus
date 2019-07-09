using UnityEngine;

[AddComponentMenu("Base/Particle/Vortex")]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleVortex : ReusableFx
{
	public Vector3 direction = Vector3.forward;
	public float strength = 1.0f;

	public bool useCurve = false;
	public AnimationCurve vortexOverLifetime;

	ParticleSystem m_ParticleSystem;
	ParticleSystem.Particle[] m_Particles;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
            Init();

        enabled = active;
    }
    #endregion

    public override void Tick(float delta) { }

    public override void LateTick(float dt)
    {
        if(!enabled) return;

        if (m_ParticleSystem.isPlaying && Mathf.Abs(strength) > Util.FloatZero)
        {
            if (m_Particles == null || m_Particles.Length < m_ParticleSystem.particleCount)
            {
                m_Particles = new ParticleSystem.Particle[m_ParticleSystem.particleCount];
            }

            int count = m_ParticleSystem.GetParticles(m_Particles);
            var dir1 = direction.normalized;
            for (var i = 0; i < count; ++i)
            {

                Vector3 pos = m_Particles[i].position;
                if (pos.sqrMagnitude > 0.0f)
                {
                    float radius = pos.magnitude;
                    var dir2 = pos / radius;
                    var oldV = m_Particles[i].velocity;

                    oldV = Vector3.Dot(oldV, dir1) * dir1;
                    Vector3 v = Vector3.Cross(dir2, dir1).normalized * radius * strength;

                    if (useCurve)
                    {
                        float lifePercent = 1.0f - m_Particles[i].remainingLifetime / m_Particles[i].startLifetime;
                        v *= vortexOverLifetime.Evaluate(lifePercent);
                    }

                    m_Particles[i].velocity = v + oldV;
                }
            }
            m_ParticleSystem.SetParticles(m_Particles, count);
        }
    }

    private void Init()
    {
        if(m_ParticleSystem == null)
            m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    void Start()
    {
        Init();
    }

    void LateUpdate()
	{
	    LateTick(0);
	}
}
