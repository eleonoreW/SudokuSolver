using System;
using System.IO;


namespace SudokuSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			// Recuperation des lignes du fichier d'entree
			string[] inputLines = File.ReadAllLines("Input.txt");

			// Taille de la grille à la ligne 0
			int gridSize = int.Parse(inputLines[0]);

			int[,] grid = new int[gridSize, gridSize];

			// Remplissage de la grille à partir des donnees
			int dataLinesLength = inputLines.Length - 1;
			if (dataLinesLength == gridSize) {
				for (int i = 0; i < gridSize; i++) {
					// Donnees de la grille dans les lignes suivantes
					string[] data = inputLines[i + 1].Split(',');
					if (data.Length == gridSize) {
						int j = 0;
						foreach (string value in data) {
							grid[i, j] = int.Parse(value);
							j++;
						}
					} else {
						// Nb colones != taille
						Console.WriteLine("Nombre de donnees en colonnes (" + data.Length + ") different de la taille de la grille (" + gridSize + ") \n");
						End();
						return;
					}
				}
			} else {
				// Nb lignes != taille
				Console.WriteLine("Nombre de donnees en lignes (" + dataLinesLength + ") different de la taille de la grille (" + gridSize + ") \n");
				End();
				return;
			}

			Sudoku sudoku = new Sudoku(gridSize);
			sudoku.grid = grid;


			Solver solveur = new Solver(sudoku);
			Console.WriteLine(Sudoku.GridToString(Solver.CspToGrille(solveur.cspDepart, solveur.taille), solveur.taille) + "\n");
			Console.WriteLine(Sudoku.GridToString(solveur.Solve(), solveur.taille) + "\n");
			End();


			void End()
			{
				Console.WriteLine("Appuyer sur une touche pour quitter ..." + "\n");
				Console.ReadKey();
			}

		}
	}
}
