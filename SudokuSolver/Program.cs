using System;
using System.IO;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            /*int tailleSudoku = 9;
            Sudoku sudoku = new Sudoku(tailleSudoku);
            sudoku.SetGrille(new int[9, 9] {
                                        { 0 , 3 , 0 , 4 , 0 , 0 , 2 , 0 , 0 },
                                        { 1 , 0 , 0 , 8 , 2 , 6 , 0 , 0 , 9},
                                        { 0 , 0 , 0 , 7 , 0 , 9 , 0 , 0 , 0},
                                        { 4 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
                                        { 0 , 0 , 9 , 0 , 0 , 1 , 0 , 8 , 7},
                                        { 0 , 0 , 5 , 0 , 0 , 0 , 3 , 4 , 0},
                                        { 0 , 5 , 0 , 0 , 0 , 0 , 0 , 0 , 0},
                                        { 0 , 9 , 0 , 0 , 0 , 0 , 0 , 0 , 3},
                                        { 7 , 0 , 0 , 0 , 0 , 2 , 1 , 0 , 4 } });*/
            int tailleSudoku = 10;
            Sudoku sudoku = new Sudoku(tailleSudoku);
            sudoku.SetGrille(new int[10, 10] {
                                        { 8 , 7 , 0 , 0 , 0 , 5 ,10 , 0 , 0, 9 },
                                        { 0 , 5 , 9 , 0 , 0 , 0 , 0 , 6 , 4, 0 },
                                        { 0 , 3 , 1 , 9 , 0 , 0 , 0 ,10 , 7, 0 },
                                        { 4 , 0 , 0 , 5 , 2 , 8 , 0 , 0 , 9, 0 },
                                        { 5 , 8 , 6 , 0 , 0 , 0 , 1 , 3 , 0, 0 },
                                        { 0 , 2 ,10 , 7 , 0 , 0 , 8 , 0 , 0, 0 },
                                        { 0 , 6 , 0 , 0 ,10 , 4 , 0 , 7 , 0, 3 },
                                        { 1 , 0 , 2 , 0 , 0 , 9 , 5 , 0 , 0, 0 },
                                        { 0 , 0 , 8 , 6 , 0 , 3 , 9 , 0 , 0, 2 },
                                        { 0 , 0 , 0 ,10 , 4 , 7 , 8 , 5 , 0, 0 }});
            Solver solveur = new Solver(sudoku);
            Console.WriteLine(Sudoku.GrilleToString(Solver.cspToGrille(solveur.cspDepart, solveur.taille), solveur.taille) + "\n");
            
            Console.WriteLine(Sudoku.GrilleToString(solveur.solve(), solveur.taille) + "\n");

            Console.ReadKey();

        }

    }
}
