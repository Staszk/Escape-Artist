using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;
using InControl;

public class GameScreenSystem : MonoBehaviour
{
    private Camera mainCamera;
    public TextMeshProUGUI moneyCount;

    public Transform worldSpacePanel;
    public Transform interactionPanel;

    private GameObject[] healthBars;
    private readonly int MAX_HEALTHBARS = 10;
    public GameObject healthbarPrefab;

    private GameObject popupObject;
    public GameObject popupPrefab;
    public GameObject[] stealthPopUps;
    public Sprite[] popupImages;

    public GameObject doorLockPrefab;
    private GameObject[] doorLocks;
    private readonly int doorLockNum = 3;

    public GameObject spamAPrefab;
    private GameObject spamAObject;

    public GameObject priceFlashPrefab;
    private GameObject priceFlash;
    public Sprite[] priceFlashIcons;

    public GameObject enemyStunPrefab;
    private GameObject[] enemyStuns;
    private readonly int enemyStunNum = 10;

    public GameObject moneyFadePrefab;
    private GameObject[] moneyFades;
    private readonly int moneyFadeNum = 5;

    public GameObject damagePopoffPrefab;
    private GameObject[] damagePops;
    private readonly int damagePopsNum = 10;

    private Vector3 healthbarOffset = new Vector3(0.0f, 3.0f, 0.0f);
    private Vector3 interactOffset = new Vector3(0.0f, 2.0f, 0.5f);
    private Vector3 priceFlashOffset = new Vector3(0.0f, 0.5f, 0.5f);
    private Vector3 damageOffset = new Vector3(0.0f, 0.5f, 0.0f);

    private Vector3 moneyScreenPos = new Vector3(-220.5f, 175f, 0f);

    public GameObject locateObject;

    [Header("Minimap")]
    public GameObject smallMinimap;
    public GameObject largeMinimap;
    public GameObject minimapQuad;
    private bool swapped = false;
    private InputDevice controller;

    [Header("Timer")]
    public GameObject timer;
    public TextMeshProUGUI timerText;


    private void OnEnable()
    {
        Enemy.ActionAllocateHealthBar += AllocateHealthBar;
        PlayerController.ActionAllocateHealthBar += AllocateHealthBar;
        Enemy.ActionUpdateHealthBar += UpdateHealthBar;
        PlayerController.ActionUpdateHealthBar += UpdateHealthBar;
        Enemy.ActionReleaseHealthBar += ReleaseHealthBar;
        PlayerController.ActionReleaseHealthBar += ReleaseHealthBar;

        Enemy.ActionAllocateStealth += AllocateStealthPopup;
        Enemy.ActionUpdateStealth += UpdateStealthPopUp;
        Enemy.ActionReleaseStealth += ReleaseStealthPopUp;
        Enemy.ActionAllocateStun += AllocateStun;
        Enemy.ActionUpdateStun += UpdateStun;
        Enemy.ActionReleaseStun += ReleaseStun;

        PlayerController.ActionShowInteraction += ShowPopup;
        PlayerController.ActionDamagePop += MakeDamage;

        PlayerHealth.ActionDamagePop += MakeDamage;

        DoorInteractable.ActionAllocateDoorLock += AllocateDoorLock;
        DoorInteractable.ActionUpdateDoorLock += UpdateDoorLock;
        DoorInteractable.ActionReleaseDoorLock += ReleaseDoorLock;

        ForceOpenDoor.ActionAllocateSpamButton += AllocateSpamButton;
        ForceOpenDoor.ActionUpdateSpamButton += UpdateSpamButton;
        ForceOpenDoor.ActionReleaseSpamButton += ReleaseSpamButton;

        ControllerInputSystem.ActionGameInput += GetController;
    }

