using System;
using SwinGameSDK;

namespace MyGame
{
    public class GameMain
    {

		 
		//multiplier for chip8 video.  This creates a 16:1 pixel output
		const int MULTIPLIER = 16;



		public static void RenderPixel (bool [,] pixelState)
		{
			for (int x = 0; x < chip8.CHIP8_X; x++) 
			{
				for (int y = 0; y < chip8.CHIP8_Y; y++) 
				{
					if (pixelState [x, y])
					{
						SwinGame.FillRectangle (Color.White, x * MULTIPLIER, y * MULTIPLIER, MULTIPLIER, MULTIPLIER);
					}
					//gridlines
					SwinGame.DrawRectangle (Color.Blue, x * MULTIPLIER, y * MULTIPLIER, MULTIPLIER, MULTIPLIER);

				}
			}
		}

		public static void HandleInput (bool[] key) 
		{
			for (int i = 0; i < 16; i++) 
			{
				key [i] = false;
			}

			if (SwinGame.KeyDown (KeyCode.KeyPad0)) { key [0] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad1)) { key [1] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad2)) { key [2] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad3)) { key [3] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad4)) { key [4] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad5)) { key [5] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad6)) { key [6] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad7)) { key [7] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad8)) { key [8] = true; }
			if (SwinGame.KeyDown (KeyCode.KeyPad9)) { key [9] = true; }
			if (SwinGame.KeyDown (KeyCode.ZKey)) { key [10] = true; }
			if (SwinGame.KeyDown (KeyCode.XKey)) { key [11] = true; }
			if (SwinGame.KeyDown (KeyCode.CKey)) { key [12] = true; }
			if (SwinGame.KeyDown (KeyCode.AKey)) { key [13] = true; }
			if (SwinGame.KeyDown (KeyCode.SKey)) { key [14] = true; }
			if (SwinGame.KeyDown (KeyCode.DKey)) { key [15] = true; }

			for (int i = 0; i < 16; i++) {
				if (key [i]) 
				{
					Console.WriteLine ("key: " + i.ToString ("X1"));
				}
			}

			    

		}

		public static void RenderOtherMethod (byte[] b) 
		{
			int x = 0;
			int y = 0;

			for (int i = 0; i < 2048; i++) 
			{
				if (b [i] == 1) 
				{
					SwinGame.FillRectangle (Color.White, x * MULTIPLIER, y * MULTIPLIER, MULTIPLIER, MULTIPLIER);
				}
				x++;
				if (x == 64) 
				{
					x = 0;
					y++;
				}

			}
		}


        public static void Main()
        {
			chip8 machine = new chip8 ();
		           
            //Run the game loop
			SwinGame.OpenGraphicsWindow ("Chip8", chip8.CHIP8_X * MULTIPLIER, chip8.CHIP8_Y * MULTIPLIER);
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
				HandleInput (machine.Keystates);
				machine.Cycle ();

				//Clear the screen and draw the framerate
				if (machine.RedrawScreen) 
				{
					machine.RedrawScreen = false;
					SwinGame.ClearScreen (Color.Black);
					//RenderOtherMethod (machine._videoMemory);
					RenderPixel (machine.PixelState);
					//SwinGame.DrawFramerate(0,0);
					//Draw onto the screen
					SwinGame.RefreshScreen (60);
				}
            }

        }
    }
}