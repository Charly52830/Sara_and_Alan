using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controla los eventos de la pantalla de game over.
/// </summary>
public class ReturnManagement : MonoBehaviour
{

	private int max_level = 6;

    /// <summary>
    /// Oculta y muestra la información correspondiente al modo de juego
    /// y al estado final de la partida pasada.
    /// </summary>
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

    /// <summary>
    /// Carga el menú principal. Este método se ejecuta al dar click
    /// sobre el botón ReturnButton.
    /// </summary>
    public void ReturnMainMenu()
    {
    	SceneManager.LoadScene("MainMenu");
		SceneManager.UnloadSceneAsync("FinalScene");
		Debug.Log("Escena final terminada, volviendo al menú principal");
    }

    /// <summary>
    /// Escribe las variables adecuadas para continuar con la próxima partida, dependiendo
    /// del modo de juego y del estado del juego anterior.
    /// </summary>
    public void ManageTryAgainButton()
    {
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

    /// <summary>
    /// Inicia un nuevo juego cargando la escena MainGame. Si el modo de juego es historia
    /// actualiza el nivel a jugar.
    /// </summary>
    public void PlayGame()
    {
        string game_type = PlayerPrefs.GetString("game_type");
        if(!(game_type.Equals("encounter") || game_type.Equals("escape")))  // Is history mode
        {
            // Update the level
            PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
        }

        SceneManager.LoadScene("MainGame");
        SceneManager.UnloadSceneAsync("FinalScene");
        Debug.Log("Escena final terminada, empezando una nueva partida");
    }

    /// <summary>
    /// Muestra en pantalla el texto asociado al nivel del juego.
    /// </summary>
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