    private void OnDisable()
    {
        Enemy.ActionAllocateHealthBar -= AllocateHealthBar;
        PlayerController.ActionAllocateHealthBar -= AllocateHealthBar;
        Enemy.ActionUpdateHealthBar -= UpdateHealthBar;
        PlayerController.ActionUpdateHealthBar -= UpdateHealthBar;
        Enemy.ActionReleaseHealthBar -= ReleaseHealthBar;
        PlayerController.ActionReleaseHealthBar -= ReleaseHealthBar;

        Enemy.ActionAllocateStealth -= AllocateStealthPopup;
        Enemy.ActionUpdateStealth -= UpdateStealthPopUp;
        Enemy.ActionReleaseStealth -= ReleaseStealthPopUp;
        Enemy.ActionAllocateStun -= AllocateStun;
        Enemy.ActionUpdateStun -= UpdateStun;
        Enemy.ActionReleaseStun -= ReleaseStun;

        PlayerController.ActionShowInteraction -= ShowPopup;
        PlayerController.ActionDamagePop -= MakeDamage;

        PlayerHealth.ActionDamagePop -= MakeDamage;

        DoorInteractable.ActionAllocateDoorLock -= AllocateDoorLock;
        DoorInteractable.ActionUpdateDoorLock -= UpdateDoorLock;
        DoorInteractable.ActionReleaseDoorLock -= ReleaseDoorLock;

        ForceOpenDoor.ActionAllocateSpamButton -= AllocateSpamButton;
        ForceOpenDoor.ActionReleaseSpamButton -= ReleaseSpamButton;
        ForceOpenDoor.ActionUpdateSpamButton -= UpdateSpamButton;

        ControllerInputSystem.ActionGameInput -= GetController;
    }
    
    public void Init()
    {
        mainCamera = Camera.main;

        PrepareHealthBars();
        PrepareEnemyStuns();
        PrepareInteractPopup();
        PrepareDoorLocks();
        PrepareDamagePopoff();

        spamAObject = Instantiate(spamAPrefab, worldSpacePanel);
        spamAObject.SetActive(false);

        PreparePriceFlash();
        PrepareMoneyFade();

        UpdateCount(0);
    }

    private void Update()
    {
        if (controller != null && (controller.LeftBumper || controller.LeftTrigger))
        {
            if (!swapped)
            {
                SwitchMinimaps();
                swapped = true;
            }
        }
        else
        {
            if (swapped)
            {
                SwitchMinimaps();
                swapped = false;
            }
        }

        controller = null;
    }

    private void GetController(InputDevice device)
    {
        controller = device;
    }

    public void ShowTimer(float time)
    {
        int minutes = Mathf.RoundToInt(time) / 60;
        int seconds = Mathf.RoundToInt(time) % 60;

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Turn it on if not already on
        if (!timer.activeSelf)
        {
            timer.SetActive(true);
        }
    }

    public void ShowSubtractTimer()
    {
        Animator timeAnim = timer.transform.GetChild(1).GetComponent<Animator>();
        timeAnim.SetBool("Subtract", true);
        StartCoroutine(MakeFalse(timeAnim, "Subtract"));
    }

    private IEnumerator MakeFalse(Animator timeAnim, string theBool)
    {
        yield return new WaitForSeconds(0.2f);
        timeAnim.SetBool(theBool, false);
    }

    public void Show60Left()
    {
        Animator timeAnim = timer.transform.GetChild(0).GetComponent<Animator>();
        timeAnim.SetBool("60Left", true);
        StartCoroutine(MakeFalse(timeAnim, "60Left"));
    }

    private void PrepareDamagePopoff()
    {
        damagePops = new GameObject[damagePopsNum];

        for (int i = 0; i < damagePopsNum; i++)
        {
            damagePops[i] = Instantiate(damagePopoffPrefab, worldSpacePanel);
            damagePops[i].SetActive(false);
        }
    }

