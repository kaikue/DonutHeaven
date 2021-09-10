using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionPlayer : MonoBehaviour
{
    private const float length = 2;
	private const float waitTime = 0.5f;
    private float time = 0;
    private Vector3 startPos;
    private Vector3 goalPos;
    private AudioSource bgm;
    private float startVol;

    public GameObject loadingScreenPrefab;

    private void Start()
    {
        startPos = transform.position;
        bgm = GameObject.Find("BGM").GetComponent<AudioSource>();
        startVol = bgm.volume;
    }

    public void SetPortal(GameObject portal)
    {
        goalPos = portal.transform.position;
    }

    private void Update()
    {
        time += Time.deltaTime;
		if (time < length)
        {
            float t = time / length;
            transform.position = Vector3.Lerp(startPos, goalPos, t);
            transform.localScale = new Vector3(1 - t, 1 - t, 1 - t);
            bgm.volume = Mathf.Lerp(startVol, 0, t);
        }
        else if (time >= length + waitTime)
        {
            Instantiate(loadingScreenPrefab);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
