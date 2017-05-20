/***************************************************************************
 *
 * Project: libcurl.NET
 *
 * Copyright (c) 2004, 2005 Jeff Phillips (jeff@jeffp.net)
 *
 * This software is licensed as described in the file COPYING, which you
 * should have received as part of this distribution.
 *
 * You may opt to use, copy, modify, merge, publish, distribute and/or sell
 * copies of this Software, and permit persons to whom the Software is
 * furnished to do so, under the terms of the COPYING file.
 *
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
 * ANY KIND, either express or implied.
 *
 * $Id: LibCurlShim.c,v 1.1 2005/02/17 22:47:24 jeffreyphillips Exp $
 **************************************************************************/

#include <time.h>
#include "shm_seq.h"
#include "shm_list.h"
#include "shm_table.h"
#include "shm_mem.h"

#include "LibCurlShim.h"
#include "curl/curl.h"
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>

#ifdef WIN32

#include <windows.h>

typedef CRITICAL_SECTION lock_type;
struct event_type
{
	HANDLE handle;
};

#else
#include <unistd.h>
#include <pthread.h>
typedef pthread_mutex_t lock_type;
struct event_type
{
	pthread_mutex_t		mutex;
	pthread_cond_t		cond;
	bool trigger;
};
#endif

//lock
void BEGIN_LOCK(lock_type* cs)
{
#ifdef WIN32
	EnterCriticalSection(cs);
#else
	pthread_mutex_lock(cs);
#endif
}

void END_LOCK(lock_type* cs)
{
#ifdef WIN32
	LeaveCriticalSection(cs);
#else
	pthread_mutex_unlock(cs);
#endif
}

void INIT_LOCK(lock_type* cs)
{
#ifdef WIN32
	InitializeCriticalSection(cs);
#else
	pthread_mutex_init(cs, NULL);
#endif
}

void DESTROY_LOCK(lock_type* cs)
{
#ifdef WIN32
	DeleteCriticalSection(cs);
#else
	pthread_mutex_destroy(cs);
#endif
}



// #define CURLOPT_WRITEFUNCTION       20011
// #define CURLOPT_WRITEDATA           10001
// #define CURLOPT_READFUNCTION        20012
// #define CURLOPT_READDATA            10009
// #define CURLOPT_PROGRESSFUNCTION    20056
// #define CURLOPT_PROGRESSDATA        10057
// #define CURLOPT_DEBUGFUNCTION       20094
// #define CURLOPT_DEBUGDATA           10095
// #define CURLOPT_HEADERFUNCTION      20079
// #define CURLOPT_HEADERDATA          10029
// #define CURLOPT_SSL_CTX_FUNCTION    20108
// #define CURLOPT_SSL_CTX_DATA        10109
// #define CURLOPT_IOCTLFUNCTION       20130
// #define CURLOPT_IOCTLDATA           10131
// 
// #define CURLFORM_END                17
// 
// #define CURLSHOPT_LOCKFUNC          3
// #define CURLSHOPT_UNLOCKFUNC        4
// #define CURLSHOPT_USERDATA          5

#pragma warning(disable : 4100 4311 4312)
    
static Table_T          g_delegateTable;
static lock_type	g_csDelegateTable;
static Table_T          g_shareDelegateTable;
static lock_type	g_csShareDelegateTable;

//typedef int   (__cdecl *CPROC)();
//typedef void* (__cdecl *CPVPROC)();
//typedef int   (__cdecl *OPTPROC)(void*, int, __int64);


void curl_shim_initialize()
{
    g_delegateTable = Table_new(16, NULL, NULL);
    INIT_LOCK(&g_csDelegateTable);
    g_shareDelegateTable = Table_new(16, NULL, NULL);
	INIT_LOCK(&g_csShareDelegateTable);
}


static void vfree(const void* key, void** value, void* cl)
{
    FREE(*value);
}

