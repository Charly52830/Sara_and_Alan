using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
	/*
	 *	State number representation
	 *
	 *	0: unknown tile
	 *	1: visited tile
	 *	2: visible tile
	 *	3: torch tile (permanent visible tile)
	 */
	public class CanvasManager
	{
		private int[,] stateTiles;
		private GameObject canvas;
		//private Dictionary<string, GameObject> tiles;
		private List<GameObject>[] tiles;
		private int rows;
		private int cols;

		//Rain
		private GameObject[,] rain_tiles;
		private bool isRaining = false;

		public CanvasManager(GameObject canvas, int rows, int cols)
		{
			this.rows = rows;
			this.cols = cols;
			tiles = new List<GameObject>[cols * rows];
			for(int x = 0; x < cols * rows; x++)
				tiles[x] = new List<GameObject>();
			stateTiles = new int[rows, cols];
			this.canvas = canvas;
			for(int x = 0; x < rows; x++)
			{
				for(int y = 0; y < cols; y++)
				{
					stateTiles[x, y] = 0;
				}
			}

			rain_tiles = new GameObject[rows, cols];
		}

		public void AddRainTile(int row, int col, GameObject rain_tile)
		{
			float tileSize = 1.0F;
			int key = col + row * cols;
			rain_tile.name = string.Format("rain_tile_{0}", key);

			float posX = col * tileSize;
			float posY = row * -tileSize;
			rain_tile.transform.position = new Vector3((float)posX, (float)posY, -1);
			rain_tile.transform.SetParent(canvas.transform);
			rain_tile.SetActive(false);
			rain_tiles[row, col] = rain_tile;
		}

		public int GetState(int row, int col)
		{
			if(row >= 0 && row < rows && col >= 0 && col < cols)
				return stateTiles[row, col];
			return -1;
		}

		public void SetState(int row, int col, int state)
		{
			stateTiles[row, col] = state;
		}

		public void AddTile(int row, int col, GameObject tile, int depth)
		{
			float tileSize = 1.0F;
			int key = col + row * cols;
			tile.name = string.Format("{0}_{1}",tile.name, key);

			float posX = col * tileSize;
			float posY = row * -tileSize;
			tile.transform.position = new Vector3((float)posX, (float)posY, depth);

			tile.transform.SetParent(canvas.transform);

			tiles[key].Add(tile);
			if(isRaining)
				rain_tiles[row, col].SetActive(true);
		}

		/**
		 *	Permanent sprites are not required to be stored its reference
		 */
		public void AddPermanentSprite(int row, int col, GameObject sprite, int depth)
		{
			float posX = (float)col;
			float posY = (float)-row;
			sprite.transform.position = new Vector3(posX, posY, depth);
			sprite.transform.SetParent(canvas.transform);
		}

		public GameObject GetTile(int row, int col, string tile_name)
		{
			int key = col + row * cols;
			tile_name = string.Format("{0}_{1}", tile_name, key);
			foreach(GameObject tile in tiles[key])
			{
				if(tile.name.Equals(tile_name))
					return tile;
			}
			return null;
		}

		public GameObject RemoveSprite(int row, int col, string name)
		{
			int key = col + row * cols;
			name = string.Format("{0}_{1}", name, key);
			GameObject ans = null;
			int contador = 0;
			foreach(GameObject tile in tiles[key])
			{
				if(tile.name.Equals(name))
				{
					ans = tile;
					contador += 1;
				}
			}
			if(contador > 1)
				Debug.Log(string.Format("Alerta de objetos repetidos: tile_name: {0}, col: {1}, row: {2}", name, col, row));

			if(ans != null)
				tiles[key].Remove(ans);
			return ans;
		}

		public void SetCanvasActive(bool bul)
		{
			canvas.SetActive(bul);
		}

		public List<GameObject> FloodMap()
		{	
			// Activate all visited tiles rain
			if(!isRaining)
			{
				for(int x = 0; x < rows; x++)
				{
					for(int y = 0; y < cols; y++)
					{
						if(GetState(x, y) > 0)
							rain_tiles[x, y].SetActive(true);
					}
				}
				isRaining = true;
			}

			// Delete random tiles
			List<GameObject> removed_tiles = new List<GameObject>();
			List<int> keys = new List<int>();
			for(int x = 0; x < rows; x++)
			{
				for(int y = 0; y < cols; y++)
				{
					int key = y + x * cols;
					if(GetState(x, y) == 1)
						keys.Add(key);
				}
			}

			// Random shuffle
			for(int x = 0; x < keys.Count; x++)
			{
				int k = Random.Range(0, x + 1);
				int temp = keys[k];
				keys[k] = keys[x];
				keys[x] = temp;	
			}

			int tile_numbers = Random.Range(3, 7);
			Debug.Log(string.Format("Se eliminaran {0} casillas", tile_numbers));
			for(int i = 0; i < Mathf.Min(tile_numbers, keys.Count); i++)
			{
				int key = keys[i];
				int x = key / cols;
				int y = key % cols;
				foreach(GameObject obj in tiles[key])
					removed_tiles.Add(obj);
				tiles[key] = new List<GameObject>();
				SetState(x, y, 0); 	//Unknown tile
				rain_tiles[x, y].SetActive(false);
			}
			return removed_tiles;
		}

		public void DryMap()
		{
			if(isRaining)
			{
				for(int x = 0; x < rows; x++)
				{
					for(int y = 0; y < cols; y++)
					{
						if(rain_tiles[x, y].activeSelf)
							rain_tiles[x, y].SetActive(false);
					}
				}
				isRaining = false;
			}
		}

	}
}