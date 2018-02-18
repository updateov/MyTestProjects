// PatternsEngineBridge.h

#pragma once

using namespace System;
using namespace System::Text;
using namespace std;

namespace PatternsEngineBridge
{
	public ref class EngineBridge
	{
	public:
		EngineBridge();
		void AddEpisode(String^ visitKey);
		void RemoveEpisode(String^ visitKey);
		void RemoveAllEpisodes();
		// PAW 2017112
		//int EngineProcessData(String^ visitKey, array<byte>^ ups, int upPosition, int upBlockSize, array<byte>^ hrs, int hrPosition, int hrBlockSize);
		int EngineProcessData(String^ visitKey, cli::array<byte>^ ups, int upPosition, int upBlockSize, cli::array<byte>^ hrs, int hrPosition, int hrBlockSize);
		bool EngineReadResults(String^ visitKey, StringBuilder^% data, int buffer_size);

	public:
		// PAW 2017112
		//property array<String^>^ ActiveVisits
		property cli::array<String^>^ ActiveVisits
		{
			//array<String^>^ get();
			cli::array<String^>^ get();
		}
	};
}
