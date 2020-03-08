#!/usr/bin/env python

import xlrd
import utils

class sheet_column:
	def __init__(self, col):
		self.col = col
		self.type = ''
		self.name = ''

	def set_type(self, type):
		self.type = type

	def set_name(self, name):
		self.name = name

class sheet:
	def __init__(self, work_sheet):
		self.name = work_sheet.name
		self.key_type = ''
		self.key_name = ''
		self.columns = {}
		self.row_datas = {}
		self.__parse(work_sheet)

	def __parse(self, work_sheet):
		for row in range(work_sheet.nrows):
			if row == 0:
				for col in range(work_sheet.ncols):
					cell = work_sheet.cell(row, col)
					if utils.check_cell_empty(cell):
						if col == 0:
							raise Exception('exception in sheet.parse(): column-type cell is empty in sheet %s' % work_sheet.name)
						else:
							continue
					column_type = utils.get_cell_string(cell)
					if column_type == '':
						raise Exception('exception in sheet.parse(): column-type cell format error in sheet %s' % work_sheet.name)
					if col == 0:
						if column_type == 'string' or column_type == 'int':
							self.key_type = column_type
						else:
							raise Exception('exception in sheet.parse(): key-type must be string or int in sheet %s' % work_sheet.name)
					column_item = self.__create_column(col)
					column_item.set_type(column_type)
			elif row == 1:
				for col in range(work_sheet.ncols):
					if not self.columns.has_key(col):
						continue
					cell = work_sheet.cell(row, col)
					if utils.check_cell_empty(cell):
						raise Exception('exception in sheet.parse(): column-name cell is empty in sheet %s' % work_sheet.name)
					column_name = utils.get_cell_string(cell)
					if column_name == '':
						raise Exception('exception in sheet.parse(): column-name cell format error in sheet %s' % work_sheet.name)
					if col == 0:
						self.key_name = column_name
					column_item = self.columns[col]
					column_item.set_name(column_name)
			elif row == 2:
				continue
			else:
				row_key = None
				row_values = {}
				cur_col = 0
				for col, column_item in self.columns.iteritems():
						cell_value = utils.get_cell_value(work_sheet.cell(row, col), column_item.type)
						row_values[column_item.name] = cell_value
						if cur_col == 0:
							row_key = cell_value
						cur_col += 1
				self.row_datas[row_key] = row_values

	def __create_column(self, col):
		column_item = sheet_column(col)
		self.columns[col] = column_item
		return column_item

	def debug_print(self):
		print('--------------------------')
		print('sheet name: %s' % self.name)
		print('sheet key-type: %s' % self.key_type)
		print('sheet key-name: %s' % self.key_name)
		print('sheet columns: %s' % len(self.columns))
		print('sheet rows: %s' % len(self.row_datas))
		print('--------------------------')
