(*[omit:boyer-moore.c]*)
/*
    Simple implementation of the fast Boyer-Moore string search algorithm.

    By X-Calibre, 2002
    
    - slight modifications by davidk (main removed, cdecl added, casting for return types)
*/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>
#include <strsafe.h>

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport) 

char *BoyerMoore( unsigned char *data, unsigned int dataLength, unsigned char *string, unsigned int strLength ) {
    unsigned int skipTable[256], i;
    unsigned char *search;
    register unsigned char lastChar;

    if (strLength == 0)
        return NULL;

    // Initialize skip lookup table
    for (i = 0; i < 256; i++)
        skipTable[i] = strLength;

    search = string;

    // Decrease strLength here to make it an index
    i = --strLength;

    do
    {
        skipTable[*search++] = i;
    } while (i--);

    lastChar = *--search;

    // Start searching, position pointer at possible end of string.
    search = data + strLength;
    dataLength -= strLength+(strLength-1);

    while ((int)dataLength > 0 )
    {
        unsigned int skip;

        skip = skipTable[*search];
        search += skip;
        dataLength -= skip;
        skip = skipTable[*search];
        search += skip;
        dataLength -= skip;
        skip = skipTable[*search];

        if (*search != lastChar) /*if (skip > 0)*/
        {
            // Character does not match, realign string and try again
            search += skip;
            dataLength -= skip;
            continue;
        }

        // We had a match, we could be at the end of the string
        i = strLength;

        do
        {
            // Have we found the entire string?
            if (i-- == 0)
                return (char * )search;
        } while (*--search == string[i]);

        // Skip past the part of the string that we scanned already
        search += (strLength - i + 1);
        dataLength--;
    }

    // We reached the end of the data, and didn't find the string
    return NULL;
}

EXTERN_DLL_EXPORT
char *boyerMoore(unsigned char *data, unsigned char *search) {

    char *str = BoyerMoore( data, strlen((const char *)data), search, strlen((const char *)search) );

    if (str == NULL)
       return "String not found";
    else
       return str;

    return "";
} 
(*[/omit]*)
module native = 
  module string_search = 
    open System.Text
    open System.Runtime.InteropServices
    
    [<DllImport(@"boyermoore.dll", EntryPoint="boyerMoore", CharSet = CharSet.Ansi)>]
    extern nativeint boyerMoore(nativeint data, nativeint search)
    
    let alloc_a (data : string) = 
      let strbuf = Encoding.UTF8.GetBytes data
      let buffer = Marshal.AllocHGlobal(strbuf.Length + 1)
      Marshal.Copy(strbuf, 0, buffer, strbuf.Length)
      Marshal.WriteByte( buffer + (nativeint strbuf.Length), 0uy)
      buffer

    let bMoore data search =
      let d,s = alloc_a <| data, alloc_a <| search
      let x = Marshal.PtrToStringAnsi(boyerMoore(d,s) )
      Marshal.FreeHGlobal d
      Marshal.FreeHGlobal s 
      x
    
native.string_search.bMoore "aaaabouaaa384982n chwercoiewar45u0943 twert3aaaaaaMarabou t9034u5t09t8493t43vkdsropgb" "Marabou"