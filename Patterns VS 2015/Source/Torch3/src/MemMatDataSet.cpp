#include "MemMatDataSet.h"
#include "IOBufferize.h"
#include "IOSub.h"
#include "IOMemBin.h"

namespace Torch {

MemMatDataSet::MemMatDataSet(MemoryXFile* file, int n_inputs_, int n_targets_,
                       bool one_file_is_one_sequence, int max_load)
{
	io_allocator = new Allocator;
  if( (n_inputs_ < 0) && (n_targets < 0) )
    error("MatDataSet: cannot guess n_inputs <and> n_targets!");

  IOSequence *io_file = new(io_allocator) IOMemBin(file, one_file_is_one_sequence, max_load);   

  init_(io_file, n_inputs_, n_targets_);
}


MemMatDataSet::~MemMatDataSet(void)
{
}



void MemMatDataSet::init_(IOSequence *io_file, int n_inputs_, int n_targets_)
{
  IOSequence *io_inputs = NULL;
  IOSequence *io_targets = NULL;

  if( (n_inputs_ > io_file->frame_size) || (n_targets_ > io_file->frame_size) )
    error("MatDataSet: n_inputs (%d) or n_targets (%d) too large (> %d) !", n_inputs_, n_targets_, io_file->frame_size);

  if(n_inputs_ < 0)
    n_inputs_ = io_file->frame_size - n_targets_;

  if(n_targets_ < 0)
    n_targets_ = io_file->frame_size - n_inputs_;

  if(io_file->frame_size != (n_inputs_ + n_targets_))
    error("MatDataSet: %d columns in the file != %d inputs + %d targets", io_file->frame_size, n_inputs_, n_targets_);

  IOBufferize *io_buffer = NULL;
  if( (n_inputs_ > 0) && (n_targets_ > 0) )
    io_buffer = new(io_allocator) IOBufferize(io_file);

  if(n_inputs_ > 0)
  {
    if(n_targets_ > 0)
      io_inputs = new(io_allocator) IOSub(io_buffer, 0, n_inputs_);
    else
      io_inputs = io_file;
  }
  if(n_targets_ > 0)
  {
    if(n_inputs_ > 0)
      io_targets = new(io_allocator) IOSub(io_buffer, n_inputs_, n_targets_);
    else
      io_targets = io_file;
  }

  MemoryDataSet::init(io_inputs, io_targets);
  message("MatDataSet: %d examples loaded", n_examples);
  delete io_allocator;
}

}