    private void PrepareMoneyFade()
    {
        moneyFades = new GameObject[moneyFadeNum];

        for (int i = 0; i < moneyFadeNum; i++)
        {
            moneyFades[i] = Instantiate(moneyFadePrefab, worldSpacePanel);
            moneyFades[i].GetComponent<MoneyFade>().SetUp();
            moneyFades[i].SetActive(false);
        }
    }

    private void PrepareInteractPopup()
    {
        popupObject = Instantiate(popupPrefab, interactionPanel);
        popupObject.SetActive(false);

        stealthPopUps = new GameObject[3];

        for (int i = 0; i < stealthPopUps.Length; i++)
        {
            stealthPopUps[i] = Instantiate(popupPrefab, worldSpacePanel);
            stealthPopUps[i].transform.GetChild(0).GetComponent<Image>().sprite = popupImages[1];
            stealthPopUps[i].SetActive(false);
        }
    }

    private void PrepareEnemyStuns()
    {
        enemyStuns = new GameObject[enemyStunNum];

        for (int i = 0; i < enemyStunNum; i++)
        {
            enemyStuns[i] = Instantiate(enemyStunPrefab, worldSpacePanel);
            enemyStuns[i].SetActive(false);
        }
    }

    private void PrepareDoorLocks()
    {
        doorLocks = new GameObject[doorLockNum];

        for (int i = 0; i < doorLockNum; i++)
        {
            doorLocks[i] = Instantiate(doorLockPrefab, worldSpacePanel);
            doorLocks[i].SetActive(false);
        }
    }

    private void PreparePriceFlash()
    {
        priceFlash = Instantiate(priceFlashPrefab, worldSpacePanel);
        priceFlash.SetActive(false);
    }

    private void PrepareHealthBars()
    {
        healthBars = new GameObject[MAX_HEALTHBARS];

        for (int i = 0; i < MAX_HEALTHBARS; i++)
        {
            healthBars[i] = Instantiate(healthbarPrefab, worldSpacePanel);
            healthBars[i].GetComponent<HealthBar>().Initialize();
            healthBars[i].SetActive(false);
        }
    }

    #region Minimap

    public void SwitchMinimaps()
    {
        smallMinimap.SetActive(!smallMinimap.activeSelf);

        largeMinimap.SetActive(!largeMinimap.activeSelf);

        minimapQuad.SetActive(smallMinimap.activeSelf);
    }

    #endregion

    #region Healthbars

    private void AllocateHealthBar(PlayerController p, int healthValue, int maxHealth)
    {
        for (int i = 0; i < MAX_HEALTHBARS; i++)
        {
            if (!healthBars[i].activeSelf)
            {
                // Set up health bar
                healthBars[i].GetComponent<HealthBar>().SetMax(maxHealth);
                healthBars[i].GetComponent<HealthBar>().SetValue(healthValue);

                // Position health bar correctly
                Vector3 screenPos = mainCamera.WorldToScreenPoint(p.transform.position + healthbarOffset);
                healthBars[i].GetComponent<RectTransform>().position = screenPos;

                // Set health Bar Active
                healthBars[i].SetActive(true);

                // Give ID to enemy
                p.healthBarReferenceID = i;

                return;
            }
        }
    }

