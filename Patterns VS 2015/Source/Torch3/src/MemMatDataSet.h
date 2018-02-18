#pragma once
#include "MemoryDataSet.h"
#include "IOSequence.h"
#include "MemoryXFile.h"

namespace Torch {

class MemMatDataSet :
	public MemoryDataSet
{
public:
	MemMatDataSet(MemoryXFile* file, int n_inputs_, int n_targets_,
                       bool one_file_is_one_sequence, int max_load);
	virtual ~MemMatDataSet(void);
private:
    void init_(IOSequence *io_file, int n_inputs_, int n_targets_);
	Allocator *io_allocator;
public:

};

}
