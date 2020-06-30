import pygame

from Mundo import World
from Mapa import Map
from Constants import *

class Background(pygame.sprite.Sprite):
	def __init__(self, image_file, location):
		pygame.sprite.Sprite.__init__(self)  #call Sprite initializer
		self.image = pygame.image.load(image_file)
		self.rect = self.image.get_rect()
		self.rect.left, self.rect.top = location

class Drawer :

	img_path = 'img/'
	
	XY = [(-1, 0, 1), (0, -1, 2), (1, 0, 4), (0, 1, 8)]
	
	def __init__(self, screen, world) :
		self.screen = screen
		self.world = world
	
	def get_img_file(self, row, col) :
		if self.world.get_tile(row, col) == HILL :
			return 'Mountain.png'
		elif self.world.get_tile(row, col) == TREE :
			return 'Tree_Modified.png'
		elif self.world.get_tile(row, col) == WATER :
			return 'WaterTile_1.png'
		elif self.world.get_tile(row, col) == ROAD :
			world_rows, world_cols = self.world.get_size()
			bitmask = 0
			for xy in self.XY :
				tmp_x = row + xy[0]
				tmp_y = col + xy[1]
				if 0 <= tmp_x and tmp_x < world_rows and 0 <= tmp_y and tmp_y < world_cols :
					if self.world.get_tile(tmp_x, tmp_y) == ROAD :
						bitmask |= xy[2]
			return 'Road_' + str(bitmask) + '.png'
		else :
			return 'GrassTile_1.png'
		
	def draw_row(self, row, col, char_map, x_screen, y_screen) :
		world_rows, world_cols = self.world.get_size()
		for i in range(COL_DISPLAY) :
			cur_col = (col + i) % world_cols
			if 0 > row or row >= world_rows or 0 > col or col >= world_cols :
				img_file = 'BlackTile.png'
			elif char_map.is_visited(row, cur_col) :
				img_file = self.get_img_file(row, cur_col)
			else :
				img_file = 'BlackTile.png'
			bg_image = Background(self.img_path + img_file, [y_screen + 32 * i, x_screen])
			self.screen.blit(bg_image.image, bg_image.rect)
	
	def draw_world(self, char_map, row, col) :
		x_screen = 0
		y_screen = 0
		_, world_cols = self.world.get_size()
		self.screen.fill([255, 255, 255])
		for i in range(ROW_DISPLAY) :
			self.draw_row((row + i) % world_cols, col, char_map, x_screen + 32 * i, y_screen)
	
	def draw_character(self, cur_x, cur_y, char_x, char_y, is_miguel) :
		rows, cols = self.world.get_size()
		x = (char_x - cur_x + rows) % rows
		y = (char_y - cur_y + cols) % cols
		if 0 <= x and x < ROW_DISPLAY and 0 <= y and y < COL_DISPLAY :
			if is_miguel :
				img_file = 'Miguel.png'
			elif not is_miguel and self.world.get_tile(char_x, char_y) != WATER :
				img_file = 'Sara.png'
			else :
				img_file = 'Water_Sara.png'
			bg_image = Background(self.img_path + img_file, [32 * y, 32 * x])
			self.screen.blit(bg_image.image, bg_image.rect)
	
	def draw_characters(self, cur_x, cur_y, location_miguel, location_sara) :
		rows, cols = self.world.get_size()
		x_miguel = location_miguel[0] - cur_x + 3
		y_miguel = location_miguel[1] - cur_y + 3
		if 0 <= x_miguel and x_miguel < ROW_DISPLAY and 0 <= y_miguel and y_miguel < COL_DISPLAY :
			img_file = 'Miguel.png'
			bg_image = Background(self.img_path + img_file, [32 * y_miguel, 32 * x_miguel])
			self.screen.blit(bg_image.image, bg_image.rect)
		
		x_sara = location_sara[0] - cur_x + 3
		y_sara = location_sara[1] - cur_y + 3
		if 0 <= x_sara and x_sara < ROW_DISPLAY and 0 <= y_sara and y_sara < COL_DISPLAY :
			if self.world.get_tile(location_sara[0], location_sara[1]) != WATER :
				img_file = 'Sara.png'
			else :
				img_file = 'Water_Sara.png'
			bg_image = Background(self.img_path + img_file, [32 * y_sara, 32 * x_sara])
			self.screen.blit(bg_image.image, bg_image.rect)
			
		
