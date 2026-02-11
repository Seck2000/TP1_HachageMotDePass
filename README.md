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
