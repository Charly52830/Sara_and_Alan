using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MyNamespace 
{

	/// <summary>
	/// Clase que proporciona los métodos necesarios para cargar en el juego un nivel
	/// del modo historia.
	/// </summary>
	public static class LevelLoader 
	{
	    
	    /// <summary>
	    /// Método que carga al juego un nivel del modo historia. Los archivos de los niveles
	    /// se encuentran en el directorio levels/ en formato de texto con una representación
	    /// minimalista del mundo, en las que cada casilla se representa con una letra del alfabeto latino:
	    /// M: montaña
		/// T: arbol
		/// p: pasto
		/// P: camino
		/// W: agua
		/// E: salida
		/// R: casilla aleatoria
		/// X: pasto o camino aleatorio
		/// r: montaña, agua o arbol aleatorio
		///
		/// El formato que siguen los archivos es el siguiente:
		/// rows cols
		/// fila_1 (WWPTpRXE... cols caracteres)
		/// fila_2 (WWPTpRXE... cols caracteres)
		/// ...
		/// numero_de_objetos
		/// row_objeto_1 col_objeto_1 tipo_objeto_1
		/// ...
		/// row_objeto_n row_objeto_n tipo_objeto_n
		/// row_alan col_alan (indexado desde 0)
		/// row_sara col_sara (indexado desde 0)
		/// row_salida col_salida (indexado desde 0)
		/// probabilidad_lluvia (número real p: 0 <= x <= 1)
		/// numero_de_turnos
		///
		/// Los objetos siguen las siguientes reglas de representación:
		/// 0: torch (bitmask 1)
		/// 1: hiking boots (bitmask 2)
		/// 2: fast boots (bitmask 4)
		/// 3: map (bitmask 8)
		/// 4: telescope (bitmask 16)
		/// 5: rain (bitmask 32)
		/// 6: mirror (bitmask 64)
	    /// </summary>
	    /// <returns>Instancia de CompressedWorld que contiene la información necesaria para
	    /// crear el mundo.</returns>
	    // TO DO: agregar orientación a los niveles del mundo.
	    public static CompressedWorld LoadLevel(string level)
	    {
	    	CompressedWorld compressedWorld = new CompressedWorld();
	    	TextAsset compressedWorld_text = Resources.Load(string.Format("levels/level_{0}", PlayerPrefs.GetInt("level"))) as TextAsset;

	    	string line;
	    	string[] parts;
	    	StringReader strReader = new StringReader(compressedWorld_text.text);

	    	// Rows and cols
	    	Point tmp = ParsePoint(strReader.ReadLine());
	    	compressedWorld.rows = tmp.x + 1;
	    	compressedWorld.cols = tmp.y + 1;

	    	int rows = compressedWorld.rows;
	    	int cols = compressedWorld.cols;
	    	Debug.Log(string.Format("rows: {0}, cols: {1}", rows, cols));

	    	// Tiles
	    	compressedWorld.tiles = new int[rows, cols];

	    	//Fill borders with mountains
			for(int x = 0; x < rows; x++)
			{
				compressedWorld.tiles[x, 0] = 0;	//Mountain
				compressedWorld.tiles[x, cols - 1] = 0;	//Mountain
			}
			for(int x = 0; x < cols; x++) 
			{
				compressedWorld.tiles[0, x] = 0;	//Mountain
				compressedWorld.tiles[rows - 1, x] = 0;	//Mountain
			}

	    	Debug.Log(string.Format("{0} {1}", rows, cols));
	    	for(int x = 1; x < rows - 1; x++)
	    	{
	    		line = strReader.ReadLine();
	    		for(int y = 1; y < cols - 1; y++)
	    		{
	    			//compressedWorld.tiles[x, y] = (int)(line[y - 1] - '0');
	    			char c = line[y - 1];
	    			if(c == 'M')	// Mountain
	    				compressedWorld.tiles[x, y] = 0;
	    			else if(c == 'T')	// Tree
	    				compressedWorld.tiles[x, y] = 1;
	    			else if(c == 'p')	// Pasture
	    				compressedWorld.tiles[x, y] = 2;
	    			else if(c == 'P')	// Path
	    				compressedWorld.tiles[x, y] = 3;
	    			else if(c == 'W')	// Water
	    				compressedWorld.tiles[x, y] = 4;
	    			else if(c == 'R')	// Random tile
	    				compressedWorld.tiles[x, y] = Random.Range(0, 5);
	    			else if(c == 'X')	// Random path or pasture
	    				compressedWorld.tiles[x, y] = Random.Range(2, 4);
	    			else if(c == 'r')	// Random mountain, water or tree
	    			{
	    				int r = Random.Range(0, 3);
	    				int[] A = {0, 1, 4};
	    				compressedWorld.tiles[x, y] = A[r];
	    			}
	    		}
	    	}

	    	// Objects
	    	compressedWorld.objectManager = new ObjectManager(rows, cols);

	    	// Number of objects
	    	int objects = int.Parse(strReader.ReadLine());

	    	// Objects description
	    	for(int x = 0; x < objects; x++)
	    	{
	    		line = strReader.ReadLine();
	    		parts = line.Split(' ');
	    		int row = int.Parse(parts[0]) + 1;
	    		int col = int.Parse(parts[1]) + 1;
	    		int obj = int.Parse(parts[2]);
	    		Debug.Log(string.Format("Napadas: {0}, {1}", row, col));
	    		compressedWorld.objectManager.AddObject(row, col, obj);
	    	}

	    	// Alan position (zero indexed)
	    	compressedWorld.alan_point = ParsePoint(strReader.ReadLine());

	    	// Sara position (zero indexed)
	    	compressedWorld.sara_point = ParsePoint(strReader.ReadLine());

	    	// Exit point 
	    	compressedWorld.exit_point = ParsePoint(strReader.ReadLine());

	    	// Rain probability
	    	compressedWorld.rain_probability = float.Parse(strReader.ReadLine());

	    	// Turns
	    	compressedWorld.turns = int.Parse(strReader.ReadLine());

	    	// Turn operand
	    	if(compressedWorld.turns > 0)
	    		compressedWorld.turn_operand = -1;
	    	else
	    	{
	    		compressedWorld.turns = 1;
	    		compressedWorld.turn_operand = 1;
	    	}

	    	return compressedWorld;
	    }

	    /// <summary>
	    /// Utilidad de la clase LevelLoader que realiza un parse de un string a un punto.
	    /// </summary>
	    private static Point ParsePoint(string line) 
	    {
	    	var parts = line.Split(' ');
	    	return new Point(int.Parse(parts[0]) + 1, int.Parse(parts[1]) + 1);
	    }
	}

}