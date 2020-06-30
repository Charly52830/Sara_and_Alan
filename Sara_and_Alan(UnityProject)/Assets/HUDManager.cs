using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MyNamespace
{
	public class HUDManager
	{
		private GameObject torchIcon;
		private GameObject rainIcon;
		private GameObject hikingBootsIcon;
		private GameObject fastBootsIcon;
		private GameObject mapIcon;
		private GameObject telescopeIcon;
		private GameObject mirrorIcon;
		private TextMeshProUGUI titleText;
		private TextMeshProUGUI turnCounterText;
		private TextMeshProUGUI movementCounterText;
		private TextMeshProUGUI gameModeText;
		private GameObject torchCountText;
		private GameObject rainCountText;
		private GameObject hikingBootsCountText;
		private GameObject skipButton;


		public HUDManager()
		{
			torchIcon = GameObject.Find("TorchIcon");
			rainIcon = GameObject.Find("RainIcon");
			hikingBootsIcon = GameObject.Find("HikingBootsIcon");
			fastBootsIcon = GameObject.Find("FastBootsIcon");
			mapIcon = GameObject.Find("MapIcon");
			telescopeIcon = GameObject.Find("TelescopeIcon");
			mirrorIcon = GameObject.Find("MirrorIcon");
			titleText = GameObject.Find("TurnTitle").GetComponent<TextMeshProUGUI>();
			turnCounterText = GameObject.Find("TurnCounterText").GetComponent<TextMeshProUGUI>();
			movementCounterText = GameObject.Find("MovementCounterText").GetComponent<TextMeshProUGUI>();
			gameModeText = GameObject.Find("GameModeText").GetComponent<TextMeshProUGUI>();

			string game_mode = PlayerPrefs.GetString("game_type");
			if(game_mode.Equals("encounter"))
				gameModeText.SetText("Encounter");
			else if(game_mode.Equals("escape"))
				gameModeText.SetText("Escape");
			else
				gameModeText.SetText(string.Format("Day {0}", PlayerPrefs.GetInt("level") + 1));
				
			torchCountText = GameObject.Find("TorchCountText");
			rainCountText = GameObject.Find("RainCountText");
			hikingBootsCountText = GameObject.Find("HikingBootsCounter");
			skipButton = GameObject.Find("SkipButton");

		}

		public void DisableSkipButton()
		{
			skipButton.SetActive(false);
		}

		private void TestCounter(int counter, GameObject icon, GameObject counter_text)
		{
			if(counter > 0)
			{
				icon.SetActive(true);
				counter_text.SetActive(true);
				TextMeshProUGUI text = counter_text.GetComponent<TextMeshProUGUI>();
				text.SetText(string.Format("{0}", counter));
			}
			else
			{
				icon.SetActive(false);
				counter_text.SetActive(false);
			}
		}

		public void UpdateHUD(CharacterManager character, bool isAlan, int movements, int turns, int rain_counter)
		{
			// Object icons
			TestCounter(rain_counter, rainIcon, rainCountText);
			int torch_count = character.GetTorchCount();
			TestCounter(torch_count, torchIcon, torchCountText);
			int hiking_boots_count = character.GetHikingBootsCount();
			TestCounter(hiking_boots_count, hikingBootsIcon, hikingBootsCountText);
			if(character.HasFastBoots())
				fastBootsIcon.SetActive(true);
			else
				fastBootsIcon.SetActive(false);
			if(character.HasMap())
				mapIcon.SetActive(true);
			else
				mapIcon.SetActive(false);
			if(character.HasTelescope())
				telescopeIcon.SetActive(true);
			else
				telescopeIcon.SetActive(false);
			if(character.HasMirror())
				mirrorIcon.SetActive(true);
			else
				mirrorIcon.SetActive(false);

			// Texts
			if(isAlan)
				titleText.SetText("TURN OF ALAN");
			else
				titleText.SetText("TURN OF SARA");
			turnCounterText.SetText(string.Format("TURN: {0}", turns));
			movementCounterText.SetText(string.Format("MOVEMENTS: {0}", movements));
		}
	}
}