#include "stdafx.h"
#include "SVMModel.h"

const char *torchhelp = "\
SVMTorch III (c) Trebolloc & Co 2002\n\
\n\
This program will train a SVM for classification or regression\n\
with a gaussian kernel (default) or a polynomial kernel.\n\
It supposes that the file you provide contains two classes,\n\
except if you use the '-class' option which trains one class\n\
against the others.\n";

namespace patterns
{
SVMModel::SVMModel(const char* modelName, int n_inputs, int n_targets, bool norm_inputs, bool norm_targets)
{
	SetZero();
	m_modelName = (char *)::malloc(strlen(modelName)+1);
	strcpy(m_modelName, modelName);	

	m_numInputs = n_inputs;
	m_numTargets = n_targets;
	m_normInputs = norm_inputs;
	m_normTargets = norm_targets;
	
	sv_allocator = new Allocator;
	io_sequence_array = new(allocator) IOSequenceArray;
}


SVMModel::~SVMModel(void)
{
	::free(m_modelName);
	delete sv_allocator;
}

bool SVMModel::Init()
{
	if(!m_initialized)
	{
		CFileFind finder;
		bool found = finder.FindFile(m_modelName);
		finder.Close();
		if(!found)
		{			
			return false;
		}
		
		clock_t start = clock();
		if(InitCmdLine())
		{
			m_pModelXFile = new(allocator) DiskXFile(m_modelName, "r");
			m_cmd.loadXFile(m_pModelXFile);
			if(normalize)
			{
				if(!LoadMeanVarNormPrams())
					return false;
			}
			if(!LoadSVMParams())
				return false;

			m_valid = true;
		}
		m_initialized = true;
		clock_t end = clock();
		long duration = end - start;
		print("init dur = %d\n", duration);
	}
	return m_initialized && m_valid;
}

void SVMModel::SetZero()
{
	m_valid = false;
	m_modelName = "";
	m_initialized = false;

	m_numInputs = 0;
	m_numTargets = 0;
	m_pModelXFile = 0;

	m_normInputs = false;
	m_normTargets = false;
	m_inputsMean = NULL;
	m_targetsMean = NULL;
	m_inputsStdv = NULL;
	m_targetsStdv = NULL;

	b = 0;
	n_support_vectors = 0;
	n_support_vectors_bound = 0;
	sv_alpha = NULL;
	sv_sequences = NULL;
	support_vectors = NULL;
	io_sequence_array = NULL;
	sv_allocator = NULL;

	file = "";
	c_cst = 100;
	stdv = 10;
	eps_regression = 0.7;
	regression = false;
	accuracy = 0.01;
	cache_size = 50.;
    iter_shrink = 100;
	k_fold = 0;
    the_seed  = -1;
    max_load = -1;
	dir_name = ".";
	model_file = "";
	binary_mode = false;
	class_against_the_others = -1;
	degree = -1;  
	normalize = false;
	  // PAW
	  exampleReport = false;
	  confusion = false;
	   // PAW: per-class c value (for 2 classes only)
	  c1_cst = c2_cst =-1;

}

bool SVMModel::LoadMeanVarNormPrams()
{
	try
	{
	  if(m_normInputs)
	  {
		m_inputsMean = (real *)allocator->alloc(sizeof(real)*m_numInputs);
		m_inputsStdv = (real *)allocator->alloc(sizeof(real)*m_numInputs);
		for(int i = 0; i < m_numInputs; i++)
		{
		  m_inputsMean[i] = 0;
		  m_inputsStdv[i] = 0;
		}
	  }

	  if(m_normTargets)
	  {
		m_targetsMean = (real *)allocator->alloc(sizeof(real)*m_numTargets);
		m_targetsStdv = (real *)allocator->alloc(sizeof(real)*m_numTargets);
		for(int i = 0; i < m_numTargets; i++)
		{
		  m_targetsMean[i] = 0;
		  m_targetsStdv[i] = 0;
		}
	  }

	  if(m_inputsMean)
	  {
		m_pModelXFile->taggedRead(m_inputsMean, sizeof(real), m_numInputs, "IMEANS");
		m_pModelXFile->taggedRead(m_inputsStdv, sizeof(real), m_numInputs, "ISTDVS");
	  }
	  if(m_targetsMean)
	  {
		m_pModelXFile->taggedRead(m_targetsMean, sizeof(real), m_numTargets, "TMEANS");
		m_pModelXFile->taggedRead(m_targetsStdv, sizeof(real), m_numTargets, "TSTDVS");
	  }
	  return true;
	}
	catch(...)
	{
	}
	return false;
}

bool SVMModel::LoadSVMParams()
{
	try
	{
	  m_pModelXFile->taggedRead(&b, sizeof(real), 1, "b");
	  m_pModelXFile->taggedRead(&n_support_vectors, sizeof(int), 1, "NSV");
	  m_pModelXFile->taggedRead(&n_support_vectors_bound, sizeof(int), 1, "NSVB");

	  sv_allocator->freeAll();
	  if(n_support_vectors > 0)
	  {
		sv_alpha = (real *)sv_allocator->alloc(sizeof(real)*n_support_vectors);
		m_pModelXFile->taggedRead(sv_alpha, sizeof(real), n_support_vectors, "SVALPHA");
    
		sv_sequences = (Sequence **)sv_allocator->alloc(sizeof(Sequence *)*n_support_vectors);
		io_sequence_array->read(m_pModelXFile, sv_sequences, n_support_vectors, sv_allocator);
		// PAW
		support_vectors = (int *)sv_allocator->alloc(sizeof(int)*n_support_vectors);
		m_pModelXFile->taggedRead(support_vectors, sizeof(int), n_support_vectors, "SUPPORT_VECTORS");
	  }
	  else
	  {
		sv_alpha = NULL;
		sv_sequences = NULL;
	  }
	  return true;
	}
	catch(...)
	{
	}
	return false;
}

bool SVMModel::InitCmdLine()
{

  // Put the help line at the beginning
  m_cmd.info(torchhelp);

  // Train mode
  m_cmd.addText("\nArguments:");
  m_cmd.addSCmdArg("file", &file, "the train file");
  m_cmd.addSCmdArg("model", &model_file, "the model file");

  m_cmd.addText("\nModel Options:");
  m_cmd.addRCmdOption("-c", &c_cst, 100., "trade off cst between error/margin");
  m_cmd.addRCmdOption("-std", &stdv, 10., "the std parameter in the gaussian kernel [exp(-|x-y|^2/std^2)]", true);

   // PAW: per-class c value (for 2 classes only)
  m_cmd.addRCmdOption("-c1", &c1_cst, -1, "trade off cst between error/margin, class 1");
  m_cmd.addRCmdOption("-c2", &c2_cst, -1, "trade off cst between error/margin, class 2");
  // PAW: per-class c value (for 2 classes only)

  m_cmd.addICmdOption("-degree", &degree, -1, "if positive, use a polynomial kernel [(a xy + b)^d] with the specified degree", true);
  m_cmd.addRCmdOption("-a", &a_cst, 1., "constant a in the polynomial kernel", true);
  m_cmd.addRCmdOption("-b", &b_cst, 1., "constant b in the polynomial kernel", true);

  m_cmd.addBCmdOption("-rm", &regression, false, "regression mode", true);
  m_cmd.addRCmdOption("-eps", &eps_regression, 0.7, "eps tube in regression");
  m_cmd.addICmdOption("-class", &class_against_the_others, -1, "train the given class against the others", true);

  m_cmd.addText("\nLearning Options:");
  m_cmd.addBCmdOption("-norm", &normalize, false, "normalize the data (mean/stdv)?", true);
  m_cmd.addRCmdOption("-e", &accuracy, 0.01, "end accuracy");
  m_cmd.addRCmdOption("-m", &cache_size, 50., "cache size in Mo");
  m_cmd.addICmdOption("-h", &iter_shrink, 100, "minimal number of iterations before shrinking");

  m_cmd.addText("\nMisc Options:");
  m_cmd.addICmdOption("-seed", &the_seed, -1, "the random seed");
  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for train");
  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");

  // KFold mode (one difference with previous mode: no model is available)
  m_cmd.addMasterSwitch("--kfold");
  m_cmd.addText("\nArguments:");
  m_cmd.addSCmdArg("file", &file, "the train file");
  m_cmd.addICmdArg("k", &k_fold, "number of folds");

  m_cmd.addText("\nModel Options:");
  m_cmd.addRCmdOption("-c", &c_cst, 100., "trade off cst between error/margin");
  m_cmd.addRCmdOption("-std", &stdv, 10., "the std parameter in the gaussian kernel", true);

  m_cmd.addICmdOption("-degree", &degree, -1, "if positive, use a polynomial kernel [(a xy + b)^d] with the specified degree", true);
  m_cmd.addRCmdOption("-a", &a_cst, 1., "constant a in the polynomial kernel", true);
  m_cmd.addRCmdOption("-b", &b_cst, 1., "constant b in the polynomial kernel", true);

  m_cmd.addBCmdOption("-rm", &regression, false, "regression mode", true);
  m_cmd.addRCmdOption("-eps", &eps_regression, 0.7, "eps tube in regression");
  m_cmd.addICmdOption("-class", &class_against_the_others, -1, "train the given class against the others", true);

  m_cmd.addText("\nLearning Options:");
  m_cmd.addBCmdOption("-norm", &normalize, false, "normalize the data (mean/stdv)?", true);
  m_cmd.addRCmdOption("-e", &accuracy, 0.01, "end accuracy");
  m_cmd.addRCmdOption("-m", &cache_size, 50., "cache size in Mo");
  m_cmd.addICmdOption("-h", &iter_shrink, 100, "minimal number of iterations before shrinking");

  m_cmd.addText("\nMisc Options:");
  m_cmd.addICmdOption("-seed", &the_seed, -1, "the random seed");
  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for train");
  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");

  // Test mode
  m_cmd.addMasterSwitch("--test");
  m_cmd.addText("\nArguments:");
  m_cmd.addSCmdArg("model", &model_file, "the model file");
  m_cmd.addSCmdArg("file", &file, "the test file");

  m_cmd.addText("\nMisc Options:");
  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for test");
  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");

   // PAW
  m_cmd.addBCmdOption("-exampleReport", &exampleReport, false, "produce classification report per example?", true);
  m_cmd.addBCmdOption("-confusion", &confusion, false, "display confusion matrices?", true);
   
  char* names[] = {"model", "file","-exampleReport"}; 
  
  char* values[] = {m_modelName, "", ""};
  
  int mode = 2; // test mode
   //Read the command line
  bool ok = m_cmd.LoadParameters(names, values, 3, mode);

 
  
  return ok;
}

}