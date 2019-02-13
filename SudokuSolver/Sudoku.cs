using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Sudoku
    {
        public int[,] grid = new int[9,9];
        override
        public string ToString()
        {
            String sudoku ="";
            for(int i=0; i< 9; i++) {
                for (int j=0; j< 9; j++) {
                    if (grid[i, j] == 0)
                        sudoku = sudoku + " .";
                    else
                        sudoku = sudoku + " "+grid[i,j].ToString();
                }
                sudoku = sudoku + "\n";
            }
            return sudoku;
        }
    }



}
