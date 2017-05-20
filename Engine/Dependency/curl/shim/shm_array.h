/**************************************************************************
 * The author of this software is David R. Hanson.
 *
 * Copyright (c) 1994,1995,1996,1997 by David R. Hanson. All Rights Reserved.
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose, subject to the provisions described below, without fee is
 * hereby granted, provided that this entire notice is included in all
 * copies of any software that is or includes a copy or modification of
 * this software and in all copies of the supporting documentation for
 * such software.
 *
 * THIS SOFTWARE IS BEING PROVIDED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
 * WARRANTY. IN PARTICULAR, THE AUTHOR DOES MAKE ANY REPRESENTATION OR
 * WARRANTY OF ANY KIND CONCERNING THE MERCHANTABILITY OF THIS SOFTWARE OR
 * ITS FITNESS FOR ANY PARTICULAR PURPOSE.
 *
 * David Hanson / drh@microsoft.com /
 * http://www.research.microsoft.com/~drh/
 * $Id: array.h,v 1.1 2005/02/17 22:47:24 jeffreyphillips Exp $
 **************************************************************************/

#ifndef SHIM_ARRAY_INCLUDED
#define SHIM_ARRAY_INCLUDED
#define T Array_T
typedef struct T *T;
extern T    Array_new (int length, int size);
extern void Array_free(T *array);
extern int  Array_length(T array);
extern int  Array_size  (T array);
extern void *Array_get(T array, int i);
extern void *Array_put(T array, int i, void *elem);
extern void Array_resize(T array, int length);
extern T    Array_copy  (T array, int length);
#undef T
#endif