void curl_shim_cleanup()
{
    Table_map(g_delegateTable, vfree, NULL);
    Table_free(&g_delegateTable);
    DESTROY_LOCK(&g_csDelegateTable);
    Table_map(g_shareDelegateTable, vfree, NULL);
    Table_free(&g_shareDelegateTable);
	DESTROY_LOCK(&g_csShareDelegateTable);
}

char* curl_shim_get_version_char_ptr(
    void* p, int offset)
{
    char* q = &((char*)p)[offset];
    char** qq = (char**)q;
    return *qq;
}

#pragma message("This will break in 64-bits, unless cURL is rebuilt")
int curl_shim_get_version_int_value(
    void* p, int offset)
{
    int* q = (int*)p;
    q += offset / sizeof(int);
    return *q;
}

int curl_shim_get_number_of_protocols(
    void* p, int protOffset)
{
    int nProtocols = 0;
    char* q = &((char*)p)[protOffset];
    char*** qq = (char***)q;
    char** rr = *qq;
    while(*rr++)
        nProtocols++;
    return nProtocols;
}

char* curl_shim_get_protocol_string(
    void* p, int protOffset, int nProt)
{
    char* q = &((char*)p)[protOffset];
    char*** qq = (char***)q;
    char** rr = *qq;
    return rr[nProt];
}

void* curl_shim_alloc_strings()
{
    Seq_T seq = Seq_new(0);
    return (void*)seq;
}

char* curl_shim_add_string(void* p, const char* pInStr)
{
    char* pOutStr;
    Seq_T seq = (Seq_T)p;

    pOutStr = (char*)malloc(strlen(pInStr) + 1);
    strcpy(pOutStr, pInStr);
    Seq_addhi(seq, pOutStr);
    return pOutStr;
}

void curl_shim_free_strings(void* p)
{
    int i, count;
    Seq_T seq = (Seq_T)p;
    
    count = Seq_length(seq);
    for (i = 0; i < count; i++)
        free(Seq_get(seq, i));
    Seq_free(&seq);
}

void* curl_shim_add_string_to_slist(
    void* lst, const char* pInStr)
{
	char* pOutStr;
    List_T list = (List_T)lst;

    pOutStr = (char*)malloc(strlen(pInStr) + 1);
    strcpy(pOutStr, pInStr);
    return List_push(list, (void*)pOutStr);
}

void* curl_shim_get_string_from_slist(
    void* lst, char** ppString)
{
    List_T list = (List_T)lst;
    *ppString = (char*)list->first;
    return (void*)list->rest;
}

void curl_shim_free_slist(void* lst)
{
    void* pvStr = NULL;
    List_T list = (List_T)lst;
    while ((list = List_pop(list, &pvStr)) != NULL)
        free(pvStr);
}

typedef int(SHIM_STD_CALL *FN_write_callback)(char* szptr, int sz,
	int nmemb, void* pvThis);
static size_t write_callback_impl(char* szptr, size_t sz,
    size_t nmemb, void* pvThis)
{
    // locate the delegates
	FN_write_callback fpWriteDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpWriteDel = (FN_write_callback)(pnDelegates[0]);
    return (size_t)fpWriteDel(szptr, (int)sz, (int)nmemb, pvThis);
}

typedef int(SHIM_STD_CALL *FN_read_callback)(void* szptr, int sz,
	int nmemb, void* pvThis);
static size_t read_callback_impl(void* szptr, size_t sz,
    size_t nmemb, void* pvThis)
{
    // locate the delegates
	FN_read_callback fpReadDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpReadDel = (FN_read_callback)(pnDelegates[1]);
    return (size_t)fpReadDel(szptr, (int)sz, (int)nmemb, pvThis);
}

typedef int(SHIM_STD_CALL *FN_progress_callback)(void* pvThis, double dlTotal,
	double dlNow, double ulTotal, double ulNow);
