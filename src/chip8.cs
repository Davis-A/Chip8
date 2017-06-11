using System;
using System.IO;
namespace MyGame
{
	public class chip8
	{

		public const int CHIP8_X = 64;
		public const int CHIP8_Y = 32;
		public const int TOTAL_REGISTERS = 16;
		public const int MAX_STACK_SIZE = 16;	//There are 16 levels of stack
		public const int TOTAL_MEMORY = 4096;
		public const ushort INITIAL_PROGRAM_ADDRESS = 0x200;
		public const int TOTAL_KEYS = 16;

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
		private bool [,] _pixelState;
		private bool _halt;		//See Opcode 0xFX0A for more details


		private ushort [] _stack;
		private byte _sp;

		//Chip8 has hex keypad 0x0 to 0xF
		private bool [] _keypad;

		//sound stuff
		private byte _delayTimer;
		private byte _soundTimer;
		private bool _redrawScreen = true;


		Random rand = new Random ();




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
			_pc = INITIAL_PROGRAM_ADDRESS;
			_opcode = 0;
			_memory = new byte [TOTAL_MEMORY];
			_registers = new byte [TOTAL_REGISTERS];
			_I = 0;
			_pixelState = new bool [chip8.CHIP8_X, chip8.CHIP8_Y];
			_stack = new ushort [MAX_STACK_SIZE];
			_sp = 0;
			_keypad = new bool [TOTAL_KEYS];
			LoadFontSet ();
			LoadGame ();
			_halt = false;

			//sound stuff
			_delayTimer = 0;
			_soundTimer = 0;

		}




		/// <summary>
		/// Main Cycle.  For optimum emulation it should be run in a loop 60x
		/// per second
		/// </summary>
		public void Cycle ()
		{
			//get opcode
			_opcode = GetOpcode ();
			//Console.WriteLine ("OpCode: {0}", _opcode.ToString ("X4"));
			//Console.WriteLine (_opcode.ToString ("X4"));

			//decode and execute OpCode
			RunOpCode ();
			//if system is halted, do not update timers.  See 0xFX0A
			if (!_halt) 
			{
				UpdateTimers ();
			}
		}


		public void LoadGame (string title)
		{
			string path = @"Resources\Games\" + title.ToUpper ();
			if (!File.Exists (path)) {
				throw new Exception ("Game not found: " + path);
			}

			byte [] gamedata = File.ReadAllBytes (path);

			for (int i = 0; i < gamedata.Length; i++) {
				_memory [INITIAL_PROGRAM_ADDRESS + i] = gamedata [i];
			}
		}

		//By default an empty LoadGame() will load Pong
		public void LoadGame ()
		{
			LoadGame ("tetris");
		}

