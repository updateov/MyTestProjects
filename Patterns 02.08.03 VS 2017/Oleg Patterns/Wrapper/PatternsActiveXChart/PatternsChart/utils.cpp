#include "stdafx.h"
#include "utils.h"

#include <iostream>
#include <iomanip>


using namespace patterns;

string utils::to_string(const TiXmlDocument& doc)
{
	TiXmlPrinter printer;
	printer.SetIndent( "\t" );

	doc.Accept(&printer );
	return printer.CStr();
}

// Encode the given parameter to a string that's safe to pass as a URL parameter
string utils::encode_url_parameter(string parameter)
{
	std::stringstream os;
	os << std::hex << std::setw(2) << std::setfill('0');

	for (string::iterator itr = parameter.begin(); itr != parameter.end(); ++itr)
	{
		os << static_cast<unsigned int>(static_cast<unsigned char>(*itr));
	}

	return os.str(); 
}

// Do a http GET request on 
string utils::perform_server_request(string url, string appname)
{
	CString appName = appname.c_str();
	CInternetSession internetSession(appName);
	CStdioFile* pRemoteData = internetSession.OpenURL(url.c_str(), 1 ,INTERNET_FLAG_TRANSFER_BINARY | INTERNET_FLAG_RELOAD);

	char* buffer = new char[50240];
	string response = "";

	try
	{
		::ZeroMemory(buffer, sizeof(buffer));

		DWORD dwBlockSize;
		while (dwBlockSize = pRemoteData->Read(buffer, sizeof(buffer) - 1)) // Read from file
		{
			buffer[dwBlockSize] = 0;
			response += (LPSTR)buffer;
		}

		delete[] buffer;
		buffer = NULL;
	}
	catch (...) 
	{
		if (buffer != NULL)
		{
			delete[] buffer;
		}
		throw;
	}

	pRemoteData->Close();
	delete pRemoteData;
	pRemoteData = NULL;

	return response;
}

string utils::perform_server_postrequest(string url, string payload, string appname, int timeout)
{
	CString appName = appname.c_str();	
	CInternetSession session(appName);
	CHttpConnection * pServer = NULL;
	CHttpFile * pFile = NULL;

	try
	{
		DWORD dwServiceType;
		CString sServer, sObject;
		unsigned short nPort;
		if (!::AfxParseURL(url.c_str(), dwServiceType, sServer, sObject, nPort))
			throw exception("Invalid url");

		// Open the connection for https
		if (dwServiceType == AFX_INET_SERVICE_HTTPS)
		{
			pServer = session.GetHttpConnection(sServer, INTERNET_FLAG_EXISTING_CONNECT | INTERNET_FLAG_RELOAD | INTERNET_FLAG_NO_CACHE_WRITE | INTERNET_FLAG_SECURE | INTERNET_FLAG_IGNORE_CERT_CN_INVALID, nPort);
			pServer->SetOption(INTERNET_OPTION_CONNECT_TIMEOUT, timeout);
			pServer->SetOption(INTERNET_OPTION_CONNECT_RETRIES, 1);

			pFile = pServer->OpenRequest(CHttpConnection::HTTP_VERB_POST, sObject, NULL, 1, NULL, NULL, INTERNET_FLAG_EXISTING_CONNECT | INTERNET_FLAG_RELOAD | INTERNET_FLAG_NO_CACHE_WRITE | INTERNET_FLAG_SECURE | INTERNET_FLAG_IGNORE_CERT_CN_INVALID);
		}
		else
		{
			pServer = session.GetHttpConnection(sServer, nPort);
			pServer->SetOption(INTERNET_OPTION_CONNECT_TIMEOUT, timeout);
			pServer->SetOption(INTERNET_OPTION_CONNECT_RETRIES, 1);

			pFile = pServer->OpenRequest(CHttpConnection::HTTP_VERB_POST, sObject, NULL, 1, NULL, NULL, INTERNET_FLAG_EXISTING_CONNECT | INTERNET_FLAG_RELOAD | INTERNET_FLAG_NO_CACHE_WRITE );
		}
		if (!pFile)
			throw exception("Unable to open the post request");

		// Prepare the input request
		pFile->AddRequestHeaders("Accept: application/xml");
		pFile->AddRequestHeaders("Content-Type: application/xml; charset=utf-8");

		pFile->SendRequestEx(payload.length());
		pFile->WriteString(payload.c_str());
		pFile->EndRequest();

		// Check for error
		DWORD status;
		pFile->QueryInfoStatusCode(status);
		if (status >= 400)
			throw exception("Request failed");

		// Read the reply
		char* buffer = new char[50240];
		string response = "";

		try
		{
			::ZeroMemory(buffer, sizeof(buffer));

			DWORD dwBlockSize;
			while (dwBlockSize = pFile->Read(buffer, sizeof(buffer) - 1)) // Read from file
			{
				buffer[dwBlockSize] = 0;
				response += (LPSTR)buffer;
			}

			delete[] buffer;
			buffer = NULL;
		}
		catch (...) 
		{
			if (buffer != NULL)
			{
				delete[] buffer;
			}
			throw;
		}

		// Clean up
		pFile->Close();
		delete pFile;
		pFile = NULL;

		pServer->Close();
		delete pServer;
		pServer = NULL;

		session.Close();

		// Done
		return response;
	}
	catch (...)
	{
		if (pFile != NULL)
		{
			pFile->Close();
			delete pFile;
			pFile = NULL;
		}
		if (pServer != NULL)
		{
			pServer->Close();
			delete pServer;
			pServer = NULL;
		}
		session.Close();

		return "";
	}
}