static int progress_callback_impl(void* pvThis, double dlTotal,
    double dlNow, double ulTotal, double ulNow)
{
    // locate the delegates
	FN_progress_callback fpProgDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpProgDel = (FN_progress_callback)(pnDelegates[2]);
    return fpProgDel(pvThis, dlTotal, dlNow, ulTotal, ulNow);
}

typedef int(SHIM_STD_CALL *FN_debug_callback)(int infoType,
	char* szMsg, int msgSize, void* pvThis);
static int debug_callback_impl(void* pvCurl, int infoType,
    char* szMsg, size_t msgSize, void* pvThis)
{
    // locate the delegates
	FN_debug_callback fpDebugDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpDebugDel = (FN_debug_callback)(pnDelegates[3]);
    return fpDebugDel(infoType, szMsg, (int)msgSize, pvThis);
}

typedef int(SHIM_STD_CALL *FN_header_callback)(char* szptr, int sz,
	int nmemb, void* pvThis);
static size_t header_callback_impl(char* szptr, size_t sz,
    size_t nmemb, void* pvThis)
{
    // locate the delegates
	FN_header_callback fpHeaderDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpHeaderDel = (FN_header_callback)(pnDelegates[4]);
    return (size_t)fpHeaderDel(szptr, (int)sz, (int)nmemb, pvThis);
}

typedef int(SHIM_STD_CALL *FN_ssl_ctx_callback)(void* ctx, void* pvThis);
static int ssl_ctx_callback_impl(void* pvCurl, void* ctx, void* pvThis)
{
    // locate the delegates
	FN_ssl_ctx_callback fpSslCtxDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0; 
	fpSslCtxDel = (FN_ssl_ctx_callback)(pnDelegates[5]);
    return fpSslCtxDel(ctx, pvThis);
}

typedef int(SHIM_STD_CALL *FN_ioctl_callback)(int cmd, void* pvThis);
static int ioctl_callback_impl(void* pvCurl, int cmd, void* pvThis)
{
    // locate the delegates
	FN_ioctl_callback fpIoctlDel;
	void** pnDelegates =
		(void**)Table_get(g_delegateTable, pvThis);

    if (!pnDelegates)
        return 0;
	fpIoctlDel = (FN_ioctl_callback)(pnDelegates[6]);
    return fpIoctlDel(cmd, pvThis);
}

int curl_shim_install_delegates(void* handle,
    void* pvThis, void* pvWriteDel, void* pvReadDel, void* pvProgDel,
    void* pvDebugDel, void* pvHeaderDel, void* pvSSLContextDel,
    void* pvIoctlDel)
{
    // cast return from GetProcAddress as a CPROC
    //CPROC pcp = (CPROC)curl_easy_setopt;

    // install all delegates through here when this works
    void** pnDelegates = malloc(7 * sizeof(void*));
    pnDelegates[0] = pvWriteDel;
    pnDelegates[1] = pvReadDel;
    pnDelegates[2] = pvProgDel;
    pnDelegates[3] = pvDebugDel;
    pnDelegates[4] = pvHeaderDel;
    pnDelegates[5] = pvSSLContextDel;
    pnDelegates[6] = pvIoctlDel;

    // add to the table (need to serialize access)
    BEGIN_LOCK(&g_csDelegateTable);
    Table_put(g_delegateTable, pvThis, pnDelegates);
    END_LOCK(&g_csDelegateTable);

    // setup the callbacks from libcurl
	curl_easy_setopt(handle, CURLOPT_WRITEFUNCTION, write_callback_impl);
	curl_easy_setopt(handle, CURLOPT_WRITEDATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_READFUNCTION, read_callback_impl);
	curl_easy_setopt(handle, CURLOPT_READDATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_PROGRESSFUNCTION, progress_callback_impl);
	curl_easy_setopt(handle, CURLOPT_PROGRESSDATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_DEBUGFUNCTION, debug_callback_impl);
	curl_easy_setopt(handle, CURLOPT_DEBUGDATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_HEADERFUNCTION, header_callback_impl);
	curl_easy_setopt(handle, CURLOPT_HEADERDATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_SSL_CTX_FUNCTION, ssl_ctx_callback_impl);
	curl_easy_setopt(handle, CURLOPT_SSL_CTX_DATA, pvThis);
	curl_easy_setopt(handle, CURLOPT_IOCTLFUNCTION, ioctl_callback_impl);
	curl_easy_setopt(handle, CURLOPT_IOCTLDATA, pvThis);

    return 0;
}

