from Constants import *

class Map :

	XY = [(-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (0, 0)]
	
	def __init__(self, rows, cols, char_x, char_y) :
		self.rows = rows
		self.cols = cols
		self.visited_grid = [[False] * (rows + 2) for _ in range((cols + 2))]
		self.char_x = char_x
		self.char_y = char_y
		self.set_visited_tiles(self.char_x, self.char_y)
	
	def set_visited_tiles(self, x, y) :
		for i in self.XY :
			tmp_x = x + i[0]
			tmp_y = y + i[1]
			
			if tmp_x >= 0 and tmp_x < self.rows and tmp_y >= 0  and tmp_y < self.cols :
				self.visited_grid[tmp_x][tmp_y] |= True
	
	def get_char_position(self) :
		return (self.char_x, self.char_y)
	
	def is_visited(self, x, y) :
		return self.visited_grid[x][y]
	
class Miguel_map(Map) :
	
	def __init__(self, rows, cols, char_x, char_y, world) :
		Map.__init__(self, rows, cols, char_x, char_y)
		self.world = world
	
	def set_char_position(self, new_x, new_y) :
		for i in range(len(self.XY) - 1) : 
			tmp_x = self.char_x + self.XY[i][0]
			tmp_y = self.char_y + self.XY[i][1]
			bul = True
			bul &= tmp_x == new_x and tmp_y == new_y
			bul &= new_x >= 0 and new_x < self.rows
			bul &= new_y >= 0 and new_y < self.cols
			bul &= self.world.get_tile(new_x, new_y) not in {HILL, WATER}
			
			if bul :
				self.char_x = new_x
				self.char_y = new_y
				self.set_visited_tiles(self.char_x, self.char_y)
				return True
		return False

class Sara_map(Map) :
	
	def __init__(self, rows, cols, char_x, char_y, world) :
		Map.__init__(self, rows, cols, char_x, char_y)
		self.world = world
	
	def set_char_position(self, new_x, new_y) :
		for i in range(len(self.XY) - 1) : 
			tmp_x = self.char_x + self.XY[i][0]
			tmp_y = self.char_y + self.XY[i][1]
			
			bul = True
			bul &= tmp_x == new_x and tmp_y == new_y
			bul &= new_x >= 0 and new_x < self.rows
			bul &= new_y >= 0 and new_y < self.cols
			bul &= self.world.get_tile(new_x, new_y) not in {HILL, TREE}
			
			if bul :
				self.char_x = new_x
				self.char_y = new_y
				self.set_visited_tiles(self.char_x, self.char_y)
				return True
		return False
		
