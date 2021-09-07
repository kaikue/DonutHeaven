using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlTutorial : MonoBehaviour
{
    public Sprite keyboardSprite;
    public Sprite controllerSprite;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (Input.GetJoystickNames().Length > 0)
        {
            sr.sprite = controllerSprite;
        }
        else
		{
            sr.sprite = keyboardSprite;
		}
    }

	private void Update()
	{
        KeyCode[] keyboardCodes = { KeyCode.Space, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.DownArrow, 
            KeyCode.A, KeyCode.D, KeyCode.S, KeyCode.LeftShift, KeyCode.RightShift };
        foreach (KeyCode keyboardCode in keyboardCodes)
		{
            if (Input.GetKeyDown(keyboardCode))
			{
                sr.sprite = keyboardSprite;
			}
        }

        KeyCode[] controllerCodes = { KeyCode.JoystickButton0, KeyCode.JoystickButton1, KeyCode.JoystickButton2, 
            KeyCode.JoystickButton3, KeyCode.JoystickButton4, KeyCode.JoystickButton5, KeyCode.JoystickButton6 };
        foreach (KeyCode controllerCode in controllerCodes)
        {
            if (Input.GetKeyDown(controllerCode))
            {
                sr.sprite = controllerSprite;
            }
        }

        if (Input.GetAxis("ControllerHorizontal") != 0 || Input.GetAxis("ControllerVertical") != 0 || 
            Input.GetAxis("LTrigger") != 0 || Input.GetAxis("RTrigger") != 0)
		{
            sr.sprite = controllerSprite;
        }
    }
}
