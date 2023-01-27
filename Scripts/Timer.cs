using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timeLeftText;

    public Image clockImage;

    int m_maxTime = 60;

    public bool paused = false;

    public int flashTimeLimit = 10;

    public AudioClip flashBeep;

    public float flashInterval = 1f;

    public Color flashColor = Color.red;

    IEnumerator m_flashRoutine;

    public void InitTimer(int maxTime = 60)
    {
        m_maxTime = maxTime;

        // make sure the image is using Radial360 fillMethod with origin at top
        if (clockImage != null)
        {
            clockImage.type = Image.Type.Filled;
            clockImage.fillMethod = Image.FillMethod.Radial360;
            clockImage.fillOrigin = (int) Image.Origin360.Top;
        }

        if (timeLeftText != null)
        {
            timeLeftText.text = maxTime.ToString();
        }
    }

    // update the clock image and time left text
    public void UpdateTimer(int currentTime)
    {
        if (paused)
        {
            return;
        }

        if (clockImage != null)
        {
            // update the clock fill
            clockImage.fillAmount = (float) currentTime / (float) m_maxTime;

            // flash and play extra beep if within the danger zone
            if (currentTime <= flashTimeLimit)
            {
                // start the flash effect
                m_flashRoutine = FlashRoutine(clockImage, flashColor, flashInterval);
                StartCoroutine(m_flashRoutine);

                if (SoundManager.Instance != null && flashBeep != null)
                {
                    SoundManager.Instance.PlayClipAtPoint(flashBeep, Vector3.zero, SoundManager.Instance.fxVolume, false);
                }
            }
        }

        // update countdown text
        if (timeLeftText != null)
        {
            timeLeftText.text = currentTime.ToString();
        }
    }
  
    // change an image to a flash color for a fraction of a interval and change the color back
    IEnumerator FlashRoutine(Image image, Color targetColor, float interval)
    {
        if (image != null)
        {
            Color originalColor = image.color;
            image.CrossFadeColor(targetColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);

            image.CrossFadeColor(originalColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
        }
    }

    // fade off any ScreenFaders components
    public void FadeOff()
    {
        // stop any flash effect currently running
        if (m_flashRoutine != null)
        {
            StopCoroutine(m_flashRoutine);
        }

        ScreenFader[] screenFaders = GetComponentsInChildren<ScreenFader>();
        foreach (ScreenFader fader in screenFaders)
        {
            fader.FadeOff();
        }
    }
}
