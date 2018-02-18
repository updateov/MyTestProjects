// LMSPatternsChart.cpp : Implementation of CLMSPatternsChartApp and DLL registration.

#include "stdafx.h"
#include "LMSPatternsChart.h"
#include "LMSPatternsChartCtrl.h"

#include "comcat.h"
#include "strsafe.h"
#include "objsafe.h"

#include <fstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CLSID_SafeItem - Necessary for safe ActiveX control
// Id taken from IMPLEMENT_OLECREATE_EX function in xxxCtrl.cpp
const CATID CLSID_SafeItem = { 0x172661, 0x82a6, 0x4c68, { 0x85, 0x85, 0xd5, 0x4e, 0x46, 0xa0, 0x7c, 0xcf } };

// HRESULT CreateComponentCategory - Used to register ActiveX control as safe
HRESULT CreateComponentCategory(CATID catid, WCHAR *catDescription)
{
	ICatRegister *pcr = NULL ;
	HRESULT hr = S_OK ;

	hr = CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL, CLSCTX_INPROC_SERVER, IID_ICatRegister, (void**)&pcr);
	if (FAILED(hr))
	{
		return hr;
	}

	// Make sure the HKCR\Component Categories\{..catid...} key is registered.
	CATEGORYINFO catinfo;
	catinfo.catid = catid;
	catinfo.lcid = 0x0409 ; // english

	// Make sure the provided description is not too long.
	// Only copy the first 127 characters if it is.
	// The second parameter of StringCchLength is the maximum number of characters that may be read into catDescription.
	// There must be room for a NULL-terminator. The third parameter contains the number of characters excluding the NULL-terminator.
	int len = wcslen(catDescription);
	if (len > 127)
	{
		len = 127;
	}

	// The second parameter of StringCchCopy is 128 because you need room for a NULL-terminator.
	wcsncpy(catinfo.szDescription, catDescription, len);

	// Make sure the description is null terminated.
	catinfo.szDescription[len] = '\0';

	hr = pcr->RegisterCategories(1, &catinfo);
	pcr->Release();

	return hr;
}

// HRESULT RegisterCLSIDInCategory - Register your component categories information
HRESULT RegisterCLSIDInCategory(REFCLSID clsid, CATID catid)
{
	// Register your component categories information.
	ICatRegister *pcr = NULL ;
	HRESULT hr = S_OK ;
	hr = CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL, CLSCTX_INPROC_SERVER, IID_ICatRegister, (void**)&pcr);
	if (SUCCEEDED(hr))
	{
		// Register this category as being "implemented" by the class.

		CATID rgcatid[1] ;
		rgcatid[0] = catid;
		hr = pcr->RegisterClassImplCategories(clsid, 1, rgcatid);
	}

	if (pcr != NULL)
	{
		pcr->Release();
	}

	return hr;
}

// HRESULT UnRegisterCLSIDInCategory - Remove entries from the registry
HRESULT UnRegisterCLSIDInCategory(REFCLSID clsid, CATID catid)
{
	ICatRegister *pcr = NULL ;
	HRESULT hr = S_OK ;

	hr = CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL, CLSCTX_INPROC_SERVER, IID_ICatRegister, (void**)&pcr);
	if (SUCCEEDED(hr))
	{
		// Unregister this category as being "implemented" by the class.
		CATID rgcatid[1] ;
		rgcatid[0] = catid;
		hr = pcr->UnRegisterClassImplCategories(clsid, 1, rgcatid);
	}

	if (pcr != NULL)
	{
		pcr->Release();
	}

	return hr;
}


CLMSPatternsChartApp theApp;

