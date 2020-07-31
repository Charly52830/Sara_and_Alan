using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MyNamespace
{
	/// <summary>
	/// Clase que controla la información mostrada en la pantalla del juego,
	/// como el número de turnos o los objetos del personaje.
	/// </summary>
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

		/// <summary>
		/// Crea un nuevo objeto HUDManager. Obtiene información importante como el tipo
		/// de juego y la muestra en pantalla.
		/// </summary>
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

		/// <summary>
		/// Deshabilita el botón de omitir turno.
		/// </summary>
		public void DisableSkipButton()
		{
			skipButton.SetActive(false);
		}

		/// <summary>
		/// Actualiza la información de un contador de la pantalla. Existen contadores para
		/// el icono de lluvia, antorchas y botas de montaña.
		/// </summary>
		/// <param name="counter">Contador del objeto.</param>
		/// <param name="icon">Referencia al icono del objeto.</param>
		/// <param name="counter_text">Referencia al texto del contador.</param>
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

		/// <summary>
		/// Este método es llamado cuando ocurre un cambio de turno de personaje. Actualiza la información
		/// en la pantalla dada la información actual del juego.
		/// </summary>
		/// <param name="character">Referencia al objeto CharacterManager del personaje del turno actual.</param>
		/// <param name="isAlan">True si el personaje actual es Alan, False si no.</param>
		/// <param name="turns">Contador del número de turnos.</param>
		/// <param name="movements">Contador del número de movimientos.</param>
		/// <param name="rain_counter">Contador del número de turnos con lluvia.</param>
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