using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
	public class RenderableColorTween : ColorTween, ITweenTarget<Color>
	{
		RenderableComponent _renderable;


		public void SetTweenedValue(Color value)
		{
			if (_renderable == null) return;
			_renderable.Color = value;
		}


		public Color GetTweenedValue()
		{

			if (_renderable == null) return Color.Black;
			return _renderable.Color;
		}


		public new object GetTargetObject()
		{
			return _renderable;
		}


		protected override void UpdateValue()
		{
			SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public void SetTarget(RenderableComponent renderable)
		{
			_renderable = renderable;
		}
		
		public override void RecycleSelf()
		{
			if (_shouldRecycleTween)
			{
				_renderable = null;
				_target = null;
				_nextTween = null;
			}

			if (_shouldRecycleTween && TweenManager.CacheColorTweens)
				Pool<RenderableColorTween>.Free(this);
		}			
	}
}
