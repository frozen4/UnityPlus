#ifndef SHIM_LIBCURLSHIM_INCLUDED
#define SHIM_LIBCURLSHIM_INCLUDED

#ifdef _WIN32
#define SHIM_STD_CALL __stdcall
#else
#define SHIM_STD_CALL
#endif

void curl_shim_initialize();
void curl_shim_cleanup();
char* curl_shim_get_version_char_ptr(
	void* p, int offset);
int curl_shim_get_version_int_value(
	void* p, int offset);
int curl_shim_get_number_of_protocols(
	void* p, int protOffset);
char* curl_shim_get_protocol_string(
	void* p, int protOffset, int nProt);
void* curl_shim_alloc_strings();
char* curl_shim_add_string(void* p, const char* pInStr);
void curl_shim_free_strings(void* p);
void* curl_shim_add_string_to_slist(
	void* lst, const char* pInStr);
void* curl_shim_get_string_from_slist(
	void* lst, char** ppString);
void curl_shim_free_slist(void* lst);
int curl_shim_install_delegates(void* handle,
	void* pvThis, void* pvWriteDel, void* pvReadDel, void* pvProgDel,
	void* pvDebugDel, void* pvHeaderDel, void* pvSSLContextDel,
	void* pvIoctlDel);
void curl_shim_cleanup_delegates(void* pvThis);
int curl_shim_install_share_delegates(
	void* handle, void* pvThis, void* pvLockDel, void* pvUnlockDel);
void curl_shim_cleanup_share_delegates(void* pvThis);
// void curl_shim_get_file_time(
// 	time_t t, int* yy, int* mm, int* dd, int* hh,
// 	int* mn, int* ss);
//int curl_shim_formadd(int* pvPosts,	void* pvItems, int nCount);
void* curl_shim_alloc_fd_sets();
void curl_shim_free_fd_sets(void* pvfdSets);
int curl_shim_multi_fdset(void* pvMulti,
	void* pvfdSets, int* maxFD);
int curl_shim_select(int maxFD, void* pvfdSets,
	int timeoutMillis);
void* curl_shim_multi_info_read(void* pvHandle,
	int* nMsgs);
void curl_shim_multi_info_free(void* pvMultiInfo);

#endif