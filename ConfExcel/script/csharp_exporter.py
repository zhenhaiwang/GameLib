#!/usr/bin/env python

import os
import sys
import xlrd
import json
import utils
from sheet import *

reload(sys)
sys.setdefaultencoding('utf-8')

# bkdr
BKDR_SEED = 123

# excel dir
EXCEL_DIR = '../excel'

# export dir
JSON_DIR = '../../Assets/Resources/CEJson'
CSHARP_DIR = '../../Assets/Scripts/CE/AutoGen'

# data
all_xlsx = {}
all_sheet = {}

def export_csharp(sheet_item):
	code = \
'''//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using System.Text;
using System.Collections;
using System.Collections.Generic;
using CE;
'''
	code += \
'''
public sealed class %s : ICELoader
{
''' % sheet_item.name
	code += '    public static readonly string CEName = "%s";\n\n' % sheet_item.name
	for col, sheet_column in sheet_item.columns.iteritems():
		code += '    public %s %s { get; private set; }\n' % (sheet_column.type, sheet_column.name)
	code += \
'''
    public void Load(Hashtable ht)
    {
'''
	for col, sheet_column in sheet_item.columns.iteritems():
		code += utils.get_convert_string(sheet_column.type, sheet_column.name)
	code += '    }\n'
	if sheet_item.key_type == 'string':
		code += \
'''
    public static %s GetElement(string elementKey)
    {
        return CEManager.instance.GetElementString(CEName, elementKey) as %s;
    }

    public static Dictionary<string, ICELoader> GetElementDict()
    {
        return CEManager.instance.GetDictString(CEName);
    }
''' % (sheet_item.name, sheet_item.name)
	elif sheet_item.key_type == 'int':
		code += \
'''
    public static %s GetElement(int elementKey)
    {
        return CEManager.instance.GetElementInt(CEName, elementKey) as %s;
    }

    public static Dictionary<int, ICELoader> GetElementDict()
    {
        return CEManager.instance.GetDictInt(CEName);
    }
''' % (sheet_item.name, sheet_item.name)
	code += \
'''
    public %s Clone()
    {
        var clone = new %s();
''' % (sheet_item.name, sheet_item.name)
	for col, sheet_column in sheet_item.columns.iteritems():
		code += '        clone.%s = %s;\n' % (sheet_column.name, sheet_column.name)
	code += \
'''        return clone;
    }
'''
	code += \
'''
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CEName).Append("->");
        sb.AppendLine();
'''
	for col, sheet_column in sheet_item.columns.iteritems():
		code += '        sb.Append("%s: ").Append(%s);\n        sb.AppendLine();\n' % (sheet_column.name, sheet_column.name)
	code += \
'''        return sb.ToString();
    }
}
'''
	utils.save_file(os.path.join(CSHARP_DIR, sheet_item.name + '.cs'), code)

def export_hash_helper():
    code = \
'''//////////////////////////////////////////////////////////////////////////
/// This is an auto-generated script, please do not modify it manually ///
//////////////////////////////////////////////////////////////////////////

using CE;
'''
    code += \
'''
public static class CEHashHelper
{
    public static ICELoader CreateLoaderFromHash(uint hash)
    {
        ICELoader loader = null;

        switch (hash)
        {
'''
    for sheet_name, sheet_item in all_sheet.iteritems():
        code += \
'''            case %s:
                {
                    loader = new %s();
                }
                break;
''' % (utils.bkdr_hash(sheet_name, BKDR_SEED), sheet_name)
    code += \
'''        }

        return loader;
    }
}
'''
    utils.save_file(os.path.join(CSHARP_DIR, 'CEHashHelper.cs'), code)

def export_json(sheet_item):
    if len(sheet_item.row_datas) == 0:
        return
    if not os.path.exists(JSON_DIR):
        os.mkdir(JSON_DIR)
    json_file = file(os.path.join(JSON_DIR, "%s.txt" % (sheet_item.name)), 'w')
    json.dump(sheet_item.row_datas, json_file, indent = 2, sort_keys = True, ensure_ascii = False)
    json_file.close()

# main
if __name__ == '__main__':

    print('\nConfExcel export start!\n')

    all_xlsx = utils.get_all_xlsx_paths(EXCEL_DIR)
    for xlsx_name, xlsx_path in all_xlsx.iteritems():
        work_sheets = utils.get_all_worksheets(xlsx_path)
        for sheet_name, work_sheet in work_sheets.iteritems():
            sheet_item = sheet(work_sheet)
            sheet_item.debug_print()
            all_sheet[sheet_name] = sheet_item

    generate_code = 1
    xlsx_file = ''

    if len(sys.argv) >= 2:
        generate_code = int(sys.argv[1])
    if len(sys.argv) >= 3:
        xlsx_file = os.path.basename(sys.argv[2])

    if xlsx_file == '':
    	utils.remove_all_files(JSON_DIR)
    	utils.remove_all_files(CSHARP_DIR)
        for sheet_name, sheet_item in all_sheet.iteritems():
            if generate_code == 1:
                export_csharp(sheet_item)
            export_json(sheet_item)
    else:
        if all_xlsx.has_key(xlsx_file):
            work_sheets = utils.get_all_worksheets(all_xlsx[xlsx_file])
            for sheet_name, work_sheet in work_sheets.iteritems():
                sheet_item = all_sheet[sheet_name]
                if generate_code == 1:
                    export_csharp(sheet_item)
                export_json(sheet_item)

    export_hash_helper()

    print('\nConfExcel export finish!\n')
