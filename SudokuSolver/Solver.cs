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
			// Domaine des solution pour 1 case
			for (int i = 0; i < taille; i = i + 1) {
				domaine[i] = i + 1;
			}

			// A partir de la grille du sudoku, on détermine quelles sont les valeurs possible pour chaque case
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

		public static int[,] cspToGrille(Dictionary<Tuple<int, int>, List<int>> csp, int taille)
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


		/**
            * Fonction de résolution
            * @return grille résolue
            */
		public int[,] solve()
		{
			if (AC3(cspDepart)) {
				if (estValide(cspDepart)) {
					return Solver.cspToGrille(cspDepart, taille);
				} else {
					Dictionary<Tuple<int, int>, List<int>> cspResultat = backtracking_search();

					return Solver.cspToGrille(cspResultat, taille);
				}
			} else {
				Console.WriteLine("Grille impossible à résoudre");
				return null;
			}
		}
		/**
        * Initialise la recherche par backtracking
        * @return CSP final
        */
		private Dictionary<Tuple<int, int>, List<int>> backtracking_search()
		{
			return recursive_backtracking(cspDepart);
		}

		/**
        * Recherche de la solution avec la technique du backtracking
        * @param csp : CSP a étudier
        * @return null si echec / CSP mis à jour si réussite
        */
		private Dictionary<Tuple<int, int>, List<int>> recursive_backtracking(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// si on a assigné toute les valeurs de la grille
			if (estValide(csp)) {
				return csp;
			}

			Tuple<int, int> caseSelec = select_unassigned_variable(csp);

			Dictionary<Tuple<int, int>, List<int>> newCSP;
			foreach (int valeur in order_domain_values(csp, caseSelec)) {
				// Si la valeur respecte les contraintes pour cette case
				if (csp[caseSelec].Contains(valeur)) {
					Dictionary<Tuple<int, int>, List<int>> mapTest = copieCSP(csp);
					Dictionary<Tuple<int, int>, List<int>> saveMap = copieCSP(csp);
					// modifie le domaine sur toutes les cases concernées
					// test inference
					if (consistent(mapTest, caseSelec, valeur)) {
						newCSP = recursive_backtracking(mapTest);
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

		/**
        * Selectionne une case à laquelle assigner une variable en utilisant MRV et Degree Heuristic
        * @param csp : CSP courant
        * @return case sélectionnée
        */
		private Tuple<int, int> select_unassigned_variable(Dictionary<Tuple<int, int>, List<int>> csp)
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

			return degree_heuristic(csp, casesSelectionnees);
		}

		/**
        * Determine laquelle des valeurs selectionnee en fonction du nombre de contrainte voisinnes en utilisant Degree heuristic (on retourne celle qui a le plus de contraintes(cases voisines avec degré 1)
        * @param csp : CSP courant
        * @param casesSelectionnees : tableau des cases retenues par MRV
        * @return meilleure case
        */
		private Tuple<int, int> degree_heuristic(Dictionary<Tuple<int, int>, List<int>> csp, List<Tuple<int, int>> casesSelectionnees)
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
					// si c'est une case voisine ET que cette case n'est pas assignée (= taille tableau contrainte > 1)
					if (estVoisin(caseSelec, caseSudoku) && casesSelectionnees.Count > 1) {
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


		/**
        * Tri les valeurs en fonction de celles qui excluent le moins de choix pour les cases voisines avec l'algorithme Least Constraining Values
        * @param csp : CSP courant
        * @param caseSelec : case Selectionnée
        * @return valeurs par ordre de preferences
        */
		private List<int> order_domain_values(Dictionary<Tuple<int, int>, List<int>> csp, Tuple<int, int> caseSelec)
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
					if (estVoisin(caseSelec, caseSudoku.Key)) {
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

		/**
        * Copie le CSP SANS faire de reférences à la map passée en parametre
        * @param csp : map à copier
        * @return copier de CSP
        */
		private Dictionary<Tuple<int, int>, List<int>> copieCSP(Dictionary<Tuple<int, int>, List<int>> csp)
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

		/**
        * Verifie si le CSP est terminé (solution du Sudoku trouvée)
        * @param csp : CSP courant
        * @return vrai si le CSP est terminé, faux sinon
        */
		private bool estValide(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// POUR CHAQUE cases du sudoku
			// SI il n'existe qu'une seule valeur possible pour chaque cases alors c'est valide
			// SINON invalide
			List<int> valeursPossibles;
			foreach (KeyValuePair<Tuple<int, int>, List<int>> caseSudoku in csp) {
				valeursPossibles = caseSudoku.Value;
				if (valeursPossibles.Count != 1) {
					return false;
				}
			}
			return true;
		}

		/**
        * Determine si l'affectation est envisageable et si on ne réduit pasle domaine d'un voisin à 0
        * @param cspModif : CSP courant
        * @param caseSelec : case courante
        * @param valeur : valeur testée
        * @return vrai si la valeur peut être affectée, faux sinon
        */
		private Boolean consistent(Dictionary<Tuple<int, int>, List<int>> cspModif, Tuple<int, int> caseSelec, int valeur)
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

				if (estVoisin(caseSelec, caseCourante)) {
					domaineMaj = cspModif[caseSudokuKey];
					domaineMaj.Remove(valeur);
					cspModif[caseCourante] = domaineMaj;
					if (domaineMaj.Count == 1) {
						if (!consistent(cspModif, caseCourante, domaineMaj[0])) {
							return false;
						}
					} else if (domaineMaj.Count == 0) {
						return false;
					}
				}
			}
			return true;
		}


		/**
        * Initialise la map avec l'algorithme AC3
        * @param csp
        * @return vrai si reussie
        */
		private bool AC3(Dictionary<Tuple<int, int>, List<int>> csp)
		{
			// Liste d'arcs Case X / Case Y
			LinkedList<Tuple<Tuple<int, int>, Tuple<int, int>>> queue = new LinkedList<Tuple<Tuple<int, int>, Tuple<int, int>>>();

			foreach (Tuple<int, int> X in csp.Keys) {
				foreach (Tuple<int, int> Y in csp.Keys) {
					if (estVoisin(X, Y)) {
						queue.AddLast(new Tuple<Tuple<int, int>, Tuple<int, int>>(X, Y));
					}
				}
			}
			Tuple<Tuple<int, int>, Tuple<int, int>> arc;
			while (!(queue.Count == 0)) {
				arc = queue.First.Value;//  supprime et recupere la premiere valeur de la liste
				queue.RemoveFirst();
				if (remove_inconsistent_values(csp, arc)) {
					if (csp[arc.Item1].Count == 0) {
						return false;
					} else {
						foreach (Tuple<int, int> voisin in csp.Keys) {
							if (estVoisin(arc.Item1, voisin)) {
								queue.AddFirst(new Tuple<Tuple<int, int>, Tuple<int, int>>(arc.Item1, voisin));
							}
						}
					}
				}
			}
			return true;
		}

		/**
        * Algorithme complémentaire à AC3 permettant supprimer la valeur d'un domaine si elle ne respecte pas les contraintes
        * @param csp : CSP Courant
        * @param arc : Cases voisines X et Y
        * @return vrai si un effacement a été effectué
        */
		private bool remove_inconsistent_values(Dictionary<Tuple<int, int>, List<int>> csp, Tuple<Tuple<int, int>, Tuple<int, int>> arc)
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





		/**
        * Determine si la caseCourante est dans la meme ligne, colonne ou carré que la case Selectionnée
        * @param caseSelec :  case de référence
        * @param caseCourante : case à tester
        * @return vrai si la case est voisine, faux sinon
*/
		private Boolean estVoisin(Tuple<int, int> caseSelec, Tuple<int, int> caseCourante)
		{
			int ligne = caseCourante.Item1;
			int col = caseCourante.Item2;

			// trouver les coordonnées du carré auquel la case selectionnée appartient
			int ligneDebutCarre = caseSelec.Item1 / 3 * 3;
			int colDebutCarre = caseSelec.Item2 / 3 * 3;

			return (!((ligne == caseSelec.Item1) && (col == caseSelec.Item2)) &&
			((ligne == caseSelec.Item1) || (col == caseSelec.Item2) ||
			(ligne >= ligneDebutCarre && ligne <= ligneDebutCarre + 2 && col >= colDebutCarre && col <= colDebutCarre + 2)));
		}
	}
}
