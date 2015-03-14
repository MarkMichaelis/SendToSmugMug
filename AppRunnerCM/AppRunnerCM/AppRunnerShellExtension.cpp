// AppRunnerShellExtension.cpp : Implementation of CAppRunnerShellExtension

#include "stdafx.h"

#include "AppRunnerShellExtension.h"

#include <atlconv.h>

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

// CAppRunnerShellExtension

HRESULT CAppRunnerShellExtension::Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hProgID)
{
	FORMATETC fmt = { CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL };
	STGMEDIUM stg = { TYMED_HGLOBAL };
	HDROP hDrop = NULL;

	// Clear old state
	mFilenames.clear();

	if (pDataObj == NULL)
	{
		return E_INVALIDARG;
	}

	// Look for CF_HDROP data in the data object.
	if (FAILED(pDataObj->GetData(&fmt, &stg)))
	{
		return E_INVALIDARG;
	}

	// Get a pointer to the actual data.
	hDrop = (HDROP)::GlobalLock(stg.hGlobal);
	if (hDrop == NULL)
	{
		return E_INVALIDARG;
	}

	UINT numFiles = ::DragQueryFile(hDrop, (UINT)-1, NULL, 0);
	HRESULT hr = (numFiles == 0) ? E_INVALIDARG : S_OK;
	TCHAR filename[MAX_PATH];

	for(UINT i = 0; i < numFiles; i++)
	{
		if (::DragQueryFile(hDrop, i, filename, MAX_PATH) == 0)
		{
			hr = E_INVALIDARG;
			break;
		}

		mFilenames.push_back(filename);
	}

	::GlobalUnlock(stg.hGlobal);
	::ReleaseStgMedium(&stg);

	return hr;
}

HRESULT CAppRunnerShellExtension::QueryContextMenu(HMENU hmenu, UINT uMenuIndex, UINT uidFirstCmd, UINT uidLastCmd, UINT uFlags)
{
	// If the flags include CMF_DEFAULTONLY then we shouldn't do anything.
	if (uFlags & CMF_DEFAULTONLY )
	{
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);
	}

	if (::InsertMenu(hmenu, uMenuIndex, MF_BYPOSITION, uidFirstCmd, _T("Send to SmugMug")))
	{
		// Put bitmap on menu item
		static HBITMAP bitmap = NULL;
		
		// Load bitmap if not previously loaded
		if (bitmap == NULL)
		{
			bitmap = (HBITMAP)::LoadImage(_AtlBaseModule.GetModuleInstance(), MAKEINTRESOURCE(IDB_BITMAP2), IMAGE_BITMAP, 0, 0, LR_CREATEDIBSECTION);
		}

		if (bitmap != NULL)
		{
			::SetMenuItemBitmaps(hmenu, uMenuIndex, MF_BYPOSITION, bitmap, bitmap);
		}

		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 1);
	}
	else
	{
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);
	}
}

HRESULT CAppRunnerShellExtension::GetCommandString(UINT_PTR idCmd, UINT uFlags, UINT* pwReserved, LPSTR pszName, UINT cchMax)
{
	// Support help string
	if ((idCmd == NULL) && ((uFlags & GCS_HELPTEXT) == GCS_HELPTEXT))
	{
		if (uFlags & GCS_UNICODE)
		{
			::lstrcpynW((LPWSTR) pszName, L"Sends photos or folder of photos to smugmug", cchMax);
		}
		else
		{
			::lstrcpynA(pszName, "Sends photos or folder of photos to smugmug", cchMax);
		}

		return S_OK;
	}

	return E_INVALIDARG;
}

HRESULT CAppRunnerShellExtension::InvokeCommand(LPCMINVOKECOMMANDINFO pCmdInfo)
{
	if (HIWORD( pCmdInfo->lpVerb) == 0 && LOWORD(pCmdInfo->lpVerb) == 0)
	{
		if (!mFilenames.empty())
		{
			// Build list of parameters, putting each filename in quotes
			std::wstring parameters = L"\"";
			for(std::vector<std::wstring>::const_iterator i = mFilenames.begin(); i < mFilenames.end(); i++)
			{
				parameters += *i;
				parameters += L"\" \"";
			}
			parameters += L"\"";

			// Get path to app to launch
			LPWSTR  dllPath = new WCHAR[MAX_PATH];
			if (::GetModuleFileName((HINSTANCE)&__ImageBase, dllPath, MAX_PATH) != 0)
			{
				// Strip off dll name.
				int i;
				for (i = lstrlenW(dllPath); i > 0; i--)
				{
					if (dllPath[i-1] == '\\')
						break;
				}

				if (i > 0) // we found a '\'
				{
					dllPath[i] = 0; // truncate it

					std::wstring appPath(dllPath);
					appPath += L"Send to SmugMug.exe";

					// Shell execute our command with the filenames as parameters
			/*		SHELLEXECUTEINFO shExecInfo;
					shExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
					shExecInfo.fMask = NULL;
					shExecInfo.hwnd = NULL;
					shExecInfo.lpVerb = NULL;
					shExecInfo.lpFile = appPath.c_str();
					shExecInfo.lpParameters = parameters.c_str();
					shExecInfo.lpDirectory = NULL;
					shExecInfo.nShow = SW_NORMAL;
					shExecInfo.hInstApp = NULL;

					::ShellExecuteEx(&shExecInfo);*/

					size_t iMyCounter = 0, iReturnVal = 0, iPos = 0;
					DWORD dwExitCode = 0;
					std::wstring sTempStr = L"";

					/* - NOTE - You should check here to see if the exe even exists */

					/* Add a space to the beginning of the Parameters */
					if (parameters.size() != 0)
					{ 
					if (parameters[0] != L' ')
					{
					parameters.insert(0,L" ");
					}
					}

					/* The first parameter needs to be the exe itself 
					sTempStr = appPath;
					iPos = sTempStr.find_last_of(L"\\");
					sTempStr.erase(0, iPos +1);
					parameters = sTempStr.append(parameters);
					*/

					/*
					CreateProcessW can modify parameters thus we
					allocate needed memory
					*/
					wchar_t * pwszParam = new wchar_t[parameters.size() + 1];
					if (pwszParam == 0)
					{
					return 1;
					} 
					const wchar_t* pchrTemp = parameters.c_str();
					wcscpy_s(pwszParam, parameters.size() + 1, pchrTemp);

					/* CreateProcess API initialization */
					STARTUPINFOW siStartupInfo;
					PROCESS_INFORMATION piProcessInfo;
					memset(&siStartupInfo, 0, sizeof(siStartupInfo));
					memset(&piProcessInfo, 0, sizeof(piProcessInfo));
					siStartupInfo.cb = sizeof(siStartupInfo);

					CreateProcessW(const_cast<LPCWSTR>(appPath.c_str()),
						  pwszParam, 0, 0, false,
						  CREATE_DEFAULT_ERROR_MODE, 0, 0,
						  &siStartupInfo, &piProcessInfo);

					/* Free memory */
					delete[]pwszParam;
					pwszParam = 0;

					/* Release handles */
					CloseHandle(piProcessInfo.hProcess);
					CloseHandle(piProcessInfo.hThread);
				}
			}
		}

		return S_OK;
	}

	return E_INVALIDARG;
}
