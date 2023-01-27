using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// plays UI effects when player reaches scoring goal
public class ScoreStar : MonoBehaviour
{
    public Image star;

    public ParticlePlayer starFX;

    public float delay = 0.5f;

    public AudioClip starSound;

    public bool activated = false;


    void Start()
    {
        SetActive(false);
    }

    // turn the icon on or off
    void SetActive(bool state)
    {
        if (star != null)
        {
            star.gameObject.SetActive(state);
        }
    }

    // activate the star
    public void Activate()
    {
        // only activate once
        if (activated)
        {
            return;
        }

        // invoke ActivateRoutine coroutine
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        // we are activated
        activated = true;

        // play the ParticlePlayer
        if (starFX != null)
        {
            starFX.Play();
        }

        // play the starSound
        if (SoundManager.Instance != null && starSound != null)
        {
            SoundManager.Instance.PlayClipAtPoint(starSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }

        yield return new WaitForSeconds(delay);

        // turn on the icon
        SetActive(true);
    }

}
