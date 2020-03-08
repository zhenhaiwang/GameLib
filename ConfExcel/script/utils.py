#!/usr/bin/env python

import os
import xlrd

def bkdr_hash(string, seed):
	hash = 0
	for char in string:
		hash = (hash * seed + ord(char)) & 0x7FFFFFFF
	return hash & 0x7FFFFFFF

def get_all_xlsx_paths(directory):
	xlsx_dict = {}
	for fp in os.listdir(directory):
		if os.path.isdir(fp):
			continue
		fp_name, fp_extension = os.path.splitext(fp)
		if not fp_name.startswith('ce_') or fp_extension != '.xlsx':
			continue
		xlsx_dict[fp_name] = os.path.join(directory, fp)
	return xlsx_dict

def get_all_worksheets(xlsx_path):
	sheet_dict = {}
	work_book = xlrd.open_workbook(xlsx_path)
	work_sheets = work_book.sheets()
	for st in work_sheets:
		if st.name.startswith('CE') and st.name != 'CE':
			sheet_dict[st.name] = st
	return sheet_dict

def save_file(file_path, file_content):
	if os.path.exists(file_path):
		os.remove(file_path)
	f = file(file_path, 'w')
	f.writelines(file_content)
	f.close()

def remove_all_files(directory):
	if not os.path.exists(directory):
		os.makedirs(directory)
	for root, dirs, files in os.walk(directory):
		for file in files:
			if not file.endswith('.meta'):
				os.remove(os.path.join(root, file))

def get_convert_string(type, key):
	if type == 'int':
		return '        %s = CEConvertHelper.O2I(ht["%s"]);\n' % (key, key)
	elif type == 'string':
		return '        %s = CEConvertHelper.O2STrim(ht["%s"]);\n' % (key, key)
	elif type == 'float':
		return '        %s = CEConvertHelper.O2F(ht["%s"]);\n' % (key, key)
	else:
		return ''

def check_cell_empty(cell):
	return cell == None or cell.value == None or cell.ctype == 0

def get_cell_string(cell):
    if check_cell_empty(cell):
    	return ''
    elif cell.ctype in (2,3):
    	float_value = float(cell.value)
        int_value = int(float_value)
        if float_value > float(int_value):
            return str(float(cell.value)) + ''
        else:
            return str(int(float(cell.value))) + ''
    else:
       	return str(cell.value) + ''

def get_cell_int(cell):
    if not check_cell_empty(cell):
        try:
            return int(cell.value)
        except Exception as e:
        	raise Exception('exception in get_cell_int(): cell value is %s' % cell.value)
    return 0

def get_cell_float(cell):
    if not check_cell_empty(cell) and len(str(cell.value)) > 0:
        try:
        	return float(cell.value)
        except Exception as e:
        	raise Exception('exception in get_cell_float(): cell value is %s' % cell.value)
    return 0.0

def get_cell_value(cell, type):
	if type == 'string':
		return get_cell_string(cell)
	elif type == 'int':
		return get_cell_int(cell)
	elif type == 'float':
		return get_cell_float(cell)
	else:
		return cell.value