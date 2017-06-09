using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Testing")]
    public class Testing : MonoBehaviour
    {
        public Sprite newSprite;

        #region MonoBehaviour

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.H))
            {
                Sprite sprite = GetComponent<SpriteRenderer>().sprite;

                Graphics.CopyTexture(newSprite.texture, sprite.texture);
            }
        }

        #endregion
    }
}
