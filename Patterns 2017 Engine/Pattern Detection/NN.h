/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection FILE: Neural Net Copyright
* LMS Medical Systems 2004 by Evonium Inc.
*/
#pragma once
#include "VectorMatrix.h"

// Used to find pos of sub string in Expert String resource
#include <string>

// Namespace for string operations
using namespace std;

namespace patterns
{

	//
	// =======================================================================================================================
	//    Uncomment these lines if you want to use tables from resources You must add the following to the .rc2 file in your
	//    project: Expert01 NeuralNet ".\\Expert01.txt" Expert02 NeuralNet ".\\Expert01.txt" ;
	//    v5h_NN_Topology.txt Topology ".\\v5h_NN_Topology.txt" v5i_NN_Topology.txt Topology ".\\v5i_NN_Topology.txt"
	//    arbiter_v5i_1x1_RR0.txt NeuralNet ".\\arbiter_v5i_1x1_RR0.txt" all_v5i\b_expert_number_1.txt NeuralNet
	//    ".\\all_v5i\\b_expert_number_1.txt" all_v5i\b_expert_number_2.txt NeuralNet ".\\all_v5i\\b_expert_number_2.txt"
	//    all_v5i\b_expert_number_3.txt NeuralNet ".\\all_v5i\\b_expert_number_3.txt" all_v5i\n_expert_number_1.txt NeuralNet
	//    ".\\all_v5i\\n_expert_number_1.txt" all_v5i\n_expert_number_2.txt NeuralNet ".\\all_v5i\\n_expert_number_2.txt"
	//    all_v5i\n_expert_number_3.txt NeuralNet ".\\all_v5i\\n_expert_number_3.txt" arbiter_v5h_1x1_RR0.txt NeuralNet
	//    ".\\arbiter_v5h_1x1_RR0.txt" all_v5h\b_expert_number_1.txt NeuralNet ".\\all_v5h\\b_expert_number_1.txt"
	//    all_v5h\b_expert_number_2.txt NeuralNet ".\\all_v5h\\b_expert_number_2.txt" all_v5h\b_expert_number_3.txt NeuralNet
	//    ".\\all_v5h\\b_expert_number_3.txt" all_v5h\n_expert_number_1.txt NeuralNet ".\\all_v5h\\n_expert_number_1.txt"
	//    all_v5h\n_expert_number_2.txt NeuralNet ".\\all_v5h\\n_expert_number_2.txt" all_v5h\n_expert_number_3.txt NeuralNet
	//    ".\\all_v5h\\n_expert_number_3.txt" ;
	//    ! CLASS: NeuralNet DESCRIPTION: Class running a neural network.
	// =======================================================================================================================
	//
	class NeuralNet
	{
		// ! Supported tranfer functions.
		enum TransferFunctions { eHardlim = 1, eHardlims, eLogsig, ePoslin, ePurelin, eRadbas, eSatlin, eSatlins, eTansig };

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		//
		// ===============================================================================================================
		//    ! NeuralNet::<default constructor>
		// ===============================================================================================================
		//
		NeuralNet(void)
		{
			m_nLayers = 0;
			m_W = NULL;
			m_bias = NULL;
			m_transfer = NULL;
		}

		//
		// ===============================================================================================================
		//    ! NeuralNet::<copy-constructor>
		// ===============================================================================================================
		//
		NeuralNet(const NeuralNet &)
		{
		}

		//
		// ===============================================================================================================
		//    ! NeuralNet::<destructor>
		// ===============================================================================================================
		//
		virtual~NeuralNet(void)
		{
			Clear();
		}

		int m_nLayers;
		PatternsMatrix* m_W;
		PatternsVector* m_bias;
		TransferFunctions *m_transfer;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:

		//
		// ===============================================================================================================
		//    These are the neural net transfer functions These are the possible function that define a neuron. They all return
		//    the transformed input value.
		// ===============================================================================================================
		//
		double hardlim(double n)
		{
			if (n >= 0.0)
			{
				return 1.0;
			}
			else
			{
				return 0.0;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double hardlims(double n)
		{
			if (n >= 0.0)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double logsig(double n)
		{
			return 1.0 / (1.0 + exp(-1.0 * n));
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double poslin(double n)
		{
			if (n >= 0)
			{
				return n;
			}
			else
			{
				return 0.0;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double purelin(double n)
		{
			return n;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double radbas(double n)
		{
			return exp(-1.0 * pow(n, 2.0));
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double satlin(double n)
		{
			if (n > 0.0 && n <= 1.0)
			{
				return n;
			}
			else if (n < 0.0)
			{
				return 0.0;
			}
			else
			{
				return 1.0;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double satlins(double n)
		{
			if (n >= -1.0 && n <= 1.0)
			{
				return n;
			}
			else if (n < -1.0)
			{
				return -1.0;
			}
			else
			{
				return 1.0;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double tansig(double n)
		{
			return 2.0 / (1.0 + exp(-2.0 * n)) - 1.0;
		};

		// Neural Net tranfer Functions that can be apply to an entire vector
		PatternsVector hardlim(PatternsVector& a);
		PatternsVector hardlims(PatternsVector& a);
		PatternsVector logsig(PatternsVector& a);
		PatternsVector poslin(PatternsVector& a);
		PatternsVector purelin(PatternsVector& a);
		PatternsVector radbas(PatternsVector& a);
		PatternsVector satlin(PatternsVector& a);
		PatternsVector satlins(PatternsVector& a);
		PatternsVector tansig(PatternsVector& a);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		// Base methods Validation functions
		long getExpectedInputSize(void);
		long getExpectedOutputSize(void);

		// How to use...
		bool LoadFromResource(const char* qpszModule, const char *qpszName, const char *qpszType);
		bool LoadFromFile(string FileName);

		bool Clear(void);
		PatternsVector Simulate(const PatternsVector& input, int nLayer = 0);
		void debug(void) const; // print entries on a single line

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		bool Parse(string strExpert);
	};
}