

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0555 */
/* at Wed Feb 24 17:31:58 2016
 */
/* Compiler settings for LMSPatternsChart.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 7.00.0555 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__


#ifndef __Patterns_OEM_Chartidl_h__
#define __Patterns_OEM_Chartidl_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef ___DLMSPatternsChart_FWD_DEFINED__
#define ___DLMSPatternsChart_FWD_DEFINED__
typedef interface _DLMSPatternsChart _DLMSPatternsChart;
#endif 	/* ___DLMSPatternsChart_FWD_DEFINED__ */


#ifndef ___DLMSPatternsChartEvents_FWD_DEFINED__
#define ___DLMSPatternsChartEvents_FWD_DEFINED__
typedef interface _DLMSPatternsChartEvents _DLMSPatternsChartEvents;
#endif 	/* ___DLMSPatternsChartEvents_FWD_DEFINED__ */


#ifndef __LMSPatternsChart_FWD_DEFINED__
#define __LMSPatternsChart_FWD_DEFINED__

#ifdef __cplusplus
typedef class LMSPatternsChart LMSPatternsChart;
#else
typedef struct LMSPatternsChart LMSPatternsChart;
#endif /* __cplusplus */

#endif 	/* __LMSPatternsChart_FWD_DEFINED__ */


#ifdef __cplusplus
extern "C"{
#endif 



#ifndef __LMSPatternsChartLib_LIBRARY_DEFINED__
#define __LMSPatternsChartLib_LIBRARY_DEFINED__

/* library LMSPatternsChartLib */
/* [control][helpstring][helpfile][version][uuid] */ 


EXTERN_C const IID LIBID_LMSPatternsChartLib;

#ifndef ___DLMSPatternsChart_DISPINTERFACE_DEFINED__
#define ___DLMSPatternsChart_DISPINTERFACE_DEFINED__

/* dispinterface _DLMSPatternsChart */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__DLMSPatternsChart;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("D497676A-B978-4B6F-85C5-A90E5ADFE179")
    _DLMSPatternsChart : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _DLMSPatternsChartVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _DLMSPatternsChart * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _DLMSPatternsChart * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _DLMSPatternsChart * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _DLMSPatternsChart * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _DLMSPatternsChart * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _DLMSPatternsChart * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _DLMSPatternsChart * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _DLMSPatternsChartVtbl;

    interface _DLMSPatternsChart
    {
        CONST_VTBL struct _DLMSPatternsChartVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _DLMSPatternsChart_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define _DLMSPatternsChart_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define _DLMSPatternsChart_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define _DLMSPatternsChart_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define _DLMSPatternsChart_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define _DLMSPatternsChart_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define _DLMSPatternsChart_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___DLMSPatternsChart_DISPINTERFACE_DEFINED__ */


#ifndef ___DLMSPatternsChartEvents_DISPINTERFACE_DEFINED__
#define ___DLMSPatternsChartEvents_DISPINTERFACE_DEFINED__

/* dispinterface _DLMSPatternsChartEvents */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__DLMSPatternsChartEvents;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("CFF0436C-FF88-4F96-8FCD-AA7BD42540FD")
    _DLMSPatternsChartEvents : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _DLMSPatternsChartEventsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _DLMSPatternsChartEvents * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _DLMSPatternsChartEvents * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _DLMSPatternsChartEvents * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _DLMSPatternsChartEvents * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _DLMSPatternsChartEvents * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _DLMSPatternsChartEvents * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _DLMSPatternsChartEvents * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _DLMSPatternsChartEventsVtbl;

    interface _DLMSPatternsChartEvents
    {
        CONST_VTBL struct _DLMSPatternsChartEventsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _DLMSPatternsChartEvents_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define _DLMSPatternsChartEvents_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define _DLMSPatternsChartEvents_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define _DLMSPatternsChartEvents_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define _DLMSPatternsChartEvents_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define _DLMSPatternsChartEvents_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define _DLMSPatternsChartEvents_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___DLMSPatternsChartEvents_DISPINTERFACE_DEFINED__ */


EXTERN_C const CLSID CLSID_LMSPatternsChart;

#ifdef __cplusplus

class DECLSPEC_UUID("00172661-82A6-4C68-8585-D54E46A07CCF")
LMSPatternsChart;
#endif
#endif /* __LMSPatternsChartLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


