#include "stdafx.h"
#include "SVMTest.h"
#include "ClassFormatDataSet.h"
#include "TwoClassFormat.h"
#include "ClassExampleMeasurer.h"
#include "SVM.h"
#include "kernel.h"
#include "SVMClassification.h"
#include "QCTrainer.h"
#include "MeanVarNorm.h"
#include "MemMatDataSet.h"


namespace patterns
{
SVMTest::SVMTest()
{	
	
	m_numExamples = 0;
}


SVMTest::~SVMTest(void)
{
}

bool SVMTest::Run(SVMInput& input, SVMModel* pModel)
{
	try
	{
		clock_t start = clock();
	
		MeanVarNorm *mv_norm = NULL;
		DataSet *data = NULL;
		m_numExamples = input.GetNumber();

		SVM *svm = NULL;
		Kernel *kernel = NULL;
  
		kernel = new(allocator) GaussianKernel(1./(pModel->stdv*pModel->stdv));
 
		svm = new(allocator) SVMClassification(kernel);
		input.GetXFile()->rewind();
		if(pModel->regression)
		{
			data = new(allocator) MemMatDataSet(input.GetXFile(), -1, 1, false, -1);
			if(pModel->normalize)
			{
				mv_norm = new(allocator) MeanVarNorm(data, true, false, true);
				mv_norm->SetXFileParams(pModel->m_numInputs, pModel->m_numTargets, pModel->m_inputsMean, pModel->m_inputsStdv, pModel->m_targetsMean, pModel->m_targetsStdv);
				data->preProcess(mv_norm);
			}
		}
		else
		{
			MemMatDataSet* pMatDataSet = new(allocator) MemMatDataSet(input.GetXFile(), -1, 1, false, -1);
			if(pModel->normalize)
			{
				mv_norm = new(allocator) MeanVarNorm(pMatDataSet, true, false,true);
				mv_norm->SetXFileParams(pModel->m_numInputs, pModel->m_numTargets, pModel->m_inputsMean, pModel->m_inputsStdv, pModel->m_targetsMean, pModel->m_targetsStdv);
				pMatDataSet->preProcess(mv_norm);
			}

			Sequence *classLabels = NULL;
			if(pModel->class_against_the_others >= 0)
			{
				int n_classes = -1;
				for(int t = 0; t < pMatDataSet->n_examples; t++)
				{
					pMatDataSet->setExample(t);
					int z =  (int)pMatDataSet->targets->frames[0][0];
					if(z > n_classes)
						n_classes = z;
				}
				n_classes++;				

				classLabels = new(allocator) Sequence(n_classes, 1);
				for(int i = 0; i < n_classes; i++)
				{
				if(i == pModel->class_against_the_others)
					classLabels->frames[i][0] =  1;
				else
					classLabels->frames[i][0] = -1;
				}
			}
			else
			{
				classLabels = new(allocator) Sequence(2,1);
				classLabels->frames[0][0] = -1;
				classLabels->frames[1][0] = 1;
			}
			data = new(allocator) ClassFormatDataSet(pMatDataSet, classLabels);
		}

		TwoClassFormat *classFormat = new(allocator) TwoClassFormat(data);
		MeasurerList measurers;
		MemoryXFile* pOutputXFile = new(allocator)MemoryXFile(m_numExamples*sizeof(int)); 
		ClassExampleMeasurer* classMeasurer = new(allocator) ClassExampleMeasurer(svm->outputs, data, classFormat, pOutputXFile);
		classMeasurer->binary_mode = true;
		measurers.addNode(classMeasurer);
		svm->SetXFileParams(pModel->b, pModel->n_support_vectors, pModel->n_support_vectors_bound, 
				pModel->sv_alpha, pModel->sv_sequences, pModel->io_sequence_array, pModel->support_vectors);	

		QCTrainer trainer(svm);
		trainer.test(&measurers);
		pOutputXFile->rewind();

		for(int i = 0; i < m_numExamples; i++)
		{
			int result = -1;
			pOutputXFile->read(&result, sizeof(int), 1);
			m_results.push_back(result);
		}
	
		clock_t end = clock();

		long duration = end - start;
		::print("test dur = %d\n", duration);
		return true;
	}
	catch(...)
	{
		return false;
	}
}

//bool SVMTest::Run(SVMInput& input, const char* modelName)
//{
//	clock_t start = clock();
//	Allocator* allocator = new Allocator;
//	if(InitCmdLine(modelName))
//	{	
//		m_pModel = new(allocator) DiskXFile(model_file, "r");
//		m_cmd.loadXFile(m_pModel);
//		MeanVarNorm *mv_norm = NULL;
//		DataSet *data = NULL;
//		m_numExamples = input.GetNumber();
//
//		SVM *svm = NULL;
//		Kernel *kernel = NULL;
//  
//		kernel = new(allocator) GaussianKernel(1./(stdv*stdv));
// 
//		svm = new(allocator) SVMClassification(kernel);
//
//		if(regression)
//		{
//			data = new(allocator) MemMatDataSet(input.GetXFile(), -1, 1, false, -1);
//			if(normalize)
//			{
//				mv_norm = new(allocator) MeanVarNorm(data);
//				mv_norm->loadXFile(m_pModel);
//				data->preProcess(mv_norm);
//			}
//		}
//		else
//		{
//			m_pMatDataSet = new(allocator) MemMatDataSet(input.GetXFile(), -1, 1, false, -1);
//			if(normalize)
//			{
//				mv_norm = new(allocator) MeanVarNorm(m_pMatDataSet);
//				mv_norm->loadXFile(m_pModel);
//				m_pMatDataSet->preProcess(mv_norm);
//			}
//
//			Sequence *classLabels = NULL;
//			if(class_against_the_others >= 0)
//			{
//				int n_classes = -1;
//				for(int t = 0; t < m_pMatDataSet->n_examples; t++)
//				{
//					m_pMatDataSet->setExample(t);
//					int z =  (int)m_pMatDataSet->targets->frames[0][0];
//					if(z > n_classes)
//						n_classes = z;
//				}
//				n_classes++;				
//
//				classLabels = new(allocator) Sequence(n_classes, 1);
//				for(int i = 0; i < n_classes; i++)
//				{
//				if(i == class_against_the_others)
//					classLabels->frames[i][0] =  1;
//				else
//					classLabels->frames[i][0] = -1;
//				}
//			}
//			else
//			{
//				classLabels = new(allocator) Sequence(2,1);
//				classLabels->frames[0][0] = -1;
//				classLabels->frames[1][0] = 1;
//			}
//			data = new(allocator) ClassFormatDataSet(m_pMatDataSet, classLabels);
//	  }
//
//		TwoClassFormat *classFormat = new(allocator) TwoClassFormat(data);
//		MeasurerList measurers;
//		MemoryXFile* pOutputXFile = new(allocator)MemoryXFile(m_numExamples*sizeof(int)); 
//		ClassExampleMeasurer* classMeasurer = new(allocator) ClassExampleMeasurer(svm->outputs, data, classFormat, pOutputXFile);
//		classMeasurer->binary_mode = true;
//		measurers.addNode(classMeasurer);
//		svm->loadXFile(m_pModel);		
//		QCTrainer trainer(svm);
//		trainer.test(&measurers);
//
//		for(int i = 0; i < m_numExamples; i++)
//		{
//			int result = -1;
//			pOutputXFile->read(&result, sizeof(int), 1);
//			m_results.push_back(result);
//		}
//	}
//	delete allocator;
//	clock_t end = clock();
//
//	long duration = end - start;
//
//	return true;
//}



//bool SVMTest::InitCmdLine(const char* modelName)
//{
// 
//  // Put the help line at the beginning
//  m_cmd.info(torchhelp);
//
//  // Train mode
//  m_cmd.addText("\nArguments:");
//  m_cmd.addSCmdArg("file", &file, "the train file");
//  m_cmd.addSCmdArg("model", &model_file, "the model file");
//
//  m_cmd.addText("\nModel Options:");
//  m_cmd.addRCmdOption("-c", &c_cst, 100., "trade off cst between error/margin");
//  m_cmd.addRCmdOption("-std", &stdv, 10., "the std parameter in the gaussian kernel [exp(-|x-y|^2/std^2)]", true);
//
//   // PAW: per-class c value (for 2 classes only)
//  m_cmd.addRCmdOption("-c1", &c1_cst, -1, "trade off cst between error/margin, class 1");
//  m_cmd.addRCmdOption("-c2", &c2_cst, -1, "trade off cst between error/margin, class 2");
//  // PAW: per-class c value (for 2 classes only)
//
//  m_cmd.addICmdOption("-degree", &degree, -1, "if positive, use a polynomial kernel [(a xy + b)^d] with the specified degree", true);
//  m_cmd.addRCmdOption("-a", &a_cst, 1., "constant a in the polynomial kernel", true);
//  m_cmd.addRCmdOption("-b", &b_cst, 1., "constant b in the polynomial kernel", true);
//
//  m_cmd.addBCmdOption("-rm", &regression, false, "regression mode", true);
//  m_cmd.addRCmdOption("-eps", &eps_regression, 0.7, "eps tube in regression");
//  m_cmd.addICmdOption("-class", &class_against_the_others, -1, "train the given class against the others", true);
//
//  m_cmd.addText("\nLearning Options:");
//  m_cmd.addBCmdOption("-norm", &normalize, false, "normalize the data (mean/stdv)?", true);
//  m_cmd.addRCmdOption("-e", &accuracy, 0.01, "end accuracy");
//  m_cmd.addRCmdOption("-m", &cache_size, 50., "cache size in Mo");
//  m_cmd.addICmdOption("-h", &iter_shrink, 100, "minimal number of iterations before shrinking");
//
//  m_cmd.addText("\nMisc Options:");
//  m_cmd.addICmdOption("-seed", &the_seed, -1, "the random seed");
//  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for train");
//  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
//  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");
//
//  // KFold mode (one difference with previous mode: no model is available)
//  m_cmd.addMasterSwitch("--kfold");
//  m_cmd.addText("\nArguments:");
//  m_cmd.addSCmdArg("file", &file, "the train file");
//  m_cmd.addICmdArg("k", &k_fold, "number of folds");
//
//  m_cmd.addText("\nModel Options:");
//  m_cmd.addRCmdOption("-c", &c_cst, 100., "trade off cst between error/margin");
//  m_cmd.addRCmdOption("-std", &stdv, 10., "the std parameter in the gaussian kernel", true);
//
//  m_cmd.addICmdOption("-degree", &degree, -1, "if positive, use a polynomial kernel [(a xy + b)^d] with the specified degree", true);
//  m_cmd.addRCmdOption("-a", &a_cst, 1., "constant a in the polynomial kernel", true);
//  m_cmd.addRCmdOption("-b", &b_cst, 1., "constant b in the polynomial kernel", true);
//
//  m_cmd.addBCmdOption("-rm", &regression, false, "regression mode", true);
//  m_cmd.addRCmdOption("-eps", &eps_regression, 0.7, "eps tube in regression");
//  m_cmd.addICmdOption("-class", &class_against_the_others, -1, "train the given class against the others", true);
//
//  m_cmd.addText("\nLearning Options:");
//  m_cmd.addBCmdOption("-norm", &normalize, false, "normalize the data (mean/stdv)?", true);
//  m_cmd.addRCmdOption("-e", &accuracy, 0.01, "end accuracy");
//  m_cmd.addRCmdOption("-m", &cache_size, 50., "cache size in Mo");
//  m_cmd.addICmdOption("-h", &iter_shrink, 100, "minimal number of iterations before shrinking");
//
//  m_cmd.addText("\nMisc Options:");
//  m_cmd.addICmdOption("-seed", &the_seed, -1, "the random seed");
//  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for train");
//  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
//  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");
//
//  // Test mode
//  m_cmd.addMasterSwitch("--test");
//  m_cmd.addText("\nArguments:");
//  m_cmd.addSCmdArg("model", &model_file, "the model file");
//  m_cmd.addSCmdArg("file", &file, "the test file");
//
//  m_cmd.addText("\nMisc Options:");
//  m_cmd.addICmdOption("-load", &max_load, -1, "max number of examples to load for test");
//  m_cmd.addSCmdOption("-dir", &dir_name, ".", "directory to save measures");
//  m_cmd.addBCmdOption("-bin", &binary_mode, false, "binary mode for files");
//
//   // PAW
//  m_cmd.addBCmdOption("-exampleReport", &exampleReport, false, "produce classification report per example?", true);
//  m_cmd.addBCmdOption("-confusion", &confusion, false, "display confusion matrices?", true);
//   
//  char* names[] = {"model", "file","-exampleReport"};
//
//  
//  char *ptr = (char *)::malloc(strlen(modelName)+1);
//  strcpy(ptr, modelName);
//  
//  char* values[] = {ptr, "", ""};
//  
//  int mode = 2; // test mode
//   //Read the command line
//  bool ok = m_cmd.LoadParameters(names, values, 3, mode);
//
//  ::free(ptr);;
//  
//  return ok;
//}

}
