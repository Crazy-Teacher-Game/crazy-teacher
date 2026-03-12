using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMiniGame : MonoBehaviour
{
    [SerializeField] float durationSeconds = 5f; //DURÉE MAX DU MINI JEU
    [SerializeField] float minDurationSeconds = 3f; //DURÉE MIN DU MINI JEU
    [SerializeField] public GameManager gameManager;

    void Start() //ptet faire autre chose pour l'appeler après un delay
    {
        // 1) Démarrer le timer commun à l’entrée du mini-jeu
        gameManager.StartTimer(durationSeconds, minDurationSeconds);

        // 2) Option : si le timer finit => on considère que c’est un échec
        gameManager.OnTimerEnded += HandleTimeout;

        // 3) Option : écoute globale si tu veux enchaîner depuis ICI après win/fail
        gameManager.OnMinigameWon   += AfterWin;
        gameManager.OnMinigameFailed+= AfterFail;
    }
    // NE PAS OUBLIER AUSSI DE DESACTIVER LES VALEURS DES AUTRES MINI-JEUX

    // --- LOGIQUE DE RÉUSSITE ---
    public void OnPlayerSucceeded()
    {
        gameManager.NotifyWin();
    }

    // --- LOGIQUE D'ECHEC ---
    void HandleTimeout()
    {
        gameManager.NotifyFail();
    }

    // --- CE QU'ON VEUT FAIRE À LA FIN D'UN MINI-JEU ---
    void AfterWin()
    {
        // Exemple : augmenter le round et charger un autre mini-jeu
        gameManager.AddRound();
        // typiquement on peut lancer ce qu'on veut après, 
        // même une animation spécifique ou autre
        // on peut aussi mettre de quoi charger une autre scène :
        // eSceneManager.LoadSceneAsync("AutreMiniJeu", LoadSceneMode.Additive);
    }

    void AfterFail()
    {
        gameManager.LoseLife();
        //idem : enchainer, rejouer un feedback, etc.
    }
}
