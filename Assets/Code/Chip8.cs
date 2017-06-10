using com.PixelismGames.UnityChip8.Controllers;
using UnityEngine;

namespace com.PixelismGames.UnityChip8
{
    public static class Chip8
    {
        private static CoreController _core;
        private static OperationsController _operations;
        private static FontController _font;
        private static ScreenController _screen;

        #region Singletons

        public static CoreController Core
        {
            get
            {
                if (_core == null)
                {
                    Debug.Log("ERROR | Singleton: Core not yet provided");
                    Debug.Break();
                }

                return (_core);
            }
        }

        public static OperationsController Operations
        {
            get
            {
                if (_operations == null)
                {
                    Debug.Log("ERROR | Singleton: Operations not yet provided");
                    Debug.Break();
                }

                return (_operations);
            }
        }

        public static FontController Font
        {
            get
            {
                if (_font == null)
                {
                    Debug.Log("ERROR | Singleton: Font not yet provided");
                    Debug.Break();
                }

                return (_font);
            }
        }

        public static ScreenController Screen
        {
            get
            {
                if (_screen == null)
                {
                    Debug.Log("ERROR | Singleton: Screen not yet provided");
                    Debug.Break();
                }

                return (_screen);
            }
        }

        #endregion

        #region Provide Routines

        public static void ProvideCore(CoreController core)
        {
            if (_core == null)
            {
                _core = core;
            }
            else
            {
                Debug.Log("ERROR | Singleton: Core already provided");
                Debug.Break();
            }
        }

        public static void ProvideOperations(OperationsController operations)
        {
            if (_operations == null)
            {
                _operations = operations;
            }
            else
            {
                Debug.Log("ERROR | Singleton: Operations already provided");
                Debug.Break();
            }
        }

        public static void ProvideFont(FontController font)
        {
            if (_font == null)
            {
                _font = font;
            }
            else
            {
                Debug.Log("ERROR | Singleton: Font already provided");
                Debug.Break();
            }
        }

        public static void ProvideScreen(ScreenController screen)
        {
            if (_screen == null)
            {
                _screen = screen;
            }
            else
            {
                Debug.Log("ERROR | Singleton: Screen already provided");
                Debug.Break();
            }
        }

        #endregion
    }
}
