using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace MyNamespace
{
	
	/// <summary>
	/// Representación del mundo del juego. Controla los tipos de casillas,
	/// las posiciones absolutas de ambos personajes del mundo, los nombres
	/// de los sprites y los objetos dentro del mundo.
	/// </summary>
	public class World
	{
		private int rows;
		private int cols;
		private int[,] tiles_type;
		private int[,] tiles_name;
		private ObjectManager objectManager = null;
		private Point current_alan_point = null;
		private Point current_sara_point = null;
		private Point current_exit_point = null;

		/// <summary>
		/// Crea un nuevo mundo a partir de un mundo comprimido.
		/// </summary>
		/// <param name="compressedWorld">Instancia del mundo comprimido.</param>
		public World(CompressedWorld compressedWorld)
		{
			rows = compressedWorld.rows;
			cols = compressedWorld.cols;
			tiles_type = compressedWorld.tiles;
			tiles_name = new int[rows, cols];
			SetTileNames();

			current_exit_point = compressedWorld.exit_point;
			current_alan_point = compressedWorld.alan_point;
			current_sara_point = compressedWorld.sara_point;

			objectManager = compressedWorld.objectManager;
		}

		/// <summary>
		/// Crea un nuevo mundo aleatorio de dimensiones dadas.
		/// Al crear el nuevo mundo se generan también objetos
		/// aleatorios, distribuidos uniformemente en las casillas
		/// del mundo.
		/// Los objetos se generan aleatoriamente con la siguiente probabilidad:
		/// Map: 5%
		/// Mirror: 5%
		/// Telescope: 10%
		/// Fast boots: 10%
		/// Hiking boots: 21%
		/// Torchs: 21%
		/// Rain: 28%
		/// 
		/// El número de objetos generados se calcula como:
		/// objetos = rows * cols * 0.02
		/// </summary>
		/// <param name="rows">Número de filas del mundo.</param>
		/// <param name="cols">Número de columnas del mundo.</param>
		public World(int rows, int cols)
		{
			this.rows = rows + 2;
			this.cols = cols + 2;
			tiles_type = new int[this.rows, this.cols];
			tiles_name = new int[this.rows, this.cols];

			//Fill borders with mountains
			for(int x = 0; x < this.rows; x++)
			{
				tiles_type[x, 0] = 0;	//Mountain
				tiles_type[x, this.cols - 1] = 0;	//Mountain
			}
			for(int x = 0; x < this.cols; x++) 
			{
				tiles_type[0, x] = 0;	//Mountain
				tiles_type[this.rows - 1, x] = 0;	//Mountain
			}

			// Fill inside with random tiles
			int[] tiles = {0, 1, 2, 4, 4, 1, 3, 4, 0, 1, 3, 4, 0, 2, 4};

			for(int x = 1; x < this.rows - 1; x++)
			{
				for(int y = 1; y < this.cols - 1; y++)
				{
					tiles_type[x, y] = tiles[Random.Range(0, tiles.Length)];
				}
			}
			SetTileNames();
		}

		/// <summary>
		/// Precomputa los nombres de las casillas del mundo.
		/// Cada casilla tiene un nombre y un tipo, el nombre representa el número de sprite
		/// de cada tipo de casilla que se utilizará en cada casilla. Los nombres de las casillas
		/// de agua y de caminos se computan al vuelo y pueden variar según la orientación.
		/// </summary>
		private void SetTileNames()
		{
			for(int x = 0; x < this.rows; x++)
			{
				for(int y = 0; y < this.cols; y++)
				{
					if(tiles_type[x, y] == 0)	//Mountain
						tiles_name[x, y] = 0;
					else if(tiles_type[x, y] == 1)	//Trees
					{
						tiles_name[x, y] = 0;
						// TO DO: agregar más sprites de árboles
					}
					else if(tiles_type[x, y] == 2)	//Pasture
					{
						tiles_name[x, y] = 0;
						// TO DO: agregar más sprites de pasto
					}
				}
			}
		}

		/// <summary>
		/// Regresa el nombre del sprite a utilizar en la casilla dada. Los nombres
		/// empatan con los que se encuentran en el diccionario de recursos. Los nombres
		/// pueden variar dependiendo de la orientación en las casillas de agua y caminos.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla</param>
		/// <param name="orientation">Orientación de la vista.
		/// Existen cuatro orientaciones distintas, cada una representada con números
		/// del 0 al 3, siendo el 0 la orientación base, y por cada suma una rotación en
		/// counter-clockwise order del mundo.</param>
		/// <returns>String con el nombre del sprite a utilizar en la casilla.</returns>
		public string GetTileName(int row, int col, int orientation)
		{
			if(row < 0 || row >= this.rows || col < 0 || col >= this.cols)
				return "NoName";
			if(tiles_type[row, col] == 0)	// Mountain
				return "hill_0";
			else if(tiles_type[row, col] == 1)	// Tree
				return string.Format("tree_{0}", tiles_name[row, col]);
			else if(tiles_type[row, col] == 2)	// Pasture
				return string.Format("pasture_{0}", tiles_name[row, col]);
			else if(tiles_type[row, col] == 5)	// Exit
				return string.Format("exit_{0}", tiles_name[row, col]);
			else {
				int[] X = {-1, 0, 1, 0};
				int[] Y = {0, -1, 0, 1};
				int[,] bits = {
					{1, 2, 4, 8},
					{2, 4, 8, 1},
					{4, 8, 1, 2},
					{8, 1, 2, 4}
				};
				int bitmask = 0;
				for(int i = 0; i < 4; i++)
				{
					int x1 = row + X[i];
					int y1 = col + Y[i];
					if(x1 >= 0 && x1 < this.rows && y1 >= 0 && y1 < this.cols && tiles_type[row, col] == tiles_type[x1, y1])
						bitmask |= bits[orientation, i];
				}
				if(tiles_type[row, col] == 3)	// Path
					return string.Format("path_{0}", bitmask);
				else	// Water
					return string.Format("water_{0}", bitmask);
			}
		}

		/// <summary>
		/// Regresa el tipo de casilla dadas sus coordenadas en el mundo.
		/// Cada tipo se representa con un número entero:
		/// 0: Montaña
		/// 1: Árbol
		/// 2: Pasto
		/// 3: Camino
		/// 4: Agua
		/// 5: Salida
		/// </summary>
		/// <param name="row">Fila del mundo</param>
		/// <param name="col">Columna del mundo</param>
		/// <returns>Entero con el tipo de casilla.</returns>
		public int GetTileType(int row, int col)
		{
			if(row < 0 || row > this.rows || col < 0 || col > this.cols)
				return -1;
			return tiles_type[row, col];
		}

		/// <summary>
		/// Devuelve como entero el número de filas del mundo.
		/// </summary>
		/// <returns>Número de filas del mundo.</returns>
		public int GetRows()
		{
			return rows;
		}

		/// <summary>
		/// Devuelve como entero el número de columnas del mundo.
		/// </summary>
		/// <returns>Número de columnas del mundo.</returns>
		public int GetCols()
		{
			return cols;
		}

		/// <summary>
		/// Devuelve la instancia del ObjectManager del mundo. Si no existe una instancia
		/// de ObjectManager asociada entonces crea una nueva y asigna objetos aleatorios
		/// distribuidos uniformemente por el mundo.
		///
		/// Los objetos se generan aleatoriamente con la siguiente probabilidad:
		/// Map: 5%
		/// Mirror: 5%
		/// Telescope: 10%
		/// Fast boots: 10%
		/// Hiking boots: 21%
		/// Torchs: 21%
		/// Rain: 28%
		/// 
		/// El número de objetos generados se calcula como:
		/// objetos = rows * cols * 0.02
		///
		/// Los objetos en una casilla son representados como un conjunto utilizando
		/// bitmasks. A continuación se detalla el bit que representa a cada objeto:
		/// 0: torch (bitmask 1)
		/// 1: hiking boots (bitmask 2)
		/// 2: fast boots (bitmask 4)
		/// 3: map (bitmask 8)
		/// 4: telescope (bitmask 16)
		/// 5: rain (bitmask 32)
		/// 6: mirror (bitmask 64)
		/// </summary>
		public ObjectManager GetObjectManager()
		{
			if(objectManager == null)
			{
				objectManager = new ObjectManager(rows, cols);
				int num_objects = (int)((float)cols * (float)rows * 0.02F);
				for(int i = 0; i < num_objects; i++)
				{
					float prob = Random.Range(0.0F, 1.0F);
					int key = Random.Range(0, (rows - 2) * (cols - 2));
					int row = key / (cols - 2) + 1;
					int col = key % (cols - 2) + 1;
					if(prob < 0.05)	// Map 5%
						objectManager.AddObject(row, col, 3);
					else if(prob < 0.1)	// Mirror 5%
						objectManager.AddObject(row, col, 6);
					else if(prob < 0.2)	// Telescope 10%
						objectManager.AddObject(row, col, 4);
					else if(prob < 0.3)	// Fast boots 10%
						objectManager.AddObject(row, col, 2);
					else if(prob < 0.51)	// Hiking boots 21%
						objectManager.AddObject(row, col, 1);
					else if(prob < 0.72)	// Torchs 21%
						objectManager.AddObject(row, col, 0);
					else	// Rain 28%
						objectManager.AddObject(row, col, 5);
					Debug.Log(string.Format("Se agregó un objeto en la posición {0}, {1} con probabilidad {2}", row, col, prob));
				}
			}
			return objectManager;
		}

		/// <summary>
		/// Devuelve las coordenadas en las que se encuentra uno de los personajes.
		/// </summary>
		/// <param name="alan_point">True representa que se está preguntando por el 
		/// personaje de Alan, False representa que se está preguntando por el personaje
		/// de Sara.
		/// </param>
		/// <returns>Instancia de Point con la fila y la columna de la casilla en la que 
		/// se encuentra el personaje.
		/// </returns>
		public Point GetPoint(bool alan_point)
		{
			if(alan_point)
			{
				if(current_alan_point == null)
				{
					int key;
					List<int> list = new List<int>();
					for(int x = 1; x < rows - 1; x++)
					{
						for(int y = 1; y < cols - 1; y++)
						{
							key = y + x * cols;
							if(tiles_type[x, y] != 0 && tiles_type[x, y] != 4)	// Is not mountain nor water
								list.Add(key);
						}
					}
					int index = Random.Range(0, list.Count);
					key = list[index];
					int row = key / cols;
					int col = key % cols;
					current_alan_point = new Point(row, col);
				}
				return current_alan_point;
			}
			else
			{
				if(current_sara_point == null)
				{
					int key;
					List<int> list = new List<int>();
					for(int x = 1; x < rows - 1; x++)
					{
						for(int y = 1; y < cols - 1; y++)
						{
							key = y + x * cols;
							if(tiles_type[x, y] != 0 && tiles_type[x, y] != 1)	// Is not mountain nor tree
								list.Add(key);
						}
					}
					int index = Random.Range(0, list.Count);
					key = list[index];
					int row = key / cols;
					int col = key % cols;
					current_sara_point = new Point(row, col);
				}
				return current_sara_point;
			}
		}

		/// <summary>
		/// Asigna una nueva posición del personaje en el mundo.
		/// </summary>
		/// <param name="alan_point">True representa una asignación al personaje de Alan,
		/// False representa una nueva asignación al personaje de Sara.
		/// </param>
		/// <param name="point">Coordenadas de la casilla a la que se mueve el personaje.
		/// </param>
		public void SetPoint(bool alan_point, Point point)
		{
			if(alan_point)
				current_alan_point = point;
			else
				current_sara_point = point;
		}

		/// <summary>
		/// Devuelve el punto en el que se encuentra la salida del mundo. Si no
		/// existe una salida asignada entonces se crea una nueva salida, Para
		/// asignarla se corre un algoritmo de BFS que encuentra todas las posibles
		/// casillas a las que puede llegar Sara sin necesidad de objetos, para
		/// al final seleccionar aleatoriamente una casilla del subconjunto de casillas
		/// con montañas.
		/// </summary>
		/// <returns>Instancia de un punto con la fila y la columna de la casilla
		/// en donde se encuentra la salida.</returns>
		public Point GetExitPoint()
		{
			if(current_exit_point == null)
			{
				int[] X = {-1, -1, 0, 1, 1, 1, 0, -1};
    			int[] Y = {0, -1, -1, -1, 0, 1, 1, 1};
				int x, y;
				bool[,] visited = new bool[rows, cols];
				List<int> keys = new List<int>();
				Queue<int> queue = new Queue<int>();

				for(x = 0; x < rows; x++)
				{
					for(y = 0; y < cols; y++)
					{
						visited[x, y] = false;
					}
				}
				Point sara_point = GetPoint(false);
				int key = sara_point.y + sara_point.x * cols;
				queue.Enqueue((int)key);

				//BFS to find all reachables mountains by Sara
				while(queue.Count > 0)
				{
					key = queue.Dequeue();
					x = key / cols;
					y = key % cols;

					if(visited[x, y])
						continue;

					visited[x, y] = true;

					if(!(sara_point.x == x && sara_point.y == y) && tiles_type[x, y] == 0)	// Mountain
					{
						keys.Add(key);
					}
					else
					{
						for(int i = 0; i < 8; i++)
						{
							int x1 = x + X[i];
							int y1 = y + Y[i];
							if(x1 >=0 && x1 < rows && y1 >= 0 && y1 < cols && tiles_type[x1, y1] != 1 && !visited[x1, y1])	// Not a tree
							{
								key = y1 + x1 * cols;
								queue.Enqueue(key);
							}
						}
						
					}
				}

				int index = Random.Range(0, keys.Count - 1);
				key = keys[index];
				x = key / cols;
				y = key % cols;

				tiles_type[x, y] = 5;	// Exit tile
				tiles_name[x, y] = 0;

				Debug.Log(string.Format("La salida se encuentra en {0}, {1}", x, y));
				current_exit_point = new Point(x, y);
			}
			return current_exit_point;
		}

		/// <summary>
		/// Deshabilita la casilla de salida.
		/// </summary>
		public void UnableExitPoint()
		{
			current_exit_point = new Point(-1000, 1000);
		}

		/// <summary>
		/// Imprime en salida estándard una representación minimalista del mundo.
		/// Este método sirve para efectos de Debugging y creación de mundos.
		/// Las casillas se representan con una letra del alfabeto latino:
		/// M: Montaña
		/// T: Árbol
		/// p: Pasto
		/// P: Camino
		/// W: Agua
		/// E: salida
		/// </summary>
		public void PrintWorld()
		{
			string S = "MTpPWE";
			var s = new StringBuilder();
			for(int x = 1; x < rows - 1; x++)
			{
				for(int y = 1; y < cols - 1; y++)
				{
					int index = tiles_type[x, y];
					s.Append(S[index]);
				}
				s.Append('\n');
			}
			Debug.Log(s);
		}
	}

	/// <summary>
	/// Representación minimalista de un mundo. Funciona como middleware para transformar un mundo
	/// en un formato entendible por el humano a un mundo del juego. Contiene los atributos necesarios
	/// para crear un mundo a partir de ellos. Los niveles del modo historia se cargan
	/// de esta forma cuando la clase LevelLoader genera una de estas instancias.
	/// Además de atributos propios del mundo contiene atributos útiles para la
	/// ejecución del juego.
	/// </summary>
	public class CompressedWorld
	{
		public int rows;
		public int cols;
		public int[,] tiles;
		public int turns;
		public int turn_operand;
		public float rain_probability;
		public Point sara_point;
		public Point alan_point;
		public Point exit_point;
		public ObjectManager objectManager;
	}

}