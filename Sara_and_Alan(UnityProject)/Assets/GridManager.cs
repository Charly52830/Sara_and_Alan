using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace MyNamespace {

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
	    private float tileSize = 1.0F;
		private Dictionary<string, GameObject> resources;

		// Rain
		private float rain_probability = 0.15F;	// Number between 0 and 1

		void Start()
		{
			hudManager = new HUDManager();
			resources = LoadResources();
			game_type =  PlayerPrefs.GetString("game_type", "escape");
			
			if(game_type.Equals("escape"))
			{
				rows = Random.Range(15, 35);
				cols = Random.Range(15, 35);
				//rows = 30;	// Borrar esta linea
				//cols = 30;	// Borrar esta linea
				//rain_probability = 0;	// Borrar Esta linea
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
				world = new World(compressedWorld);
				rows -= 2;
				cols -= 2;
			}

			cols += 2;
			rows += 2;

			//world.PrintWorld();

			Point alan_point = world.GetPoint(true);
			Point sara_point = world.GetPoint(false);
			last_alan_position = new Point(alan_point.x, alan_point.y);
			last_sara_position = new Point(sara_point.x, sara_point.y);

			alanCanvasManager = new CanvasManager(GameObject.Find("CanvasAlan"), rows, cols);
			saraCanvasManager = new CanvasManager(GameObject.Find("CanvasSara"), rows, cols);

			sara = new CharacterManager(false);
			alan = new CharacterManager(true);

			MarkTiles(world.GetPoint(false), saraCanvasManager, sara);	// First tiles of Sara
			alan_turn = true;
			MarkTiles(world.GetPoint(true), alanCanvasManager, alan);	// First tiles of Alan

			PrepareRain(alanCanvasManager);
			PrepareRain(saraCanvasManager);

			saraCanvasManager.SetCanvasActive(false);
			alanCanvasManager.SetCanvasActive(true);

			CenterCamera();
			movements = 2;
			currentCanvasManager = alanCanvasManager;
			currentCharacter = alan;

			hudManager.UpdateHUD(currentCharacter, alan_turn, movements, turns, rain_counter);
		}

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
					
					// Update last alan position
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

		void DisplayPermanentSprite(CanvasManager canvasManager, int row, int col, GameObject sprite_obj, int depth)
		{
			GameObject sprite = (GameObject)Instantiate(sprite_obj, transform);

			canvasManager.AddPermanentSprite(row, col, sprite, depth);
		}

		GameObject DisplaySprite(CanvasManager canvasManager, int row, int col, GameObject tile_object, int depth, string tile_name = "tile")
		{
			GameObject tile = (GameObject)Instantiate(tile_object, transform);

			tile.name = tile_name;
			canvasManager.AddTile(row, col, tile, depth);
			
			return tile;
		}

		void SetTorch(int row, int col, CanvasManager canvasManager)
		{
			if(canvasManager.GetState(row, col) < 2)
				SetVisibleTile(row, col, canvasManager);
			canvasManager.SetState(row, col, 3);
		}

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

		/*
		 *	1: torch
		 *	2: hiking boots
		 *	4: rapid boots
		 *	8: map
		 *	16: telescope
		 * 	32: rain
		 *	64: mirror
		 */
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

		            if(0 <= col && col < cols && 0 <= -row && -row < rows)
		            {
		            	// Dio click en una casilla y está dentro del rango
		            	return new Point(-row, col);
		            }
		        }
	        }
	        return null;
		}

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

		/*
		 *	Depth rules:
		 *
		 *	-2: characters
		 *	-1: rain
		 *	0: objects
		 *	1: dark_tile on
		 *	2: tile
		 *	3: dark_tile off
		 */
		public void SetVisibleTile(int row, int col, CanvasManager canvasManager)
		{
			if(canvasManager.GetState(row, col) == 3)	// Torch tile
				return;
			if(canvasManager.GetState(row, col) == 0)
			{
				DisplaySprite(canvasManager, row, col, resources[world.GetTileName(row, col)], 2);
				canvasManager.SetState(row, col, 2);	// Visible
			}
			else if(canvasManager.GetState(row, col) == 1)
			{
				GameObject dark_tile = canvasManager.GetTile(row, col, "dark_tile");
				float posX = col * tileSize;
				float posY = row * -tileSize;
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
				
				float posX = col * tileSize;
				float posY = row * -tileSize;
				dark_tile.transform.position = new Vector3((float)posX, (float)posY, 1);

				canvasManager.SetState(row, col, 1);	// Visited
			}
		}

		public void CenterCamera()
	    {
	    	Camera cam = Camera.main;
	    	Point point = world.GetPoint(alan_turn);
	    	//Debug.Log(string.Format("Desde CenterCamera {0}, {1}", point.x, point.y));
	    	float width = (float)point.x;
	    	float height = (float)point.y;
	    	cam.transform.position = new Vector3(height, - width , -10);
	    }

	    public void ExitGame()
	    {
	    	//SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
	    	SceneManager.LoadScene("MainMenu");
	    	SceneManager.UnloadSceneAsync("MainGame");
	    	Debug.Log("Escena MainGame terminada");
	    }

	}



}