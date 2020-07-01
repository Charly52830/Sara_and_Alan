using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ReturnManagement : MonoBehaviour
{
    // Start is called before the first frame update

	private int max_level = 6;

    void Start()
    {
        GameObject completedText = GameObject.Find("CompletedText");
        GameObject saraWonsText = GameObject.Find("SaraWonsText");
        GameObject alanWonsText = GameObject.Find("AlanWonsText");
        GameObject loseText = GameObject.Find("LoseText");

        completedText.SetActive(false);
        saraWonsText.SetActive(false);
        alanWonsText.SetActive(false);
        loseText.SetActive(false);

        string status = PlayerPrefs.GetString("game_status");
        string game_type = PlayerPrefs.GetString("game_type");
        if(status.Equals("completed"))
        {
            completedText.SetActive(true);
            if(!game_type.Equals("encounter")) // Is history mode
            {
                Debug.Log("Es modo historia");
                //Debug.Log(string.Format("EL nivel completado es: {0}", PlayerPrefs.GetInt("level")));
                int level = PlayerPrefs.GetInt("level");
                level = (level + 1) % max_level;

                PlayerPrefs.SetInt("level", level);
                //Debug.Log(string.Format("El nivel actual es: {0}", PlayerPrefs.GetInt("level")));
                TextMeshProUGUI buttonText = GameObject.Find("TryAgainText").GetComponent<TextMeshProUGUI>();
                buttonText.SetText("Continue");
            }
            else
            {
                Debug.Log("No es modo historia");
            }
        }
        else if(status.Equals("sara_wons"))
        {
            saraWonsText.SetActive(true);
        }
        else if(status.Equals("alan_wons"))
        {
            alanWonsText.SetActive(true);
        }
        else
        {
            loseText.SetActive(true);
        }
        
    }

    public void ReturnMainMenu()
    {
    	SceneManager.LoadScene("MainMenu");
		SceneManager.UnloadSceneAsync("FinalScene");
		Debug.Log("Escena final terminada, volviendo al menú principal");
    }

    public void ManageTryAgainButton()
    {
        //Debug.Log(string.Format("En ManageTryAgainButton: {0}", PlayerPrefs.GetInt("level")));
        string game_type = PlayerPrefs.GetString("game_type");
        GameObject preGameWindow = (GameObject)GameObject.Find("PreGameWindow");
        if(game_type.Equals("encounter") || game_type.Equals("escape")) // Arcade mode
        {
            preGameWindow.SetActive(false);
            PlayGame();
        }
        else
        {
            ManagePreGameWindow();
        }
    }

    public void PlayGame()
    {
        string game_type = PlayerPrefs.GetString("game_type");
        if(!(game_type.Equals("encounter") || game_type.Equals("escape")))  // Is history mode
        {
            // Update the level
            Debug.Log(string.Format("En PlayGame el nivel es: {0}", PlayerPrefs.GetInt("level")));
            PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
            Debug.Log("Actualizando el modo historia");
            Debug.Log(string.Format("Modo de juego: {0}", PlayerPrefs.GetString("game_type")));
        }

        SceneManager.LoadScene("MainGame");
        SceneManager.UnloadSceneAsync("FinalScene");
        Debug.Log("Escena final terminada, empezando una nueva partida");
    }

    private void ManagePreGameWindow()
    {
        TextMeshProUGUI gameModeText = GameObject.Find("GameModeTitle").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = GameObject.Find("DescriptionText").GetComponent<TextMeshProUGUI>();

        //Must be history mode
        gameModeText.SetText(string.Format("Day {0}", PlayerPrefs.GetInt("level") + 1));
        // Load text from day
        TextAsset level_text = Resources.Load(string.Format("levels_text/level_{0}_text", PlayerPrefs.GetInt("level"))) as TextAsset;
        descriptionText.SetText(level_text.text);
    }

}
