using System;
using System.IO;
namespace MyGame
{
	public class chip8
	{

		public const int CHIP8_X = 64;
		public const int CHIP8_Y = 32;
		public const int TOTAL_REGISTERS = 16;
		public const int MAX_STACK_SIZE = 16;
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
		bool [,] _pixelState;

		//There are 16 levels of stack
		private ushort [] _stack;
		//stack pointer  //BUG unsure if the SP should be 8bit (byte) of 16 bits (ushort).
		private byte _sp;

		//Chip8 has hex keypad 0x0 to 0xF
		private bool [] _keypad;

		//sound stuff
		private byte _delayTimer;
		private byte _soundTimer;


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
			//_opcode = GetOpcode ();
			//decode and execute OpCode
			_I = 0x1 * 5;
			RunOpCode ();



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
				_memory [INITIAL_PROGRAM_ADDRESS + i] = gamedata [i];
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
		/// testing use only
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

		/// <summary>
		/// Iterate through all bytes and set to FF
		/// Testing use only
		/// </summary>
		public void SetAllMemoryOn () 
		{
			for (int i = 0; i < TOTAL_MEMORY; i++) 
			{
				_memory [i] = 0xFF;
			}
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
		/// Sprite is 8x8
		/// Bit state is determined from reading memory at the index register 
		/// If a pixel is on and it is turned off.  Register[0xF] is set to 1 (collision detection)
		/// </summary>
		private void Op0xDXYN () 
		{
			//If this exception is hit investigate Opcode DXY0.  
			// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
			//It is a Super Chip 8 opcode
			if (OpcodeN == 0) 
			{
				throw new Exception ("Opcode 0xDXYN hit an outlier where N = 0.  Investigate an additional opcode DXY0 where sprite is 16x16");
			}



			/*
			 * BUG potential there may be a bug based on the order of bits drawn
			 * assume a sprite where N = 2 then an 8x2 sprite
			 * And the data is 1100110 11001110
			 * Do we read from byte0 least signficant to most, then byte1 from least to most
			 * http://imgur.com/a/4xq1w
			 * Given testing for font sprites it's in reverse
			 */

			/*
			 * As each sprite is 8 pixels horizontal, each row is one byte
			 * Therefore xDelta can be both the horizontal offset for the pixel
			 * and the index for which bit from the array is being considered
			 * Given above as each byte is a horizontal row
			 * To get the next byte in memory we can use the yDelta
			 */

			//TODO write code to handle sprites wrapping around the screen so if it goes off an edge, half of it appears of the other sid




			_registers [0xF] = 0;
			for (int yDelta = 0; yDelta < OpcodeN; yDelta++) 
			{
				for (int xDelta = 0; xDelta < 8; xDelta++) 
				{
					if ( (_pixelState [OpcodeX + xDelta, OpcodeY + yDelta] == true) && (IsbitOn (_memory [_I + yDelta], xDelta) == false))
					{
						_registers [0xF] = 1;
					}
					_pixelState [OpcodeX + xDelta, OpcodeY + yDelta] = IsbitOn (_memory [_I + yDelta], xDelta);
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
			 * We need to create a way for the machine to not internally change its state
			 * while allowing it to continue cycling so external input can change a key state
			 * If the program counter isn't incremented each code loop will result in an identical chip8 loop
			 * Therefore it will continue to enter this opcode until a condition to increment the program counter is met
			 * Will iterate over the keypad.  
			 * If a key is down then it sets the register[X] to it's keypad number and set a trigger boolean
			 * After the loop, if the trigger has been set the program counter will be incremented
			 */
			bool trigger = false;

			for (int keynum = 0; keynum > TOTAL_KEYS; keynum++) 
			{
				if (_keypad [keynum]) 
				{
					_registers [OpcodeX] = (byte)keynum;
					trigger = true;
				}

				if (trigger) 
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
			 * Took a rough stab at implementation
			 * If font is loaded at the start of memory
			 * And each font sprite takes 5 bytes. 
			 * Therefore font location is
			 * 0 + fontNumber * 5
			 */
			_I = (ushort)(_registers [OpcodeX] * 5);
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
									case 0x0000:	Op0x8XY0 (); 	break;
									case 0x0001:	Op0x8XY1 ();	break;
									case 0x0002:	Op0x8XY2 (); 	break;
									case 0x0003:	Op0x8XY3 (); 	break;
									case 0x0004:	Op0x8XY4 (); 	break;
									case 0x0005:	Op0x8XY5 ();	break;
									case 0x0006:	Op0x8XY6 (); 	break;
									case 0x0007:	Op0x8XY7 (); 	break;
									case 0x000E:	Op0x8XYE (); 	break;
									default: 		Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4"));	break;
								}break;
				case 0x9000:	Op0x9XY0 ();	break;
				case 0xA000:	Op0xANNN ();	break;
				case 0xB000: 	Op0xBNNN (); 	break;
				case 0xC000: 	Op0xCXNN (); 	break;
				case 0xD000: 	Op0xDXYN (); 	break;
				case 0xE000: 	switch (_opcode & 0x00FF) 
								{
									case 0x009E: 	Op0xEX9E ();	break;
									case 0x00A1: 	Op0xEXA1 ();	break;
									default: Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4")); break;
								}break;
				case 0xF000:	switch (_opcode & 0x00FF) 
								{
									case 0x0007: 	Op0xFX07 (); 	break;
									case 0x000A: 	Op0xFX0A (); 	break;
									case 0x0015: 	Op0xFX15 (); 	break;
									case 0x0013: 	Op0xFX18 ();	break;
									case 0x001E: 	Op0xFX1E (); 	break;
									case 0x0029:	Op0xFX29 ();	break;
								}break;


				case 0x0000:	switch (_opcode & 0x00FF)
								{
									case 0x00E0: 	Op0x00E0 ();	break;
									case 0x00EE:	Op0x00EE ();	break;
									default:		Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4"));	break;
								}break;

				//default action if opcode doesn't exist
				default:		Console.WriteLine ("I don't know an OpCode {0}", _opcode.ToString ("X4"));	break;
			}
		}
		/*
		 * ****************************
		 * Opcode definitions end here
		 * ****************************
		 */



	}
}
