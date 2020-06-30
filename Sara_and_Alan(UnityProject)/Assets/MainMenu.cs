using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MyNamespace {

	public class MainMenu : MonoBehaviour
	{

		void Start()
		{
			if(!PlayerPrefs.HasKey("level"))
				PlayerPrefs.SetInt("level", 0);
		}

		public void ManageHistoryButton()
		{
			GameObject continueButton = GameObject.Find("ContinueButton");
			Debug.Log(PlayerPrefs.GetInt("level"));
			if(PlayerPrefs.GetInt("level") == 0)
				continueButton.SetActive(false);
		}

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

		public void ManageContinueButton()
		{
			PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
			ManagePreGameWindow();
		}

	    public void PlayGame()
	    {
	    	SceneManager.LoadScene("MainGame");
	    }

	    public void StartNewGame()
	    {
	    	PlayerPrefs.SetInt("level", 0);
	    	PlayerPrefs.SetString("game_type", string.Format("level_{0}", PlayerPrefs.GetInt("level")));
	    	ManagePreGameWindow();
	    }

	    public void LoadEncounterGame()
	    {
	    	PlayerPrefs.SetString("game_type", "encounter");
		    ManagePreGameWindow();
	    }

	    public void LoadEscapeGame()
	    {
	    	PlayerPrefs.SetString("game_type", "escape");
	    	ManagePreGameWindow();
	    }

	    public void QuitGame()
	    {
	    	Debug.Log("Quitting...");
	    	Application.Quit();
	    }

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