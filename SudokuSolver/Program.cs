﻿using System;
using System.IO;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Sudoku s = new Sudoku();
            s.grid = new int[9, 9] {
                                        { 0 , 3 , 0 , 4 , 0 , 0 , 2 , 0 , 0 },
                                        { 1 , 0 , 0 , 8 , 2 , 6 , 0 , 0 , 9},
                                        { 0 , 0 , 0 , 7 , 0 , 9 , 0 , 0 , 0},
                                        { 4 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
                                        { 0 , 0 , 9 , 0 , 0 , 1 , 0 , 8 , 7},
                                        { 0 , 0 , 5 , 0 , 0 , 0 , 3 , 4 , 0},
                                        { 0 , 5 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
                                        { 0 , 9 , 0 , 0 , 0 , 0 , 0 , 0 , 3},
                                        { 7 , 0 , 0 , 0 , 0 , 2 , 1 , 0 , 4 } };
            Console.WriteLine(s.ToString());
            while(true) { }

        }

    }
}
