﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MuteWarning
{
    internal class WindowHelper
    {
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int) intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        public enum WindowStylesOperations
        {
            Set,
            Remove
        }

        public static void ChangeWindowStyles(Window window, WindowStylesOperations operation, params ExtendedWindowStyles[] styles)
        {
            if (styles?.Any() != true)
            {
                return;
            }

            WindowInteropHelper wndHelper = new WindowInteropHelper(window);
            int exStyle = (int) GetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GWL_EXSTYLE);

            foreach (var style in styles)
            {
                if (WindowStylesOperations.Set == operation)
                {
                    exStyle |= (int) style;
                }
                else
                {
                    exStyle &= ~(int) style;
                }
            }

            SetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GWL_EXSTYLE, (IntPtr) exStyle);
        }
    }
}