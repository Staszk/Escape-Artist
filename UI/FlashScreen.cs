using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FlashScreen : MonoBehaviour
{
    public Image goodImage;
    public Image badImage;

    private void OnEnable()
    {
        AudienceInteractableManager.ActionFlashInteractable += FlashImage;
    }

    private void OnDisable()
    {
        AudienceInteractableManager.ActionFlashInteractable -= FlashImage;
    }

    private void FlashImage(bool goodTeam, Sprite iconSprite)
    {
        if (goodTeam)
        {
            goodImage.sprite = iconSprite;
            StartCoroutine(FlashGood());
        }
        else
        {
            badImage.sprite = iconSprite;
            StartCoroutine(FlashBad());
        }
    }

    private IEnumerator FlashGood()
    {
        goodImage.gameObject.SetActive(true);

        Color col = Color.white;
        col.a = 0.75f;

        while (col.a > 0)
        {
            col.a -= Time.deltaTime;

            goodImage.color = col;

            yield return null;
        }

        goodImage.gameObject.SetActive(false);
    }

    private IEnumerator FlashBad()
    {
        badImage.gameObject.SetActive(true);

        Color col = Color.white;
        col.a = 0.75f;

        while (col.a > 0)
        {
            col.a -= Time.deltaTime;

            badImage.color = col;

            yield return null;
        }

        badImage.gameObject.SetActive(false);
    }
}
