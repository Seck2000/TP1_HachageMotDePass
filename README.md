# TP1 – Hachage et dictionnaire de mots de passe

Implémentation en C# (.NET 8, WPF) d’un générateur de dictionnaire de mots de passe (AppDictionnaire) et d’une application de validation de hachages bcrypt (AppHachage). Le projet a été développé avec **Visual Studio Community**.

---

## Prérequis et frameworks .NET

- **.NET SDK 8.0** (ou supérieur compatible avec .NET 8).
- **Charge de travail « Développement .NET desktop »** (inclut WPF) dans Visual Studio.
- **Windows** : les applications ciblent `net8.0-windows` et s’exécutent sous Windows.

Aucun autre framework .NET spécifique n’est requis. AppHachage utilise le package NuGet **BCrypt.Net-Next** ; il est restauré automatiquement à l’ouverture ou à la génération de la solution.

---

## Lancer l’exécution des applications (Visual Studio Community)

### Ouvrir la solution

1. Lancer **Visual Studio Community**.
2. **Fichier → Ouvrir → Projet/Solution** (ou *Open a project or solution*).
3. Sélectionner le fichier **`TP1_HachageMotDePass.sln`** à la racine du dépôt.
4. Attendre la fin de la **restauration des packages NuGet** (barre d’état en bas ; première ouverture possible de quelques secondes).

### Exécuter AppDictionnaire

1. Dans l’**Explorateur de solutions**, repérer le projet **AppDictionnaire**.
2. Clic droit sur **AppDictionnaire** → **Définir comme projet de démarrage** (*Set as Startup Project*).
3. Appuyer sur **F5** (avec débogage) ou **Ctrl+F5** (sans débogage), ou cliquer sur le bouton **Démarrer** (flèche verte).
4. La fenêtre du générateur de dictionnaire s’ouvre. Configurer longueur, caractères, chemin de sauvegarde puis lancer la génération.

### Exécuter AppHachage

1. Clic droit sur le projet **AppHachage** → **Définir comme projet de démarrage**.
2. **F5** ou **Ctrl+F5** (ou bouton **Démarrer**).
3. La fenêtre de validation s’ouvre. Saisir le hash bcrypt cible, choisir le fichier dictionnaire (ex. généré par AppDictionnaire ou `dico_fr.txt`), puis cliquer sur **Démarrer la validation**.

### Lancer depuis la ligne de commande (optionnel)

À la racine du dépôt (dossier contenant le `.sln`) :

```powershell
# Restaurer et compiler toute la solution
dotnet restore
dotnet build

# Lancer AppDictionnaire
dotnet run --project AppDictionnaire

# Lancer AppHachage (depuis un autre terminal ou après fermeture de l’autre)
dotnet run --project AppHachage
```

Cela suppose que le **.NET SDK 8** est installé sur la machine (`dotnet --version` pour vérifier).
