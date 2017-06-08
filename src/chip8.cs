using System;
using System.IO;
namespace MyGame
{
	public class chip8
	{

		public const int CHIP8_X = 64;
		public const int CHIP8_Y = 32;
		public const ushort MEMORY_START = 0x200;

		/*
		 * All ushorts should be ushorts as they only need 16 bits
		 * C# is deciding i cant case bytes into ushorts but i can into ushorts
		 * so for now i'm using ushorts
		 * could create issues with overflows
		 */

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
		//stack pointer  //BUG unsure if the SP should be 8bit (byte) of 16 bits (ushort).
		private byte _sp;

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
			_pc = MEMORY_START;
			_opcode = 0;
			_memory = new byte [4096];
			_registers = new byte [16];
			_I = 0;
			_pixelState = new bool [chip8.CHIP8_X, chip8.CHIP8_Y];
			_stack = new ushort [16];
			_sp = 0;
			_keypad = new bool [16];
			LoadFontSet ();
			LoadGame ();

			//sound stuff
			//_delayTimer = 0;
			//_soundTimer = 0;

		}




		/// <summary>
		/// Main Cycle.  For optimum emulation it should be run in a loop 60x
		/// per second
		/// </summary>
		public void Cycle ()
		{
			//get opcode
			_opcode = GetOpcode ();
			//temp opcode manipulation
			_opcode = 0x5AB0;
			//decode and execute OpCode
			RunOpCode ();
			//execute opcode

			//update Timers
		}







		public void LoadGame (string title)
		{
			string path = @"Resources\Games\" + title.ToUpper ();
			if (!File.Exists (path)) {
				throw new Exception ("Game not found: " + path);
			}

			byte [] gamedata = File.ReadAllBytes (path);

			for (int i = 0; i < gamedata.Length; i++) {
				_memory [MEMORY_START + i] = gamedata [i];
			}


		}

		//By default an empty LoadGame() will load Pong
		public void LoadGame ()
		{
			LoadGame ("Pong");
		}

