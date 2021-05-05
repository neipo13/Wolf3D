using Nez.Textures;

namespace Nez.Sprites
{
	public class SpriteAnimation
	{
		public readonly Sprite[] Sprites;
		public readonly float FrameRate;

		public SpriteAnimation(Sprite[] sprites, float frameRate = 12)
		{
			Sprites = sprites;
			FrameRate = frameRate;
		}
	}
}
