using System;
namespace MyGame
{
	public class chip8
	{
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


		public chip8 (bool [,] pixelState)
		{
			_memory = new byte [4096];
			_registers = new byte [16];
			_pixelState = pixelState;
		}
	}
}