		public void UpdateTimers () 
		{
			if (_soundTimer > 0) 
			{
				_soundTimer--;
			}

			if (_delayTimer > 0) 
			{
				_delayTimer--;
			}
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

		public bool RedrawScreen 
		{
			get { return _redrawScreen; }
			set { _redrawScreen = value; }
		}

		public bool [] Keystates 
		{
			get { return _keypad; }
		}

		/// <summary>
		/// Method will return the value of the bit at that index
		/// 7 is least significant bit
		/// 0 is most significant bit
		/// </summary>
		public bool IsbitOn (byte b, int q) 
		{
			int i = 7 - q;

			if (i < 0 || i > 7) 
			{
				throw new Exception ("IsbitOn must be given an index (inclusive) betwen 0 & 7");
			}


			byte result = (byte)(b >> i);
			result = (byte)(result & 0x01);
			if (result == 1) 
			{
				return true;
			} else 
			{
				return false;
			}
		}


		////Testing method to change
		//public ushort Opcode
		//{
		//	get { return _opcode; }
		//	set { _opcode = value;}
		//}

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
		public ushort OpcodeNN
		{
			get
			{
				return (ushort)(_opcode & 0x00FF);
			}
		}

		/// <summary>
		/// Returns the 4th nibble from Opcode
		/// (OPCODE AND 0x000F)
		/// </summary>
		public ushort OpcodeN 
		{
			get 
			{
				return (ushort)(_opcode & 0x000F);
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

		public void RunOpCode ()
		{
			switch (_opcode & 0xF000) {
			case 0x0000:
				switch (_opcode & 0x00FF) {
				case 0x00E0: Op0x00E0 (); break;
				case 0x00EE: Op0x00EE (); break;
				default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
				}
				break;
			case 0x1000: Op0x1NNN (); break;
			case 0x2000: Op0x2NNN (); break;
			case 0x3000: Op0x3XNN (); break;
			case 0x4000: Op0x4XNN (); break;
			case 0x5000: Op0x5XY0 (); break;
			case 0x6000: Op0x6XNN (); break;
			case 0x7000: Op0x7XNN (); break;
			case 0x8000:
				switch (_opcode & 0x000F) {
				case 0x0000: Op0x8XY0 (); break;
				case 0x0001: Op0x8XY1 (); break;
				case 0x0002: Op0x8XY2 (); break;
				case 0x0003: Op0x8XY3 (); break;
				case 0x0004: Op0x8XY4 (); break;
				case 0x0005: Op0x8XY5 (); break;
				case 0x0006: Op0x8XY6 (); break;
				case 0x0007: Op0x8XY7 (); break;
				case 0x000E: Op0x8XYE (); break;
				default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
				}
				break;
			case 0x9000: Op0x9XY0 (); break;
			case 0xA000: Op0xANNN (); break;
			case 0xB000: Op0xBNNN (); break;
			case 0xC000: Op0xCXNN (); break;
			case 0xD000: Op0xDXYN (); break;
			case 0xE000:
				switch (_opcode & 0x00FF) {
				case 0x009E: Op0xEX9E (); break;
				case 0x00A1: Op0xEXA1 (); break;
				default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
				}
				break;
			case 0xF000:
				switch (_opcode & 0x00FF) {
				case 0x0007: Op0xFX07 (); break;
				case 0x000A: Op0xFX0A (); break;
				case 0x0015: Op0xFX15 (); break;
				case 0x0018: Op0xFX18 (); break;
				case 0x001E: Op0xFX1E (); break;
				case 0x0029: Op0xFX29 (); break;
				case 0x0033: Op0xFX33 (); break;
				case 0x0055: Op0xFX55 (); break;
				case 0x0065: Op0xFX65 (); break;
				default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
				}
				break;
			//default action if opcode doesn't exist
			default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
			}
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
			_redrawScreen = true;
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
			//BUG don't know whether to increment stack pointer before or after
			//_sp++;
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
				_pc += 2;
			}
			_pc += 2;
		
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
		/// register[X] = register[X] Bitwise AND register[Y]
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
		/// Register[0xF] (final register) is set to 1 if there is a carry 0 otherwise
		///
		/// </summary>
		private void Op0x8XY4 ()
		{
			if ((_registers [OpcodeX] += _registers [OpcodeY]) > 255)
			{
				_registers [0xF] = 1;
			} else
			{
				_registers [0xF] = 0;
			}
			_registers [OpcodeX] += _registers [OpcodeY];
			_pc += 2;
		}

		/// <summary>
		/// Register[X] = Register[X] - Register[Y]
		/// Register[0xF] (final register) is set to 0 when there is a borrow, 1 otherwise
		/// </summary>
		private void Op0x8XY5 () 
		{
			if (_registers [OpcodeX] < _registers [OpcodeY]) 
			{
				_registers [0xF] = 0;
			} else 
			{
				_registers [0xF] = 1;
			}

			_registers [OpcodeX] -= _registers [OpcodeY];
			_pc += 2;
		}

		/// <summary>
		/// register[0xF] is set to the least significant bit of register [X];
		/// register[X] is bitshifted right one.
		/// /// </summary>
		private void Op0x8XY6 () 
		{
			_registers [0xF] = (byte)(_registers [OpcodeX] & 0x01);
			_registers [OpcodeX] = (byte)(_registers [OpcodeX] >> 1);
			_pc += 2;
		}

		/// <summary>
		/// register[X] = register[Y] - register[X];
		/// register[0xF] is set to 0 when there is a borrow, 1 when there isn't
		/// </summary>
		private void Op0x8XY7 () 
		{
			if (_registers [OpcodeY] < _registers [OpcodeX]) 
			{
				_registers [0xF] = 0;
			} else 
			{
				_registers [0xF] = 1;
			}
			_registers [OpcodeX] = (byte)(_registers [OpcodeY] - _registers [OpcodeX]);
			_pc += 2;
		}

		/// <summary>
		/// Register[0xF] is set to the most valuable bit of Register[X]
		/// Register[0xF] = Register[X] & 0x80
		/// Register[X] is shifted left by one
		/// </summary>
		private void Op0x8XYE () 
		{
			_registers [0xF] = (byte)(_registers [OpcodeX] & 0xF);
			_registers [OpcodeX] = (byte)(_registers [OpcodeX] << 1);
			_pc += 2;
		}
		/// <summary>
		/// if Register[X] != Register[Y] then skip an intruction.
		/// </summary>
		private void Op0x9XY0 () 
		{
			if (_registers [OpcodeX] != (_registers [OpcodeY])) 
			{
				_pc += 4;
			} else 
			{
				_pc += 2;
			}
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
		/// jump to address Register[0] + NNN
		/// </summary>
		private void Op0xBNNN () 
		{
			_pc = (ushort)(_registers [0] + OpcodeNNN);
			/*
			 * BUG Given that we are assigning the program counter a new value
			 * Do we assume that it doesn't need to be incremented?
			 */
		}

		/// <summary>
		/// register[X] is assigned a random number based on a generated random 
		/// and a bitwise against NN
		/// </summary>
		private void Op0xCXNN () 
		{
			_registers [OpcodeX] = (byte)(rand.Next (0, 255) & OpcodeNN);
			_pc += 2;
		}



		/// <summary>
		/// Set Register[0xF] = 0
		/// Draws a sprite at coordinate (Register[X], Register[Y])
		/// Sprite is 8xN
		/// Bit state is determined from reading memory at the index register and XOR against itself
		/// If a pixels state is changed (from on to off or off to on) then Register[0xF] is set to 1 (collision detection)
		/// </summary>
		private void Op0xDXYN () 
		{
			_redrawScreen = true;
			//If this exception is hit investigate Opcode DXY0.  
			// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
			//It is a Super Chip 8 opcode
			if (OpcodeN == 0) 
			{
				throw new Exception ("Opcode 0xDXYN hit an outlier where N = 0.  Investigate an additional opcode DXY0 where sprite is 16x16");
			}

			_registers [0xF] = 0;
			uint xpos;
			uint ypos;
			for (int yDelta = 0; yDelta < OpcodeN; yDelta++) 
			{
				for (int xDelta = 0; xDelta < 8; xDelta++) 
				{

					xpos = (uint)(_registers [OpcodeX] + xDelta);
					ypos = (uint)(_registers [OpcodeY] + yDelta);

					//code to wrap
					if (xpos >= CHIP8_X) {
						xpos = xpos - CHIP8_X;
					}

					if (ypos >= CHIP8_Y) {
						ypos = ypos - CHIP8_Y;
					}

					//collision detection
					if ((_pixelState [xpos, ypos]) && (IsbitOn (_memory [_I + yDelta], xDelta))) 
					{
						_registers [0xF] = 1;
					}

					//set the pixel state
					_pixelState [xpos, ypos] = _pixelState [xpos, ypos] ^ IsbitOn (_memory [_I + yDelta], xDelta);
				}
			}
			_pc += 2;
		}


		/// <summary>
		/// If keypad[X] is on skip an instruction
		/// </summary>
		private void Op0xEX9E () 
		{
			if (_keypad [OpcodeX]) 
			{
				_pc += 2;
			}
			_pc += 2;
		}


		/// <summary>
		/// If keypad[X] is off skip an instruction
		/// </summary>
		private void Op0xEXA1 ()
		{
			if (!_keypad [OpcodeX]) 
			{
				_pc += 2;
			}
			_pc += 2;
		}

		/// <summary>
		/// Sets Register[X] = DelayTimer
		/// </summary>
		private void Op0xFX07 () 
		{
			_registers [OpcodeX] = _delayTimer;
			_pc += 2;
		}


		/// <summary>
		/// Halts system till a key is pressed
		/// Stores keynumber of key pressed in Register[X]
		/// </summary>
		private void Op0xFX0A ()
		{
			/*
			 * Halt bool.  This Opcode tells the Chip8 to halt pending an interrupt on the keypad 
			 * However to get input we need to escape this opcode method and allow the calling program to process inputs and modify keystates
			 * When halt bool is set to true this opcode will not increment the program counter
			 * Nor will the system update the sound and countdown timers
			 * This effectively halts the systems state as it will continue to execute this opcode
			 * When a keystate has been set to down (on / true) then the halt bool will be turned off and allow the program counter to increment
			 */

			_halt = true;

			for (int keynum = 0; keynum > TOTAL_KEYS; keynum++) 
			{
				if (_keypad [keynum]) 
				{
					_registers [OpcodeX] = (byte)keynum;
					_halt = false;
				}

				if (!_halt) 
				{
					_pc += 2;
				}

			}
		}

		/// <summary>
		/// sets Delay_Timer = Register[X]
		/// </summary>
		private void Op0xFX15 () 
		{
			_delayTimer = _registers [OpcodeX];
			_pc += 2;
		}


		/// <summary>
		/// Sets Sound_Timer to Register[X]
		/// </summary>
		private void Op0xFX18 () 
		{
			_soundTimer = _registers [OpcodeX];
			_pc += 2;
		}

		/// <summary>
		/// Adds Register[X] to the MemoryIndex
		/// I += _Register[X]
		/// </summary>
		private void Op0xFX1E () 
		{
			_I += _registers [OpcodeX];
			_pc += 2;
		}






		/// <summary>
		/// Each font digit has a sprite to draw it to screen
		/// Register[X] contains a number that needs to be drawn to the screen
		/// Sets Memory_Index to the memory location of the sprite corrisponding in Register[X]
		/// </summary>
		private void Op0xFX29 () 
		{
			/*
			 * Font is loaded at the start of memory
			 * And each font sprite takes 5 bytes. 
			 * Therefore font location is
			 * 0 + fontNumber * 5
			 */
			_I = (ushort)(_registers [OpcodeX] * 5);
			_pc += 2;
		}

		/// <summary>
		/// Stores Binary-Coded Decimal of Register[X] at memory[Memory_Index]
		/// </summary>
		private void Op0xFX33 () 
		{
			_memory [_I] = (byte)(_registers [OpcodeX] / 10);
			_memory [_I + 1] = (byte)((_registers [OpcodeX] / 10) % 10);
			_memory [_I + 2] = (byte)(_registers [OpcodeX] % 10);
			_pc += 2;
		}

		/// <summary>
		/// Stores Register[0] to (inclusive) Register[X] in memory
		/// Starting at memory[Memory_Index]
		/// </summary>
		private void Op0xFX55 () 
		{
			for (int i = 0; i <= OpcodeX; i++) 
			{
				_memory [_I + i] = _registers [0 + i];
			}
			_pc += 2;
		}

		/// <summary>
		/// Loads memory into registers
		/// Starting at memory[Memory_Index]
		/// Loaded into Register[0] to (inclusive) Register[X]
		/// </summary>
		private void Op0xFX65 () 
		{
			for (int i = 0; i <= OpcodeX; i++)
			{
				 _registers [0 + i] = _memory [_I + i];
			}
			_pc += 2;
		}

		/*
			 * ****************************
			 * Opcode definitions end here
			 * ***************************
			 */

	}
}
