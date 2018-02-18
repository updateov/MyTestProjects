
#include "IOMemBin.h"

namespace Torch {


IOMemBin::IOMemBin(MemoryXFile* pMemXFile, bool one_file_is_one_sequence_, int max_load_, bool is_sequential_)
{
	m_pFile = pMemXFile;
	// Boaf...
	one_file_is_one_sequence = one_file_is_one_sequence_;
	max_load = max_load_;
	is_sequential = is_sequential_;


	m_pFile->read(&n_total_frames, sizeof(int), 1);
	m_pFile->read(&frame_size, sizeof(int), 1);
	m_pFile->rewind();

	if( (max_load > 0) && (max_load < n_total_frames) && (!one_file_is_one_sequence) )
	{
		n_total_frames = max_load;
		message("IOBin: loading only %d frames", n_total_frames);
	}

	if(one_file_is_one_sequence)
		n_sequences = 1;
	else
		n_sequences = n_total_frames;  

	current_frame_index = -1;  

}

IOMemBin::~IOMemBin(void)
{
}

void IOMemBin::saveSequence(XFile *file, Sequence *sequence)
{
  file->write(&sequence->n_frames, sizeof(int), 1);
  file->write(&sequence->frame_size, sizeof(int), 1);

  for(int i = 0; i < sequence->n_frames; i++)
    file->write(sequence->frames[i], sizeof(real), sequence->frame_size);
}



void IOMemBin::getSequence(int t, Sequence *sequence)
{
  // Cas simple: on lit tout le bordel
  if(one_file_is_one_sequence)
  {
    //file = new(allocator) DiskXFile(filename, "r");
    int murielle;
    m_pFile->read(&murielle, sizeof(int), 1); // fseek non car marche pas dans pipes
    m_pFile->read(&murielle, sizeof(int), 1);
    for(int i = 0; i < n_total_frames; i++)
      m_pFile->read(sequence->frames[i], sizeof(real), frame_size);
	m_pFile->rewind();
    //allocator->free(file);
  }
  else
  {
    // Sequentiel ?
    if(is_sequential)
    {
      if(t != current_frame_index+1)
        error("IOBin: sorry, data are accessible only in a sequential way");
      
      // Doit-on ouvrir le putain de fichier ?
      if(current_frame_index < 0)
      {
        //file = new(allocator) DiskXFile(filename, "r");
        int murielle;
        m_pFile->read(&murielle, sizeof(int), 1); // fseek non car marche pas dans pipes
        m_pFile->read(&murielle, sizeof(int), 1);
      }
    }
    else
    {
      //file = new(allocator) DiskXFile(filename, "r");
      if(m_pFile->seek(t*frame_size*sizeof(real)+2*sizeof(int), SEEK_CUR) != 0)
        error("IOBin: cannot seek in your file!");
    }

    // Lis la frame mec
    m_pFile->read(sequence->frames[0], sizeof(real), frame_size);

    if(is_sequential)
    {
      // Si je suis a la fin du fichier, je le zigouille.
      current_frame_index++;
      if(current_frame_index == n_total_frames-1)
      {
        //allocator->free(file);
		 m_pFile->rewind();
        current_frame_index = -1;
      }
    }
    else
		m_pFile->rewind();
    //  allocator->free(file);
  }
}

int IOMemBin::getNumberOfFrames(int t)
{
  if(one_file_is_one_sequence)
    return n_total_frames;
  else
    return 1;
}

int IOMemBin::getTotalNumberOfFrames()
{
  return n_total_frames;
}

}
