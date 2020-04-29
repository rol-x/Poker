using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    /// <summary>
    /// Static class for simple console operations.
    /// </summary>
    static class ConsoleEditor
    {
        /// <summary>
        /// Clear last n lines in console, from current position.
        /// </summary>
        public static void ClearLastLines(int n)
        {
            Console.CursorTop -= n;
            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < Console.BufferWidth - 1; j++)
                    Console.Write(" ");
                Console.WriteLine();
            }
            Console.CursorTop -= n;
        }
    }
}