const GUID CDECL BASED_CODE _tlid = { 0xDC599246, 0xED72, 0x45B2, { 0xB7, 0xA4, 0x34, 0xC0, 0x64, 0x36, 0x88, 0xBE } };
const WORD _wVerMajor = 1;
const WORD _wVerMinor = 0;
//const CATID CATID_SafeForScripting = {0x7dd95801,0x9882,0x11cf,{0x9f,0xa9,0x00,0xaa,0x00,0x6c,0x42,0xc4}};
//const CATID CATID_SafeForInitializing = {0x7dd95802,0x9882,0x11cf,{0x9f,0xa9,0x00,0xaa,0x00,0x6c,0x42,0xc4}};
const GUID CDECL BASED_CODE _ctlid = { 0x172661, 0x82a6, 0x4c68, { 0x85, 0x85, 0xd5, 0x4e, 0x46, 0xa0, 0x7c, 0xcf } };

// By default, timeout dialogs in 60 seconds
int CLMSPatternsChartApp::s_TimeoutDialog = 1;

// CLMSPatternsChartApp::InitInstance - DLL initialization
BOOL CLMSPatternsChartApp::InitInstance()
{

	InitCommonControls();
	
	BOOL bInit = COleControlModule::InitInstance();
	
	if (bInit)
	{
		CLMSPatternsChartCtrl::initialize_bitmaps();
	}
	
	return bInit;
}



// CLMSPatternsChartApp::ExitInstance - DLL termination
int CLMSPatternsChartApp::ExitInstance()
{
	return COleControlModule::ExitInstance();
}

// DllRegisterServer - Adds entries to the system registry
STDAPI DllRegisterServer(void)
{
	HRESULT hr;// HResult used by Safety Functions

	AFX_MANAGE_STATE(_afxModuleAddrThis);

	if (!AfxOleRegisterTypeLib(AfxGetInstanceHandle(), _tlid))
		return ResultFromScode(SELFREG_E_TYPELIB);

	if (!COleObjectFactoryEx::UpdateRegistryAll(TRUE))
		return ResultFromScode(SELFREG_E_CLASS);

	if (FAILED(CreateComponentCategory(CATID_SafeForScripting, L"Controls that are safely scriptable") ))
		return ResultFromScode(SELFREG_E_CLASS);

	if (FAILED(CreateComponentCategory(CATID_SafeForInitializing, L"Controls safely initializable from persistent data") ))
		return ResultFromScode(SELFREG_E_CLASS);

	if (FAILED(RegisterCLSIDInCategory(_ctlid, CATID_SafeForScripting) ))
		return ResultFromScode(SELFREG_E_CLASS);

	if (FAILED(RegisterCLSIDInCategory(_ctlid, CATID_SafeForInitializing) ))
		return ResultFromScode(SELFREG_E_CLASS);

	hr = CreateComponentCategory(CATID_SafeForInitializing, L"Controls safely initializable from persistent data!");
	if (FAILED(hr))
		return hr;

	hr = RegisterCLSIDInCategory(CLSID_SafeItem, CATID_SafeForInitializing);
	if (FAILED(hr))
		return hr;

	// Mark the control as safe for scripting.
	hr = CreateComponentCategory(CATID_SafeForScripting, L"Controls safely scriptable!");
	if (FAILED(hr))
		return hr;

	hr = RegisterCLSIDInCategory(CLSID_SafeItem, CATID_SafeForScripting);
	if (FAILED(hr))
		return hr;

	return NOERROR;
}

// DllUnregisterServer - Removes entries from the system registry
STDAPI DllUnregisterServer(void)
{
	HRESULT hr;// HResult used by Safety Functions

	AFX_MANAGE_STATE(_afxModuleAddrThis);

	if (!AfxOleUnregisterTypeLib(_tlid, _wVerMajor, _wVerMinor))
		return ResultFromScode(SELFREG_E_TYPELIB);

	if (!COleObjectFactoryEx::UpdateRegistryAll(FALSE))
		return ResultFromScode(SELFREG_E_CLASS);

	hr=UnRegisterCLSIDInCategory(CLSID_SafeItem, CATID_SafeForInitializing);
	if (FAILED(hr))
		return hr;

	hr=UnRegisterCLSIDInCategory(CLSID_SafeItem, CATID_SafeForScripting);
	if (FAILED(hr))
		return hr;

	return NOERROR;
}
