using System;
using SwinGameSDK;

namespace MyGame
{
    public class GameMain
    {

		 
		//chip 8 is 64x32
		//Therefore 4:1 is 128x64
		//256x128 is 4:1 of above
		//512x256 is 8:1
		const int CHIP8_X = 64;
		const int CHIP8_Y = 32;
		const int MULTIPLIER = 16;



		public static void RenderPixel (bool [,] pixelState)
		{
			for (int x = 0; x < CHIP8_X; x++) 
			{
				for (int y = 0; y < CHIP8_Y; y++) 
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
         	bool [,] pixelState = new bool [CHIP8_X, CHIP8_Y];
			SwinGame.OpenGraphicsWindow ("GameMain", CHIP8_X * MULTIPLIER, CHIP8_Y * MULTIPLIER);
	

			pixelState [0, 0] = true;
			pixelState [1, 1] = true;
			pixelState [0, 2] = true;
			pixelState [10, 10] = true;

			chip8 machine = new chip8 (pixelState);
            
            //Run the game loop
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
                
                //Clear the screen and draw the framerate
				SwinGame.ClearScreen(Color.Black);

				RenderPixel (pixelState);  

				//SwinGame.DrawRectangle (Color.Blue, 50,50, 100, 100);
               //SwinGame.DrawFramerate(0,0);
                
                //Draw onto the screen
                SwinGame.RefreshScreen(60);
            }
        }
    }
}