using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    class FarmerSprite : StardewValley.FarmerSprite
    {
        public FarmerSprite()
            : base()
        {
        }

        public FarmerSprite(string texture)
            : base(texture)
        {
        }

        public FarmerSprite(StardewValley.FarmerSprite copyFrom)
            : base(copyFrom.textureName.Value)
        {
            this.interval = copyFrom.interval;
            this.SpriteWidth = copyFrom.SpriteWidth;
            this.SpriteHeight = copyFrom.SpriteHeight;

            this.currentAnimationIndex = copyFrom.SpriteHeight;
            this.oldFrame = copyFrom.oldFrame;
            this.currentAnimation = copyFrom.currentAnimation;
            this.textureUsesFlippedRightForLeft = copyFrom.textureUsesFlippedRightForLeft;
            this.ignoreStopAnimation = copyFrom.ignoreStopAnimation;
            this.ignoreSourceRectUpdates = copyFrom.ignoreSourceRectUpdates;
            this.loop = copyFrom.loop;
            this.tempSpriteHeight = copyFrom.tempSpriteHeight;
            this.currentFrame = copyFrom.currentFrame;
            this.framesPerAnimation = copyFrom.framesPerAnimation;
            this.interval = copyFrom.interval;
            this.timer = copyFrom.timer;
            this.sourceRect = copyFrom.sourceRect;
            // this.endOfAnimationFunction = copyFrom.endOfAnimationFunction;
            // this.contentManager = copyFrom.contentManager;

            this.SpriteWidth = copyFrom.SpriteWidth;
            this.SpriteHeight = copyFrom.SpriteHeight;
            this.SourceRect = copyFrom.SourceRect;
            this.CurrentFrame = copyFrom.CurrentFrame;
            this.CurrentAnimation = copyFrom.CurrentAnimation;

            this.intervalModifier = copyFrom.intervalModifier;
            this.animatingBackwards = copyFrom.animatingBackwards;
            this.nextOffset = copyFrom.nextOffset;
            this.currentStep = copyFrom.currentStep;
            this.currentSingleAnimationInterval = copyFrom.currentSingleAnimationInterval;
            this.ignoreDefaultActionThisTime = copyFrom.ignoreDefaultActionThisTime;
            this.pauseForSingleAnimation = copyFrom.pauseForSingleAnimation;
            this.freezeUntilDialogueIsOver = copyFrom.freezeUntilDialogueIsOver;
            this.loopThisAnimation = copyFrom.loopThisAnimation;
            this.animateBackwards = copyFrom.animateBackwards;

            this.CurrentToolIndex = copyFrom.CurrentToolIndex;
            this.PauseForSingleAnimation = copyFrom.PauseForSingleAnimation;
            this.CurrentFrame = copyFrom.CurrentFrame;
        }

        // We can't override most of our parent classes stuff,
        // but we can override CurrentFrame, which luckily gets accessed
        // every time our parent class has just updated CurrentAnimation,
        // which is what we actually want to hook into.
        public override int CurrentFrame
        {
            get
            {
                FixCurrentAnimation();
                return base.CurrentFrame;
            }
            set
            {
                FixCurrentAnimation();
                base.CurrentFrame = value;
            }
        }

        public override void setCurrentAnimation(List<FarmerSprite.AnimationFrame> animation)
        {
            base.setCurrentAnimation(animation);
            FixCurrentAnimation();
        }

        private bool IsHarvestAnimation()
        {
            return this.CurrentAnimation != null
                && this.CurrentAnimation.Count == 6
                && (this.CurrentAnimation[0].frame == 62 || this.CurrentAnimation[0].frame == 54 || this.CurrentAnimation[0].frame == 58)
                && this.CurrentAnimation[1].frameBehavior != null
                && this.CurrentAnimation[1].frameBehavior == Farmer.showItemIntake;
        }

        private void FixCurrentAnimation()
        {
            if (IsHarvestAnimation())
            {
                this.CurrentAnimation = new AnimationFrame[6]
                {
                    FixAnimationFrame(this.CurrentAnimation[0]),
                    FixAnimationFrame(this.CurrentAnimation[1]),
                    FixAnimationFrame(this.CurrentAnimation[2]),
                    FixAnimationFrame(this.CurrentAnimation[3]),
                    FixAnimationFrame(this.CurrentAnimation[4]),
                    FixAnimationFrame(this.CurrentAnimation[5])
                }.ToList();
            }
        }

        private AnimationFrame FixAnimationFrame(AnimationFrame animationFrame)
        {
            return new AnimationFrame(animationFrame.frame, animationFrame.milliseconds, animationFrame.secondaryArm, animationFrame.flip, FixAnimationFrameBehavior(animationFrame.frameBehavior), animationFrame.behaviorAtEndOfFrame);
        }

        private endOfAnimationBehavior FixAnimationFrameBehavior(endOfAnimationBehavior frameBehavior)
        {
            if (frameBehavior == null)
                return null;

            return InterceptFarmerShowItemIntake;
        }

        private static void InterceptFarmerShowItemIntake(Farmer who)
        {
            if (who.mostRecentlyGrabbedItem is EasterEggItem easterEggItem)
            {
                ShowEasterEggItemIntake(who);
            }
            else
            {
                Farmer.showItemIntake(who);
            }
        }

        private static void ShowEasterEggItemIntake(Farmer who)
        {
            // TODO
        }
    }
}
