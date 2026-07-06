using Klondike;
using UnityEngine;

public class KlondikeController : MonoBehaviour
{
    [SerializeField] Klondike.KlondikeController _ui;

    void Start()
    {
        KlondikeGame game = new KlondikeGame(_ui);
        game.SetupGame();
    }
}
