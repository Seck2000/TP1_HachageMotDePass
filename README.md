# 🔐 TP1 - Sécurité & Hachage de Mots de Passe

> Une suite d'outils performants pour l'analyse de sécurité des mots de passe, développée en C# avec une architecture moderne WPF/MVVM.

## 📋 Présentation du Projet

Ce projet a été réalisé dans le cadre du cours de **Sécurité Informatique**. Il simule une mission d'audit de sécurité où l'objectif est de démontrer les faiblesses des mots de passe mal choisis face à des attaques par force brute.

La solution se compose de deux applications distinctes et complémentaires :

1.  **AppDictionnaire** : Un générateur de dictionnaires de mots de passe hautement configurable (longueur, charset, règles personnalisées).
2.  **AppHachage** : Un outil de validation de hachages **BCrypt** (coût 10) capable de tester des millions de combinaisons à partir d'un dictionnaire.

## 🚀 Fonctionnalités Clés

### 1. Générateur de Dictionnaire (AppDictionnaire)
*   **Algorithme Optimisé** : Génération récursive rapide de toutes les permutations possibles.
*   **Flexibilité Totale** : Choix des jeux de caractères (a-z, A-Z, 0-9, Spécial) ou définition d'un alphabet personnalisé.
*   **Performance** : Écriture bufferisée pour gérer des fichiers de sortie volumineux (plusieurs Go) sans saturation mémoire.
*   **UX Soignée** : Interface non-bloquante avec barre de progression en temps réel (Asynchrone).

### 2. Validateur de Hachage (AppHachage)
*   **Support BCrypt** : Implémentation robuste de la vérification de mots de passe hachés (avec gestion automatique du sel).
*   **Multi-Threading** : Utilisation de `Task` pour paralléliser les traitements et maintenir l'interface réactive.
*   **Monitoring** : Tableau de bord en temps réel (tentatives/seconde, temps écoulé, progression).
*   **Sécurité** : Arrêt immédiat dès la découverte du mot de passe.

## 🛠 Technologies & Compétences Mises en Œuvre

Ce projet démontre la maîtrise des technologies suivantes :

*   **Langage** : C# (.NET 8.0)
*   **Framework UI** : WPF (Windows Presentation Foundation)
*   **Architecture** : **MVVM** (Model-View-ViewModel) pour une séparation stricte entre la vue et la logique métier.
*   **Programmation Asynchrone** : Utilisation intensive de `async/await` et `Task.Run` pour des applications fluides.
*   **Cryptographie** : Utilisation de la bibliothèque `BCrypt.Net-Next` pour le hachage sécurisé.
*   **Gestion de Version** : Git & GitHub (travail collaboratif avec fusion de branches).

## 💻 Prérequis

*   **Système d'exploitation** : Windows 10 ou 11 (Architecture x64).
*   **Runtime** : .NET Desktop Runtime 8.0 (ou SDK .NET 8.0 pour la compilation).

## 🔧 Installation et Lancement

1.  **Cloner le dépôt** :
    ```bash
    git clone https://github.com/Seck2000/TP1_HachageMotDePass.git
    cd TP1_HachageMotDePass
    ```

2.  **Lancer le Générateur de Dictionnaire** :
    ```bash
    dotnet run --project AppDictionnaire
    ```

3.  **Lancer le Validateur de Hachage** :
    ```bash
    dotnet run --project AppHachage
    ```

## 👥 Auteurs

*   **[Votre Nom]** - *Développeur Backend & Architecture MVVM*
*   **[Nom Collègue 1]** - *Développeur UI & Algorithme Hachage*
*   **[Nom Collègue 2]** - *Développeur...*
*   **[Nom Collègue 3]** - *Développeur...*

---
*Projet académique réalisé au Collège de Rosemont - Hiver 2026.*
