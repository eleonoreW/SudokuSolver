using System;


namespace SudokuSolver
{
	class Sudoku
	{
		public readonly int gridSize;
		public int[,] grid;

		public Sudoku(int size)
		{
			gridSize = size;
			grid = new int[gridSize, gridSize];

		}

		public static string GridToString(int[,] grid, int size)
		{
			string sudoku = "";
			for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {
					sudoku = grid[i, j] == 0 ? sudoku + " ." : sudoku + " " + grid[i, j].ToString();
				}
				sudoku = sudoku + "\n";
			}
			return sudoku;
		}

	}

}
