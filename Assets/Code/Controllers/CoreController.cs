using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Core Controller")]
    public class CoreController : MonoBehaviour
    {
        private const ushort MEMORY_SIZE = 0x1000;
        private const byte REGISTER_COUNT = 0x10;
        private const byte STACK_SIZE = 0x10;
        private const ushort MEMORY_ROM_OFFSET = 0x200;

        public const byte OPCODE_SIZE = 0x2;

        private const int CYCLE_FREQUENCY = 2000;
        private const int INTERNAL_TIMER_FREQUENCY = 60;

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

        private float _leftoverUnclockedTime;
        private float _internalTimerAccumulator;
        //private bool _keyDownChecked;

        private Dictionary<KeyCode, byte> _keyValueMap;

        //private int _cycleFrequency;

        //private bool _running;
        //private bool _paused;

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
            get { return (_screen); }
            set { _screen = value; }
        }

        public byte DelayTimer
        {
            get { return (_delayTimer); }
            set { _delayTimer = value; }
        }

        public byte SoundTimer
        {
            get { return (_soundTimer); }
            set { _soundTimer = value; }
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
            Screen = new byte[ScreenController.SCREEN_WIDTH * ScreenController.SCREEN_HEIGHT];

            //_keyValueMap = new Dictionary<KeyCode, byte>()
            //{
            //    { KeyCode.Alpha0, 0x0 }, { KeyCode.Alpha1, 0x1 }, { KeyCode.Alpha2, 0x2 }, { KeyCode.Alpha3, 0x3 },
            //    { KeyCode.Alpha4, 0x4 }, { KeyCode.Alpha5, 0x5 }, { KeyCode.Alpha6, 0x6 }, { KeyCode.Alpha7, 0x7 },
            //    { KeyCode.Alpha8, 0x8 }, { KeyCode.Alpha9, 0x9 }, { KeyCode.A, 0xA }, { KeyCode.B, 0xB },
            //    { KeyCode.C, 0xC }, { KeyCode.D, 0xD }, { KeyCode.E, 0xE }, { KeyCode.F, 0xF }
            //};

            _keyValueMap = new Dictionary<KeyCode, byte>()
            {
                { KeyCode.Alpha0, 0x0 }, { KeyCode.Alpha1, 0x1 }, { KeyCode.Alpha2, 0x2 }, { KeyCode.Alpha3, 0x3 },
                { KeyCode.UpArrow, 0x4 }, { KeyCode.LeftArrow, 0x5 }, { KeyCode.RightArrow, 0x6 }, { KeyCode.DownArrow, 0x7 },
                { KeyCode.Alpha8, 0x8 }, { KeyCode.Alpha9, 0x9 }, { KeyCode.A, 0xA }, { KeyCode.B, 0xB },
                { KeyCode.C, 0xC }, { KeyCode.D, 0xD }, { KeyCode.E, 0xE }, { KeyCode.F, 0xF }
            };

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

            foreach (byte[] fontCharacter in Chip8.Font.FontCharacters)
                Buffer.BlockCopy(fontCharacter, 0, _memory, (0x0000 + (Chip8.Font.FontCharacters.IndexOf(fontCharacter) * FontController.FONT_CHARACTER_SIZE)), FontController.FONT_CHARACTER_SIZE);

            //byte[] rom = File.ReadAllBytes("./Contrib/LogoROM.ch8");
            //byte[] rom = File.ReadAllBytes("./Contrib/tetris.ch8");
            byte[] rom = File.ReadAllBytes("./Contrib/spaceinvaders.ch8");
            Buffer.BlockCopy(rom, 0, _memory, (int)MEMORY_ROM_OFFSET, rom.Length);

            _i = 0;
            _pc = MEMORY_ROM_OFFSET;
            _sp = 0;
            DelayTimer = 0;
            SoundTimer = 0;

            //Running = true;
        }

        public void Update()
        {
            _internalTimerAccumulator += Time.deltaTime;
            while (_internalTimerAccumulator >= (1f / INTERNAL_TIMER_FREQUENCY))
            {
                _internalTimerAccumulator -= (1f / INTERNAL_TIMER_FREQUENCY);
                internalTimerClock();
            }

            float oldlut = _leftoverUnclockedTime;
            float deltaTime = Time.deltaTime + _leftoverUnclockedTime;
            int clockCount = (int)(deltaTime * CYCLE_FREQUENCY);
            _leftoverUnclockedTime = deltaTime - ((float)clockCount / (float)CYCLE_FREQUENCY);
            for (int loop = 0; loop < clockCount; loop++)
                clock();

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

            //clock();

            //        cycleCount++;

            //        if (cycleCount >= (CycleFrequency / BATCH_FREQUENCY))
            //        {
            //            timingStopwatch.Stop();

            //            if (timingStopwatch.ElapsedMilliseconds < (Global.MILLISECONDS_PER_SECOND / BATCH_FREQUENCY))
            //                Thread.Sleep((int)((Global.MILLISECONDS_PER_SECOND / BATCH_FREQUENCY) - timingStopwatch.ElapsedMilliseconds));
            //        }
            //    }
            //}
        }

        #endregion

        #region Clock

        private void clock()
        {
            ushort opcode = (ushort)((_memory[_pc] << 8) | _memory[_pc + 1]);
            _pc += OPCODE_SIZE;

            Chip8.Operations.ProcessOpcode(opcode);
        }

        private void internalTimerClock()
        {
            if (DelayTimer > 0)
                DelayTimer--;

            if (SoundTimer > 0)
            {
                SoundTimer--;

                //if (SoundTimer == 0)
                //    StopTone();
            }
        }

        #endregion

        #region Input

        public byte GetKeypress()
        {
            foreach (KeyCode keyCode in _keyValueMap.Keys)
                if (Input.GetKey(keyCode))
                    return (_keyValueMap[keyCode]);

            return (0xFF);
        }

        #endregion
    }
}
