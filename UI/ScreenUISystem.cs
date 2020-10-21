using UnityEngine;
using System.Collections;

public class ScreenUISystem : MonoBehaviour
{
    private GameObject[] UIPanels;
    private GameScreenSystem gameScreen;
    private EndMenuSystem endScreen;
    private VisualMenu visualMenu;

    private int currentActivePanel;

    public Material transitionMaterial;

    private void Start()
    {
        UIPanels = new GameObject[4];

        UIPanels[0] = transform.GetChild(0).gameObject;
        UIPanels[1] = transform.GetChild(1).gameObject;

        gameScreen = UIPanels[1].GetComponent<GameScreenSystem>();
        gameScreen.Init();

        UIPanels[2] = transform.GetChild(2).gameObject;

        endScreen = UIPanels[2].GetComponent<EndMenuSystem>();

        UIPanels[3] = transform.GetChild(3).gameObject;

        visualMenu = UIPanels[3].GetComponent<VisualMenu>();

        currentActivePanel = 2;
        SetActivePanel(0);

        transitionMaterial.SetFloat("_Cutoff", 0f);
    }

    public void SetActivePanel(int index)
    {
        UIPanels[currentActivePanel].SetActive(false);

        currentActivePanel = index;

        UIPanels[currentActivePanel].SetActive(true);
    }

    public void UpdateGameScreen(int value, int newValue, bool bonus = false)
    {
        gameScreen.UpdateCount(value);
        gameScreen.ShowMoneyFade(newValue, bonus);
    }

    public void EndTransition(bool result, Stats stats)
    {
        UIPanels[2].GetComponent<EndMenuSystem>().PrepareScreen(result, stats);
        StartCoroutine(CutoffShader());
    }

    private IEnumerator CutoffShader()
    {
        foreach (GameObject panel in UIPanels)
        {
            panel.SetActive(false);
        }

        float cutoff = 0.0f;

        while (cutoff < 0.98f)
        {
            cutoff += 0.02f;

            transitionMaterial.SetFloat("_Cutoff", cutoff);

            yield return null;
        }

        SetActivePanel(2);
        UIPanels[2].GetComponent<EndMenuSystem>().StartImages(); 
    }

    public GameScreenSystem GetGameScreen()
    {
        return gameScreen;
    }
}
