

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0555 */
/* at Wed Feb 24 17:31:58 2016
 */
/* Compiler settings for CRIPatternsChart.idl:
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


#ifndef __Patterns_CRI_Chartidl_h__
#define __Patterns_CRI_Chartidl_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef ___DCRIPatternsChart_FWD_DEFINED__
#define ___DCRIPatternsChart_FWD_DEFINED__
typedef interface _DCRIPatternsChart _DCRIPatternsChart;
#endif 	/* ___DCRIPatternsChart_FWD_DEFINED__ */


#ifndef ___DCRIPatternsChartEvents_FWD_DEFINED__
#define ___DCRIPatternsChartEvents_FWD_DEFINED__
typedef interface _DCRIPatternsChartEvents _DCRIPatternsChartEvents;
#endif 	/* ___DCRIPatternsChartEvents_FWD_DEFINED__ */


#ifndef __CRIPatternsChart_FWD_DEFINED__
#define __CRIPatternsChart_FWD_DEFINED__

#ifdef __cplusplus
typedef class CRIPatternsChart CRIPatternsChart;
#else
typedef struct CRIPatternsChart CRIPatternsChart;
#endif /* __cplusplus */

#endif 	/* __CRIPatternsChart_FWD_DEFINED__ */


#ifdef __cplusplus
extern "C"{
#endif 



#ifndef __CRIPatternsChartLib_LIBRARY_DEFINED__
#define __CRIPatternsChartLib_LIBRARY_DEFINED__

/* library CRIPatternsChartLib */
/* [control][helpstring][helpfile][version][uuid] */ 


EXTERN_C const IID LIBID_CRIPatternsChartLib;

#ifndef ___DCRIPatternsChart_DISPINTERFACE_DEFINED__
#define ___DCRIPatternsChart_DISPINTERFACE_DEFINED__

/* dispinterface _DCRIPatternsChart */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__DCRIPatternsChart;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("41681E08-E263-42D9-A359-843CBD340543")
    _DCRIPatternsChart : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _DCRIPatternsChartVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _DCRIPatternsChart * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _DCRIPatternsChart * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _DCRIPatternsChart * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _DCRIPatternsChart * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _DCRIPatternsChart * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _DCRIPatternsChart * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _DCRIPatternsChart * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _DCRIPatternsChartVtbl;

    interface _DCRIPatternsChart
    {
        CONST_VTBL struct _DCRIPatternsChartVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _DCRIPatternsChart_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define _DCRIPatternsChart_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define _DCRIPatternsChart_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define _DCRIPatternsChart_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define _DCRIPatternsChart_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define _DCRIPatternsChart_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define _DCRIPatternsChart_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___DCRIPatternsChart_DISPINTERFACE_DEFINED__ */


#ifndef ___DCRIPatternsChartEvents_DISPINTERFACE_DEFINED__
#define ___DCRIPatternsChartEvents_DISPINTERFACE_DEFINED__

/* dispinterface _DCRIPatternsChartEvents */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__DCRIPatternsChartEvents;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("1A73C536-B666-4FDC-9D96-897242EE3048")
    _DCRIPatternsChartEvents : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _DCRIPatternsChartEventsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _DCRIPatternsChartEvents * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _DCRIPatternsChartEvents * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _DCRIPatternsChartEvents * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _DCRIPatternsChartEvents * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _DCRIPatternsChartEvents * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _DCRIPatternsChartEvents * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _DCRIPatternsChartEvents * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _DCRIPatternsChartEventsVtbl;

    interface _DCRIPatternsChartEvents
    {
        CONST_VTBL struct _DCRIPatternsChartEventsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _DCRIPatternsChartEvents_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define _DCRIPatternsChartEvents_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define _DCRIPatternsChartEvents_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define _DCRIPatternsChartEvents_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define _DCRIPatternsChartEvents_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define _DCRIPatternsChartEvents_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define _DCRIPatternsChartEvents_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___DCRIPatternsChartEvents_DISPINTERFACE_DEFINED__ */


EXTERN_C const CLSID CLSID_CRIPatternsChart;

#ifdef __cplusplus

class DECLSPEC_UUID("822D5CDC-3062-4C71-8FA5-7156E38B2F1C")
CRIPatternsChart;
#endif
#endif /* __CRIPatternsChartLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


