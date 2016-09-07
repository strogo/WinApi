﻿using System;
using System.Runtime.Remoting.Messaging;
using WinApi.DwmApi;
using WinApi.Gdi32;
using WinApi.User32;

namespace WinApi.XWin
{
    public struct WindowMessage
    {
        public WM Id;
        public IntPtr WParam;
        public IntPtr LParam;
        public IntPtr Result;
    }

    public class Window : NativeWindow
    {
        protected override void OnSourceInitialized(bool isFactoryInitialized)
        {
            base.OnSourceInitialized(isFactoryInitialized);
            if (!isFactoryInitialized)
            {
                OnCreate();
            }
        }

        protected virtual void OnCreate() {}

        protected virtual void OnActivate() {}

        protected virtual void OnClose() {}

        protected virtual void OnDestroy() {}

        protected virtual void OnPaint(IntPtr hdc, Rectangle paintRectangle, bool shouldErase) {}

        protected virtual bool OnMessageProcessDefault(ref WindowMessage msg)
        {
            return true;
        }

        protected virtual bool OnMessage(ref WindowMessage msg)
        {
            switch (msg.Id)
            {
                case WM.ACTIVATE:
                {
                    OnActivate();
                    return true;
                }
                case WM.CREATE:
                {
                    OnCreate();
                    return true;
                }
                case WM.DESTROY:
                {
                    OnDestroy();
                    return true;
                }
                case WM.CLOSE:
                {
                    OnClose();
                    return true;
                }
                case WM.PAINT:
                {
                    PaintStruct ps;
                    var hdc = User32Methods.BeginPaint(Handle, out ps);
                    OnPaint(hdc, ps.PaintRectangle, ps.ShouldEraseBackground);
                    User32Methods.EndPaint(Handle, ref ps);
                    return true;
                }
                default:
                {
                    return OnMessageProcessDefault(ref msg);
                }
            }
        }

        protected internal override IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var wmsg = new WindowMessage
            {
                Id = (WM) msg,
                WParam = wParam,
                LParam = lParam,
                Result = IntPtr.Zero
            };

            return OnMessage(ref wmsg) ? base.WindowProc(hwnd, msg, wParam, lParam) : wmsg.Result;
        }
    }

    public class MainWindow : Window
    {
        protected override bool OnMessageProcessDefault(ref WindowMessage msg)
        {
            IntPtr res;
            if (DwmApiMethods.DwmDefWindowProc(Handle, (uint) msg.Id, msg.WParam, msg.LParam, out res) > 0)
            {
                msg.Result = res;
                return false;
            }
            return base.OnMessageProcessDefault(ref msg);
        }

        protected override void OnDestroy()
        {
            User32Methods.PostQuitMessage(0);
            base.OnDestroy();
        }
    }
}