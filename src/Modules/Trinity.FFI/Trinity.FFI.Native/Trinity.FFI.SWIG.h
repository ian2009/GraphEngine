#pragma once

#include <vector>
#include "Trinity.FFI.Native.h"

extern TRINITY_INTERFACES* g_TrinityInterfaces;

void Init()
{
    g_TrinityInterfaces = TRINITY_FFI_GET_INTERFACES();
}

class Cell
{
public:
	Cell(TCell cell) : m_cell(cell) {}
	TCell m_cell;
	
	char* GetField(char* field)
	{
		return g_TrinityInterfaces->cell_getfield(m_cell, field);
	}

	//TODO errno
	void SetField(char* field, char* content)
	{
		g_TrinityInterfaces->cell_setfield(m_cell, field, content);
	}

	void AppendField(char* field, char* content)
	{
		g_TrinityInterfaces->cell_appendfield(m_cell, field, content);
	}

	void RemoveField(char* field)
	{
		g_TrinityInterfaces->cell_removefield(m_cell, field);
	}

	int HasField(char* field)
	{
		return g_TrinityInterfaces->cell_hasfield(m_cell, field);
	}

	~Cell()
	{
		printf("hey dtor\n");
		g_TrinityInterfaces->cell_dispose(m_cell);
	}

	std::vector<char*> GetFieldNames()
	{
		std::vector<char*> vec;
		do
		{
			TEnumerator etor;
			TFieldInfo fi = NULL;
			char* val = NULL;
			if (TrinityErrorCode::E_SUCCESS != g_TrinityInterfaces->cell_fieldenum_get(m_cell, &etor)) break;
			while (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->cell_fieldenum_movenext(etor, fi))
			{
				g_TrinityInterfaces->cell_fieldenum_current(etor, &fi);
				if (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->cell_fieldinfo_name(fi, &val)) {
					vec.push_back(val);
				}
			}
			g_TrinityInterfaces->cell_fieldenum_dispose(etor);
		} while (false);

		return vec;
	}
};

Cell* LoadCell(int64_t cellId)
{
	Cell* pret = new Cell(0);
	if (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->local_loadcell(cellId, &pret->m_cell))
	{
		return pret;
	}
	else
	{
		delete pret;
		return nullptr;
	}
}

bool SaveCell(int64_t cellId, Cell* pcell)
{
	return (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->local_savecell_1(cellId, pcell->m_cell));
}

bool SaveCell2(int64_t cellId, Cell* pcell, CellAccessOptions options)
{
	return (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->local_savecell_2(cellId, options, pcell->m_cell));
}

Cell* NewCell(char* cellType)
{
	Cell* pcell = new Cell(0);
	if (TrinityErrorCode::E_SUCCESS == g_TrinityInterfaces->newcell_1(cellType, &pcell->m_cell))
		return pcell;
	else
	{
		delete pcell;
		return nullptr;
	}
}
