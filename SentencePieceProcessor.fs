namespace GiraffeApp.SentencePieceProcessor

open System.Runtime.InteropServices
open System.Text

#nowarn "9"

//import sentencepiece::SentencePieceProcessorXXX class
module InteropWithNative =
    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorCreate()

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void SentencePieceProcessorDestroy(nativeint sp)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorLoad(nativeint sp, string model)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorEncode(nativeint sp, string input, int* len, nativeint* ids)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool SentencePieceProcessorStatusOk(nativeint sp)
    
    
    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void SentencePieceProcessorStatusMessage(nativeint sp, int* len, StringBuilder result)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorDecode(nativeint sp, int len, nativeint ids, nativeint result, nativeint result_len )

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorSetEncodeExtraOptions(nativeint sp, string options)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint SentencePieceProcessorSampleEncode(nativeint sp, string input, int nbest_size, float alpha, nativeint ids_len, nativeint ids)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void SentencePieceProcessorStatusDestroy(nativeint spst)

    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void SentencePieceProcessorDestroyIntArray(nativeint pArray)
    
    [<DllImport(@"libsentencepiece_proxy.so", CallingConvention = CallingConvention.Cdecl)>]
    extern int SentencePieceProcessorGetPieceSize(nativeint sp)

