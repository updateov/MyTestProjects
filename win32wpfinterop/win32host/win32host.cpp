#include <Windows.h>

// Constants
namespace {
   TCHAR * windowClassName = TEXT("win32host");
   TCHAR * windowTitle     = TEXT("Win32 Host (Win32 WPF Interop)");
   int          windowWidth     = 200;
   int          windowHeight    = 100;
}

// Window message procedure
LRESULT CALLBACK WindowProc(
  HWND hwnd,
  UINT uMsg,
  WPARAM wParam,
  LPARAM lParam)
{
   switch (uMsg) {
   case WM_DESTROY:
      ::PostQuitMessage (0);
      return 0;
      break;
   default:
      return ::DefWindowProc (hwnd, uMsg, wParam, lParam);
   }
}

// Main program entry  point
[System::STAThread] // This is IMPORTANT, but it's for in C++/CLR  only
int CALLBACK WinMain(
   HINSTANCE hInstance,
   HINSTANCE hPrevInstance,
   LPSTR lpCmdLine,
   int nCmdShow)
{
   // Register our Window class
   ::WNDCLASS wndclass;
   wndclass.style = CS_VREDRAW | CS_HREDRAW;
   wndclass.lpfnWndProc = &WindowProc;
   wndclass.cbClsExtra = 0;
   wndclass.cbWndExtra = 0;
   wndclass.hInstance = hInstance;
   wndclass.hIcon = NULL;
   wndclass.hCursor = NULL;
   wndclass.hbrBackground = reinterpret_cast <HBRUSH> (COLOR_BTNFACE + 1);
   wndclass.lpszMenuName = NULL;
   wndclass.lpszClassName =windowClassName;
   ::RegisterClass(&wndclass);

   // Create our main, raw win32 API window
   // We create the window invisible (meaning that we do not provide WS_VISIBLE as the window style parameter), because making it visible and then
   // adding a HwndSource will make it flicker.
   HWND mainWindow = ::CreateWindow(
      windowClassName,
      windowTitle,
      0,
      CW_USEDEFAULT,
      CW_USEDEFAULT,
      windowWidth,
      windowHeight,
      NULL,
      NULL,
      hInstance,
      0);

   // Here comes the magic - create a HwndSource object. This is a first-class WPF citizen AND a first-class Win32 citizen at the same time --
   // you can modify it with e.g. ::SetWindowPos(), but use it as a WPF control's parent at the same time.
   System::Windows::Interop::HwndSource ^ hwndSource = gcnew System::Windows::Interop::HwndSource (
      CS_VREDRAW | CS_HREDRAW, // window class styles
      WS_CHILD,                // window flags
      0,                       // extended windows styles
      0,                       // x position (will be overridden later)
      0,                       // y position (will be overridden later)
      "WPF Interop",           // window title (not visible)
      static_cast <System::IntPtr> (mainWindow)); // parent window

   HWND hwndSourceHandle = reinterpret_cast <HWND> (hwndSource->Handle.ToInt32 ());
   ::SetWindowPos (
      hwndSourceHandle,
      NULL, // ignored
      10,   // x position
      10,   // y position
      180,  // width
      80,   // height
      SWP_NOZORDER | SWP_SHOWWINDOW);

   // We will add a label to the HwndSource so that we know it actually works
   System::Windows::Controls::Label ^ label = gcnew System::Windows::Controls::Label ();
   label->Content = gcnew System::String ("WPF Label -- it works!");
   label->Background = System::Windows::Media::Brushes::White;

   // And here is another important piece of the puzzle: Use the RootVisual to provide ONE single WPF element as the content
   hwndSource->RootVisual = label;

   // Now that setting up the HwndSource is finished, we can finally make our window visible
   ::ShowWindow (mainWindow, SW_SHOW);

   // Start message processing
   ::MSG message;
   while (::GetMessageA(&message, 0, 0, 0)) {
      switch (message.message) {
      case WM_QUIT:
         break;
      default:
         ::TranslateMessage(& message);
         ::DispatchMessage(& message);
         break;
      }
   }
   return 0;
}
