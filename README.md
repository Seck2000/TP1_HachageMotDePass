# Rapport de Développement - Partie #1 : Application Dictionnaire

## 1. Objectif de l'application
L'application **AppDictionnaire** a pour but de générer un fichier texte contenant toutes les combinaisons possibles de mots de passe (un "dictionnaire") en fonction de critères définis par l'utilisateur (longueur, types de caractères). Ce dictionnaire servira de base pour simuler une attaque par force brute dans la seconde partie du projet.

## 2. Architecture et Choix Technologiques

### 2.1 Langage et Framework
*   **Langage :** C# (.NET 8.0)
*   **Interface Graphique :** WPF (Windows Presentation Foundation)
*   **Architecture :** MVVM (Model-View-ViewModel)

### 2.2 Pourquoi MVVM ?
Nous avons choisi le patron de conception **MVVM** plutôt que l'approche classique événementielle (WinForms) pour plusieurs raisons :
1.  **Séparation des préoccupations :** La logique métier (`MainViewModel.cs`) est totalement séparée de l'interface graphique (`MainWindow.xaml`).
2.  **Maintenabilité :** Le code est plus propre et plus facile à modifier.
3.  **Data Binding :** La liaison de données permet de mettre à jour l'interface (barre de progression, status) automatiquement sans manipuler directement les contrôles graphiques.

## 3. Algorithme de Génération

### 3.1 Approche : Force Brute Récursive
Le cœur de l'application repose sur un algorithme de génération combinatoire. Puisque la longueur du mot de passe est variable (définie par l'utilisateur), nous ne pouvons pas utiliser un nombre fixe de boucles imbriquées.

Nous avons opté pour une **approche récursive**.

### 3.2 Fonctionnement
1.  **Construction de l'alphabet (Charset) :** On concatène les types de caractères choisis (minuscules, majuscules, chiffres, spéciaux) ou on utilise la chaîne personnalisée fournie.
2.  **Boucle de longueur :** On itère de la `Longueur Min` à la `Longueur Max`.
3.  **Récursion :** Pour chaque longueur cible, une fonction s'appelle elle-même pour ajouter un caractère à la fois jusqu'à atteindre la taille désirée.

**Pseudo-code de l'algorithme :**
```text
Fonction Generer(prefixe, longueurCible):
    SI taille(prefixe) == longueurCible ALORS
        Écrire prefixe dans le fichier
        Retourner
    FIN SI

    POUR CHAQUE caractère C DANS Alphabet FAIRE
        Generer(prefixe + C, longueurCible)
    FIN POUR
```

### 3.3 Complexité
La complexité temporelle est exponentielle : **O(N^L)**
*   **N** = Nombre de caractères dans l'alphabet.
*   **L** = Longueur du mot de passe.

Cela explique pourquoi la génération devient extrêmement longue dès que la longueur dépasse 6 ou 7 caractères avec un alphabet complet.

## 4. Détails d'Implémentation

### 4.1 Gestion de la Performance (Asynchronisme)
Pour éviter que l'interface ne gèle ("Ne répond pas") pendant la génération d'un gros dictionnaire :
*   Le calcul est exécuté dans un **Thread séparé** via `Task.Run()`.
*   L'écriture dans le fichier utilise `StreamWriter` pour une écriture bufférisée efficace.

### 4.2 Mise à jour de l'UI
Puisque le calcul se fait sur un thread secondaire, nous ne pouvons pas modifier l'interface directement. Nous utilisons le `Dispatcher` de WPF pour mettre à jour la barre de progression :

```csharp
Application.Current.Dispatcher.Invoke(() => {
    ProgressValue = percent;
});
```

### 4.3 Structure des Fichiers Clés
*   **`MainViewModel.cs`** : Contient toute la logique. Il implémente `INotifyPropertyChanged` pour notifier la vue des changements.
*   **`MainWindow.xaml`** : Définit l'interface en XAML (Grids, StackPanels, Controls).
*   **`RelayCommand.cs`** : Permet de lier les clics des boutons (ICommand) aux méthodes du ViewModel.

## 5. Fonctionnalités Utilisateur
L'application permet de :
1.  Définir une plage de longueur (Min/Max).
2.  Sélectionner des jeux de caractères standards (a-z, A-Z, 0-9, Spécial).
3.  **Priorité :** Saisir une chaîne personnalisée qui surcharge les sélections précédentes (ex: "abc" pour test rapide).
4.  Visualiser l'avancement via une barre de progression en temps réel.
5.  Choisir l'emplacement de sauvegarde via une boîte de dialogue native Windows.

# Rapport de Développement - Partie #2 : Application Hachage

## 1. Objectif de l'application
L'application **AppHachage** sert à valider un hachage bcrypt (facteur de coût 10) en parcourant les mots contenus dans le dictionnaire généré à la partie #1. Elle simule le fonctionnement d'une attaque par dictionnaire : chaque entrée est hachée avec le même sel que le hash cible et comparée pour retrouver le mot d'origine.

