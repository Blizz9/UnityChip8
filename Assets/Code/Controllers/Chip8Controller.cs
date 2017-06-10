using Unity.Linq;
using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Chip-8 Controller")]
    public class Chip8Controller : MonoBehaviour
    {
        #region MonoBehaviour

        public void Awake()
        {
            Chip8.ProvideCore(gameObject.Children().OfComponent<CoreController>().First());
            Chip8.ProvideOperations(gameObject.Children().OfComponent<OperationsController>().First());
            Chip8.ProvideFont(gameObject.Children().OfComponent<FontController>().First());
            Chip8.ProvideScreen(gameObject.Children().OfComponent<ScreenController>().First());
        }

        #endregion
    }
}
