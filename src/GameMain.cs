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





		           
            //Run the game loop
			SwinGame.OpenGraphicsWindow ("Chip8", chip8.CHIP8_X * MULTIPLIER, chip8.CHIP8_Y * MULTIPLIER);
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
				machine.Cycle ();
                
                //Clear the screen and draw the framerate
				SwinGame.ClearScreen(Color.Black);

				RenderPixel (machine.PixelState);  

              	
                SwinGame.DrawFramerate(0,0);
                //Draw onto the screen
                SwinGame.RefreshScreen(10);
            }

        }
    }
}