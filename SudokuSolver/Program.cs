using System;
using System.IO;


namespace SudokuSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			// Recuperation des lignes du fichier d'entree
			string[] inputLines = File.ReadAllLines("../../Input.txt"); // Chemin temporaire pour debug => bin\Debug\Input.txt

			// Taille de la grille à la ligne 0
			int gridSize = int.Parse(inputLines[0]);

			int[,] grid = new int[gridSize, gridSize];

			// Remplissage de la grille à partir des donnees
			for (int i = 0; i < gridSize; i++) {
				// Donnees de la grille dans les lignes suivantes
				string[] data = inputLines[i + 1].Split(',');
				int j = 0;
				foreach (string value in data) {
					grid[i, j] = int.Parse(value);
					j++;
				}
			}

			Sudoku sudoku = new Sudoku(gridSize);
			sudoku.SetGrille(grid);

			// OLD ---
			//int tailleSudoku = 9;
			//Sudoku sudoku = new Sudoku(tailleSudoku);
			//sudoku.SetGrille(new int[,] {
			//                            { 0 , 3 , 0 , 4 , 0 , 0 , 2 , 0 , 0 },
			//                            { 1 , 0 , 0 , 8 , 2 , 6 , 0 , 0 , 9},
			//                            { 0 , 0 , 0 , 7 , 0 , 9 , 0 , 0 , 0},
			//                            { 4 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
			//                            { 0 , 0 , 9 , 0 , 0 , 1 , 0 , 8 , 7},
			//                            { 0 , 0 , 5 , 0 , 0 , 0 , 3 , 4 , 0},
			//                            { 0 , 5 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
			//                            { 0 , 9 , 0 , 0 , 0 , 0 , 0 , 0 , 3},
			//                            { 7 , 0 , 0 , 0 , 0 , 2 , 1 , 0 , 4 } });
			// ---

			Solver solveur = new Solver(sudoku);
			Console.WriteLine(Sudoku.GrilleToString(Solver.cspToGrille(solveur.cspDepart, solveur.taille), solveur.taille) + "\n");
			Console.WriteLine(Sudoku.GrilleToString(solveur.solve(), solveur.taille) + "\n");

			Console.ReadKey();
		}
	}
}
