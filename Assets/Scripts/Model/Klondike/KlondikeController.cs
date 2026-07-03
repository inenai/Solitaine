using Klondike;
using UnityEngine;

public class KlondikeController : MonoBehaviour
{
    [SerializeField] KlondikeUI _ui;

    void Start()
    {
        KlondikeGame game = new KlondikeGame(_ui);
        _ui.Setup(game);
        game.StartGame();
    }

}
