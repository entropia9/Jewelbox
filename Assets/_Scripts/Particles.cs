using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Particles : MonoBehaviour
{
    public enum ParticleValue
    {
        Amber,
        Aquamarine,
        Darkred,
        Peridot,
        Purple,
        Ruby
    }
    public ParticleValue particleValue;
    ParticleSystem particles;
    ParticleSystem.Particle[] activeParticles;
    public Vector3 target;
    float elapsedTime = 0f;
    Gauge correspondingGauge;

    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        correspondingGauge = FindCorrespondingGauge();
        activeParticles = new ParticleSystem.Particle[20];
    }

    Gauge FindCorrespondingGauge()
    {
        List<Gauge> gauges = FindObjectsOfType<Gauge>().ToList();
        foreach (Gauge gauge in gauges)
        {
            if (gauge.gaugeValue.ToString() == particleValue.ToString())
            {
                return gauge;
            }
        }
        return null;
    }
    
    // Update is called once per frame
    public void ReleaseParticle(Vector3 position)
    {
        this.transform.position = new Vector3(position.x, position.y, 150f);
        StartCoroutine(ReleaseParticlesRoutine());
    }

    IEnumerator ReleaseParticlesRoutine()
    {
        yield return null;
        target = correspondingGauge.transform.position;
        bool reachedDestination = false;
        if (!SpecialAbilitiesManager.Instance.isAbilityEnabled)
        {



            particles.Play();

            yield return new WaitForSeconds(2f);

                activeParticles = new ParticleSystem.Particle[particles.main.maxParticles];
                elapsedTime = 0;
            if (!correspondingGauge.IsFull)
            {
                while (!reachedDestination)
                {


                    if (particles == null) particles = GetComponent<ParticleSystem>();
                    float gdistance = Vector3.Distance(target, transform.position);
                    float timeToMove = particles.main.startLifetimeMultiplier - 0.05f;
                    var count = particles.GetParticles(activeParticles);
                    elapsedTime += Time.deltaTime;
                    for (int i = 0; i < count; i++)
                    {
                        var particle = activeParticles[i];

                        float distance = Vector3.Distance(particle.position, target);
                        
                        if (distance > 2f)
                        {

                            float t = elapsedTime / timeToMove;
                            t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                            particle.position = Vector3.Lerp(particle.position, target, t);

                            activeParticles[i] = particle;

                        }
                        else
                        {

                            activeParticles[i].remainingLifetime = 0f;
                            if (!reachedDestination)
                            {
                                reachedDestination = true;
                            }

                        }
                    }



                    particles.SetParticles(activeParticles, count);
                    yield return null;
                }

            }


            yield return new WaitForSeconds(0.1f);
            this.gameObject.SetActive(false);
        }
        else
        {
            particles.Play();
        }
    }


    void OnDisable()
    {
        if (particles != null)
        {
            particles.externalForces.RemoveAllInfluences();
            if (particleValue == ParticleValue.Amber)
            {
                particles.GetComponentInChildren<ParticleSystem>().externalForces.RemoveAllInfluences();
            }
        }
    }
}
