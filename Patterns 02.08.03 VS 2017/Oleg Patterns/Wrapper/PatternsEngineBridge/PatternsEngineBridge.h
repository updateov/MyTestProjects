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
		int EngineProcessData(String^ visitKey, cli::array<byte>^ ups, int upPosition, int upBlockSize, cli::array<byte>^ hrs, int hrPosition, int hrBlockSize);
		bool EngineReadResults(String^ visitKey, StringBuilder^% data, int buffer_size);

	public:
		property cli::array<String^>^ ActiveVisits
		{
			cli::array<String^>^ get();
		}
	};
}
