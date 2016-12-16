//===============================================================================
// Copyright (c) Serhiy Perevoznyk.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Karna.Windows.UI
{
    internal static class NativeMethods
    {
        public const int WM_KILLTIMER = 0x402;
        public const int WM_TIMER = 0x113;

        public const int CM_BASE = 0xB000;
        public const int CM_LEFT = CM_BASE + 1;
        public const int CM_RIGHT = CM_BASE + 2;
        public const int CM_TIMER = CM_BASE + 3;
        public const int CM_ROTATE = CM_BASE + 4;
        public const int CM_DOWN = CM_BASE + 5;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool KillTimer(HandleRef hwnd, HandleRef idEvent);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetTimer(HandleRef hWnd, HandleRef nIDEvent, int uElapse, HandleRef lpTimerProc);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    }
}
