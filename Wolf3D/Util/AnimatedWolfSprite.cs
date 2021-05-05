using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;

namespace Wolf3D.Util
{
    public class AnimatedWolfSprite : WolfSprite, IUpdatable
    {
        public enum LoopMode
        {
            /// <summary>
            /// Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
            /// </summary>
            Loop,

            /// <summary>
            /// Play the sequence once [A][B][C] then pause and set time to 0 [A]
            /// </summary>
            Once,

            /// <summary>
            /// Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing
            /// </summary>
            ClampForever,

            /// <summary>
            /// Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
            /// </summary>
            PingPong,

            /// <summary>
            /// Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
            /// </summary>
            PingPongOnce
        }

        public enum State
        {
            None,
            Running,
            Paused,
            Completed
        }

        /// <summary>
        /// fired when an animation completes, includes the animation name;
        /// </summary>
        public event Action<string> OnAnimationCompletedEvent;

        /// <summary>
        /// animation playback speed
        /// </summary>
        public float Speed = 1;

        /// <summary>
        /// the current state of the animation
        /// </summary>
        public State AnimationState { get; private set; } = State.None;

        /// <summary>
        /// the current animation
        /// </summary>
        public SpriteAnimation CurrentAnimation { get; private set; }

        /// <summary>
        /// the name of the current animation
        /// </summary>
        public string CurrentAnimationName { get; private set; }

        /// <summary>
        /// index of the current frame in sprite array of the current animation
        /// </summary>
        public int CurrentFrame { get; set; }

        /// <summary>
        /// checks to see if the CurrentAnimation is running
        /// </summary>
        public bool IsRunning => AnimationState == State.Running;

        /// <summary>
        /// Provides access to list of available animations
        /// </summary>
        public Dictionary<string, SpriteAnimation> Animations { get { return _animations; } }

        readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();

        float _elapsedTime;
        LoopMode _loopMode;

        Dictionary<string, Color[]> FrameColors;


        public AnimatedWolfSprite(PlayerState playerstate) : base(playerstate)
        {
            _animations = new Dictionary<string, SpriteAnimation>();
            FrameColors = new Dictionary<string, Color[]>();
        }

        //caching colors changes for 
        public override void SetSprite(Sprite Sprite)
        {
            this.Sprite = Sprite;
            var subString = Sprite.ToString();
            if (FrameColors.ContainsKey(subString))
            {
                SpriteColors = FrameColors[subString];
            }
            else
            {
                SpriteColors = Util.SpriteHelpers.GetColors(Sprite);
                FrameColors.Add(subString, SpriteColors);
            }
        }

        public virtual void Update()
        {
            if (AnimationState != State.Running || CurrentAnimation == null)
                return;

            var animation = CurrentAnimation;
            var secondsPerFrame = 1 / (animation.FrameRate * Speed);
            var iterationDuration = secondsPerFrame * animation.Sprites.Length;
            var pingPongIterationDuration = animation.Sprites.Length < 3 ? iterationDuration : secondsPerFrame * (animation.Sprites.Length * 2 - 2);

            _elapsedTime += Time.DeltaTime;
            var time = Math.Abs(_elapsedTime);

            // Once and PingPongOnce reset back to Time = 0 once they complete
            if (_loopMode == LoopMode.Once && time > iterationDuration ||
                _loopMode == LoopMode.PingPongOnce && time > pingPongIterationDuration)
            {
                AnimationState = State.Completed;
                _elapsedTime = 0;
                CurrentFrame = 0;
                SetSprite(animation.Sprites[0]);
                OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
                return;
            }

            if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
            {
                AnimationState = State.Completed;
                CurrentFrame = animation.Sprites.Length - 1;
                SetSprite(animation.Sprites[CurrentFrame]);
                OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
                return;
            }

            // figure out which frame we are on
            int i = Mathf.FloorToInt(time / secondsPerFrame);
            int n = animation.Sprites.Length;
            if (n > 2 && (_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce))
            {
                // create a pingpong frame
                int maxIndex = n - 1;
                CurrentFrame = maxIndex - Math.Abs(maxIndex - i % (maxIndex * 2));
            }
            else
                // create a looping frame
                CurrentFrame = i % n;

            SetSprite(animation.Sprites[CurrentFrame]);
        }

        /// <summary>
        /// adds all the animations from the SpriteAtlas
        /// </summary>
        public void AddAnimationsFromAtlas(SpriteAtlas atlas)
        {
            for (var i = 0; i < atlas.AnimationNames.Length; i++)
                _animations.Add(atlas.AnimationNames[i], atlas.SpriteAnimations[i]);
        }

        /// <summary>
        /// Adds a SpriteAnimation
        /// </summary>
        public void AddAnimation(string name, SpriteAnimation animation)
        {
            // if we have no sprite use the first frame we find
            if (Sprite == null && animation.Sprites.Length > 0)
                SetSprite(animation.Sprites[0]);
            _animations[name] = animation;

            // fill the frame color cache
            for (int i = 0; i < animation.Sprites.Length; i++)
            {
                var subTexture = animation.Sprites[i];
                var subString = subTexture.ToString();
                if (!FrameColors.ContainsKey(subString))
                {
                    FrameColors.Add(subString, SpriteHelpers.GetColors(subTexture));
                }
            }
        }

        public void AddAnimation(string name, Sprite[] sprites, float fps = 10) => AddAnimation(name, fps, sprites);

        public void AddAnimation(string name, float fps, params Sprite[] sprites)
        {
            AddAnimation(name, new SpriteAnimation(sprites, fps));
        }

        #region Playback

        /// <summary>
        /// plays the animation with the given name. If no loopMode is specified it is defaults to Loop
        /// </summary>
        public void Play(string name, LoopMode? loopMode = null, bool startOver = false)
        {
            if (!startOver && CurrentAnimationName == name && AnimationState == State.Running) return;
            CurrentAnimation = _animations[name];
            CurrentAnimationName = name;
            CurrentFrame = 0;
            AnimationState = State.Running;

            SetSprite(CurrentAnimation.Sprites[0]);
            _elapsedTime = 0;
            _loopMode = loopMode ?? LoopMode.Loop;
        }

        /// <summary>
        /// checks to see if the animation is playing (i.e. the animation is active. it may still be in the paused state)
        /// </summary>
        public bool IsAnimationActive(string name) => CurrentAnimation != null && CurrentAnimationName.Equals(name);

        /// <summary>
        /// pauses the animator
        /// </summary>
        public void Pause() => AnimationState = State.Paused;

        /// <summary>
        /// unpauses the animator
        /// </summary>
        public void UnPause() => AnimationState = State.Running;

        /// <summary>
        /// stops the current animation and nulls it out
        /// </summary>
        public void Stop()
        {
            CurrentAnimation = null;
            CurrentAnimationName = null;
            CurrentFrame = 0;
            AnimationState = State.None;
        }

        #endregion
    }
}
