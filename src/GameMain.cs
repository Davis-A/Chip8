using System;
using SwinGameSDK;

namespace MyGame
{
    public class GameMain
    {
		const int chip8_X = 64;
		const int chip8_Y = 32;
		const int ScreenWidth = 512;
		const int ScreenHeight = 256;
		const int ScreenMultiplier = 8;


		public static void tempSetAllbool (bool [] pixelState)
		{
			for (int i = 0; i < 2048; i++) 
			{
				pixelState [i] = true;
			}
		}

		public static void tempAlternatebool (bool [] pixelState) 
		{
			bool state = true;

			for (int i = 0; i < 2048; i++)
			{
				if (state) 
				{
					pixelState [i] = true;
					state = false;
				}else 
				{
					pixelState [i] = false;
					state = true;
				}
			}
		}


		public static void RenderPixel (bool [,] pixelState)
		{
			for (int x = 0; x < chip8_X; x++) 
			{
				for (int y = 0; y < chip8_Y; y++) 
				{
					if (pixelState [x, y]) 
					{
					SwinGame.FillRectangle (Color.White, x * ScreenMultiplier, y * ScreenMultiplier, ScreenMultiplier, ScreenMultiplier);
					}

				}
			}
		}

		//render pixel with 1d array
		/*

		public static void RenderPixel (bool [] pixilState)
		{
			float x = 0;
			float y = 0;

			foreach (bool b in pixilState) {
				if (b) {
					SwinGame.FillRectangle (Color.Black, x, y, 8, 8);
				}
				x = x + 8;
				if (x >= 512) {
					x = 0;
					y = y + 8;
				}

			}
		}
		*/


        public static void Main()
        {
            //Open the game window
			//chip 8 is 64x32
			//Therefore 4:1 is 128x64
			//256x128 is 4:1 of above
			//512x256
            SwinGame.OpenGraphicsWindow("GameMain", 512, 256);
			//a 2d array would make more sense

			//1d pixel array code
			/*
			bool [] pixelState = new bool [2048];
			tempAlternatebool (pixelState);
			tempSetAllbool (pixelState);
			*/

			//pixelState [0] = true;

			bool [,] pixelState = new bool [chip8_X, chip8_Y];
			pixelState [0, 0] = true;
			pixelState [1, 1] = true;
			pixelState [0, 2] = true;
			pixelState [10, 10] = true;


           
            
            //Run the game loop
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
                
                //Clear the screen and draw the framerate
				SwinGame.ClearScreen(Color.Black);

				RenderPixel (pixelState);  











				//SwinGame.DrawRectangle (Color.Blue, 50,50, 100, 100);
                SwinGame.DrawFramerate(0,0);
                
                //Draw onto the screen
                SwinGame.RefreshScreen(60);
            }
        }
    }
}