using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    private int score;

    private void Start()
    {
        CheckFirstRun();
        NewGame();
    }

    private void CheckFirstRun()
    {
        
        if (!PlayerPrefs.HasKey("FirstRun"))
        {
           
            PlayerPrefs.DeleteAll(); 
            PlayerPrefs.SetInt("FirstRun", 1);
            PlayerPrefs.Save();
        }
    }

    public void NewGame()
    {
        SetScore(0);
        gameOver.alpha = 0f;
        gameOver.interactable = false;
        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
        LoadHighScore();
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;
        StartCoroutine(Fade(gameOver, 1f, 1f));

        SaveHighScore();
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }

    private void LoadHighScore()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            highscoreText.text = "" + highScore;
        }
        else
        {
            highscoreText.text = "0";
        }
    }

    private void SaveHighScore()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            if (score > highScore)
            {
                PlayerPrefs.SetInt("HighScore", score);
                highscoreText.text = "" + score;
            }
        }
        else
        {
            PlayerPrefs.SetInt("HighScore", score);
            highscoreText.text = "" + score;
        }
    }
}
