using Klondike;
using UnityEngine;

public class KlondikeController : MonoBehaviour
{
    [SerializeField] KlondikeUIController _ui;

    void Start()
    {
        KlondikeGame game = new KlondikeGame(_ui);
        game.StartGame();
    }
}