void curl_shim_cleanup_delegates(void* pvThis)
{
    void* pvDelegates;
	BEGIN_LOCK(&g_csDelegateTable);
    pvDelegates = Table_remove(g_delegateTable, pvThis);
    END_LOCK(&g_csDelegateTable);
    if (pvDelegates)
        free(pvDelegates);
}

typedef void(*FN_lock_callback)(int data,
	int access, void* pvThis);
static void lock_callback_impl(void* pvHandle, int data,
    int access, void* pvThis)
{
    // locate the delegates
	FN_lock_callback fpLockDel;
    unsigned int* pnDelegates =
        (unsigned int*)Table_get(g_shareDelegateTable, pvThis);

    if (pnDelegates)
    {
		fpLockDel = (FN_lock_callback)pnDelegates[0];
        fpLockDel(data, access, pvThis);
    }
}

typedef void(*FN_unlock_callback)(int data,
	void* pvThis);
static void unlock_callback_impl(void* pvHandle, int data,
    void* pvThis)
{
    // locate the delegates
	FN_unlock_callback fpUnlockDel;
    unsigned int* pnDelegates =
        (unsigned int*)Table_get(g_shareDelegateTable, pvThis);

    if (pnDelegates)
    {
		fpUnlockDel = (FN_unlock_callback)pnDelegates[1];
        fpUnlockDel(data, pvThis);
    }
}

int curl_shim_install_share_delegates(
    void* handle, void* pvThis, void* pvLockDel, void* pvUnlockDel)
{
    // cast return from GetProcAddress as a CPROC
    //CPROC pcp = (CPROC)curl_share_setopt;

    // install delegates
    unsigned int* pnDelegates = malloc(2 * sizeof(unsigned int));
    pnDelegates[0] = (unsigned int)pvLockDel;
    pnDelegates[1] = (unsigned int)pvUnlockDel;

    // add to the table, with serialized access
    BEGIN_LOCK(&g_csShareDelegateTable);
    Table_put(g_shareDelegateTable, pvThis, pnDelegates);
    END_LOCK(&g_csShareDelegateTable);

    // set up the callbacks from libcurl
	curl_share_setopt(handle, CURLSHOPT_LOCKFUNC, lock_callback_impl);
	curl_share_setopt(handle, CURLSHOPT_UNLOCKFUNC, unlock_callback_impl);
	curl_share_setopt(handle, CURLSHOPT_USERDATA, pvThis);

    return 0;
}

void curl_shim_cleanup_share_delegates(void* pvThis)
{
    void* pvDelegates;
	BEGIN_LOCK(&g_csShareDelegateTable);
    pvDelegates = Table_remove(g_shareDelegateTable, pvThis);
    END_LOCK(&g_csDelegateTable);
    if (pvDelegates)
        free(pvDelegates);
}

/*
void curl_shim_get_file_time(
    time_t t, int* yy, int* mm, int* dd, int* hh,
    int* mn, int* ss)
{
    struct tm* ptm = localtime(&t);
    *yy = ptm->tm_year + 1900;
    *mm = ptm->tm_mon + 1;
    *dd = ptm->tm_mday;
    *hh = ptm->tm_hour;
    *mn = ptm->tm_min;
    *ss = ptm->tm_sec;
}
*/

