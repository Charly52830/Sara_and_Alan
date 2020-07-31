using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{

	/// <summary>
	/// Clase que controla los sprites mostrados en pantalla. Guarda información
	/// útil de las casillas visitadas por el personaje.
	///
	/// Cada personaje tiene su propio CanvasManager y estas instancias son activadas o
	/// desactivadas cada que ocurre un cambio de turno. Solo debe haber un canvas activo.
	/// </summary>
	public class CanvasManager
	{
		private int[,] stateTiles;
		private GameObject canvas;
		private List<GameObject>[] tiles;
		private int rows;
		private int cols;

		//Rain
		private GameObject[,] rain_tiles;
		private bool isRaining = false;

		// Orientation
		private int orientation;

		/// <summary>
		/// Constructor que crea una nueva instancia de un CanvasManager a partir de
		/// datos básicos.
		/// </summary>
		/// <param name="canvas">El canvas es el objeto más importante de la clase.
		/// Es el objeto de Unity donde se muestran los sprites del juego, cada instancia
		/// de CanvasManager debe tener su propio canvas.
		/// </param>
		/// <param name="rows">Número de filas del mundo.</param>
		/// <param name="cols">Número de columnas del mundo.</param>
		/// <param name="orientation">Orientación del personaje. 
		/// Existen cuatro orientaciones distintas, cada una representada con números
		/// del 0 al 3, siendo el 0 la orientación base, y por cada suma una rotación en
		/// counter-clockwise order del mundo.
		///
		/// La base para que funcione la orientación son los métodos TransformPoint y
		/// DetransformPoint.
		/// </param>
		public CanvasManager(GameObject canvas, int rows, int cols, int orientation)
		{
			this.rows = rows;
			this.cols = cols;
			this.orientation = orientation;
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

		/// <summary>
		/// Agrega una instancia del sprite de lluvia al canvas. Se le asigna un
		/// identificador único a la instancia y se guarda en memoria. Este método
		/// es ejecutado múltiples veces antes de iniciar el juego.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <param name="rain_tile">Instancia del sprite de lluvia.</param>
		public void AddRainTile(int row, int col, GameObject rain_tile)
		{
			int key = col + row * cols;
			rain_tile.name = string.Format("rain_tile_{0}", key);

			Point trans_point = TransformPoint(row, col);
			float posX = (float)trans_point.y;
			float posY = (float)-trans_point.x;

			rain_tile.transform.position = new Vector3((float)posX, (float)posY, -1);
			rain_tile.transform.SetParent(canvas.transform);
			rain_tile.SetActive(false);
			rain_tiles[row, col] = rain_tile;
		}

		/// <summary>
		/// Regresa el estado de una casilla. Los estados son representados por números enteros,
		/// son independientes los estados de cada CanvasManager.
		///
		/// Representación de los estados:
		///	0: unknown tile (casillas borradas o que el personaje no a visto).
		/// 1: visited tile (casillas visitadas pero no visibles por el personaje).
		/// 2: visible tile (casillas visibles por el personaje).
		/// 3: torch tile (casillas visibles que no pueden volver a ser visitadas o desconocidas).
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		public int GetState(int row, int col)
		{
			if(row >= 0 && row < rows && col >= 0 && col < cols)
				return stateTiles[row, col];
			return -1;
		}

		/// <summary>
		/// Asigna un estado a una casilla.
		/// El control sobre las reglas de asignación se lleva a cabo en la clase GridManager.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <param name="state">Estado de la casilla.</param>
		public void SetState(int row, int col, int state)
		{
			stateTiles[row, col] = state;
		}

		/// <summary>
		/// Devuelve la orientación del canvas.
		/// </summary>
		/// <returns>Orientación del canvas.</returns>
		public int GetOrientation()
		{
			return orientation;
		}

		/// <summary>
		/// Agrega un nuevo sprite al canvas. Le asigna un identificador único y
		/// guarda un apuntador a la instancia en memoria. Más tarde este sprite
		/// puede ser destruido.
		/// </summary>
		/// <param name="row">Fila del mundo.</param>
		/// <param name="col">Columna del mundo.</param>
		/// <param name="tile">Instancia del sprite.</param>
		/// <param name="depth">Profundidad del sprite en el canvas.</param>
		public void AddTile(int row, int col, GameObject tile, int depth)
		{
			int key = col + row * cols;
			tile.name = string.Format("{0}_{1}",tile.name, key);

			Point trans_point = TransformPoint(row, col);
			float posX = (float)trans_point.y;
			float posY = (float)-trans_point.x;

			tile.transform.position = new Vector3((float)posX, (float)posY, depth);
			tile.transform.SetParent(canvas.transform);

			tiles[key].Add(tile);
			if(isRaining)
				rain_tiles[row, col].SetActive(true);
		}

		/// <summary>
		/// Agrega un nuevo sprite al canvas.
		/// </summary>
		/// <param name="row">Fila del mundo.</param>
		/// <param name="col">Columna del mundo.</param>
		/// <param name="tile">Instancia del sprite.</param>
		/// <param name="depth">Profundidad del sprite en el canvas.</param>
		public void AddPermanentSprite(int row, int col, GameObject sprite, int depth)
		{
			Point trans_point = TransformPoint(row, col);
			float posX = (float)trans_point.y;
			float posY = (float)-trans_point.x;

			sprite.transform.position = new Vector3(posX, posY, depth);
			sprite.transform.SetParent(canvas.transform);
		}

		/// <summary>
		/// Busca un sprite y devuelve la referencia al objeto de este, o null si no se
		/// encuentra.
		/// </summary>
		/// <param name="row">Fila de la casilla del sprite.</param>
		/// <param name="col">Columna de la casilla del sprite</param>
		/// <param name="tile_name">Nombre del sprite que se está buscando.</param>
		/// <returns>Referencia al sprite o null si no existe.</returns>
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

		/// <summary>
		/// Elimina un sprite del canvas dado su coordenada y su nombre.
		/// Busca la referencia al objeto del sprite para devolverla y que el GridManager
		/// destruya al objeto. Si se encuentra la referencia esta se quita de la lista,
		/// si no, devuelve un valor nulo.
		/// </summary>
		/// <param name="row">Fila de la casilla del sprite.</param>
		/// <param name="col">Columna de la casilla del sprite</param>
		/// <param name="name">Nombre del sprite que se está buscando.</param>
		/// <returns>Referencia al sprite o null si no existe.</returns>
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

		/// <summary>
		/// Activa o desactiva el canvas para ocultar o mostrar todos sus objetos.
		/// </summary>
		/// <param name="bul">True activa el canvas, False lo desactiva.</param>
		public void SetCanvasActive(bool bul)
		{
			canvas.SetActive(bul);
		}

		/// <summary>
		/// Remueve casillas activas aleatorias (causado por el evento de lluvia).
		/// Si los sprites de lluvia no están activos entonces se activan.
		/// </summary>
		/// <returns>Lista de referencias a sprites, que son los sprites que se
		/// eliminarán.</returns>
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

		/// <summary>
		/// Desactiva todos los sprites de lluvia si no está lloviendo.
		/// </summary>
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

		/// <summary>
		/// Método que funciona como middleware para hacer posible la orientación del mapa.
		/// Transforma una coordenada del mundo con orientación 0 a una coordenada del mundo
		/// con la orientación del CanvasManager.
		/// </summary>
		/// <param name="row">Fila de la casilla (con orientación 0).</param>
		/// <param name="col">Columna de la casilla (con orientación 0).</param>
		/// <returns>Objeto Point con las coordenadas del mundo orientadas al CanvasManager.</returns>
		public Point TransformPoint(int row, int col)
		{
			if(orientation == 0)
				return new Point(row, col);
			else if(orientation == 1)
				return new Point(cols - col - 1, row);
			else if(orientation == 2)
				return new Point(rows - 1 - row, cols - 1 - col);
			else
				return new Point(col, rows - row - 1);
		}

		/// <summary>
		/// Realiza la acción contraria a TransformPoint. Transforma una coordenada del CanvasManager a una
		/// coordenada con orientación 0.
		/// </summary>
		/// <param name="row">Fila de la casilla del CanvasManager.</param>
		/// <param name="col">Columna de la casilla del CanvasManager.</param>
		/// <returns>Objeto Point con las coordenadas del mundo orientadas a 0.</returns>
		public Point DetransformPoint(int row, int col)
		{
			if(orientation == 0)
				return new Point(row, col);
			else if(orientation == 1)
				return new Point(col, cols - 1 - row);
			else if(orientation == 2)
				return new Point(rows - 1 - row, cols - 1 - col);
			else
				return new Point(rows - 1 - col, row);
		}

		/// <summary>
		/// Evalua si una coordenada está o no dentro del mundo del CanvasManager considerando su
		/// orientación.
		/// </summary>
		/// <param name="row">Fila de la coordenada con orientación 0.</param>
		/// <param name="col">Columna de la coordenada con orientación 0.</param>
		/// <returns>True si la coordenada está dentro del rango, False si no.</returns>
		public bool EvaluateClick(int row, int col)
		{
			if((orientation & 1) == 1)	// Orientación impar
				return 0 <= row && row <= cols && 0 <= col && col < rows;
			else	// Orientación par
				return 0 <= row && row <= rows && 0 <= col && col < cols;
		}
	}
}