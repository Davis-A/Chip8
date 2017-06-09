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


        public static void Main()
        {
			
			chip8 machine = new chip8 ();
			//machine.SetAllPixels (true);


			machine.Opcode = 0xD115;
			machine.Cycle ();

			//to test pixel on/off state
			/*
			machine.PixelState [0, 0] = true;
			machine.PixelState  [1, 1] = true;
			machine.PixelState  [0, 2] = true;
			machine.PixelState  [10, 10] = true;
			*/
		           
            //Run the game loop
			SwinGame.OpenGraphicsWindow ("Chip8", chip8.CHIP8_X * MULTIPLIER, chip8.CHIP8_Y * MULTIPLIER);
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
                
                //Clear the screen and draw the framerate
				SwinGame.ClearScreen(Color.Black);

				RenderPixel (machine.PixelState);  

              	
                //SwinGame.DrawFramerate(0,0);
                //Draw onto the screen
                SwinGame.RefreshScreen(60);
            }

        }
    }
}