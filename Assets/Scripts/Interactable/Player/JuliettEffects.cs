using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuliettEffects : MonoBehaviour
{
    public GameObject[] AttackEffects;
    public GameObject UppercutEffect;

    public void PlayAttackEffects(int attackNumber, bool headingRight)
    {
        GameObject effect = Instantiate(AttackEffects[attackNumber], transform);
        if (!headingRight)
        {
            Vector3 scale = effect.transform.localScale;
            scale.x = -scale.x;
            effect.transform.localScale = scale;
        }
        effect.SetActive(true);
    }

    public void PlayUppercutEffect(bool headingRight)
    {
        GameObject effect = Instantiate(UppercutEffect, transform);
        if (!headingRight)
        {
            Vector3 scale = effect.transform.localScale;
            scale.x = -scale.x;
            effect.transform.localScale = scale;
        }
        effect.SetActive(true);
    }
}
