#pragma once
#include "DiskXFile.h"
#include "MemCmdLine.h"
#include "Sequence.h"
#include "IOSequenceArray.h"


using namespace Torch;

namespace patterns
{
class SVMModel : public Object
{
	friend class SVMTest;
public:
	SVMModel(const char* modelName, int n_inputs = 6, int n_targets = 1, bool norm_inputs = true, bool norm_targets = false);
	virtual ~SVMModel(void);

protected:

	DiskXFile* m_pModelXFile;
	MemCmdLine m_cmd;
public:
	bool Init();
	bool LoadMeanVarNormPrams();
	bool LoadSVMParams();
protected:
	bool InitCmdLine();
private:
	void SetZero();
protected:
	char* m_modelName;
	bool m_initialized;
	bool m_valid;
	    /// Input frame size
    int m_numInputs;

    /// Target frame size
    int m_numTargets;
	//////MeanVarNormPrams
	bool m_normInputs;
	bool m_normTargets;
    /// Inputs means array
    real *m_inputsMean;

    /// Targets means array
    real *m_targetsMean;

    /// Inputs standard deviations array
    real *m_inputsStdv;

    /// Targets standard deviations array
    real *m_targetsStdv;

    ///
	////////////SVMParams:
	real b;
	int n_support_vectors;
	int n_support_vectors_bound;
	real* sv_alpha;
	Sequence** sv_sequences;
	int* support_vectors;
	IOSequenceArray* io_sequence_array;
	Allocator* sv_allocator;
protected:
	// dummy cmd line arguments
  char *file;
  real c_cst, stdv, eps_regression;
  bool regression;
  real accuracy, cache_size;
  int iter_shrink, k_fold;
  int the_seed;
  int max_load;
  char *dir_name;
  char *model_file;
  bool binary_mode;
  int class_against_the_others;
  int degree;
  real a_cst, b_cst;
  bool normalize;
  // PAW
  bool exampleReport;
  bool confusion;

   // PAW: per-class c value (for 2 classes only)
  real c1_cst, c2_cst;
  // PAW: per-class c value (for 2 classes only)

};
}

