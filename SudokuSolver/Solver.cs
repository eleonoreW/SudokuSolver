using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Solver
    {
        public Dictionary<Tuple<int, int>, List<int>> cspDepart;
        public int taille;
        private int[] domaine;

        public Solver(Sudoku sudoku)
        {
            taille = sudoku.GetTaille();
            domaine = new int[taille];
            cspDepart = new Dictionary<Tuple<int, int>, List<int>>();
            // Domaine des solution pour 1 case
            for (int i = 0; i < taille; i = i + 1)
            {
                domaine[i]= i + 1;
            }

            // A partir de la grille du sudoku, on détermine quelles sont les valeurs possible pour chaque case
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    Tuple<int, int> coord = new Tuple<int, int>(i, j);
                    List<int> valeursPossibles = new List<int>();
                    if (sudoku.GetGrille()[i, j] == 0)
                    {
                        valeursPossibles.AddRange(domaine); // Toute les valeurs du domaine sont possible dans cette case
                    }
                    else
                    {
                        valeursPossibles.Add(sudoku.GetGrille()[i, j]); // La seule valeur possible est la valeur imposée
                    }
                    cspDepart.Add(coord, valeursPossibles);
                }
            }
        }

        public static int[,] cspToGrille(Dictionary<Tuple<int, int>, List<int>> csp, int taille)
        {
            int[,] grille = new int[taille, taille];
            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    if(csp[Tuple.Create(i, j)].Count()> 1)
                    {
                        grille[i, j] = 0;
                    }
                    else
                    {
                        grille[i, j] = csp[Tuple.Create(i, j)].First();
                    }
                }
            }
            return grille;
        }

    }
}
