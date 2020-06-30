using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace {

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
		public void AddObject(int row, int col, int type)
		{
			if(row > 0 && row < rows - 1 && col > 0 && col < cols -1 )
				gridObjectBitmask[row, col] |= 1 << type;
		}

		public int GetObjects(int row, int col)
		{
			if(row > 0 && row < rows - 1 && col > 0 && col < cols -1 )
				return gridObjectBitmask[row, col];
			return 0;
		}

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
