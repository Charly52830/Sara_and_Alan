using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
	/// <summary>
	/// Administra la posesión de objetos de un personaje.
	/// </summary>
	public class CharacterManager
	{
		private int torch_counter;
		private int hiking_boots_counter;
		private bool map;
		private bool fast_boots;
		private bool telescope;
		private bool isAlan;
		private bool mirror;

		public CharacterManager(bool isAlan)
		{
			torch_counter = 0;
			hiking_boots_counter = 0;
			map = false;
			fast_boots = false;
			telescope = false;
			mirror = false;
			this.isAlan = isAlan;
		}

		public void SumTorch(int operand)
		{
			torch_counter += operand;
			if(torch_counter < 0)
				torch_counter = 0;
		}

		public int GetTorchCount()
		{
			return torch_counter;
		}

		public void SumHikingBoots(int operand)
		{
			hiking_boots_counter += operand;
			if(hiking_boots_counter < 0)
				hiking_boots_counter = 0;
		}

		public int GetHikingBootsCount()
		{
			return hiking_boots_counter;
		}

		public void ActivateMap()
		{
			map = true;
		}

		public bool HasMap()
		{
			return map;
		}

		public void ActivateFastBoots()
		{
			fast_boots = true;
		}

		public bool HasFastBoots()
		{
			return fast_boots;
		}

		public void ActivateTelescope()
		{
			telescope = true;
		}

		public bool HasTelescope()
		{
			return telescope;
		}

		public void ActivateMirror()
		{
			mirror = true;
		}

		public bool HasMirror()
		{
			return mirror;
		}

		/// <summary>
		/// Evalúa si avanzar a una nueva casilla es un movimiento válido considerando los
		/// objetos del personaje y el tipo de la casilla.
		/// </summary>
		/// <returns>True si el personaje puede avanzar a una casilla del tipo dado, False si no.</returns>
		public bool IsValidMove(int tile_type)
		{
			if(tile_type == 2 || tile_type == 3)	// Pasture or path tile
				return true;
			if(tile_type == 0 && hiking_boots_counter > 0)	// Mountain tile and has hiking_boots
			{
				SumHikingBoots(-1);
				return true;
			}
			if(tile_type == 1 && isAlan)	// Tree tile and is Alan
				return true;
			if((tile_type == 4  || tile_type == 5) && !isAlan)	// Water or exit tile and is Sara
				return true;
			return false;
		}
		
	}
}