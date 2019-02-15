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
                domaine[i] = i + 1;
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
                    if (csp[Tuple.Create(i, j)].Count() > 1)
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


        /**
            * Fonction de résolution
            * @return grille résolue
            */
        public int[,] solve()
        {
            if (AC3(cspDepart))
            {
                if (estValide(cspDepart))
                {
                    return Solver.cspToGrille(cspDepart, taille);
                }
                else
                {
                    Dictionary<Tuple<int, int>, List<int>> cspResultat = backtracking_search();
                    return Solver.cspToGrille(cspResultat, taille);
                }
            }
            else
            {
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
        * Recherche de la solution avec la technide du backtracking
        * @param csp : CSP a étudier
        * @return null si echec / CSP mis à jour si réussite
        */
        private Dictionary<Tuple<int, int>, List<int>> recursive_backtracking(Dictionary<Tuple<int, int>, List<int>> csp)
        {
            // si on a assigné toute les valeurs de la grille
            if (estValide(csp))
            {
                return csp;
            }

            Tuple<int, int> caseSelec = select_unassigned_variable(csp);

            Dictionary<Tuple<int, int>, List<int>> newCSP;
            for (int valeur : order_domain_values(csp, caseSelec))
            {
                // Si la valeur respecte les contraintes pour cette case
                if (csp.get(caseSelec).contains(valeur))
                {

                    Dictionary<Tuple<int, int>, List<int>> mapTest = copieCSP(csp);
                    Dictionary<Tuple<int, int>, List<int>> saveMap = copieCSP(csp);
                    // modifie le domaine sur toutes les cases concernées
                    // test inference

                    bool valide = consistent(mapTest, caseSelec, valeur);

                    if (valide)
                    {
                        newCSP = recursive_backtracking(mapTest);
                        if (newCSP != null)
                        {
                            return newCSP;
                        }
                    }
                    // récupère l'ancienne carte si les tests ont échouées
                    csp = saveMap;
                    if (csp.get(caseSelec).size() == 1)
                    {
                        return null;//si c'etait la seule valeur possible et qu'elle est pas possible
                    }
                    else
                    {
                        csp.get(caseSelec).remove((int)valeur);
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
            int minDomaine = 10; // on initialise le domaine a la valeur maximale
            List<Tuple<int, int>> casesSelectionnees = new List<Tuple<int, int>>();    // Il peut y avoir plusieurs cases Selectionnée si elles ont un domaine de taille égale, degree heuristic se chargera de choisir le meilleur
            for (Map.Entry<Tuple<int, int>, List<int>> caseSudoku : csp.entrySet())
            {
                List<int> valeursPossibles = caseSudoku.getValue();
                if (valeursPossibles.Count > 1 && valeursPossibles.Count <= minDomaine)
                {
                    if (valeursPossibles.Count < minDomaine)
                    {
                        casesSelectionnees.Clear(); // on vide le tableau
                        minDomaine = valeursPossibles.Count;
                        casesSelectionnees.Add(caseSudoku.getKey());
                    }
                    else
                    { // minDomaine == valeursPossibles.size();
                        casesSelectionnees.Add(caseSudoku.getKey());
                    }
                }
            }

            return degree_heuristic(csp, casesSelectionnees);
        }

        /**
        * Determine laquelle des valeur selectionner en fonction du nombre de contrainte voisinnes en utilisant Degree heuristic (on retourne celle qui a le plus de contraintes(cases voisines avec degré1)
        * @param csp : CSP courant
        * @param casesSelectionnees : tableau des cases retenues par MRV
        * @return meilleure case
        */
        private Tuple<int, int> degree_heuristic(Dictionary<Tuple<int, int>, List<int>> csp, ArrayList<Tuple<int, int>> casesSelectionnees)
        {

            if (casesSelectionnees.size() == 1)
                return casesSelectionnees.get(0);


            int degreMax = 0;
            int degre;
            Tuple<int, int> caseDegreMax = null;
            for (Tuple<int, int> caseSelec : casesSelectionnees)
            {
                degre = 0;
                for (Tuple<int, int> caseSudoku : csp.keySet())
                {
                    // si c'est une case voisine ET que cette case n'est pas assignée (= taille tableau contrainte > 1)
                    if (estVoisin(caseSelec, caseSudoku) && casesSelectionnees.size() > 1)
                        degre++;
                }

                if (degre > degreMax)
                {
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
            if (csp.get(caseSelec).size() == 1)
                return csp.get(caseSelec);

            HashMap<int, int> nbContraintesParValeur = new HashMap<>();
            int compteurContraintes;
            // pour chaque valeur du domaine pour var
            for (int valeur : csp.get(caseSelec))
            {
                compteurContraintes = 0;
                // regarde le nombre de conflits avec les voisins pour cette valeur
                for (Map.Entry<Tuple<int, int>, ArrayList<int>> caseSudoku : csp.entrySet())
                {
                    if (estVoisin(caseSelec, caseSudoku.getKey()))
                    {
                        if (caseSudoku.getValue().size() > 1 && caseSudoku.getValue().contains(valeur))
                            compteurContraintes++;
                    }
                }
                nbContraintesParValeur.put(valeur, compteurContraintes);
            }


            // On trie la csp par ordre croissant de contraintes domaine par valeur
            HashMap<int, int> mapTrieParNbContrainte = nbContraintesParValeur.entrySet()
                .stream()
                .sorted(Map.Entry.comparingByValue())
                .collect(Collectors.toMap(Map.Entry::getKey, Map.Entry::getValue, (e1, e2)->e1, LinkedHashMap::new));

            List<int> valeursSelec = new List<int>();

            for (Map.Entry<int, int> contrainteDeValeur : mapTrieParNbContrainte.entrySet())
                valeursSelec.add(contrainteDeValeur.getKey());
            return valeursSelec;
        }

        /**
        * Copie le CSP SANS faire de reférences à la map passée en parametre
        * @param csp : map à copier
        * @return copier de CSP
        */
        private Dictionary<Tuple<int, int>, List<int>> copieCSP(Dictionary<Tuple<int, int>, List<int>> csp)
        {
            Dictionary<Tuple<int, int>, List<int>> copie = new HashMap<>();

            for (Map.Entry<Tuple<int, int>, ArrayList<int>> caseSudoku : csp.entrySet())
            {
                ArrayList<int> copieContraintes = new ArrayList<>();
                for (int val : caseSudoku.getValue())
                {
                    copieContraintes.add(val);
                }
                copie.put(new Tuple<int, int>(caseSudoku.getKey().getKey(), caseSudoku.getKey().getValue()), copieContraintes);
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
            ArrayList<int> valeursPossibles;
            for (Map.Entry<Tuple<int, int>, ArrayList<int>> caseSudoku : csp.entrySet())
            {
                valeursPossibles = caseSudoku.getValue();
                if (valeursPossibles.size() != 1) return false;
            }
            return true;
        }

        /**
        * Determine si l'affectation est envisageable si on ne réduit le domaine d'un voisin à 0
        * @param cspModif : CSP courant
        * @param caseSelec : case courante
        * @param valeur : valeur testée
        * @return vrai si la valeur peut être affectée, faux sinon
        */
        private boolean consistent(Dictionary<Tuple<int, int>, List<int>> cspModif, Tuple<int, int> caseSelec, int valeur)
        {

            List<int> domaineMaj = new List<int>();
            domaineMaj.Add(valeur);
            cspModif.Add(caseSelec, domaineMaj);

            for (Map.Entry<Tuple<int, int>, ArrayList<int>> caseSudoku : cspModif.entrySet())
            {
                Tuple<int, int> caseCourante = caseSudoku.getKey();

                if ((!caseSudoku.getValue().contains(valeur)) || (caseCourante.getKey() == caseSelec.getKey() && caseCourante.getValue() == caseSelec.getValue()))
                    continue;

                if (estVoisin(caseSelec, caseCourante))
                {

                    domaineMaj = caseSudoku.getValue();
                    domaineMaj.remove((int)valeur);
                    cspModif.put(caseCourante, domaineMaj);
                    if (domaineMaj.size() == 1)
                    {
                        if (!consistent(cspModif, caseCourante, domaineMaj.get(0)))
                            return false;
                    }
                    else if (domaineMaj.size() == 0)
                    {
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

            for (Tuple<int, int> X : csp.keySet())
            {
                for (Tuple<int, int> Y : csp.keySet())
                    if (estVoisin(X, Y))
                    {
                        queue.AddLast(new Tuple<int, int>(X, Y));
                    }
            }
            Tuple<Tuple<int, int>, Tuple<int, int>> arc;
            while (!(queue.Count == 0))
            {
                arc = queue.First.Value;//  supprime et recupere la premiere valeur de la liste
                queue.RemoveFirst();
                if (remove_inconsistent_values(csp, arc))
                {
                    if (csp.get(arc.getKey()).size() == 0)
                        return false;
                    else
                    {
                        for (Tuple<int, int> voisin : csp.keySet())
                        {
                            if (estVoisin(arc.getKey(), voisin))
                            {
                                queue.AddFirst(new Tuple<int, int>(arc.getKey(), voisin));
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
            List<int> valeursX = csp.get(arc.Item1);
            List<int> valeursY = csp.get(arc.Item2);

            // si l'un des noeud a deja une valeur assignée efface cette valeur du domaine de celui non assigné

            if (valeursX.Count == 1 && valeursY.Count != 1 && valeursY.Contains(valeursX.ElementAt(0)))
            {
                valeursY.remove((int)valeursX.get(0));
                csp.Add(arc.Item2, valeursY);
                effacer = true;
            }
            if (valeursY.Count == 1 && valeursX..Count != 1 && valeursX.Contains(valeursY.ElementAt(0)))
            {
                valeursX.remove((int)valeursY.get(0));
                csp.Add(arc.Item1, valeursX);
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