		public ushort GetOpcode ()
		{

			 /*
			 * Each chunk of memory is 8bits (1byte) and an opcode is 16 bits (2bytes)
			 * The first half of the opcode is stored at memory[program counter]
			 * the seond at memory[program counter +1]
			 * Lets say memory[pc] == 0xFF (255) and memory[pc+1] == 0xAA (170)
			 * then the opcode would be 0xFFAA (decimal: 65450)
			 * take the first byte, shift it 8 bits then or it with the second
			 */


			return (ushort)(_memory [_pc] << 8 | _memory [_pc + 1]);
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





		/// <summary>
		/// Iterate through all pixels and set them
		/// equal to  the parameter
		/// </summary>
		public void SetAllPixels (bool setvalue)
		{
			for (int x = 0; x < chip8.CHIP8_X; x++)
			{
				for (int y = 0; y < chip8.CHIP8_Y; y++)
				{
					_pixelState [x, y] = setvalue;
				}
			}
		}


		//temp method
		public ushort Opcode
		{
			get { return _opcode; }
			set { _opcode = value;}
		}

		/// <summary>
		/// Returns the second nibble from Opcode
		/// (Opcode AND 0x0F00) >> 8
		/// </summary>
		/// <value>The x.</value>
		public ushort OpcodeX
		{
			get
			{
				return (ushort)((_opcode & 0x0F00) >> 8);
			}
		}

		/// <summary>
		/// Returns the third nibble from Opcode
		/// (Opcode AND 0x00F0) >> 4
		/// </summary>
		/// <value>The y.</value>
		public ushort OpcodeY
		{
			get
			{
				return (ushort)((_opcode & 0x00F0) >> 4);
			}
		}
		/// <summary>
		/// Returns the third and 4th nibble from Opcode
		/// (OPCODE AND 0x00FF)
		/// </summary>
		/// <value>The nn.</value>
		public ushort OpcodeNN
		{
			get
			{
				return (ushort)(_opcode & 0x00FF);
			}
		}

		/// <summary>
		/// Returns all but the first nibble from Opcode
		/// (OPCODE AND 0x0FFF)
		/// </summary>
		/// <value>The nnn.</value>
		public ushort OpcodeNNN {
			get
			{
				return (ushort)(_opcode & 0x0FFF);
			}
		}



		public bool [,] PixelState
		{
			get { return _pixelState; }
		}

		/*
		 * ****************************
		 * Opcode definitions begin here
		 * ****************************
		 */

		/// <summary>
		/// 0x00E0 clears the screen
		/// </summary>
		private void Op0x00E0 ()
		{
			SetAllPixels (false);
			_pc += 2;
		}


		/// <summary>
		/// returns from subroutine
		/// </summary>
		private void Op0x00EE ()
		{
			_sp--;
			_pc = _stack [_sp];
			_pc += 2;
		}

		/// <summary>
		/// Sets _I to NNN
		/// </summary>
		private void Op0xANNN ()
		{
			_I = OpcodeNNN;
			_pc += 2;
		}

		/// <summary>
		/// Jups to address NNN
		/// </summary>
		private void Op0x1NNN ()
		{
			_pc = OpcodeNNN;
		}

		/// <summary>
		/// Calls the subroutine at NNN
		/// </summary>
		private void Op0x2NNN ()
		{
			_stack [_sp] = _pc;
			_sp++;
			_pc = OpcodeNNN;
		}

		/// <summary>
		/// Skip next instruction if register[X] == NN
		/// </summary>
		private void Op0x3XNN ()
		{
			if (_registers [OpcodeX] == OpcodeNN)
			{
				_pc += 4;
			} else
			{
				_pc += 2;
			}
		}
		/// <summary>
		/// Skip next instruction if register[X] != NN
		/// </summary>
		private void Op0x4XNN ()
		{
			if (_registers [OpcodeX] != OpcodeNN)
			{
				_pc += 4;
			} else
			{
				_pc += 2;
			}
		}

		/// <summary>
		/// Skip next instruction if register[X] == register[Y]
		/// </summary>
		private void Op0x5XY0 ()
		{
			if (_registers [OpcodeX] == (_registers [OpcodeY]))
			{
				_pc += 4;
			} else
			{
				_pc += 2;
			}

		}

		/// <summary>
		/// sets register[X] to NN
		/// </summary>
		private void Op0x6XNN ()
		{
			_registers [OpcodeX] = (byte)OpcodeNN;
			_pc += 2;
		}

		/// <summary>
		/// Adds NN to register[X]
		/// </summary>
		private void Op0x7XNN ()
		{
			_registers [OpcodeX] += (byte)OpcodeNN;
			_pc += 2;
		}

		/// <summary>
		/// set register[X] = register[Y]
		/// </summary>
		private void Op0x8XY0 ()
		{
			_registers [OpcodeX] = _registers [OpcodeY];
			_pc += 2;
		}

		/// <summary>
		/// Register X is set to register[X] Bitwise OR regester[Y]
		/// register[X] = register[X] | register[Y]
		/// </summary>
		private void Op0x8XY1 ()
		{
			_registers [OpcodeX] = (byte)((_registers [OpcodeX]) | (_registers [OpcodeY]));
			_pc += 2;
		}

		/// <summary>
		/// Register X is set to register[X] Bitwise AND regester[Y]
		/// register[X] = register[X] & register[Y]
		/// </summary>
		private void Op0x8XY2 ()
		{
			_registers [OpcodeX] = (byte)((_registers [OpcodeX]) & (_registers [OpcodeY]));
			_pc += 2;
		}

		/// <summary>
		/// Register[X] is set to register[X] Bitwise XOR register[Y]
		/// register[X] = register[X] ^ register[Y]
		/// </summary>
		private void Op0x8XY3 ()
		{
			_registers [OpcodeX] = (byte)((_registers [OpcodeX]) ^ (_registers [OpcodeY]));
			_pc += 2;
		}



		/// <summary>
		/// Register[X] is set to Register[Y]
		/// Register[F] (final register) is set to 1 if there is a carry 0 otherwise
		///
		/// </summary>
		private void Op0x8XY4 ()
		{
			//todo deal with with values when overflowing
			if ((_registers [OpcodeX] += _registers [OpcodeY]) > 255)
			{
				_registers [OpcodeX] += _registers [OpcodeY];
				_registers [0xF] = 1;


			} else
			{
				_registers [OpcodeX] += _registers [OpcodeY];
				_registers [0xF] = 0;
			}
			_pc += 2;
		}










		//Run opcode will be moved higher up later
		public void RunOpCode ()
		{
			switch (_opcode & 0xF000)
			{
				case 0x1000:	Op0x1NNN ();	break;
				case 0x2000:	Op0x2NNN ();	break;
				case 0x3000:	Op0x3XNN ();	break;
				case 0x4000:	Op0x4XNN ();	break;
				case 0x5000:	Op0x5XY0 ();	break;
				case 0x6000:	Op0x6XNN ();	break;
				case 0x7000: 	Op0x7XNN (); 	break;
				case 0x8000: 	switch (_opcode & 0x000F)
								{
									case 0x0000: Op0x8XY0 (); 	break;
									case 0x0001: Op0x8XY1 ();	break;
									case 0x0002: Op0x8XY2 (); 	break;
									case 0x0003: Op0x8XY3 (); 	break;
									//case 0x0004: Op0x8XY4 (); 	break;
								}break;





				case 0xA000:	Op0xANNN ();	break;









				//non standard opcodes that relies on more than first digit to determine action
				case 0x0000:
				switch (_opcode & 0x00FF)
				{
					case 0x00E0: 	Op0x00E0 ();	break;
					case 0x00EE:	Op0x00EE ();	break;

				default:
					Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4"));
					break;
				}
				break;

				//default action if opcode doesn't exist
			default:
				Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4"));
				break;
			}
		}
		/*
		 * ****************************
		 * Opcode definitions end here
		 * ****************************
		 */


	}
}
