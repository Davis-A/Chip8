using System;
namespace MyGame
{
	public class chip8
	{
		#pragma warning disable 414
		//current opcode
		private ushort _opcode;
		//chip8 memory
		private byte [] _memory = new byte [4096];
		//general purpose registers
		private byte [] _registers = new byte [16];
		//Index register
		private ushort _I;
		//program counter
		private ushort _pc;
		bool [,] _pixelState;

		//There are 16 levels of stack
		private ushort [] _stack = new ushort [16];
		//stack pointer
		private ushort _sp;

		//Chip8 has hex keypad 0x0 to 0xF
		private bool [] _keypad = new bool [16];

		//sound stuff
		/*
		private byte _delayTimer;
		private byte _soundTimer;
		*/
		#pragma warning restore 414
	
		public chip8 (bool [,] pixelState)
		{
			_pixelState = pixelState;
		}


		public void Cycle () 
		{
			//fetch opcode
			//decode opcode
			//execute opcode

			//update Timers
		}

		private ushort GetOpcode () 
		{
			ushort p1 = _memory [_pc];
			ushort p2 = _memory [_pc + 1];

			//return _memory [_pc] << 8 | _memory [_pc + 1];
			return 0;
		}
	}
}
