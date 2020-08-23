using System;
using System.Runtime.InteropServices;

namespace SnakeCs
{
    namespace Gfx
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Coord
        {
            public short X { get; set; }
            public short Y { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public short StartX { get; set; }
            public short StartY { get; set; }
            public short EndX { get; set; }
            public short EndY { get; set; }
        }


        public enum ForegroundColor : short
        {
            Black = 0x0000,
            Blue = 0x0001,
            Green = 0x0002,
            Red = 0x0004,
            // Aqua = Blue | Green
            Aqua = 0x0001 | 0x0002,
            // Magenta = Blue | Red
            Magenta = 0x0001 | 0X0004,
            // You get the idea
            Yellow = 0x0002 | 0x0004,
            White = 0x0001 | 0x0002 | 0x0004,

            // Bright variants
            BrightBlue = 0x0001 | 0x0008,
            BrightGreen = 0x0002 | 0x0008,
            BrightRed = 0x0004 | 0x0008,
            BrightAqua = 0x0001 | 0x0002 | 0x0008,
            BrightMagenta = 0x0001 | 0x0004 | 0x0008,
            BrightYellow = 0x0002 | 0x0004 | 0x0008
        }

        public enum BackgroundColor : short
        {
            Black = 0x0000,
            Blue = 0x0010,
            Green = 0x0020,
            Red = 0x0040,
            // Aqua = Blue | Green
            Aqua = 0x0010 | 0x0020,
            // Magenta = Blue | Red
            Magenta = 0x0010 | 0X0040,
            // You get the idea
            Yellow = 0x0020 | 0x0040,
            White = 0x0010 | 0x0020 | 0x0040,

            // Bright variants
            BrightBlue = 0x0010 | 0x0080,
            BrightGreen = 0x0020 | 0x0080,
            BrightRed = 0x0040 | 0x0080,
            BrightAqua = 0x0010 | 0x0020 | 0x0080,
            BrightMagenta = 0x0010 | 0x0040 | 0x0080,
            BrightYellow = 0x0020 | 0x0040 | 0x0080
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        internal struct CharInfo
        {
            [FieldOffset(0)]
            public char UnicodeChar;
            [FieldOffset(0)]
            public byte AsciiChar;
            [FieldOffset(2)]
            public short Attributes;
        }

        internal struct ConsoleCursorInfo
        {
            public short Size;
            public bool Visible;
        }

        internal class Platform
        {
            [DllImport("kernel32.dll", ExactSpelling = true)]
            public static extern IntPtr GetConsoleWindow();

            public enum StdHandle : short
            {
                Input = -10,
                Output = -11,
                Error = -12
            }

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr GetStdHandle(StdHandle handleId);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool WriteConsoleOutputW(IntPtr consoleHandle, CharInfo[] buffer, Coord bufferSize, Coord offset, ref Rect dstRect);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool SetWindowText(IntPtr Hwnd, string title);

            [DllImport("kernel32.dll")]
            public static extern short GetLastError();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleCursorInfo(IntPtr consoleHandle, ref ConsoleCursorInfo cursorInfo);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern long SetWindowLongPtrW(IntPtr Hwnd, int index, long value);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern long GetWindowLongPtrW(IntPtr Hwnd, int index);
        }

        class ConsoleGraphics
        {
            private IntPtr consoleWindowHandle;
            private IntPtr consoleHandle;

            private CharInfo[] writeBuffer;
            private Coord writeBufferSize;

            // Resizes console to specified width and height
            public ConsoleGraphics(int width, int height)
            {
                consoleWindowHandle = Platform.GetConsoleWindow();
                consoleHandle = Platform.GetStdHandle(Platform.StdHandle.Output);
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(width, height);

                writeBuffer = new CharInfo[width * height];
                writeBufferSize = new Coord { X = (short)width, Y = (short)height };
            }

            // Waits until any key is pressed, then returns that key
            public ConsoleKey WaitForKey()
            {
                return Console.ReadKey().Key;
            }

            // This clears both the input and output buffer
            public void Clear()
            {
                ClearWriteBuffer();
                Console.Clear();
            }

            public void SetTitle(string title)
            {
                Platform.SetWindowText(consoleWindowHandle, title);
            }

            public void SetAllowManualResize(bool allow)
            {
                const long WS_SIZEBOX = 0x00040000;
                const int GWL_STYLE = -16;
                long originalStyle = Platform.GetWindowLongPtrW(consoleWindowHandle, GWL_STYLE);
                long newStyle = allow ? originalStyle | WS_SIZEBOX : originalStyle & ~WS_SIZEBOX;
                Platform.SetWindowLongPtrW(consoleWindowHandle, GWL_STYLE, newStyle);
            }

            public void SetCursorShown(bool shown)
            {
                ConsoleCursorInfo cursorInfo = new ConsoleCursorInfo { Size = 1, Visible = shown };
                Platform.SetConsoleCursorInfo(consoleHandle, ref cursorInfo);
            }

            public void DrawLine(short xStart, short yStart, short xEnd, short yEnd, BackgroundColor color = BackgroundColor.White)
            {
                short dx = (short)(xEnd - xStart);
                short dy = (short)(yEnd - yStart);

                if (dy == 0)
                {
                    // Special case for horizontal lines to avoid division by zero.
                    // We can implement this with a rectangle
                    DrawRectangle(xStart, yStart, dx, 1, color);
                    return;
                }

                float dxdy = (float)dx / dy;
                for (short y = yStart; y <= yEnd; ++y)
                {
                    short ySinceStart = (short)Math.Abs(y - yStart);
                    short x = (short)(xStart + ySinceStart * dxdy);
                    DrawPoint(x, y, color);
                }
            }

            public void DrawRectangle(short xPos, short yPos, short width, short height, BackgroundColor color = BackgroundColor.White)
            {
                // We can draw a rectangle by drawing a bunch of spaces
                for (short y = yPos; y < yPos + height; ++y)
                {
                    for (short x = xPos; x < xPos + width; ++x)
                    {
                        DrawPoint(x, y, color);
                    }
                }
            }

            public void DrawPoint(short xPos, short yPos, BackgroundColor color = BackgroundColor.White)
            {
                Write(' ', xPos, yPos, ForegroundColor.Black, color);
            }

            // Writes a single character
            public void Write(char character, short xPos, short yPos,
                ForegroundColor foregroundColor = ForegroundColor.White, BackgroundColor backgroundColor = BackgroundColor.Black)
            {
                ref CharInfo info = ref WriteBufferAt(xPos, yPos);
                info.UnicodeChar = character;
                info.Attributes = (short)((short)backgroundColor | (short)foregroundColor);
            }

            // If the text is longer than the width of the console, it will wrap around, starting at position wrapXStart
            public void WriteText(string text, short xPos, short yPos, short wrapXStart = 0,
                ForegroundColor foregroundColor = ForegroundColor.White, BackgroundColor backgroundColor = BackgroundColor.Black)
            {
                short x = xPos;
                short y = yPos;
                foreach (char character in text)
                {
                    Write(character, x, y, foregroundColor, backgroundColor);
                    // Advance position
                    ++x;
                    if (x >= writeBufferSize.X)
                    {
                        x = wrapXStart;
                        ++y;
                    }
                    // If y is out of bounds, the text doesn't fit
                    if (y >= writeBufferSize.Y)
                    {
                        throw new ArgumentOutOfRangeException("Text does not fit the console window");
                    }
                }
            }

            // Force writes to be visible.
            public void Flush()
            {
                Rect dstRect = new Rect { StartX = 0, StartY = 0, EndX = writeBufferSize.X, EndY = writeBufferSize.Y };
                Coord writeBufferOffset = new Coord { X = 0, Y = 0 };
                bool success = Platform.WriteConsoleOutputW(consoleHandle, writeBuffer, writeBufferSize, writeBufferOffset, ref dstRect);
                ClearWriteBuffer();
                if (!success)
                {
                    short errorCode = Platform.GetLastError();
                    throw new Exception($"WriteConsoleOutputW failed with error code {errorCode}");
                }
            }

            private void ClearWriteBuffer()
            {
                for (uint idx = 0; idx < writeBufferSize.X * writeBufferSize.Y; ++idx)
                {
                    writeBuffer[idx].UnicodeChar = (char)0;
                    writeBuffer[idx].Attributes = 0;
                }
            }

            private ref CharInfo WriteBufferAt(short xPos, short yPos)
            {
                return ref writeBuffer[yPos * writeBufferSize.X + xPos];
            }
        }

    }
}
