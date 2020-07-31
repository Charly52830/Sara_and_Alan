using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace MyNamespace {

	/// <summary>
	/// Clase principal que invoca al resto de los elementos del juego.
	/// Es la encargada de detectar los eventos del juego, manejar los turnos,
	/// comunicación entre el mundo y el canvas, entre otras cosas.
	/// </summary>
	public class GridManager : MonoBehaviour
	{
		// Scene parameters
		public string game_type;

		private int rows = 20;
		private int cols = 20;
		private World world;
		private HUDManager hudManager;
		private ObjectManager objectManager;
		private CharacterManager alan;
		private CharacterManager sara;
		private GameObject canvas_sara;
		private CanvasManager alanCanvasManager;
		private CanvasManager saraCanvasManager;

		// Turns
		private bool alan_turn = false;
		private int movements;
		private int turns = 1;
		// Turn operand must be -1 if there are a fixed number of moves to complete the level
		private int turn_operand = 1;
		private int rain_counter = 0;
		private CanvasManager currentCanvasManager;
		private CharacterManager currentCharacter;
		private Point last_sara_position;
		private Point last_alan_position;

		//Click
		private Vector2 lastClickDown;
	    public GameObject particle;

	    // Display sprites
		private Dictionary<string, GameObject> resources;

		// Rain
		private float rain_probability = 0.15F;	// Number between 0 and 1

		/// <summary>
		/// Prepara todo el ambiente sobre el que correrá el juego. Crea un nuevo
		/// mundo aleatorio o carga un mundo del modo historia. Inicializa los componentes
		/// básicos del juego, como el HUDManager y el CanvasManager de ambos personajes.
		/// </summary>
		void Start()
		{
			hudManager = new HUDManager();
			resources = LoadResources();
			game_type =  PlayerPrefs.GetString("game_type", "escape");
			
			if(game_type.Equals("escape"))
			{
				rows = Random.Range(15, 35);
				cols = Random.Range(15, 35);
				//rows = 43;	// DEBUG
				//cols = 35;	// DEBUG
				//rain_probability = 0.0F;	// DEBUG
				world = new World(rows, cols);
				world.GetExitPoint();
			}
			else if(game_type.Equals("encounter"))
			{
				rows = Random.Range(15, 35);
				cols = Random.Range(15, 35);
				world = new World(rows, cols);
				world.UnableExitPoint();

				Debug.Log(string.Format("rows: {0}, cols: {1}", rows, cols));
				turns = (int)((float)cols * (float)rows * Random.Range(0.03F, 0.055F));
				turn_operand = -1;
			}
			else
			{
				// Load level
				CompressedWorld compressedWorld = LevelLoader.LoadLevel(game_type);
				
				rows = compressedWorld.rows;
				cols = compressedWorld.cols;
				turns = compressedWorld.turns;
				turn_operand = compressedWorld.turn_operand;

				rain_probability = compressedWorld.rain_probability;
				Debug.Log(string.Format("Rain probability: {0}", rain_probability));
				world = new World(compressedWorld);
				rows -= 2;
				cols -= 2;
			}

			cols += 2;
			rows += 2;

			//world.PrintWorld();	// DEBUG

			Point alan_point = world.GetPoint(true);
			Point sara_point = world.GetPoint(false);
			last_alan_position = new Point(alan_point.x, alan_point.y);
			last_sara_position = new Point(sara_point.x, sara_point.y);

			alanCanvasManager = new CanvasManager(GameObject.Find("CanvasAlan"), rows, cols, 0);
			saraCanvasManager = new CanvasManager(GameObject.Find("CanvasSara"), rows, cols, 0);

			sara = new CharacterManager(false);
			alan = new CharacterManager(true);

			MarkTiles(world.GetPoint(false), saraCanvasManager, sara);	// First tiles of Sara
			alan_turn = true;
			MarkTiles(world.GetPoint(true), alanCanvasManager, alan);	// First tiles of Alan

			PrepareRain(alanCanvasManager);
			PrepareRain(saraCanvasManager);

			saraCanvasManager.SetCanvasActive(false);
			alanCanvasManager.SetCanvasActive(true);

			movements = 2;
			currentCanvasManager = alanCanvasManager;
			currentCharacter = alan;
			CenterCamera();

			hudManager.UpdateHUD(currentCharacter, alan_turn, movements, turns, rain_counter);
		}

		/// <summary>
		/// Prepara los sprites de la lluvia en un CanvasManager para que puedan ser mostrados
		/// cuando se requiera.
		/// </summary>
		/// <param name="canvasManager">Canvas Manager para el que se está preparandolos sprites.</param>
		void PrepareRain(CanvasManager canvasManager)
		{
			for(int x = 0; x < rows; x++)
			{
				for(int y = 0; y < cols; y++)
				{
					GameObject rain_tile = (GameObject)Instantiate(resources["rain_tile"], transform);
					canvasManager.AddRainTile(x, y, rain_tile);
				}
			}
		}

		/// <summary>
		/// Método que verifica si existe un estado en el que el juego termina.
		/// El juego puede terminar por tres razones (varía según el modo de juego):
		/// 1: Los personajes se encuentran en el mismo lugar.
		/// 2: El personaje Sara encontró la salida del mapa (propio del modo Escape).
		/// 3: Se acabó el número de turnos.
		/// </summary>
		private void CheckFinishGame()
		{
			Point alan_point = world.GetPoint(true);
			Point sara_point = world.GetPoint(false);
			Point exit_point = world.GetExitPoint();

			bool finishGame = false;

			if(alan_point.x == sara_point.x && alan_point.y == sara_point.y)
			{
				finishGame = true;
				string game_type = PlayerPrefs.GetString("game_type", "encounter");
				if(game_type.Equals("escape")) 
					PlayerPrefs.SetString("game_status", "alan_wons");
				else
					PlayerPrefs.SetString("game_status", "completed");
			}
			else if(sara_point.x == exit_point.x && sara_point.y == exit_point.y)
			{
				finishGame = true;
				PlayerPrefs.SetString("game_status", "sara_wons");
			}
			else if(turns == 0)
			{
				finishGame = true;
				PlayerPrefs.SetString("game_status", "lose");
			}

			if(finishGame)
			{
				SceneManager.LoadScene("FinalScene");
		    	SceneManager.UnloadSceneAsync("MainGame");
		    	Debug.Log("Escena MainGame terminada, continuando con la escena final");
			}
		}

		/// <summary>
		/// Método que se ejecuta en cada frame del juego. Simula un trabajo por eventos, como clicks o
		/// estados del juego, fin de turno o predicción de lluvia. Se comunica con otras entidades
		/// como los CanvasManager o el HUDManager a través de llamadas a métodos.
		/// </summary>
		void Update()
		{
			CheckFinishGame();
			Point click_point = HandleClick();
			if(click_point != null)
			{
				if(move(click_point, currentCharacter, currentCanvasManager)) 
				{
					movements -= 1;
					hudManager.UpdateHUD(currentCharacter, alan_turn, movements, turns, rain_counter);
				}
			}
			if(movements == 0)
			{
				if(alan_turn)
				{
					currentCharacter = sara;
					currentCanvasManager = saraCanvasManager;
					alanCanvasManager.SetCanvasActive(false);
					saraCanvasManager.SetCanvasActive(true);
				}
				else
				{
					currentCharacter = alan;
					currentCanvasManager = alanCanvasManager;
					saraCanvasManager.SetCanvasActive(false);
					alanCanvasManager.SetCanvasActive(true);
					turns += turn_operand;

					if(rain_counter > 0)
						rain_counter --;

					float prob = Random.Range(0.0F, 1.0F);
					Debug.Log(string.Format("Probabilidad de lluvia: {0}", prob));
					if(prob < rain_probability)
						rain_counter += 3;
				}
				alan_turn = !alan_turn;

				CenterCamera();
				movements = 2;

				if(currentCharacter.HasFastBoots())
					movements ++;

				if(alan_turn)
				{
					float extra_movement_probability = Random.Range(0.0F, 1.0F);
					Debug.Log(string.Format("Probabilidad: {0}", extra_movement_probability));
					if(extra_movement_probability < 0.3F)
						movements++;
				}
				
				if(rain_counter > 0)
				{
					List<GameObject> destroy_objects = currentCanvasManager.FloodMap();
					foreach(GameObject obj in destroy_objects)
						Destroy(obj);
				}
				else
					currentCanvasManager.DryMap();
				
				hudManager.UpdateHUD(currentCharacter, alan_turn, movements, turns, rain_counter);
			}
		}

		/// <summary>
		/// Método que intenta realizar un movimiento del jugador, dicho movimiento puede
		/// ser avanzar a una posición del mundo o colocar una antorcha.
		///
		/// Si el jugador intenta avanzar y es un movimiento válido entonces se actualizará
		/// la posición de los personajes en el mundo así como los estados del CanvasManager.
		/// Controla la aparición de los sprites de ambos personajes y de los tiles del mundo
		/// actualizando las variables de las últimas posiciones de los personajes y los estados
		/// en los CanvasManager.
		///
		/// Si el movimiento que intenta hacer el personaje es de colocar una antorcha, y es válido,
		/// entonces coloca los sprites correspondientes y actualiza los estados en el canvasManager
		/// del personaje. También actualiza el inventario de antorchas del personaje.
		///
		/// Ambas acciones están programadas en esta misma función debido a que ambas se hacen a
		/// través de clicks.
		/// </summary>
		/// <param name="point">Clase que contiene las coordenadas del mundo sobre las que se hizo
		/// click.</param>
		/// <param name="character">Instancia del manejador del personaje del que se trata de realizar
		/// un movimiento. Se utiliza para controlar el inventario de objetos del personaje.</param>
		/// <param name="canvasManager">Instancia del CanvasManager del personaje que trata de realizar
		/// un movimiento.</param>
		/// <returns>Valor booleano: true si se pudo completar un movimiento, false en caso contrario.
		/// El valor regresado es utilizado en la función Update para controlar el número de movimientos
		/// restantes del turno actual, sin embargo, la actualización del inventario y de los sprites
		/// se lleva a cabo aquí y no en el Update.
		/// </returns>
		bool move(Point point, CharacterManager character, CanvasManager canvasManager)
		{
			Point character_position = world.GetPoint(alan_turn);
			int manhattan_len = Mathf.Abs(character_position.x - point.x) + Mathf.Abs(character_position.y - point.y);

			// Evaluate if it is a valid move

			bool validMove = false;
			int[] X = {-1, -1, 0, 1, 1, 1, 0, -1};
    		int[] Y = {0, -1, -1, -1, 0, 1, 1, 1};

    		for(int i = 0; i < 8; i++)
    			validMove |= character_position.x + X[i] == point.x && character_position.y + Y[i] == point.y;

    		validMove |= canvasManager.GetState(point.x, point.y) >= 2 && character.HasMirror() && manhattan_len > 0;

			// Try to move
			if(validMove && character.IsValidMove(world.GetTileType(point.x, point.y))) 
			{
				// Set visited current tiles
				UnmarkTiles(character_position, canvasManager, character);

				// Update character position
				world.SetPoint(alan_turn, point);

				if(alan_turn)
				{
					// Update Alan position in Sara Canvas
					if(saraCanvasManager.GetState(last_alan_position.x, last_alan_position.y) >= 2)
					{
						GameObject sprite_char = saraCanvasManager.RemoveSprite(last_alan_position.x, last_alan_position.y, "alan_tile");
						Destroy(sprite_char);
					}
					
					if(saraCanvasManager.GetState(world.GetPoint(true).x, world.GetPoint(true).y) >= 2)
						DisplaySprite(saraCanvasManager, world.GetPoint(true).x, world.GetPoint(true).y, resources["stand_alan"], -2, "alan_tile");

					// Update last Alan position
					last_alan_position.x = world.GetPoint(true).x;
					last_alan_position.y = world.GetPoint(true).y;
				}
				else
				{
					// Update Sara position in Alan Canvas
					if(alanCanvasManager.GetState(last_sara_position.x, last_sara_position.y) >= 2)	// Visible or torch tile
					{
						GameObject sprite = alanCanvasManager.RemoveSprite(last_sara_position.x, last_sara_position.y, "sara_tile");
						Destroy(sprite);
					}
					string resource_name;
					if(world.GetTileType(world.GetPoint(false).x, world.GetPoint(false).y) == 4)	// Water tile
						resource_name = "water_sara";
					else
						resource_name = "stand_sara";

					if(alanCanvasManager.GetState(world.GetPoint(false).x, world.GetPoint(false).y) >= 2)	// Visible or torch tile
						DisplaySprite(alanCanvasManager, world.GetPoint(false).x, world.GetPoint(false).y, resources[resource_name], -2, "sara_tile");
					
					// Update last Sara position
					last_sara_position.x = world.GetPoint(false).x;
					last_sara_position.y = world.GetPoint(false).y;
				}

				MarkTiles(point, canvasManager, character);

				return true;
			}
			// Try to set Torch in current position
			else if(manhattan_len == 0 && character.GetTorchCount() > 0)
			{
				character.SumTorch(-1);
				for(int x = point.x - 1; x <= point.x + 1; x++)
				{
					for(int y = point.y - 1; y <= point.y + 1; y++)
					{
						if(x >= 0 && x < rows && y >=0 && y < cols)
						{
							Debug.Log(string.Format("Se puso antorcha en: {0}, {1}", x, y));
							SetTorch(x, y, canvasManager);
						}
					}
				}
				DisplayPermanentSprite(canvasManager, point.x, point.y, resources["torch_0"], 0);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Manda a mostrar un sprite permanentemente en un CanvasManager (siempre y cuando el 
		/// canvas se encuentre activo). Es la forma en la que se muestran los sprites de antorchas.
		/// </summary>
		/// <param name="canvasManager">CanvasManager en el que se mostrará permanentemente un
		/// sprite.</param>
		/// <param name="row">Fila de la casilla sobre la que se mostrará el sprite.</param>
		/// <param name="col">Columna de la casilla sobre la que se mostrará el sprite.</param>
		/// <param name="sprite_obj">Instancia del sprite a mostrar. Los objetos de los sprites
		/// se encuentran en un diccionario creado por el método LoadResources.</param>
		/// <param name="depth">Profundidad del sprite en el canvas.</param>
		void DisplayPermanentSprite(CanvasManager canvasManager, int row, int col, GameObject sprite_obj, int depth)
		{
			GameObject sprite = (GameObject)Instantiate(sprite_obj, transform);
			canvasManager.AddPermanentSprite(row, col, sprite, depth);
		}

		/// <summary>
		/// Manda a mostrar un sprite en un CanvasManager.
		/// </summary>
		/// <param name="canvasManager">CanvasManager en el que se mostrará el sprite.</param>
		/// <param name="row">Fila de la casilla sobre la que se mostrará el sprite.</param>
		/// <param name="col">Columna de la casilla sobre la que se mostrará el sprite.</param>
		/// <param name="sprite_obj">Instancia del sprite a mostrar. Los objetos de los sprites
		/// se encuentran en un diccionario creado por el método LoadResources.</param>
		/// <param name="depth">Profundidad del sprite en el canvas.
		/// A continuación se presentan las reglas de profundidad de sprites:
		/// -2: characters
		///	-1: rain
		///	0: objects
		///	1: dark_tile on
		///	2: tile
		///	3: dark_tile off
		/// Los sprites con menor profundidad tienen precedencia para mostrarse sobre los de mayor
		/// profundidad.</param>
		/// <param name="sprite_name">(Opcional) nombre con el que identificará el sprite en el canvas.
		/// Si no se especifica nombre el valor por defecto será tile.
		/// A continuación se listan los posibles nombres:
		/// tile: identificador del sprite del tipo de casilla.
		/// sara_tile: identificador del sprite de Sara en una casilla.
		/// alan_tile: identificador del sprite de Alan en una casilla.
		/// chest_tile: identificador del sprite de un cofre en una casilla.
		/// dark_tile: identificador del sprite de casilla obscura.</param>
		/// <returns>Instancia del objeto creado</returns>
		GameObject DisplaySprite(CanvasManager canvasManager, int row, int col, GameObject sprite_object, int depth, string sprite_name = "tile")
		{
			GameObject tile = (GameObject)Instantiate(sprite_object, transform);
			tile.name = sprite_name;
			canvasManager.AddTile(row, col, tile, depth);
			return tile;
		}

		/// <summary>
		/// Actualiza el estado de una casilla a Siempre Visible (estado 3) y actualiza los sprites en la
		/// casilla. NO coloca los sprites de antorchas.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <param name="canvasManager">Canvas sobre el que se actualiza la casilla.</param>
		void SetTorch(int row, int col, CanvasManager canvasManager)
		{
			if(canvasManager.GetState(row, col) < 2)
				SetVisibleTile(row, col, canvasManager);
			canvasManager.SetState(row, col, 3);
		}

		/// <summary>
		/// Marca como visitadas las casillas alrrededor de un punto dado, considerando el
		/// terreno y los objetos del personaje. Se utiliza cuando un movimiento es exitoso
		/// y el personaje se traslada de posición.
		/// </summary>
		/// <param name="point">Casilla central de las casillas que se pondrán como visitadas.</param>
		/// <param name="canvasManager">Canvas del personaje que se está moviendo.</param>
		/// <param name="character">Objeto del personaje que se está moviendo.</param>
		void UnmarkTiles(Point point, CanvasManager canvasManager, CharacterManager character)
		{
			int borders = 1;
			if(character.HasTelescope())
				borders += 1;
			if(world.GetTileType(point.x, point.y) == 0)	// Character is over a mountain
				borders += 1;
			for(int x = point.x - borders; x <= point.x + borders; x++)
			{
				for(int y = point.y - borders; y <= point.y + borders; y++)
				{
					if(x >= 0 && x < rows && y >= 0 && y < cols)
						SetVisitedTile(x, y, canvasManager);
				}
			}

			// Remove character from point
			string sprite_name;
			if(alan_turn)
				sprite_name = "alan_tile";
			else
				sprite_name = "sara_tile";
			GameObject sprite_char = canvasManager.RemoveSprite(point.x, point.y, sprite_name);
			Destroy(sprite_char);
		}

		/// <summary>
		/// Marca como visibles las casillas alrrededor de un punto dado, considerando el
		/// terreno y los objetos del personaje. Se utiliza cuando un movimiento es exitoso
		/// y el personaje se traslada de posición para mostrar las nuevas casillas.
		/// </summary>
		/// <param name="point">Casilla central de las casillas que se pondrán como visibles.</param>
		/// <param name="canvasManager">Canvas del personaje que se está moviendo.</param>
		/// <param name="character">Objeto del personaje que se está moviendo.</param>
		void MarkTiles(Point point, CanvasManager canvasManager, CharacterManager character)
		{
			// Activar los nuevos objetos a los que llega
			int bitmask = world.GetObjectManager().GetObjects(point.x, point.y);
			if(bitmask > 0) 
			{
				ActivateObjects(bitmask, character);

				// Special item: map
				if((bitmask & 8) > 0 && !character.HasMap())
				{
					character.ActivateMap();
					for(int x = 0; x < rows; x++) 
					{
						for(int y = 0; y < cols; y++)
						{
							if(canvasManager.GetState(x, y) != 0)
								SetTorch(x, y, canvasManager);
						}
					}
				}

				world.GetObjectManager().TakeObjects(point.x, point.y);
				if(alanCanvasManager.GetState(point.x, point.y) >= 2)	// Visible or torch
				{
					GameObject chest_sprite = alanCanvasManager.RemoveSprite(point.x, point.y, "chest_tile");
					Destroy(chest_sprite);
				}
				if(saraCanvasManager.GetState(point.x, point.y) >= 2)	// Visible or torch
				{
					GameObject chest_sprite = saraCanvasManager.RemoveSprite(point.x, point.y, "chest_tile");
					Destroy(chest_sprite);
				}
			}

			int borders = 1;
			if(character.HasTelescope())
				borders += 1;
			if(world.GetTileType(point.x, point.y) == 0)	// New position is a mountain
				borders += 1;

			for(int x = point.x - borders; x <= point.x + borders; x++ )
			{
				for(int y = point.y - borders; y <= point.y + borders; y++)
				{
					if(x >= 0 && x < rows && y >= 0 && y < cols)
					{
						if(!character.HasMap())
							SetVisibleTile(x, y, canvasManager);
						else
							SetTorch(x, y, canvasManager);
					}
				}
			}

			// Set character sprite at new point
			string sprite_name, object_name = "sara_tile";
			if(alan_turn)
			{
				sprite_name = "stand_alan";
				object_name = "alan_tile";
			}
			else if(world.GetTileType(point.x, point.y) == 4)	// Water
				sprite_name = "water_sara";
			else
				sprite_name = "stand_sara";
			DisplaySprite(canvasManager, point.x, point.y, resources[sprite_name], -2, object_name);
		}

		/// <summary>
		/// Método que activa un conjunto de objetos a un personaje.
		/// </summary>
		/// <param name="bitmask">Máscara de bits del conjunto de objetos.
		/// A continuación se describen los objetos que representan cada bit:
		/// 1: torch
		/// 2: hiking boots
		/// 4: rapid boots
		/// 8: map
		/// 16: telescope
		/// 32: rain
		/// 64: mirror
		/// </param>
		void ActivateObjects(int bitmask, CharacterManager character)
		{
			if((bitmask & 1) > 0)	// Torch
			{
				for(int i = 0; i < 3; i++)
					character.SumTorch(1);
			}
			if((bitmask & 2) > 0)	// Hiking boots
			{
				for(int i = 0; i < 3; i++)
					character.SumHikingBoots(1);
			}
			if((bitmask & 4) > 0)	// Fast boots
				character.ActivateFastBoots();
			if((bitmask & 16) > 0)	// Telescope
				character.ActivateTelescope();
			if((bitmask & 32) > 0)	// Rain
				rain_counter += 3;
			if((bitmask & 64) > 0)	// Mirror
				character.ActivateMirror();
		}

		/// <summary>
		/// Administra el evento de detección de clicks.
		/// </summary>
		/// <returns>Objeto Point con las coordenadas de la casilla que se dió click
		/// en caso de que sí se haya detectado, de lo contrario regresa null.</returns>
		Point HandleClick()
		{
			if(Input.GetButtonDown("Fire1"))
	        {
	            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	            if (Physics.Raycast(ray))
	                Instantiate(particle, transform.position, transform.rotation);

	            lastClickDown = new Vector2(ray.origin.x, ray.origin.y);
	        }
	        if(Input.GetButtonUp("Fire1"))
	        {
	        	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	            if (Physics.Raycast(ray))
	                Instantiate(particle, transform.position, transform.rotation);
	            if(lastClickDown.x == ray.origin.x && ray.origin.y == lastClickDown.y) 
	            {
	            	int col = (int) ray.origin.x;
		            col += (int)(ray.origin.x * 2 - 2 * (float)col);

		            int row = (int) ray.origin.y;
		            row += (int)(ray.origin.y * 2 - 2 * (float)row);

		            if(currentCanvasManager.EvaluateClick(-row, col))
		            {
		            	// Dio click en una casilla y está dentro del rango
		            	return currentCanvasManager.DetransformPoint(-row, col);
		            }
		        }
	        }
	        return null;
		}

		/// <summary>
		/// Carga en memoria los objetos de los distintos sprites que utiliza el juego.
		/// </summary>
		/// <returns>Diccionario de GameObject con instancias de los objetos de sprites.</returns>
		private Dictionary<string, GameObject> LoadResources()
	    {
	    	var resources = new Dictionary<string, GameObject>();
	    	for(int i = 0; i < 16; i++)
	    	{
	    		string resource_name = string.Format("water_{0}", i);
	    		resources[resource_name] = (GameObject)Instantiate(Resources.Load(resource_name));
	    		resource_name = string.Format("path_{0}", i);
	    		resources[resource_name] = (GameObject)Instantiate(Resources.Load(resource_name));
	    	}
	    	resources["hill_0"] = (GameObject)Instantiate(Resources.Load("hill_0"));
	    	resources["pasture_0"] = (GameObject)Instantiate(Resources.Load("pasture_0"));
	    	resources["tree_0"] = (GameObject)Instantiate(Resources.Load("tree_0"));
	    	resources["dark_tile"] = (GameObject)Instantiate(Resources.Load("dark_tile_0"));
	    	resources["tree_0"] = (GameObject)Instantiate(Resources.Load("tree_0"));
	    	resources["stand_alan"] = (GameObject)Instantiate(Resources.Load("stand_alan"));
	    	resources["stand_sara"] = (GameObject)Instantiate(Resources.Load("stand_sara"));
	    	resources["water_sara"] = (GameObject)Instantiate(Resources.Load("water_sara"));
	    	resources["water_chest"] = (GameObject)Instantiate(Resources.Load("water_chest"));
	    	resources["chest_tile"] = (GameObject)Instantiate(Resources.Load("chest_tile"));
	    	resources["tree_chest"] = (GameObject)Instantiate(Resources.Load("tree_chest"));
	    	resources["rain_tile"] = (GameObject)Instantiate(Resources.Load("rain_tile_0"));
	    	resources["exit_0"] = (GameObject)Instantiate(Resources.Load("exit_0"));
	    	resources["torch_0"] = (GameObject)Instantiate(Resources.Load("torch_0"));
	    	return resources;
	    }

		/// <summary>
		/// Pone una casilla como visible y actualiza su estado en un CanvasManager.
		/// Muestra los sprites correspondientes en la casilla visible. Los sprites
		/// siguen las siguientes reglas de profundidad:
		/// -2: characters
		/// -1: rain
		/// 0: objects
		/// 1: dark_tile on
		/// 2: tile
		/// 3: dark_tile off
		/// Los sprites con menor profundidad tienen precedencia para mostrarse sobre los de mayor
		/// profundidad.</param>
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <param name="canvasManager">CanvasManager del personaje que está activando la casilla.</param>
		public void SetVisibleTile(int row, int col, CanvasManager canvasManager)
		{
			if(canvasManager.GetState(row, col) == 3)	// Torch tile
				return;
			if(canvasManager.GetState(row, col) == 0)
			{
				DisplaySprite(canvasManager, row, col, resources[world.GetTileName(row, col, canvasManager.GetOrientation())], 2);
				canvasManager.SetState(row, col, 2);	// Visible
			}
			else if(canvasManager.GetState(row, col) == 1)
			{
				GameObject dark_tile = canvasManager.GetTile(row, col, "dark_tile");

				Point trans_point = canvasManager.TransformPoint(row, col);
				float posX = (float)trans_point.y;
				float posY = (float)-trans_point.x;

				dark_tile.transform.position = new Vector3((float)posX, (float)posY, 3);
				canvasManager.SetState(row, col, 2);	// Visible
			}
			int bitmask = world.GetObjectManager().GetObjects(row, col);
			if(bitmask > 0)
			{
				int tile_type = world.GetTileType(row, col);
				string chest_name;
				if(tile_type == 1)	// Tree
					chest_name = "tree_chest";
				else if(tile_type == 4)	//Water
					chest_name = "water_chest";
				else
					chest_name = "chest_tile";
				DisplaySprite(canvasManager, row, col, resources[chest_name], 0, "chest_tile");
			}
			if(alan_turn)
			{
				Point sara_point = world.GetPoint(false);
				if(sara_point.x == row && sara_point.y == col)
				{
					if(world.GetTileType(row, col) == 4)	//Water
						DisplaySprite(canvasManager, row, col, resources["water_sara"], -2, "sara_tile");
					else
						DisplaySprite(canvasManager, row, col, resources["stand_sara"], -2, "sara_tile");
				}
			}
			else
			{
				Point alan_point = world.GetPoint(true);
				if(alan_point.x == row && alan_point.y == col)
					DisplaySprite(canvasManager, row, col, resources["stand_alan"], -2, "alan_tile");
			}
		}

		/// <summary>
		/// Método contrario a SetVisibleTile: cambia el estado de una casilla de visible a visitada y
		/// actualiza los sprites correspondientes en la casilla visitada. Se activan los sprites obscuros
		/// siguiendo las siguientes reglas de profundidad:
		/// -2: characters
		/// -1: rain
		/// 0: objects
		/// 1: dark_tile on
		/// 2: tile
		/// 3: dark_tile off
		/// Los sprites con menor profundidad tienen precedencia para mostrarse sobre los de mayor
		/// profundidad.</param>
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <param name="canvasManager">CanvasManager del personaje que está desactivando la casilla.</param>
		public void SetVisitedTile(int row, int col, CanvasManager canvasManager)
		{
			if(canvasManager.GetState(row, col) < 3)	// Not in torch
			{
				// Destroy object sprite on position (row, col)
				GameObject chest_sprite = canvasManager.RemoveSprite(row, col, "chest_tile");
				Destroy(chest_sprite);

				//Destroy characters
				GameObject sprite_char = canvasManager.RemoveSprite(row, col, "alan_tile");
				Destroy(sprite_char);
				sprite_char = canvasManager.RemoveSprite(row, col, "sara_tile");
				Destroy(sprite_char);

				GameObject dark_tile = canvasManager.GetTile(row, col, "dark_tile");
				if(dark_tile == null)
					dark_tile = DisplaySprite(canvasManager, row, col, resources["dark_tile"], 1, "dark_tile");
				
				Point trans_point = canvasManager.TransformPoint(row, col);
				float posX = (float)trans_point.y;
				float posY = (float)-trans_point.x;
				dark_tile.transform.position = new Vector3((float)posX, (float)posY, 1);

				canvasManager.SetState(row, col, 1);	// Visited
			}
		}

		/// <summary>
		/// Apoyado de la variable alan_turn centra la cámara en la posición del
		/// personaje del turno actual.
		/// </summary>
		public void CenterCamera()
	    {
	    	Camera cam = Camera.main;
	    	Point point = world.GetPoint(alan_turn);

	    	Point trans_point = currentCanvasManager.TransformPoint(point.x, point.y);
	    	float width = (float)-trans_point.x;
	    	float height = (float)trans_point.y;
	    	cam.transform.position = new Vector3(height, width , -10);
	    }

	    /// <summary>
	    /// Método que termina la escena del juego. Se utiliza en la llamada a QuitGame.
	    /// </summary>
	    public void ExitGame()
	    {
	    	SceneManager.LoadScene("MainMenu");
	    	SceneManager.UnloadSceneAsync("MainGame");
	    	Debug.Log("Escena MainGame terminada");
	    }

	}

}