﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;

namespace WpfTest
{
	public class ContentScene : WpfGame
	{
		#region Fields

		private int posX = 100, posY = 100;
		private IGraphicsDeviceService _graphicsDeviceManager;
		private WpfKeyboard _keyboard;
		private KeyboardState _keyboardState;
		private WpfMouse _mouse;
		private MouseState _mouseState;
		private float _rotation;
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		private MouseState _previousMouseState;
		private SpriteFont _font;

		#endregion

		#region Methods

		protected override void Draw(GameTime time)
		{
			GraphicsDevice.Clear(_mouseState.LeftButton == ButtonState.Pressed ? Color.Black : Color.CornflowerBlue);

			// since we share the GraphicsDevice with all hosts, we need to save and reset the states
			// this has to be done because spriteBatch internally sets states and doesn't reset themselves, fucking over any 3D rendering (which happens in the DemoScene)

			var blend = GraphicsDevice.BlendState;
			var depth = GraphicsDevice.DepthStencilState;
			var raster = GraphicsDevice.RasterizerState;
			var sampler = GraphicsDevice.SamplerStates[0];

			_spriteBatch.Begin();
			_spriteBatch.Draw(_texture, new Rectangle(posX, posY, 100, 20), null, Color.White, _rotation, new Vector2(_texture.Width, _texture.Height) / 2f, SpriteEffects.None, 0);

			// mouse diagnostics
			_spriteBatch.DrawString(_font, $"Prev Scrollwheel: {_previousMouseState.ScrollWheelValue}", new Vector2(5, 5), Color.White);
			_spriteBatch.DrawString(_font, $"Curr Scrollwheel: {_mouseState.ScrollWheelValue}", new Vector2(5, 25), Color.White);
			_spriteBatch.End();

			// this base.Draw call will draw "all" components (we only added one)
			// since said component will use a spritebatch to render we need to let it draw before we reset the GraphicsDevice
			// otherwise it will just alter the state again and fuck over all the other hosts
			base.Draw(time);

			GraphicsDevice.BlendState = blend;
			GraphicsDevice.DepthStencilState = depth;
			GraphicsDevice.RasterizerState = raster;
			GraphicsDevice.SamplerStates[0] = sampler;

		}

		protected override void Initialize()
		{
			base.Initialize();
			_graphicsDeviceManager = new WpfGraphicsDeviceService(this);

			_texture = Content.Load<Texture2D>("hello");

			// default font is pre-compiled font for Windows (Arial 12, ? as default char)
			// I get away with this because
			// 1) it's just a demo application
			// 2) it can only run on windows/directX anyway (interop for WPF afterall)
			// 3) This means it doesn't require content compiler to be installed on any machine that runs this demo

			_font = Content.Load<SpriteFont>("DefaultFont");

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_keyboard = new WpfKeyboard(this);
			_mouse = new WpfMouse(this);

			Components.Add(new DrawMeComponent(this));
		}

		protected override void Update(GameTime time)
		{
			_previousMouseState = _mouseState;
			_mouseState = _mouse.GetState();
			_keyboardState = _keyboard.GetState();

			if (!_keyboardState.IsKeyDown(Keys.Space))
			{
				_rotation += (float)(2f * time.ElapsedGameTime.TotalSeconds);
			}
			base.Update(time);
		}

		#endregion
	}
}