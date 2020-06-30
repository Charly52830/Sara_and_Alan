from random import seed, randint
from Constants import *

class World :
	
	box = [HILL, TREE, WATER, WATER, ROAD, PASTURE]
	
	def __init__ (self, w_seed, rows, cols) :
		self.grid = [ [0] * rows for _ in range(cols)]
		self.rows = rows
		self.cols = cols
		seed(w_seed)
		for i in range(1, rows - 1) :
			for j in range(1, cols - 1) :
				self.grid[i][j] = self.box[randint(0, len(self.box) - 1)]
				#print(self.grid[i][j], end ='')
			#print('')
		
		for i in range(rows) :
			self.grid[i][0] = HILL
			self.grid[i][rows - 1] = HILL
		
		for i in range(cols) :
			self.grid[0][i] = HILL
			self.grid[cols - 1][i] = HILL
	
	def get_tile(self, x, y) :
		if x < 0 or x >= self.rows or y < 0 or y >= self.cols :
			return -1
		return self.grid[x][y]
	
	def get_size(self) :
		return (self.rows, self.cols)

