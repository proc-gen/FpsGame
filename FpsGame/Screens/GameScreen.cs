using FpsGame.Common.Constants;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Screens
{
    public class GameScreen : Screen
    {
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100f);

        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        public GameScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {
            Models.Add("cube", game.Content.Load<Model>("cube"));
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            DrawModel(Vector3.Zero, Models["cube"]);
        }

        private void DrawModel(Vector3 position, Model model)
        {
            foreach(var mesh in model.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}
