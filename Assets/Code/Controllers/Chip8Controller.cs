using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Chip-8 Controller")]
    public class Chip8Controller : MonoBehaviour
    {
        public Sprite temp;

        public const byte SCREEN_WIDTH = 64;
        public const byte SCREEN_HEIGHT = 32;

        private const ushort MEMORY_SIZE = 0x1000;
        private const byte REGISTER_COUNT = 0x10;
        private const byte STACK_SIZE = 0x10;
        private const ushort MEMORY_ROM_OFFSET = 0x200;

        public const byte OPCODE_SIZE = 0x2;

        //private const int BATCH_FREQUENCY = 50;
        //private const int INITIAL_CYCLE_FREQUENCY = 600;
        //private const int INTERNAL_TIMER_FREQUENCY = 60;

        //private const string SAVE_STATE_FILENAME = "SaveState.ss";

        private byte[] _memory;
        private ushort _pc;
        private byte[] _v;
        private ushort _i;
        private ushort[] _stack;
        private byte _sp;
        private byte _delayTimer;
        private byte _soundTimer;
        private byte[] _screen;

        private Operations _operations;
        //private Font _font;

        //private Thread _coreThread;
        //private System.Timers.Timer _internalTimer;
        //private int _cycleFrequency;

        private readonly object _sync = new object();

        //private bool _running;
        //private bool _paused;

        //private Func<byte> _getKeypress;
        //private Action _playTone;
        //private Action _stopTone;

        #region Accessible Properties

        //public bool Running
        //{
        //    get { lock (_sync) { return (_running); } }
        //    set { lock (_sync) { _running = value; } }
        //}

        //public bool Paused
        //{
        //    get { lock (_sync) { return (_paused); } }
        //    set { lock (_sync) { _paused = value; } }
        //}

        public byte[] Memory
        {
            get { return (_memory); }
            set { _memory = value; }
        }

        public ushort PC
        {
            get { return (_pc); }
            set { _pc = value; }
        }

        public byte[] V
        {
            get { return (_v); }
            set { _v = value; }
        }

        public ushort I
        {
            get { return (_i); }
            set { _i = value; }
        }

        public ushort[] Stack
        {
            get { return (_stack); }
            set { _stack = value; }
        }

        public byte SP
        {
            get { return (_sp); }
            set { _sp = value; }
        }

        public byte[] Screen
        {
            get { lock (_sync) { return (_screen); } }
            set { lock (_sync) { _screen = value; } }
        }

        public byte DelayTimer
        {
            get { lock (_sync) { return (_delayTimer); } }
            set { lock (_sync) { _delayTimer = value; } }
        }

        public byte SoundTimer
        {
            get { lock (_sync) { return (_soundTimer); } }
            set { lock (_sync) { _soundTimer = value; } }
        }

        //public int CycleFrequency
        //{
        //    get { lock (_sync) { return (_cycleFrequency); } }
        //    set { lock (_sync) { _cycleFrequency = value; } }
        //}

        #endregion

        #region MonoBehaviour

        public void Awake()
        {
            _memory = new byte[MEMORY_SIZE];
            _v = new byte[REGISTER_COUNT];
            _stack = new ushort[STACK_SIZE];
            Screen = new byte[SCREEN_WIDTH * SCREEN_HEIGHT];

            _operations = new Operations(this);
            //_font = new Font();

            //_internalTimer = new System.Timers.Timer(Global.MILLISECONDS_PER_SECOND / INTERNAL_TIMER_FREQUENCY);
            //_internalTimer.Elapsed += internalTimerClock;

            //_cycleFrequency = INITIAL_CYCLE_FREQUENCY;

            //Running = false;
            //Paused = false;
        }

        public void Start()
        {
            //if ((_coreThread != null) && _coreThread.IsAlive)
            //{
            //    _internalTimer.Stop();
            //    Running = false;

            //    while (_coreThread.IsAlive) ;
            //}

            Array.Clear(_memory, 0, _memory.Length);
            Array.Clear(_v, 0, _v.Length);
            Array.Clear(_stack, 0, _stack.Length);
            Array.Clear(Screen, 0, Screen.Length);

            //foreach (byte[] fontCharacter in _font.FontCharacters)
            //    Buffer.BlockCopy(fontCharacter, 0, _memory, (0x0000 + (_font.FontCharacters.IndexOf(fontCharacter) * Font.FONT_CHARACTER_SIZE)), Font.FONT_CHARACTER_SIZE);

            byte[] rom = File.ReadAllBytes("./Contrib/LogoROM.ch8");
            Buffer.BlockCopy(rom, 0, _memory, (int)MEMORY_ROM_OFFSET, rom.Length);

            _i = 0;
            _pc = MEMORY_ROM_OFFSET;
            _sp = 0;
            DelayTimer = 0;
            SoundTimer = 0;

            //Running = true;

            //_coreThread = new Thread(loop);
            //_coreThread.Start();

            //_internalTimer.Start();
        }

        public void Update()
        {
            //Stopwatch timingStopwatch = new Stopwatch();
            //int cycleCount = 0;

            //while (Running)
            //{
            //    if (Paused)
            //        Thread.Sleep(1);
            //    else
            //    {
            //        if (!timingStopwatch.IsRunning)
            //        {
            //            cycleCount = 0;
            //            timingStopwatch.Reset();
            //            timingStopwatch.Start();
            //        }

                      clock();

            //        cycleCount++;

            //        if (cycleCount >= (CycleFrequency / BATCH_FREQUENCY))
            //        {
            //            timingStopwatch.Stop();

            //            if (timingStopwatch.ElapsedMilliseconds < (Global.MILLISECONDS_PER_SECOND / BATCH_FREQUENCY))
            //                Thread.Sleep((int)((Global.MILLISECONDS_PER_SECOND / BATCH_FREQUENCY) - timingStopwatch.ElapsedMilliseconds));
            //        }
            //    }
            //}

            Texture2D screenTexture = new Texture2D(SCREEN_WIDTH, SCREEN_HEIGHT, TextureFormat.RGB24, false);
            for (int screenY = 0; screenY < SCREEN_HEIGHT; screenY++)
            {
                for (int screenX = 0; screenX < SCREEN_WIDTH; screenX++)
                {
                    if (_screen[(screenY * SCREEN_WIDTH) + screenX] == 1)
                    {
                        screenTexture.SetPixel(screenX, screenY, Color.red);
                    }
                }
            }
            screenTexture.Apply();

            Graphics.CopyTexture(screenTexture, GameObject.Find("Screen").GetComponent<SpriteRenderer>().sprite.texture);
        }

        #endregion

        #region Clock

        private void clock()
        {
            ushort opcode = (ushort)((_memory[_pc] << 8) | _memory[_pc + 1]);
            _pc += OPCODE_SIZE;

            _operations.ProcessOpcode(opcode);
        }

        #endregion
    }
}
