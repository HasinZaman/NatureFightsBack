using System;
using System.Runtime.InteropServices;

public class OpenAI
{
    // Import the Rust DLL
    [DllImport("open_ai_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr prompt_query_extern(string input);

    [DllImport("open_ai_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void free_string(IntPtr ptr);

    public static string prompt(string prompt)
    {
        IntPtr ptr = prompt_query_extern(prompt);

        string response = Marshal.PtrToStringAnsi(ptr);

        free_string(ptr);

        return response.Trim();
    }
}
