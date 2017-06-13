using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.PixelismGames.UnityChip8.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Operations Controller")]
    public class OperationsController : MonoBehaviour
    {
        private Dictionary<byte, Action<ushort>> _opcodeMap;
        private Dictionary<byte, Action<ushort>> _opcodeMap00EX;
        private Dictionary<byte, Action<ushort>> _opcodeMap8XXX;
        private Dictionary<byte, Action<ushort>> _opcodeMapEXXX;
        private Dictionary<byte, Action<ushort>> _opcodeMapFXXX;

        #region MonoBehaviour

        public void Awake()
        {
            _opcodeMap = new Dictionary<byte, Action<ushort>>();
            _opcodeMap.Add(0x0, map00EXOperations);
            _opcodeMap.Add(0x1, jump);
            _opcodeMap.Add(0x2, callSubroutine);
            _opcodeMap.Add(0x3, jumpIfEqualTo);
            _opcodeMap.Add(0x4, jumpIfNotEqualTo);
            _opcodeMap.Add(0x5, jumpIfEqual);
            _opcodeMap.Add(0x6, load);
            _opcodeMap.Add(0x7, appendValue);
            _opcodeMap.Add(0x8, map8XXXOperations);
            _opcodeMap.Add(0x9, jumpIfNotEqual);
            _opcodeMap.Add(0xA, loadIndex);
            _opcodeMap.Add(0xB, jumpWithOffset);
            _opcodeMap.Add(0xC, random);
            _opcodeMap.Add(0xD, drawSprite);
            _opcodeMap.Add(0xE, mapEXXXOperations);
            _opcodeMap.Add(0xF, mapFXXXOperations);

            _opcodeMap00EX = new Dictionary<byte, Action<ushort>>();
            _opcodeMap00EX.Add(0xE0, clearScreen);
            _opcodeMap00EX.Add(0xEE, returnSubroutine);

            _opcodeMap8XXX = new Dictionary<byte, Action<ushort>>();
            _opcodeMap8XXX.Add(0x0, copy);
            _opcodeMap8XXX.Add(0x1, or);
            _opcodeMap8XXX.Add(0x2, and);
            _opcodeMap8XXX.Add(0x3, xor);
            _opcodeMap8XXX.Add(0x4, add);
            _opcodeMap8XXX.Add(0x5, subtract);
            _opcodeMap8XXX.Add(0x6, shiftRight);
            _opcodeMap8XXX.Add(0x7, subtractReverse);
            _opcodeMap8XXX.Add(0xE, shiftLeft);

            _opcodeMapEXXX = new Dictionary<byte, Action<ushort>>();
            _opcodeMapEXXX.Add(0x9E, jumpIfKeyPressed);
            _opcodeMapEXXX.Add(0xA1, jumpIfKeyNotPressed);

            _opcodeMapFXXX = new Dictionary<byte, Action<ushort>>();
            _opcodeMapFXXX.Add(0x07, readDelayTimer);
            _opcodeMapFXXX.Add(0x0A, waitForKeypress);
            _opcodeMapFXXX.Add(0x15, loadDelayTimer);
            _opcodeMapFXXX.Add(0x18, loadSoundTimer);
            _opcodeMapFXXX.Add(0x1E, addToIndex);
            _opcodeMapFXXX.Add(0x29, addressFontCharacter);
            _opcodeMapFXXX.Add(0x33, storeBCD);
            _opcodeMapFXXX.Add(0x55, dumpRegisters);
            _opcodeMapFXXX.Add(0x65, fillRegisters);
        }

        #endregion

        #region Process

        public void ProcessOpcode(ushort opcode)
        {
            byte opcodeMSN = (byte)((opcode & 0xF000) >> 12);
            _opcodeMap[opcodeMSN](opcode);
        }

        #endregion

        #region Mapping

        private void map00EXOperations(ushort opcode)
        {
            byte opcodeLSB = (byte)(opcode & 0x00FF);
            _opcodeMap00EX[opcodeLSB](opcode);
        }

        private void map8XXXOperations(ushort opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x000F);
            _opcodeMap8XXX[opcodeLSN](opcode);
        }

        private void mapEXXXOperations(ushort opcode)
        {
            byte opcodeLSB = (byte)(opcode & 0x00FF);
            _opcodeMapEXXX[opcodeLSB](opcode);
        }

        private void mapFXXXOperations(ushort opcode)
        {
            byte opcodeLSB = (byte)(opcode & 0x00FF);
            _opcodeMapFXXX[opcodeLSB](opcode);
        }

        #endregion

        #region Operations

        #region 00EX Operations

        // 00E0
        private void clearScreen(ushort opcode)
        {
            Array.Clear(Chip8.Core.Screen, 0, Chip8.Core.Screen.Length);
        }

        // 00EE
        private void returnSubroutine(ushort opcode)
        {
            Chip8.Core.SP--;
            Chip8.Core.PC = Chip8.Core.Stack[Chip8.Core.SP];
        }

        #endregion

        // 1NNN
        private void jump(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            Chip8.Core.PC = address;
        }

        // 2NNN
        private void callSubroutine(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            Chip8.Core.Stack[Chip8.Core.SP] = Chip8.Core.PC;
            Chip8.Core.SP++;
            Chip8.Core.PC = address;
        }

        // 3XNN
        private void jumpIfEqualTo(ushort opcode)
        {
            byte register = (byte)((opcode & 0x0F00) >> 8);
            byte value = (byte)(opcode & 0x00FF);
            if (Chip8.Core.V[register] == value)
                Chip8.Core.PC += CoreController.OPCODE_SIZE;
        }

        // 4XNN
        private void jumpIfNotEqualTo(ushort opcode)
        {
            byte register = (byte)((opcode & 0x0F00) >> 8);
            byte value = (byte)(opcode & 0x00FF);
            if (Chip8.Core.V[register] != value)
                Chip8.Core.PC += CoreController.OPCODE_SIZE;
        }

        // 5XY0
        private void jumpIfEqual(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            if (Chip8.Core.V[registerX] == Chip8.Core.V[registerY])
                Chip8.Core.PC += CoreController.OPCODE_SIZE; ;
        }

        // 6XNN
        private void load(ushort opcode)
        {
            byte register = (byte)((opcode & 0x0F00) >> 8);
            byte value = (byte)(opcode & 0x00FF);
            Chip8.Core.V[register] = value;
        }

        // 7XNN
        private void appendValue(ushort opcode)
        {
            byte register = (byte)((opcode & 0x0F00) >> 8);
            byte value = (byte)(opcode & 0x00FF);
            Chip8.Core.V[register] += value;
        }

        #region 8XXX Operations

        // 8XY0
        private void copy(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            Chip8.Core.V[registerX] = Chip8.Core.V[registerY];
        }

        // 8XY1
        private void or(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] | Chip8.Core.V[registerY]);
        }

        // 8XY2
        private void and(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] & Chip8.Core.V[registerY]);
        }

        // 8XY3
        private void xor(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] ^ Chip8.Core.V[registerY]);
        }

        // 8XY4
        private void add(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);

            int result = Chip8.Core.V[registerX] + Chip8.Core.V[registerY];
            Chip8.Core.V[0xF] = result > byte.MaxValue ? (byte)1 : (byte)0;
            Chip8.Core.V[registerX] = (byte)result;
        }

        // 8XY5
        private void subtract(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);

            Chip8.Core.V[0xF] = Chip8.Core.V[registerX] > Chip8.Core.V[registerY] ? (byte)1 : (byte)0;
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] - Chip8.Core.V[registerY]);
        }

        // 8XY6
        private void shiftRight(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            Chip8.Core.V[0xF] = (byte)(Chip8.Core.V[registerX] & 0x01);
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] >> 1);
        }

        // 8XY7
        private void subtractReverse(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);

            Chip8.Core.V[0xF] = Chip8.Core.V[registerY] > Chip8.Core.V[registerX] ? (byte)1 : (byte)0;
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerY] - Chip8.Core.V[registerX]);
        }

        // 8XYE
        private void shiftLeft(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            Chip8.Core.V[0xF] = (byte)((Chip8.Core.V[registerX] & 0x80) >> 7);
            Chip8.Core.V[registerX] = (byte)(Chip8.Core.V[registerX] << 1);
        }

        #endregion

        // 9XY0
        private void jumpIfNotEqual(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            byte registerY = (byte)((opcode & 0x00F0) >> 4);
            if (Chip8.Core.V[registerX] != Chip8.Core.V[registerY])
                Chip8.Core.PC += CoreController.OPCODE_SIZE;
        }

        // ANNN
        private void loadIndex(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            Chip8.Core.I = address;
        }

        // BNNN
        private void jumpWithOffset(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            Chip8.Core.PC = (ushort)(address + Chip8.Core.V[0x0]);
        }

        // CXNN
        private void random(ushort opcode)
        {
            byte register = (byte)((opcode & 0x0F00) >> 8);
            byte value = (byte)(opcode & 0x00FF);

            System.Random random = new System.Random();
            byte randomValue = (byte)random.Next(256);

            Chip8.Core.V[register] = (byte)(randomValue & value);
        }

        // DXYN
        private void drawSprite(ushort opcode)
        {
            byte xRegister = (byte)((opcode & 0x0F00) >> 8);
            byte yRegister = (byte)((opcode & 0x00F0) >> 4);
            byte numberOfSpriteLines = (byte)(opcode & 0x000F);

            byte xLocation = (byte)(Chip8.Core.V[xRegister] & 0x3F);
            byte yLocation = (byte)(Chip8.Core.V[yRegister] & 0x1F);

            byte[] sprite = new byte[numberOfSpriteLines];
            Buffer.BlockCopy(Chip8.Core.Memory, (int)Chip8.Core.I, sprite, 0, numberOfSpriteLines);

            bool collision = false;

            for (int spriteLineIndex = 0; spriteLineIndex < numberOfSpriteLines; spriteLineIndex++)
            {
                byte pixelMask = 0x80;

                while (pixelMask != 0x00)
                {
                    if ((sprite[spriteLineIndex] & pixelMask) != 0)
                    {
                        if (Chip8.Core.Screen[(yLocation * ScreenController.SCREEN_WIDTH) + xLocation] == 0)
                            Chip8.Core.Screen[(yLocation * ScreenController.SCREEN_WIDTH) + xLocation] = 1;
                        else
                        {
                            Chip8.Core.Screen[(yLocation * ScreenController.SCREEN_WIDTH) + xLocation] = 0;
                            collision = true;
                        }
                    }

                    pixelMask = (byte)(pixelMask >> 1);
                    xLocation++;

                    if (xLocation >= ScreenController.SCREEN_WIDTH)
                        break;
                }

                xLocation = Chip8.Core.V[xRegister];
                yLocation++;

                if (yLocation >= ScreenController.SCREEN_HEIGHT)
                    break;
            }

            Chip8.Core.V[0xF] = collision ? (byte)1 : (byte)0;
        }

        #region EXXX Operations

        // EX9E
        private void jumpIfKeyPressed(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            byte keypress = Chip8.Core.GetKeypress();

            if (Chip8.Core.V[registerX] == keypress)
                Chip8.Core.PC += CoreController.OPCODE_SIZE;
        }

        // EXA1
        private void jumpIfKeyNotPressed(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            byte keypress = Chip8.Core.GetKeypress();

            if (Chip8.Core.V[registerX] != keypress)
                Chip8.Core.PC += CoreController.OPCODE_SIZE;
        }

        #endregion

        #region FXXX Operations

        // FX07
        private void readDelayTimer(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            Chip8.Core.V[registerX] = Chip8.Core.DelayTimer;
        }

        // FX0A
        private void waitForKeypress(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            byte keypress = 0xFF;

            while (keypress == 0xFF)// && Chip8.Core.Running && !Chip8.Core.Paused)
                keypress = Chip8.Core.GetKeypress();

            Chip8.Core.V[registerX] = keypress;
        }

        // FX15
        private void loadDelayTimer(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            Chip8.Core.DelayTimer = Chip8.Core.V[registerX];
        }

        // FX18
        private void loadSoundTimer(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            Chip8.Core.SoundTimer = Chip8.Core.V[registerX];

            if (Chip8.Core.SoundTimer > 0)
                Chip8.Core.PlayTone();
        }

        // FX1E
        private void addToIndex(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            Chip8.Core.I += Chip8.Core.V[registerX];
        }

        // FX29
        private void addressFontCharacter(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);
            Chip8.Core.I = (byte)(Chip8.Core.V[registerX] * FontController.FONT_CHARACTER_SIZE);
        }

        // FX33
        private void storeBCD(ushort opcode)
        {
            byte registerX = (byte)((opcode & 0x0F00) >> 8);

            Chip8.Core.Memory[Chip8.Core.I] = (byte)((Chip8.Core.V[registerX] / 100) % 10);
            Chip8.Core.Memory[Chip8.Core.I + 1] = (byte)((Chip8.Core.V[registerX] / 10) % 10);
            Chip8.Core.Memory[Chip8.Core.I + 2] = (byte)(Chip8.Core.V[registerX] % 10);
        }

        // FX55
        private void dumpRegisters(ushort opcode)
        {
            byte endRegister = (byte)((opcode & 0x0F00) >> 8);

            for (int register = 0; register <= endRegister; register++)
                Chip8.Core.Memory[Chip8.Core.I + register] = Chip8.Core.V[register];
        }

        // FX65
        private void fillRegisters(ushort opcode)
        {
            byte endRegister = (byte)((opcode & 0x0F00) >> 8);

            for (int register = 0; register <= endRegister; register++)
                Chip8.Core.V[register] = Chip8.Core.Memory[Chip8.Core.I + register];
        }

        #endregion

        #endregion
    }
}
