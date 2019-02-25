using System;
using System.Collections.Generic;
using System.Linq;


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
			// Domaine des solution pour n case
			for (int i = 0; i < taille; i = i + 1) {
				domaine[i] = i + 1;
			}

			// A partir de la grille du sudoku, on détermine quelles sont les valeurs possible pour chaque case, 0: toute les valeurs possible, sinon le chiffre
			for (int i = 0; i < taille; i++) {
				for (int j = 0; j < taille; j++) {
					Tuple<int, int> coord = new Tuple<int, int>(i, j);
					List<int> valeursPossibles = new List<int>();
					if (sudoku.GetGrille()[i, j] == 0) {
						valeursPossibles.AddRange(domaine); // Toute les valeurs du domaine sont possible dans cette case
					} else {
						valeursPossibles.Add(sudoku.GetGrille()[i, j]); // La seule valeur possible est la valeur imposée
					}
					cspDepart[coord] = valeursPossibles;

				}
			}

		}
        
		public static int[,] CspToGrille(Dictionary<Tuple<int, int>, List<int>> csp, int taille)
		{
			int[,] grille = new int[taille, taille];
			for (int i = 0; i < taille; i++) {
				for (int j = 0; j < taille; j++) {
					if (csp[Tuple.Create(i, j)].Count() > 1) {
						grille[i, j] = 0;
					} else {
						grille[i, j] = csp[Tuple.Create(i, j)].First();
					}
				}
			}
			return grille;
		}
        
        /// <summary>
        /// Fonction à appeller pour résoudre un Sudoku
        /// </summary>
        /// <returns>La grille correspondant au Sudoku résolu ou null si le sudoku n'est pas résolvable.</returns>
        public int[,] Solve()
		{
			if (AC3(cspDepart)) {
				if (EstValide(cspDepart)) {
					return Solver.CspToGrille(cspDepart, taille);
				} else {
					Dictionary<Tuple<int, int>, List<int>> cspResultat = Backtracking_search();

					return Solver.CspToGrille(cspResultat, taille);
				}
			} else {
				Console.WriteLine("Grille impossible à résoudre");
				return null;
			}
        }
        /// <summary>
        /// Point d'entrée de la méthode récursive de backtracking.
        /// </summary>
        /// <returns> Le CSP final</returns>
        private Dictionary<Tuple<int, int>, List<int>> Backtracking_search()
		{
			return Recursive_backtracking(cspDepart);
		}
        /**
        * Recherche de la solution avec la technique du backtracking
        * @param csp : CSP a étudier
        * @return null si echec / CSP mis à jour si réussite
        */


        /// <summary>
        /// Backtracking Search
        /// </summary>
        /// <param name="csp">CSP à évaluer</param>
        /// <returns>Le CSP modifié si réussi, null sinon</returns>
        private Dictionary<Tuple<int, int>, List<int>> Recursive_backtracking(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// si on a assigné toute les valeurs de la grille
			if (EstValide(csp)) {
				return csp;
			}

			Tuple<int, int> caseSelec = SelectUnassignedVariable(csp);

			Dictionary<Tuple<int, int>, List<int>> newCSP;
			foreach (int valeur in OrderDomainValues(csp, caseSelec)) {
				// Si la valeur respecte les contraintes pour cette case
				if (csp[caseSelec].Contains(valeur)) {
					Dictionary<Tuple<int, int>, List<int>> mapTest = CopieCSP(csp);
					Dictionary<Tuple<int, int>, List<int>> saveMap = CopieCSP(csp);
					// modifie le domaine sur toutes les cases concernées
					// test inference
					if (Consistent(mapTest, caseSelec, valeur)) {
						newCSP = Recursive_backtracking(mapTest);
						if (newCSP != null) {
							return newCSP;
						}
					}
					// récupère l'ancienne carte si les tests ont échouées
					csp = saveMap;
					if (csp[caseSelec].Count == 1) {
						return null;//si c'etait la seule valeur possible et qu'elle est pas possible
					} else {
						csp[caseSelec].Remove(valeur);
					}
				}
			}
			return null;
		}
        
        /// <summary>
        /// On choisit la case pour laquelle on va déterminer la valeur. Pour chosiir la meilleure case possible on utilise MRV,Degree Heuristic (qui utilise Least Constraining value)
        /// </summary>
        /// <param name="csp">CSP Courant</param>
        /// <returns>La case à laquelle on va attribuer une valeur</returns>
        private Tuple<int, int> SelectUnassignedVariable(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// choix de la case a traiter avec le domaine le plus petit qui soit superieur à 1 (si a 1 c'est qu'on a deja trouvé la bonne valeur)
			int minDomaine = taille + 1; // on initialise le domaine a la valeur maximale
			List<Tuple<int, int>> casesSelectionnees = new List<Tuple<int, int>>();    // Il peut y avoir plusieurs cases selectionnée si elles ont un domaine de taille égale, degree heuristic se chargera de choisir le meilleur
			foreach (KeyValuePair<Tuple<int, int>, List<int>> caseSudoku in csp) {
				List<int> valeursPossibles = caseSudoku.Value;
				if (valeursPossibles.Count > 1 && valeursPossibles.Count <= minDomaine) {
					if (valeursPossibles.Count < minDomaine) {
						casesSelectionnees.Clear(); // on vide le tableau
						minDomaine = valeursPossibles.Count;
						casesSelectionnees.Add(caseSudoku.Key);
					} else { // minDomaine == valeursPossibles.size();
						casesSelectionnees.Add(caseSudoku.Key);
					}
				}
			}

			return DegreeHeuristic(csp, casesSelectionnees);
		}
        
        /// <summary>
        /// Détermine laquelle des valeurs selectionnee en fonction du nombre de contrainte voisinnes en utilisant Degree heuristic (on retourne celle qui a le plus de contraintes(cases voisines avec degré 1)
        /// </summary>
        /// <param name="csp">CSP Courant</param>
        /// <param name="casesSelectionnees">Tableau contenant les cases retenue après application de MRV</param>
        /// <returns>La meilleure case possible</returns>
        private Tuple<int, int> DegreeHeuristic(Dictionary<Tuple<int, int>, List<int>> csp, List<Tuple<int, int>> casesSelectionnees)
		{

			if (casesSelectionnees.Count == 1) {
				return casesSelectionnees[0];
			}

			int degreMax = 0;
			int degre;
			Tuple<int, int> caseDegreMax = null;
			foreach (Tuple<int, int> caseSelec in casesSelectionnees) {
				degre = 0;
				foreach (Tuple<int, int> caseSudoku in csp.Keys) {
					// si c'est une case voisine ET que cette case n'est pas assignée)
					if (EstVoisin(caseSelec, caseSudoku) && casesSelectionnees.Count > 1) {
						degre++;
					}
				}

				if (degre > degreMax) {
					degreMax = degre;
					caseDegreMax = caseSelec;
				}
			}

			return caseDegreMax;
		}
        
        /// <summary>
        /// On trie les valeurs possibles d'une case en fonction du nombre de contraintes qu'impose la valeur (l'impact qu'a la valeur sur ses voisins). La moins contraignante est en début de liste.
        /// </summary>
        /// <param name="csp">CSP Courant</param>
        /// <param name="caseSelec">case sélectionnée pour évaluation</param>
        /// <returns>Une liste ordonnée par impact de la valeur sur les voisins de la case évaluée</returns>
        private List<int> OrderDomainValues(Dictionary<Tuple<int, int>, List<int>> csp, Tuple<int, int> caseSelec)
		{
			// si il n'y a qu'une seule valeur dans le tableau retourne cette valeur
			if (csp[caseSelec].Count == 1) {
				return csp[caseSelec];
			}

			Dictionary<int, int> nbContraintesParValeur = new Dictionary<int, int>();
			int compteurContraintes;
			// pour chaque valeur du domaine pour var
			foreach (int valeur in csp[caseSelec]) {
				compteurContraintes = 0;
				// regarde le nombre de conflits avec les voisins pour cette valeur
				foreach (KeyValuePair<Tuple<int, int>, List<int>> caseSudoku in csp) {
					if (EstVoisin(caseSelec, caseSudoku.Key)) {
						if (caseSudoku.Value.Count > 1 && caseSudoku.Value.Contains(valeur)) {
							compteurContraintes++;
						}
					}
				}
				nbContraintesParValeur[valeur] = compteurContraintes;
			}
			List<int> valeursSelec = new List<int>();
			// On trie la csp par ordre croissant de contraintes domaine par valeur
			foreach (KeyValuePair<int, int> contrainteDeValeur in nbContraintesParValeur.OrderBy(i => i.Value)) {
				valeursSelec.Add(contrainteDeValeur.Key);
			}
			return valeursSelec;
		}
    
        /// <summary>
        /// Fait une copie du CSP 
        /// </summary>
        /// <param name="csp">CSP à copier</param>
        /// <returns>copie du CSP</returns>
        private Dictionary<Tuple<int, int>, List<int>> CopieCSP(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			Dictionary<Tuple<int, int>, List<int>> copie = new Dictionary<Tuple<int, int>, List<int>>();

			foreach (KeyValuePair<Tuple<int, int>, List<int>> caseSudoku in csp) {
				List<int> copieContraintes = new List<int>();
				foreach (int val in caseSudoku.Value) {
					copieContraintes.Add(val);
				}
				copie[new Tuple<int, int>(caseSudoku.Key.Item1, caseSudoku.Key.Item2)] = copieContraintes;
			}
			return copie;
		}

        /// <summary>
        /// Détermine si la grille est résolue (il n'y a qu'une valeur possible pour chacune des cases).
        /// </summary>
        /// <param name="csp">CSP à évaluer </param>
        /// <returns>Vrai si la grille est résolue, Faux sinon</returns>
        private bool EstValide(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			List<int> valeursPossibles;
			foreach (KeyValuePair<Tuple<int, int>, List<int>> caseSudoku in csp) {
				valeursPossibles = caseSudoku.Value;
				if (valeursPossibles.Count != 1) {
					return false;
				}
			}
			return true;
		}

        /// <summary>
        /// Détermine si le changement est possible, soit si on ne réduit pas le domaine d'un voisin à un ensemble vide.
        /// </summary>
        /// <param name="cspModif">CSP courant</param>
        /// <param name="caseSelec">Case courante</param>
        /// <param name="valeur">Valeur à tester pour la case caseSelec</param>
        /// <returns>Vrai si le changement est possible, Faux sinon</returns>
        private Boolean Consistent(Dictionary<Tuple<int, int>, List<int>> cspModif, Tuple<int, int> caseSelec, int valeur)
		{
			List<int> domaineMaj = new List<int>();
			domaineMaj.Add(valeur);
			cspModif[caseSelec] = domaineMaj;
			List<Tuple<int, int>> keys = new List<Tuple<int, int>>(cspModif.Keys);
			foreach (Tuple<int, int> caseSudokuKey in keys) {
				Tuple<int, int> caseCourante = caseSudokuKey;
				if ((!cspModif[caseSudokuKey].Contains(valeur)) || (caseCourante.Item1 == caseSelec.Item1 && caseCourante.Item2 == caseSelec.Item2)) {
					continue;
				}
				if (EstVoisin(caseSelec, caseCourante)) {
					domaineMaj = cspModif[caseSudokuKey];
					domaineMaj.Remove(valeur);
					cspModif[caseCourante] = domaineMaj;
					if (domaineMaj.Count == 1) {
						if (!Consistent(cspModif, caseCourante, domaineMaj[0])) {
							return false;
						}
					} else if (domaineMaj.Count == 0) {
						return false;
					}
				}
			}
			return true;
		}

        /// <summary>
        /// Initialise le csp avec l'algorithme AC3
        /// </summary>
        /// <param name="csp">CSP de départ</param>
        /// <returns>Faux si la grille est impossible à résoudre, Vrai sinon</returns>

        private bool AC3(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// Liste d'arcs Case X / Case Y
			LinkedList<Tuple<Tuple<int, int>, Tuple<int, int>>> queue = new LinkedList<Tuple<Tuple<int, int>, Tuple<int, int>>>();

			foreach (Tuple<int, int> X in csp.Keys) {
				foreach (Tuple<int, int> Y in csp.Keys) {
					if (EstVoisin(X, Y)) {
						queue.AddLast(new Tuple<Tuple<int, int>, Tuple<int, int>>(X, Y));
					}
				}
			}
			Tuple<Tuple<int, int>, Tuple<int, int>> arc;
			while (!(queue.Count == 0)) {
				arc = queue.First.Value;//  supprime et recupere la premiere valeur de la liste
				queue.RemoveFirst();
				if (RemoveInconsistentValues(csp, arc)) {
					if (csp[arc.Item1].Count == 0) {
						return false;
					} else {
						foreach (Tuple<int, int> voisin in csp.Keys) {
							if (EstVoisin(arc.Item1, voisin)) {
								queue.AddFirst(new Tuple<Tuple<int, int>, Tuple<int, int>>(arc.Item1, voisin));
							}
						}
					}
				}
			}
			return true;
		}

        /// <summary>
        /// Enlève les valeurs d'un domaine si elles ne respectent pas les contraintes
        /// </summary>
        /// <param name="csp">CSP Courant</param>
        /// <param name="arc">Arc reliant une case x et y </param>
        /// <returns> vrai si on a enlevé une ou plusieurs valeurs.</returns>
        private bool RemoveInconsistentValues(Dictionary<Tuple<int, int>, List<int>> csp, Tuple<Tuple<int, int>, Tuple<int, int>> arc)
		{
			bool effacer = false;
			List<int> valeursX = csp[arc.Item1];
			List<int> valeursY = csp[arc.Item2];

			// si l'un des noeud a deja une valeur assignée efface cette valeur du domaine de celui non assigné
			if (valeursX.Count == 1 && valeursY.Count != 1 && valeursY.Contains(valeursX.ElementAt(0))) {
				valeursY.Remove(valeursX[0]);
				csp[arc.Item2] = valeursY;
				effacer = true;
			}
			if (valeursY.Count == 1 && valeursX.Count != 1 && valeursX.Contains(valeursY.ElementAt(0))) {
				valeursX.Remove(valeursY[0]);
				csp[arc.Item1] = valeursX;
				effacer = true;
			}


			return effacer;
		}

        /// <summary>
        /// Détermine si la case courante est dans la meme ligne, colonne ou carré que la case selectionnée
        /// </summary>
        /// <param name="caseSelec">Case selectionnée</param>
        /// <param name="caseCourante">Case courante</param>
        /// <returns>Vrai si la case selectionnée est voisine de la case courante</returns>
        private Boolean EstVoisin(Tuple<int, int> caseSelec, Tuple<int, int> caseCourante)
		{
			int ligne = caseCourante.Item1;
			int col = caseCourante.Item2;

            // Si le sudoku est de taille n²xn², la grille est divisé en blocks carrés
            if(Math.Sqrt(taille)%1 ==0 ){
                // trouver les coordonnées du carré auquel la case selectionnée appartient
			    int ligneDebutCarre = caseSelec.Item1 / (int) Math.Sqrt(taille);
			    int colDebutCarre = caseSelec.Item2 / (int) Math.Sqrt(taille);

			    return (!((ligne == caseSelec.Item1) && (col == caseSelec.Item2)) &&
			    ((ligne == caseSelec.Item1) || (col == caseSelec.Item2) ||
			    (ligne >= ligneDebutCarre && ligne <= ligneDebutCarre + 2 && col >= colDebutCarre && col <= colDebutCarre + 2)));
            }
            else
            {
                return false;
            }
			
		}
	}
}
