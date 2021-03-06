using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text scoreText;
    public Player player;
    public Manager manager;

    private int score;

    void Start()
    {
        score = 0;
    }

    private void Update()
    {
        if (player.gameInSession)
        {
            setScoreText($"Score: {score}\nGeneration: {manager.curGeneration}\nPopulation: {manager.curPopulation}");
        }
        else
        {
            setScoreText($"Score: {score}\nGeneration: {manager.curGeneration}\nPopulation: {manager.curPopulation}\nPress Space To Play");
        }
    }

    public void setScoreText(string text)
    {
        scoreText.text = text;
    }

    public void increaseScore()
    {
        score++;
    }

    public void resetScore()
    {
        score = 0;
    }

    public int getScore()
    {
        return score;
    }
}
