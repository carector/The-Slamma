using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (!PlayerPrefs.HasKey("SLAMMA_HAS_PLAYED_TUTORIAL") || PlayerPrefs.GetInt("SLAMMA_HAS_PLAYED_TUTORIAL") == 0)
            StartCoroutine(Load(2));
        else
            StartCoroutine(Load(3));
    }

    IEnumerator Load(int index)
    {
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        while (!asyncLoadLevel.isDone)
            yield return null;
    }
}
