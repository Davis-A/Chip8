using System;
namespace MyGame
{
	public class chip8
	{
		
		public const int CHIP8_X = 64;
		public const int CHIP8_Y = 32;
	


		//current opcode
		private ushort _opcode;
		//chip8 memory
		private byte [] _memory;
		//general purpose registers
		private byte [] _registers;
		//Index register
		private ushort _I;
		//program counter
		private ushort _pc;
		bool [,] _pixelState;

		//There are 16 levels of stack
		private ushort [] _stack;
		//stack pointer
		private ushort _sp;

		//Chip8 has hex keypad 0x0 to 0xF
		private bool [] _keypad;

		//sound stuff
		/*
		private byte _delayTimer;
		private byte _soundTimer;
		*/
	
	
		public chip8 ()
		{
			Init ();
		}


		/// <summary>
		/// Init resets the memory of everything.
		/// </summary>
		public void Init () 
		{
			//Reset all values
			//progam counter starts at 0x200 because a game should be in memory
			_pc = 0x200;
			_opcode = 0;
			_memory = new byte [4096];
			_registers = new byte [16];
			_I = 0;
			_pixelState = new bool [chip8.CHIP8_X, chip8.CHIP8_Y];
			_stack = new ushort [16];
			_sp = 0;
			_keypad = new bool [16];
			LoadFontSet ();

			//sound stuff
			//_delayTimer = 0;
			//_soundTimer = 0;

		}



		public void Cycle () 
		{
			//get opcode
			_opcode = GetOpcode ();
			//fetch opcode
			//decode opcode
			//execute opcode

			//update Timers
		}

		public ushort GetOpcode () 
		{

			 /* 
			 * Each chunk of memory is 8bits (1byte) and an opcode is 16 bits (2bytes)
			 * The first half of the opcode is stored at memory[program counter]
			 * the seond at memory[program counter +1]
			 * Lets say memory[pc] == 0xFF (255) and memory[pc+1] == 0xAA (170)
			 * then the opcode would be 0xFFAA (65,450)
			 * take the first byte, shift it 8 bits then or it with the second
			 */

			/*
			 * BUG
			 * There may be a bug here.
			 * It won't work with a ushort.
			 * so i created a uint, cast the opcode to the uint
			 * then cast the uint to a ushort;
			 */ 

			uint result = (uint)_memory [_pc] << 8 | _memory [_pc + 1];
			return (ushort) result;
		}

		private void LoadFontSet () 
		{ 
			// this is the fontset as per a bunch of guides.  Unsure how it works
			byte [] fontset =
				{
					0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
           		 	0x20, 0x60, 0x20, 0x20, 0x70, // 1
			        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
				};
			//fontset is stored in the first 80 bytes of memory
			for (int i = 0; i < 80; i++) 
			{
				_memory [i] = fontset [i];
			}
		}




		public bool [,] PixelState 
		{
			get { return _pixelState; }
		}
	}
}
