using System;
using SwinGameSDK;

namespace MyGame
{
    public class GameMain
    {

		/// <summary>
		/// temporary method to fill up pixelstate array to test screen renderer.
		/// </summary>
		/// <param name="pixilState">Pixil state.</param>
		///


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


        public static void Main()
        {
            //Open the game window
			//chip 8 is 64x32
			//Therefore 4:1 is 128x64
			//256x128 is 4:1 of above
			//512x256
            SwinGame.OpenGraphicsWindow("GameMain", 512, 256);
			//a 2d array would make more sense
			bool [] pixelState = new bool [2048];
			tempAlternatebool (pixelState);
			tempSetAllbool (pixelState);


			//pixelState [0] = true;
           
            
            //Run the game loop
            while(false == SwinGame.WindowCloseRequested())
            {
                //Fetch the next batch of UI interaction
                SwinGame.ProcessEvents();
                
                //Clear the screen and draw the framerate
                SwinGame.ClearScreen(Color.White);

				RenderPixel (pixelState);











				//SwinGame.DrawRectangle (Color.Blue, 50,50, 100, 100);
                SwinGame.DrawFramerate(0,0);
                
                //Draw onto the screen
                SwinGame.RefreshScreen(60);
            }
        }
    }
}