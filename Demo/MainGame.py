import pygame
import random
from random import randint
from Mundo import World
from Mapa import *
from Drawer import Drawer

if __name__ == '__main__' :
	
	seed = 52830
	rows = 17
	cols = 17
	world = World(seed, rows, cols)
	width = 32 * ROW_DISPLAY
	height = 32 * COL_DISPLAY
	
	miguel_x, miguel_y = randint(2, rows - 2), randint(2, cols - 2)
	sara_x, sara_y = randint(2, rows - 2), randint(2, cols - 2)
	maps = [Miguel_map(rows, cols,miguel_x, miguel_y, world), Sara_map(rows, cols, sara_x, sara_y, world)]
	turn = 1
	turn_counter = 0
	moves = 0
	
	pygame.init()
	size = (width, height)
	screen = pygame.display.set_mode(size)
	drawer = Drawer(screen, world)
	
	clock = pygame.time.Clock()
	game_over = False
	
	while not game_over:
		for event in pygame.event.get():
			if event.type == pygame.QUIT:
				gameOver = True
			if event.type == pygame.MOUSEBUTTONUP:
				pos = pygame.mouse.get_pos()
				x_click = cur_x + pos[1] // 32 - 3
				y_click = cur_y + pos[0] // 32 - 3
				
				if maps[turn].set_char_position(x_click, y_click) :
					moves -= 1
					turn_counter += 1
					
			if event.type == pygame.KEYDOWN :
				if event.key == pygame.K_LEFT :
					cur_y = (cur_y + 1) % cols
				if event.key == pygame.K_RIGHT :
					cur_y = (cur_y - 1 + cols) % cols
				if event.key == pygame.K_UP :
					cur_x = (cur_x + 1) % rows
				if event.key == pygame.K_DOWN :
					cur_x = (cur_x - 1 + rows) % rows
		#print('Miguel:', maps[0].get_char_position())
		#print('Sara:', maps[1].get_char_position())
		if moves <= 0 :
			turn = (turn + 1) % 2
			cur_x, cur_y = maps[turn].get_char_position()
			char_x, char_y = cur_x, cur_y
			if turn == 0:
				pygame.display.set_caption("Turno de Miguel")
			else :
				pygame.display.set_caption("Turno de Sara")
			moves = 2
		drawer.draw_world(maps[turn], (cur_x - 3 + rows) % rows, (cur_y - 3 + cols) % cols)
		if maps[turn].is_visited(maps[(turn + 1) % 2].get_char_position()[0], maps[(turn + 1) % 2].get_char_position()[1]) :
			drawer.draw_characters(cur_x, cur_y, maps[0].get_char_position(), maps[1].get_char_position())
		else :
			char_x, char_y = maps[turn].get_char_position()
			drawer.draw_character((cur_x - 3 + rows) % rows, (cur_y - 3 + cols) % cols, char_x, char_y, turn == 0)
		pygame.display.flip()
		#clock.tick(5)
		if maps[0].get_char_position() == maps[1].get_char_position() :
			print('Felicidades, haz completado el juego en %s turnos'%(turn_counter))
			game_over = True
	pygame.quit()
