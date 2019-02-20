using System;


namespace SudokuSolver
{
	class Sudoku
	{
		private int tailleGrille;
		private int[,] grille;

		public Sudoku(int taille)
		{
			tailleGrille = taille;
			grille = new int[tailleGrille, tailleGrille];

		}

		public static string GrilleToString(int[,] grid, int taille)
		{
			String sudoku = "";
			for (int i = 0; i < taille; i++) {
				for (int j = 0; j < taille; j++) {
					if (grid[i, j] == 0) {
						sudoku = sudoku + " .";
					} else {
						sudoku = sudoku + " " + grid[i, j].ToString();
					}
				}
				sudoku = sudoku + "\n";
			}
			return sudoku;
		}

		public int GetTaille()
		{
			return tailleGrille;
		}

		public int[,] GetGrille()
		{
			return grille;
		}
		public void SetGrille(int[,] g)
		{
			grille = g;
		}

	}



}
