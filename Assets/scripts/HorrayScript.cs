using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrayScript : MonoBehaviour
{
    ParticleSystem PartSys;
    int partcount = 0;
    // Use this for initialization
    void Start ()
    {
        PartSys = GetComponent<ParticleSystem>();

        //particleAni.
        //Color[] aniColors = particleAni.colorAnimation;

        //for (int i = 0; i < aniColors.Length; i++)
        //{
        //    aniColors[i].r = Random.Range(0.0f, 1.0f);
        //    aniColors[i].g = Random.Range(0.0f, 1.0f);
        //    aniColors[i].b = Random.Range(0.0f, 1.0f);
        //    aniColors[i].a = Random.Range(0.0f, 1.0f);
        //}
        //particleAni.colorAnimation = aniColors;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (PartSys.isPlaying)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[PartSys.particleCount];
            PartSys.GetParticles(particles);

            for (int p = partcount; p < particles.Length; p++)
            {
                switch(p % 3)
                {
                    case 0:
                        particles[p].startColor = new Color32(255, 55, 55, 255);
                        break;
                    case 1:
                        particles[p].startColor = new Color32(55, 255, 102, 255);
                        break;
                    case 2:
                        particles[p].startColor = new Color32(62, 68, 255, 255);
                        break;
                }
            }

            partcount = particles.Length;
            PartSys.SetParticles(particles, particles.Length);
        }
        else
        {
            partcount = 0;
        }
    }
}