    private void AllocateHealthBar(Enemy e, int healthValue, int maxHealth)
    {
        for (int i = 0; i < MAX_HEALTHBARS; i++)
        {
            if (!healthBars[i].activeSelf)
            {
                // Set up health bar
                healthBars[i].GetComponent<HealthBar>().SetMax(maxHealth);
                healthBars[i].GetComponent<HealthBar>().SetValue(healthValue);

                // Position health bar correctly
                Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + healthbarOffset);
                healthBars[i].GetComponent<RectTransform>().position = screenPos;

                // Set health Bar Active
                healthBars[i].SetActive(true);

                // Give ID to enemy
                e.healthBarReferenceID = i;

                return;
            }
        }
    }

    private void UpdateHealthBar(Enemy e, int healthValue, int ID)
    {
        // Adjust value of health bar
        healthBars[ID].GetComponent<HealthBar>().SetValue(healthValue);

        // Adjust position of Health bar
        Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + healthbarOffset);
        healthBars[ID].GetComponent<RectTransform>().position = screenPos;
    }

    private void UpdateHealthBar(PlayerController p, int healthValue, int ID)
    {
        // Adjust value of health bar
        healthBars[ID].GetComponent<HealthBar>().SetValue(healthValue);

        // Adjust position of Health bar
        Vector3 screenPos = mainCamera.WorldToScreenPoint(p.transform.position + healthbarOffset);
        healthBars[ID].GetComponent<RectTransform>().position = screenPos;
    }

    private void ReleaseHealthBar(Enemy e, int ID)
    {
        healthBars[ID].SetActive(false);

        // Return escape ID to enemy
        e.healthBarReferenceID = -1;
    }

    private void ReleaseHealthBar(PlayerController p, int ID)
    {
        healthBars[ID].SetActive(false);

        // Return escape ID to enemy
        p.healthBarReferenceID = -1;
    }

    #endregion

    #region Popup

    private void AllocateStealthPopup(Enemy e)
    {
        for (int i = 0; i < 3; i++)
        {
            if (!stealthPopUps[i].activeSelf)
            {
                // Position icon correctly
                Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + healthbarOffset);
                stealthPopUps[i].GetComponent<RectTransform>().position = screenPos;

                // Set health Bar Active
                stealthPopUps[i].SetActive(true);

                // Give ID to enemy
                e.stealthReferenceID = i;

                return;
            }
        }
    }

    private void UpdateStealthPopUp(Enemy e, int ID)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + healthbarOffset);
        stealthPopUps[ID].GetComponent<RectTransform>().position = screenPos;
    }

    private void ReleaseStealthPopUp(Enemy e, int ID)
    {
        stealthPopUps[ID].SetActive(false);

        e.stealthReferenceID = -1;
    }

    private void ShowPopup(Vector3 positionInWorld, int popup, bool show)
    {
        if (!show)
        {
            StopShowingPopup();
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(positionInWorld + interactOffset);
        popupObject.GetComponent<RectTransform>().position = screenPos;
        popupObject.SetActive(true);
    }

    private void StopShowingPopup()
    {
        popupObject.SetActive(false);
    }

    #endregion

    #region DoorLock

    private void AllocateDoorLock(DoorInteractable d)
    {
        for (int i = 0; i < doorLockNum; i++)
        {
            if (!doorLocks[i].activeSelf)
            {
                // Set up health bar
                doorLocks[i].transform.GetChild(0).GetComponent<Image>().fillAmount = 1;

                // Position health bar correctly
                Vector3 screenPos = mainCamera.WorldToScreenPoint(d.transform.position + interactOffset);
                doorLocks[i].GetComponent<RectTransform>().position = screenPos;

                // Set health Bar Active
                doorLocks[i].SetActive(true);

                // Give ID to enemy
                d.referenceID = i;

                return;
            }
        }
    }

    private void UpdateDoorLock(DoorInteractable d, int ID, float amount)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(d.transform.position + interactOffset);
        doorLocks[ID].GetComponent<RectTransform>().position = screenPos;
        doorLocks[ID].transform.GetChild(0).GetComponent<Image>().fillAmount = amount;
    }

    private void ReleaseDoorLock(DoorInteractable d, int ID)
    {
        doorLocks[ID].SetActive(false);

        d.referenceID = -1;
    }

    #endregion

    #region Spam A

    private void AllocateSpamButton(GameObject g)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(g.transform.position + healthbarOffset);
        spamAObject.GetComponent<RectTransform>().position = screenPos;

        spamAObject.SetActive(true);
    }

    private void UpdateSpamButton(GameObject g)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(g.transform.position + healthbarOffset);
        spamAObject.GetComponent<RectTransform>().position = screenPos;
    }

    private void ReleaseSpamButton()
    {
        spamAObject.SetActive(false);
    }

    #endregion

    #region Price Flash

    public void ShowPriceFlash(Vector3 position, int iconToShow)
    {
        StopAllCoroutines();

        Vector3 screenPos = mainCamera.WorldToScreenPoint(position + priceFlashOffset);
        priceFlash.GetComponent<RectTransform>().position = screenPos;

        priceFlash.transform.GetChild(0).GetComponent<Image>().sprite = priceFlashIcons[iconToShow];

        StartCoroutine(FadeFlash(position));
    }

    private IEnumerator FadeFlash(Vector3 position)
    {
        Color col = Color.white;

        Image flash = priceFlash.transform.GetChild(0).GetComponent<Image>();
        RectTransform rt = priceFlash.GetComponent<RectTransform>();

        priceFlash.SetActive(true);

        while (col.a > 0)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(position + priceFlashOffset);
            rt.position = screenPos;

            col.a -= Time.deltaTime / 2f;

            flash.color = col;

            yield return null;
        }

        priceFlash.SetActive(false);
    }

    #endregion

    #region Enemy Stun

    private void AllocateStun(Enemy e)
    {
        for (int i = 0; i < enemyStunNum; i++)
        {
            if (!enemyStuns[i].activeSelf)
            {
                // Set up health bar
                enemyStuns[i].transform.GetChild(0).GetComponent<Image>().fillAmount = 1;

                // Position health bar correctly
                Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + interactOffset);
                enemyStuns[i].GetComponent<RectTransform>().position = screenPos;

                // Set health Bar Active
                enemyStuns[i].SetActive(true);

                // Give ID to enemy
                e.stunReferenceID = i;

                return;
            }
        }
    }

    private void UpdateStun(Enemy e, int ID, float amount)
    {
        // Set up health bar
        enemyStuns[ID].transform.GetChild(0).GetComponent<Image>().fillAmount = amount;

        // Position health bar correctly
        Vector3 screenPos = mainCamera.WorldToScreenPoint(e.transform.position + interactOffset);
        enemyStuns[ID].GetComponent<RectTransform>().position = screenPos;

    }

    private void ReleaseStun(Enemy e, int ID)
    {
        enemyStuns[ID].SetActive(false);

        e.stunReferenceID = -1;
    }

    #endregion

    #region Money Fade

    public void ShowMoneyFade(int money, bool bonus)
    {
        for (int i = 0; i < moneyFadeNum; i++)
        {
            if (!moneyFades[i].activeSelf)
            {
                moneyFades[i].GetComponent<MoneyFade>().SetText(money.ToString());

                Color32 col = bonus ? Color.yellow : Color.green;

                moneyFades[i].GetComponent<MoneyFade>().SetColor(col);

                moneyFades[i].GetComponent<RectTransform>().localPosition = moneyScreenPos;

                moneyFades[i].SetActive(true);

                return;
            }
        }
    }

    #endregion

    #region Damage Popoff

    private void MakeDamage(GameObject obj, int damage, bool player, bool isCrit)
    {
        int hitType;

        if (!player)
        {

            hitType = isCrit ? 1 : 0;
        }
        else
        {
            hitType = 2;
        }

        for (int i = 0; i < damagePopsNum; i++)
        {
            if (!damagePops[i].activeSelf)
            {
                Vector3 pos = mainCamera.WorldToScreenPoint(obj.transform.position + damageOffset);

                damagePops[i].GetComponent<RectTransform>().position = pos;

                damagePops[i].SetActive(true);
                damagePops[i].GetComponent<DamagePopoff>().PopOff(hitType, damage.ToString());

                return;
            }
        }
    }

    #endregion

    public void UpdateCount(int count)
    {
        moneyCount.text = "$" + count.ToString();
    }
}
