using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MyNamespace {

	/// <summary>
	/// Controla los eventos del menú principal.
	/// </summary>
	public class MainMenu : MonoBehaviour
	{

		/// <summary>Verifica la existencia de las variables de control del nivel.</summary>
		void Start()
		{
			if(!PlayerPrefs.HasKey("level"))
				PlayerPrefs.SetInt("level", 0);
		}

		/// <summary>Controla el menú de historia en función del nivel actual del personaje.</summary>
		public void ManageHistoryButton()
		{
			GameObject continueButton = GameObject.Find("ContinueButton");
			Debug.Log(PlayerPrefs.GetInt("level"));
			if(PlayerPrefs.GetInt("level") == 0)
				continueButton.SetActive(false);
		}

		/// <summary>Controla el menú de juego nuevo.</summary>
		public void ManageNewGameButton()
		{
			if(PlayerPrefs.GetInt("level") == 0) 
			{
				GameObject newGameMenu = GameObject.Find("NewGameConfirm");
				if(newGameMenu != null)
					newGameMenu.SetActive(false);
				PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
				ManagePreGameWindow();
			}
			else
			{
				GameObject preGameWindow = GameObject.Find("PreGameWindow");
				if(preGameWindow != null)
					preGameWindow.SetActive(false);
			}
		}

		/// <summary>Carga la pantalla de carga.</summary>
		public void ManageContinueButton()
		{
			PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
			ManagePreGameWindow();
		}

		/// <summary>Inicia un nuevo juego.</summary>
	    public void PlayGame()
	    {
	    	SceneManager.LoadScene("MainGame");
	    }

	    /// <summary>Reinicia el modo historia del juego para empezar desde el primer nivel.</summary>
	    public void StartNewGame()
	    {
	    	PlayerPrefs.SetInt("level", 0);
	    	PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
	    	ManagePreGameWindow();
	    }

	    /// <summary>Prepara un juego del modo encounter.</summary>
	    public void LoadEncounterGame()
	    {
	    	PlayerPrefs.SetString("game_type", "encounter");
		    ManagePreGameWindow();
	    }

	    /// <summary>Prepara un juego del modo escape.</summary>
	    public void LoadEscapeGame()
	    {
	    	PlayerPrefs.SetString("game_type", "escape");
	    	ManagePreGameWindow();
	    }

	    /// <summary>Termina la ejecución de todos los procesos del juego.</summary>
	    public void QuitGame()
	    {
	    	Debug.Log("Quitting...");
	    	Application.Quit();
	    }

	    /// <summary>Controla el texto de la pantalla de carga.</summary>
	    public void ManagePreGameWindow()
	    {
	    	TextMeshProUGUI gameModeText = GameObject.Find("GameModeTitle").GetComponent<TextMeshProUGUI>();
	    	TextMeshProUGUI descriptionText = GameObject.Find("DescriptionText").GetComponent<TextMeshProUGUI>();

	    	string game_mode = PlayerPrefs.GetString("game_type");
			if(game_mode.Equals("encounter"))
			{
				gameModeText.SetText("Encounter");
				descriptionText.SetText("Get Sara and Alan together before you run out of moves. This game could be played by one or two players. Player(s) are placed in a random generated world, to win Sara and Alan must be moved to the same place.");
			}
			else if(game_mode.Equals("escape"))
			{
				gameModeText.SetText("Escape");
				descriptionText.SetText("This game mode must be played by 2 players: players are placed in a random generated world, the player who controls Sara tries to find the exit of the map to win, the player who controls Alan tries to catch Sara before she escapes.");
			}
			else
			{
				gameModeText.SetText(string.Format("Day {0}", PlayerPrefs.GetInt("level") + 1));
				// Load text from day
				TextAsset level_text = Resources.Load(string.Format("levels_text/level_{0}_text", PlayerPrefs.GetInt("level"))) as TextAsset;
				descriptionText.SetText(level_text.text);
			}
	    }
	}
}