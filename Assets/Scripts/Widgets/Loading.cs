using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [Header("Text objects")]
    public GameObject loadingText;

    private int periodCount = 1;
    private int periodLimit = 5;
    private string loadingBaseText = "Loading, please wait";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeLoadingText());
    }

    /// <summary>
    /// Instead of using the Update() method, create a basic timer that allows us to change
    /// the stettings of the project every second to greatly reduced computing power.
    /// </summary>
    IEnumerator ChangeLoadingText()
    {
        while (true)
        {
            UpdateLoadingText();

            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Change the amount of periods after the base text to give the impression it is loading.
    /// </summary>
    private void UpdateLoadingText()
    {
        string periods = new string('.', periodCount);
        periodCount = (periodCount % periodLimit) + 1;

        loadingText.GetComponent<Text>().text = loadingBaseText + periods;
    }
}