## 2. Architecture et Choix Technologiques

### 2.1 Langage et Framework
* **Langage :** C# (.NET 8.0)
* **Interface Graphique :** WPF (Windows Presentation Foundation)
* **Architecture :** MVVM (ViewModel : `HachageViewModel`)
* **Bibliothèque de hachage :** [`BCrypt.Net-Next`](https://www.nuget.org/packages/BCrypt.Net-Next) pour garantir la compatibilité avec bcrypt coût 10.

### 2.2 Pourquoi MVVM ?
Les raisons sont similaires à la partie 1 :
1. **Séparation claire** entre la logique (`HachageViewModel.cs`) et l'interface (`HachageWindow.xaml`).
2. **Data Binding** pour mettre à jour les statistiques (tentatives, temps écoulé, statut) en temps réel sans manipulations directes de l'UI.
3. **Tests facilité** : la logique métier peut être exercée sans lancer la fenêtre.

## 3. Algorithme de Validation

### 3.1 Étapes générales
1. **Lecture du dictionnaire** mot par mot (`File.ReadLines` pour éviter le chargement complet en mémoire).
2. **Extraction du sel** à partir du hash cible (les 29 premiers caractères d'un hash bcrypt incluent version, coût, sel).
3. **Hachage du mot courant** avec `BCrypt.HashPassword(candidate, salt)` ; l'utilisation d’un sel fixe évite de régénérer un nouveau sel aléatoire.
4. **Comparaison stricte** (`StringComparison.Ordinal`) avec le hash cible.
5. **Arrêt immédiat** si une correspondance est trouvée ou si l'utilisateur annule.

### 3.2 Gestion des tentatives
* Un compteur incrémente pour chaque mot testé.
* Un timer (`DispatcherTimer`) rafraîchit l'affichage du temps écoulé toutes les 200 ms.
* L'algorithme s'exécute dans un `Task.Run` avec `CancellationToken` pour conserver une UI réactive.

## 4. Détails d'Implémentation

### 4.1 Interactions Utilisateur
* **Champ hash cible :** exige un hash bcrypt valide (version `$2x`/`$2y`/`$2a`) ; un message d'erreur est affiché si le format est incorrect.
* **Sélection du dictionnaire :** via `OpenFileDialog` ; le ViewModel compte immédiatement les entrées valides pour informer l'utilisateur.
* **Boutons :**
  * `Démarrer la validation` active l'exécution asynchrone.
  * `Annuler` déclenche `_cancellationSource.Cancel()` et affiche un message de confirmation.

### 4.2 Gestion du Threading
* Les mises à jour du compteur se font via `Application.Current.Dispatcher.Invoke` pour respecter le thread UI.
* Le `Stopwatch` fournit une mesure précise du temps, synchronisée avec le `DispatcherTimer`.

### 4.3 Structure des Fichiers Clés
* **`HachageViewModel.cs`** — Logique principale : commandes, validations, boucle de hachage.
* **`HachageWindow.xaml`** — Interface et liaisons des contrôles.
* **`RelayCommand.cs`** — Commande générique réutilisable pour MVVM.
* **`HachageWindow.xaml.cs`** — Initialisation de la fenêtre et du ViewModel.

## 5. Fonctionnalités Utilisateur
1. Saisir le hash bcrypt cible (coût 10).
2. Choisir un fichier dictionnaire (généré en partie #1 ou autre).
3. Lancer la validation et suivre :
   * le nombre total de mots du dictionnaire,
   * le nombre de tentatives exécutées,
   * le temps écoulé depuis le début,
   * un message de statut (en cours, trouvé, annulé, erreur).
4. Visualiser le mot de passe correspondant et le sel lorsqu'une correspondance est détectée.
5. Annuler la validation à tout moment pour interrompre l'exécution.

## 6. Échecs et Gestion d'Erreurs
* **Fichier inexistant** : message explicite « Le fichier dictionnaire est introuvable ».
* **Hash invalide** : information « Format de hachage non reconnu ».
* **Dictionnaire vide** : compteur à 0 et message d’avertissement.
* **Exceptions IO** : capturées et reportées dans `StatusMessage` pour aider au diagnostic.

## 7. Améliorations Possibles
* Support de dictionnaires volumineux via lecture en streaming et rapport d’avancement plus fin.
* Export des résultats ou enregistrement des statistiques dans un fichier.
* Ajout de tests unitaires (mocks du dictionnaire) pour valider les cas trouvés / non trouvés / annulés.

---

Les deux parties du TP fonctionnent de concert : **AppDictionnaire** produit les listes de mots et **AppHachage** les exploite pour analyser un hash bcrypt. Ce README documente désormais la conception, l’implémentation et les scénarios d'utilisation des deux applications.
