/* * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *  Author: Timeless Puck (2025)                       *
 *  This code is open and free to use for any purpose. *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using CoolCustomText.Source;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CoolCustomText
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;
        private CustomText _customText;
        private CustomText _infoCustomText;
        private Texture2D _pixelTex;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(_spriteBatch);

            string text = "Hello stranger, are you <fx 2,0,0,1>good</fx> ?\n<fx 1,1,0,0>*************************************</fx><fx 6,0,1,0>This line is scared</fx> <fx 6,1,0,0>></fx><fx 7,0,0,0>0123456789</fx><fx 6,1,0,0><</fx>";
            Vector2 textDim = new(284f, 60f); // The dimension is multiplied by 4 later.
            Vector2 position = new(50f);

            _customText = new(this, "PixellariFont", text, position, textDim)
            {
                Scale = new(4f), // Scale the dimension. This is useful if you're working with scaled UI and want to have a coherent dimension.
                Color = new(255, 244, 196),
                Padding = new(20f, 0f),
                ShadowColor = new(128, 85, 111), // Color.Transparent to disable it.
            };
            _customText.Refresh(); // Don't forget to refresh the text after the initialization and after you change the text properties.

            _infoCustomText = new(this, "SmallPixellariFont",
                "The gray box represents the dimension of the custom text but\nthe input text is rendered into the green box because we have set a padding.\n" +
                "Also the text overflows on the y axis as can see here." +
                "\nNewlines works\nperfectly too           (consecutives spaces        too).\n\n" +
                "Finally to give a <fx 5,1,0,1>special effect</fx> to your text, use the fx tag by setting the profile of the specific effect " +
                "(ignore effect with zero), the syntax is:\n" +
                "< fx Color Palette profile,Wave profile,Shake profile,Hang profile>text< /fx>\n" +
                "<fx 0,1,0,0>  ^</fx> ignore this space                                                 ignore this space <fx 0,1,0,0>^</fx>\n" +
                "See README.md to know how to create new profiles.",
                new(40f, 310f), new(1200f, 90f), padding: new(0, 10f));
            _infoCustomText.Refresh();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _pixelTex = Content.Load<Texture2D>("WhitePixel");
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _customText.Update(deltaTime);
            _infoCustomText.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            DrawCustomTextBounds(_customText);
            _customText.Draw();

            DrawCustomTextBounds(_infoCustomText);
            _infoCustomText.Draw();

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draw the bounds of a custom text to have a visual debug.
        /// </summary>
        /// <param name="t">The custom text.</param>
        public void DrawCustomTextBounds(CustomText t)
        {
            Color dimColor = new(64, 64, 64, 64);
            Color paddingColor = new(0, 64, 0, 64);
            Vector2 scale = t.Dimension * t.Scale;

            _spriteBatch.Draw(_pixelTex, t.Position, _pixelTex.Bounds, dimColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_pixelTex, t.Position + t.Padding, _pixelTex.Bounds, paddingColor, 0f, Vector2.Zero,
                scale - 2 * t.Padding, SpriteEffects.None, 0f);
        }
    }
}
