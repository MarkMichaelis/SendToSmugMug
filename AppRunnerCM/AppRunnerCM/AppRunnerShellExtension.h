// AppRunnerShellExtension.h : Declaration of the CAppRunnerShellExtension

#pragma once
#include "resource.h"       // main symbols

#include "AppRunnerCM.h"

#include <shlobj.h>
#include <comdef.h>
#include <vector>

// Workaround comdef weirdness
struct __declspec(uuid("000214e4-0000-0000-c000-000000000046")) IContextMenu;

_COM_SMARTPTR_TYPEDEF(IContextMenu, __uuidof(IContextMenu));


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CAppRunnerShellExtension

class ATL_NO_VTABLE CAppRunnerShellExtension :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CAppRunnerShellExtension, &CLSID_AppRunnerShellExtension>,
	public IDispatchImpl<IAppRunnerShellExtension, &IID_IAppRunnerShellExtension, &LIBID_AppRunnerCMLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IShellExtInit,
	public IContextMenu
{
public:
	CAppRunnerShellExtension()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_APPRUNNERSHELLEXTENSION)


BEGIN_COM_MAP(CAppRunnerShellExtension)
	COM_INTERFACE_ENTRY(IAppRunnerShellExtension)
	COM_INTERFACE_ENTRY(IDispatch)
    COM_INTERFACE_ENTRY(IShellExtInit)
    COM_INTERFACE_ENTRY(IContextMenu)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

    // IShellExtInit
    STDMETHOD(Initialize)(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hProgID);

    // IContextMenu
    STDMETHOD(GetCommandString)(UINT_PTR idCmd, UINT uFlags, UINT* pwReserved, LPSTR pszName, UINT cchMax);
	STDMETHOD(InvokeCommand)(LPCMINVOKECOMMANDINFO pCmdInfo);
    STDMETHOD(QueryContextMenu)(HMENU hmenu, UINT uMenuIndex, UINT uidFirstCmd, UINT uidLastCmd, UINT uFlags);

protected:
	std::vector<std::wstring> mFilenames;
};

OBJECT_ENTRY_AUTO(__uuidof(AppRunnerShellExtension), CAppRunnerShellExtension)
