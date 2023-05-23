using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Dice : MonoBehaviour
{
    private Animator diceAnimation;
    private SpriteRenderer currentFrame;
    private float timeElapsed = 0;
    private float timeToRollDiceSec;
    private bool roll = false;

    public int Result;
    public Player Player;

    public void Roll()
    {
        timeToRollDiceSec = Random.Range(1f, 2f);
        diceAnimation.enabled = true;
        roll = true;
    }

    public bool RollingFinished()
    {
        return !roll;
    }

    private void Start()
    {
        currentFrame = GetComponent<SpriteRenderer>();
        diceAnimation = GetComponent<Animator>();
        currentFrame.color = Player.Color;
        diceAnimation.enabled = false;
        timeElapsed = 0;
    }

    private void Update()
    {
        if (roll)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= timeToRollDiceSec)
            {
                diceAnimation.enabled = false;
                Result = int.Parse(currentFrame.sprite.name.Substring("Dice".Length));
                timeElapsed = 0;
                roll = false;
            }
        }
    }
}
