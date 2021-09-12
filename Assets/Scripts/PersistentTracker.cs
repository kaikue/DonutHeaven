using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentTracker : MonoBehaviour
{
    public bool destroying = false;
    public int sprinkles = 0;
    public int possibleSprinkles = 0;
    public float time = 0;

    private void Awake()
    {
        PersistentTracker[] persistents = FindObjectsOfType<PersistentTracker>();

        if (persistents.Length > 1)
        {
            destroying = true;
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
        int levelSprinkles = FindObjectsOfType<CollectibleSprinkle>().Length;
        possibleSprinkles += levelSprinkles;
        //print(levelSprinkles + " " + possibleSprinkles);
    }

	private void Update()
	{
		if (!SceneManager.GetActiveScene().name.Contains("Menu"))
		{
            time += Time.deltaTime;
		}
	}
}