/*
int curl_shim_formadd(int* pvPosts,
    void* pvItems, int nCount)
{
    FARPROC fp = (FARPROC)curl_formadd;
    int argPairs = (nCount - 1) / 2 - 1;
    int stackFix = sizeof(int) * (nCount + 2);
    int retVal = 0;
    int* ppLast  = &pvPosts[1];
    int* ppFirst = &pvPosts[0];

    // here, wer're calling a vararg function
    __asm
    {
        push CURLFORM_END               ; we know to be last value
        mov  ecx, argPairs              ; number of arg pairs in ecx
        mov  ebx, pvItems               ; start of args
Args:   mov  eax, [ebx + 8 * ecx + 4]   ; argpair->value
        push eax                        ; get it onto stack
        mov  eax, [ebx + 8 * ecx]       ; argpair->code
        push eax                        ; put it on the stack
        dec  ecx                        ; decrement argpair counter
        jns  Args                       ; jump if not negative

        push ppLast                     ; push the last item
        push ppFirst                    ; and the first item

        call fp                         ; call curl_formadd
        mov  retVal, eax                ; store the return value
        add  esp, stackFix              ; fix the stack
    }

    return retVal;    
}
*/

void* curl_shim_alloc_fd_sets()
{
    // three contigous fd_sets: one for read, one for write,
    // and one for error
    void *pvfdSets;
    int nSize = 3 * sizeof(fd_set);
    pvfdSets = malloc(nSize);
    memset(pvfdSets, 0, nSize);
    return pvfdSets;
}

void curl_shim_free_fd_sets(void* pvfdSets)
{
    free(pvfdSets);
}

int curl_shim_multi_fdset(void* pvMulti,
    void* pvfdSets, int* maxFD)
{
    // cast return from GetProcAddress as a CPROC
    //CPROC pcp = (CPROC)curl_multi_fdset;
    fd_set* pfdSets = (fd_set*)pvfdSets;
    int retVal;

    FD_ZERO(&pfdSets[0]);
    FD_ZERO(&pfdSets[1]);
    FD_ZERO(&pfdSets[2]);
	retVal = curl_multi_fdset(pvMulti, &pfdSets[0], &pfdSets[1], &pfdSets[2], maxFD);
    return retVal;
}

int curl_shim_select(int maxFD, void* pvfdSets,
    int timeoutMillis)
{
    int retVal;
    struct timeval timeout;
    fd_set* pfdSets = (fd_set*)pvfdSets;

    timeout.tv_sec  = timeoutMillis / 1000;
    timeout.tv_usec = (timeoutMillis % 1000) * 1000;
	retVal = select(maxFD, &pfdSets[0], &pfdSets[1], &pfdSets[2],
        &timeout);    
    return retVal;
}

void* curl_shim_multi_info_read(void* pvHandle,
    int* nMsgs)
{
    // cast return from GetProcAddress as a CPROC
    List_T lst = NULL;
    //CPVPROC pcp = (CPVPROC)curl_multi_info_read;
    void* pvItem;
    int i, nLocalMsgs, j = 0;
    unsigned int *pnReturn = NULL;
    unsigned int *pnItem;

    *nMsgs = 0;
	while ((pvItem = curl_multi_info_read(pvHandle, &nLocalMsgs)) != NULL)
        lst = List_push(lst, pvItem);

    *nMsgs = List_length(lst);
    if (*nMsgs == 0)
        return NULL;
    pnReturn = (unsigned int*)malloc(3 * (*nMsgs) * sizeof(unsigned int));
    for (i = 0; i < (*nMsgs); i++)
    {
        lst = List_pop(lst, (void**)&pnItem);
        pnReturn[j++] = pnItem[0];
        pnReturn[j++] = pnItem[1];
        pnReturn[j++] = pnItem[2];            
    }
    List_free(&lst);
    return pnReturn;
}

void curl_shim_multi_info_free(void* pvMultiInfo)
{
    if (pvMultiInfo)
        free(pvMultiInfo);
}
