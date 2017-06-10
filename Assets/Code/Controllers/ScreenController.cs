using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Screen Controller")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScreenController : MonoBehaviour
    {
        public const byte SCREEN_WIDTH = 64;
        public const byte SCREEN_HEIGHT = 32;

        private SpriteRenderer _spriteRenderer;

        #region MonoBehaviour

        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void LateUpdate()
        {
            Texture2D screenTexture = new Texture2D(SCREEN_WIDTH, SCREEN_HEIGHT, TextureFormat.RGB24, false);
            for (int screenY = 0; screenY < SCREEN_HEIGHT; screenY++)
            {
                for (int screenX = 0; screenX < SCREEN_WIDTH; screenX++)
                {
                    Color pixelColor = Chip8.Core.Screen[(screenY * SCREEN_WIDTH) + screenX] == 1 ? Color.white : Color.black;
                    screenTexture.SetPixel(screenX, (SCREEN_HEIGHT - screenY - 1), pixelColor);
                }
            }
            screenTexture.Apply();

            Graphics.CopyTexture(screenTexture, _spriteRenderer.sprite.texture);
        }

        #endregion
    }
}
