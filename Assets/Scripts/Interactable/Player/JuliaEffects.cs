using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuliaEffects : MonoBehaviour
{
    public ParticleSystem[] RollingEffects;

    public void SetRollingEffects(bool active)
    {
        for (int i = 0; i < RollingEffects.Length; i++)
        {
            if (active)
                RollingEffects[i].Play();
            else
                RollingEffects[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
