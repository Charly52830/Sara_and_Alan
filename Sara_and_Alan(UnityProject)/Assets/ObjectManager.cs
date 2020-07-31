using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace {

	/// <summary>
	/// Controla los objetos existentes en un mundo.
	/// </summary>
	public class ObjectManager
	{
		private int[,] gridObjectBitmask;
		private int rows;
		private int cols;

		public ObjectManager(int rows, int cols)
		{
			this.cols = cols;
			this.rows = rows;
			gridObjectBitmask = new int[rows, cols];
			for(int x = 0; x < rows; x++)
			{
				for(int y = 0; y < cols; y++)
				{
					gridObjectBitmask[x, y] = 0;	//Empty tiles
				}
			}
		}

		/*
		 *	Object number representation
		 *
		 *	0: torch (bitmask 1)
		 *	1: hiking boots (bitmask 2)
		 *	2: rapid boots (bitmask 4)
		 *	3: map (bitmask 8)
		 *	4: telescope (bitmask 16)
		 *	6: rain (bitmask 32)
		 *	5: mirror (bitmask 64)
		 */


		/// <summary>
		/// Agrega un nuevo objeto al mundo.
		/// </summary>
		/// <param name="row">Fila de la casilla en la que se quiere agregar un objeto.</param>
		/// <param name="col">Columna de la casilla en la que se quiere agregar un objeto.</param>
		/// <param name="type">Tipo de objeto a agregar. Cada objeto se encuentra representado por
		/// un bit en una máscara de bits. A continuación se presentan los números que representan
		/// a cada objeto:
		/// 0: torch (bitmask 1)
		/// 1: hiking boots (bitmask 2)
		/// 2: rapid boots (bitmask 4)
		/// 3: map (bitmask 8)
		/// 4: telescope (bitmask 16)
		/// 6: rain (bitmask 32)
		/// 5: mirror (bitmask 64)
		/// </param>
		public void AddObject(int row, int col, int type)
		{
			if(row > 0 && row < rows - 1 && col > 0 && col < cols -1 )
				gridObjectBitmask[row, col] |= 1 << type;
		}

		/// <summary>
		/// Regresa el conjunto de objetos que existen en una casilla del mundo
		/// representado como una máscara de bits.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <returns>Conjunto de objetos en la casilla representado como una máscara de bits.</returns>
		public int GetObjects(int row, int col)
		{
			if(row > 0 && row < rows - 1 && col > 0 && col < cols -1 )
				return gridObjectBitmask[row, col];
			return 0;
		}

		/// <summary>
		/// Remueve los objetos en la casilla dada.
		/// </summary>
		/// <param name="row">Fila de la casilla.</param>
		/// <param name="col">Columna de la casilla.</param>
		/// <returns>Conjunto de objetos en la casilla representado como una máscara de bits.</returns>
		public int TakeObjects(int row, int col)
		{
			if(row > 0 && row < rows - 1 && col > 0 && col < cols -1 )
			{
				int ans = gridObjectBitmask[row, col];
				gridObjectBitmask[row, col] = 0;
				return ans;
			}
			return 0;
		}
	}

}
