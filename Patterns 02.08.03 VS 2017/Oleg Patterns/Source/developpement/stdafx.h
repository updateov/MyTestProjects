// stdafx.h : Fichier Include pour les fichiers Include système standard, ou les
// fichiers Include spécifiques aux projets qui sont utilisés fréquemment, et sont
// rarement modifiés
#pragma once
#ifndef WINVER					// Autorise l'utilisation des fonctionnalités spécifiques à Windows 95 et Windows NT 4 ou version ultérieure.
#define WINVER	0x0501			// Attribuez la valeur appropriée à cet élément pour cibler Windows 98 et Windows 2000 ou version ultérieure.
#endif
#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN			// Exclure les en-têtes Windows rarement utilisés
#endif

#define _CRT_SECURE_NO_WARNINGS
#define _CRT_NON_CONFORMING_SWPRINTFS
#define _CRT_NONSTDC_NO_WARNINGS

#if (_MSC_VER > 1310) // VS2005
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

// Modifiez les définitions suivantes si vous devez cibler une plate-forme avant
// celles spécifiées ci-dessous. Reportez-vous à MSDN pour obtenir les dernières
// informations sur les valeurs correspondantes pour les différentes
// plates-formes.
#ifndef _WIN32_WINNT			// Autorise l'utilisation des fonctionnalités spécifiques à Windows NT 4 ou version ultérieure.
#define _WIN32_WINNT	0x0400	// Attribuez la valeur appropriée à cet élément pour cibler Windows 98 et Windows 2000 ou version ultérieure.
#endif
#ifndef _WIN32_WINDOWS			// Autorise l'utilisation des fonctionnalités spécifiques à Windows 98 ou version ultérieure.
#define _WIN32_WINDOWS	0x0410	// Attribuez la valeur appropriée à cet élément pour cibler Windows Me ou version ultérieure.
#endif
#ifndef _WIN32_IE				// Autorise l'utilisation des fonctionnalités spécifiques à IE 4.0 ou version ultérieure.
#define _WIN32_IE	0x0400		// Attribuez la valeur appropriée à cet élément pour cibler IE 5.0 ou version ultérieure.
#endif
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// certains constructeurs CString seront explicites

// désactive le masquage MFC de certains messages d'avertissement courants et par
// ailleurs souvent ignorés
#define _AFX_ALL_WARNINGS

#include <afxwin.h>		// composants MFC principaux et standard
#include <afxext.h>		// extensions MFC

#include <afxdtctl.h>	// Prise en charge des MFC pour les contrôles communs Internet Explorer 4
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>		// Prise en charge des MFC pour les contrôles communs Windows
#endif // _AFX_NO_AFXCMN_SUPPORT
