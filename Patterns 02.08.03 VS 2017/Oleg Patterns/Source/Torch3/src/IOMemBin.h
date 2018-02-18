#pragma once
#include "IOSequence.h"
#include "MemoryXFile.h"

namespace Torch {
class IOMemBin :
	public IOSequence
{
public:	
	IOMemBin(MemoryXFile* pMemXFile, bool one_file_is_one_sequence_, int max_load_, bool is_sequential_ = true);
	virtual ~IOMemBin(void);
  protected:
    MemoryXFile *m_pFile;
    int current_frame_index;

  public:
    bool one_file_is_one_sequence;
    int n_total_frames;    
    int max_load;
    bool is_sequential;

    /** Reads the sequence contained in #filename#.
        If #one_file_is_one_sequence# is false, #getSequence()# will return one sequence
        with one frame at each call. (If calling #getSequence(t, foo)#,
        it will put in the sequence #foo# the frame corresponding to the line #t# of the file).
        Note also that if #one_file_is_one_sequence# is false, the access to the IO must be
        sequential when calling #getSequence()# if #is_sequential# is true. (Sequential mode
        is faster).
        If #max_load_# is positive, it loads only the first #max_load_# frames,
        if #one_file_is_one_sequence# is false.
        The file will be opened when reading the first sequence, and closed when reading the
        last one if #is_sequential# is true. Otherwise, the file will be opened and closed
        each time you call #getSequence()#.
     */
    

    /// Saves #sequence# in #file# using the binary format.
    static void saveSequence(XFile *file, Sequence *sequence);

    virtual void getSequence(int t, Sequence *sequence);
    virtual int getNumberOfFrames(int t);
    virtual int getTotalNumberOfFrames();

};

}

