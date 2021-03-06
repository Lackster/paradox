﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// Configuration class the Long Press gestures.
    /// </summary>
    /// <remarks>A longPress gesture can be composed of 1 or more fingers.</remarks>
    public sealed class GestureConfigLongPress : GestureConfig
    {
        /// <summary>
        /// The value represents the maximum distance a finger can translate during the longPress action.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        /// <remarks>
        /// By increasing this value, the user allows small movements of the fingers during the long press.
        /// By decreasing this value, the user forbids any movements during the long press.</remarks>
        public float MaximumTranslationDistance
        {
            get { return maximumTransDst; }
            set
            {
                CheckNotFrozen();

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                maximumTransDst = value;
            }
        }
        private float maximumTransDst;

        /// <summary>
        /// The time the user has to hold his finger on the screen to trigger the gesture.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public TimeSpan RequiredPressTime
        {
            get { return requiredPressTime; }
            set
            {
                CheckNotFrozen();

                requiredPressTime = value;
            }
        }
        private TimeSpan requiredPressTime;

        /// <summary>
        /// Create a default LongPress gesture configuration. 
        /// </summary>
        /// <remarks>Single finger and 1 second long press.</remarks>
        public GestureConfigLongPress()
        {
            AssociatedGestureType = GestureType.LongPress;

            RequiredNumberOfFingers = 1;
            RequiredPressTime = TimeSpan.FromSeconds(0.75);
            MaximumTranslationDistance = 0.02f;
        }

        internal override GestureRecognizer CreateRecognizerImpl(float screenRatio)
        {
            return new GestureRecognizerLongPress(this, screenRatio);
        }
    }
}